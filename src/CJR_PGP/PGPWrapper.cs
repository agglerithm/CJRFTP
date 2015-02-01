namespace CJR_PGP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CJR.Common;
    using CJR.Common.IO;
    using CJR.Common.Extensions;
    using SBPGPKeys;

    public interface IPGPWrapper
    {
        void DecryptFile(string origin_path, string dest_path);

        void DecryptAll(string origin_path, string dest_path);
        void EncryptAndSignFile(string origin_path, string dest_path, string pubkey);
        IList<FileEntity> EncryptAndSignAll(string origin_path, string dest_path, string pubkey);
        void EncryptFile(string origin_path, string dest_path, string pubkey);
    }

    public class PGPWrapper : IPGPWrapper
    {
        private readonly IFileUtilities _fUtil;
        private readonly IPGPKeys _keys;
        private readonly IPGPIO _files;
        private readonly string _our_public_key_path;
        private readonly string _our_private_key_path;
        private readonly string _passphrase; 

        public PGPWrapper(IFileUtilities f_util, IPGPKeys keys, IPGPIO files, string our_public_key_path,
                          string our_private_key_path, string passphrase)
        {
            _fUtil = f_util;
            SBUtils.__Global.SetLicenseKey(
                SBUtils.__Global.BytesOfString("0190B807FC3C84EE4CAE3222EA8EBCC503B55E8ADEF6693234E36786EB2D9D4FC7AE8D79E5FAF39B757F24D91612EB46AB96F54B154B88E5DC3F1E17B9A1F4C9DC47ED662B1C088FD381213252B7633FFEC2D4D88DE70415BC9F89403D1D49A26E8279017526C44AFE15A6B2F707F60A984E25E7A8AFA6ABE01DFB9A7DC12A20B82ACB542145733EC394913AB6966604BA3ED0FEE2145667889094BC993B33E11648552AF6D8574D751BF22CE5BE648EFF26A3B0B785B95F0C5A8C239F567FAA19FB293E3EFAA0A7D17D58EE9295841A0FD08EBCA3A74580C338F6F614833F3C00CD217E4B74977070AA6D37831EA712542B7784481C178E0789F29BC749773A")); 
            _keys = keys;
            _files = files;
            _our_public_key_path = our_public_key_path;
            _our_private_key_path = our_private_key_path;
            _passphrase = passphrase;
        }

        public void DecryptFile(string origin_path, string dest_path)
        {
            var decrypter = get_decrypter(dest_path);
            decrypter.DecryptAndVerifyFile(origin_path, dest_path);
        }

        private IPGPReader get_decrypter(string dest_path)
        {
            _keys.LoadMasterKeyring(_our_public_key_path, _our_private_key_path); 
            var decrypter = _files.SetDecrypter(_passphrase,_keys.Master,dest_path, decrypter_OnPassphrase); 
            return decrypter;
        }

        public void DecryptAll(string origin_path, string dest_path )
        {
            var decrypter = get_decrypter(dest_path);
            var files = _fUtil.GetListFromFolder(origin_path, "*.*", DateTime.Today.AddYears(-30));
            files.ForEach(file =>
                              {
                                  decrypter.DecryptAndVerifyFile(file.FullPath,
                                                                 dest_path.ConcatenateFilePaths(
                                                                 file.FileName));
                                  _fUtil.MoveFileWithOverwrite(file.FullPath, 
                                      getArchivePath(file.ContainingFolder).ConcatenateFilePaths(file.FileName));
                                   
                              });
        }

        private string getArchivePath(string path)
        {
            return _fUtil.EnsurePath(path.ConcatenateFilePaths("archive"));
        }

        void decrypter_OnPassphrase(object Sender, ref string Passphrase, ref bool Cancel)
        {
            Passphrase = _passphrase;
        }


        public void EncryptAndSignFile(string originPath, string destPath, string theirPublicKey)
        { 
            var encrypter = get_encrypter(theirPublicKey);
            encrypter.EncryptAndSign(originPath, destPath);
        }

 

        private IPGPWriter get_encrypter(string their_public_key)
        {
            _keys.LoadMasterKeyring(_our_public_key_path, _our_private_key_path);
            _keys.RemovePublicKey(false); 
            _keys.GeneratePublicKey(their_public_key);
            var encrypter = _files.SetEncrypter(_keys.Public, _keys.Private, _passphrase, encrypter_OnKeyPassphrase);

            return encrypter;
        }

        public IList<FileEntity> EncryptAndSignAll(string originPath, string destPath, 
            string theirPublicKey)
        {
            ensurePath(destPath);
            var workingPath = destPath.ConcatenateFilePaths(@"working\");
            ensurePath(workingPath);
            var encrypter = get_encrypter(theirPublicKey);
            var files = _fUtil.GetListFromFolder(originPath, "*.*", DateTime.Today.AddYears(-30));
            files.ForEach(file =>
                              {
                                    try
                                    { 
                                      encrypter.EncryptAndSign(file.FullPath, workingPath.ConcatenateFilePaths(file.FileName));
                                      moveToSendFolder(file, destPath, workingPath);
                                        moveToArchiveFolder(file, originPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(this, string.Format("Could not find encrypted file {0}.", file.FileName), ex);
                                    }
                              }); 
            //_fUtil.MoveAll(originPath, _fUtil.EnsurePath(originPath.ConcatenateFilePaths(@"Archive\")));
            return _fUtil.GetListFromFolder(destPath, "*.*", DateTime.Today.AddYears(-30));
        }

        private static void moveToArchiveFolder(FileEntity file, string originPath)
        {
            var archiveFile = originPath.ConcatenateFilePaths(@"Archive\").ConcatenateFilePaths(file.FileName);
            File.Move(file.FullPath,archiveFile);
        }

        private static void ensurePath(string destPath)
        {
            if(!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);
        }

        private void moveToSendFolder(FileEntity fileEntity, string destPath, string workingPath)
        { 
            var workingFileName = workingPath.ConcatenateFilePaths(fileEntity.FileName);
            var sendFileName = destPath.ConcatenateFilePaths(fileEntity.FileName);
                _fUtil.MoveFileWithOverwrite(workingFileName, sendFileName);
        }

        public void EncryptFile(string origin_path, string dest_path, string their_public_key)
        { 
            var encrypter = get_encrypter(their_public_key); 
            encrypter.Encrypt(origin_path, dest_path);
        }



        void encrypter_OnKeyPassphrase(object Sender,  TElPGPCustomSecretKey Key, 
                                       ref string Passphrase, ref bool Cancel)
        {
            Passphrase = _passphrase;
        }


    }





}