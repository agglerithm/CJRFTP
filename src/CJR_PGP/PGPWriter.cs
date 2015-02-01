namespace CJR_PGP
{
    using System.Collections;
    using System.IO;
    using SBPGP;
    using SBPGPKeys;
    using SBPGPStreams;

    public interface IPGPWriter
    {
        IPGPKeyring EncryptingKeys { get; set; }
        TSBPGPEncryptionType EncryptionType { get; set; }
        string Filename { get; set; }
        bool Armor { get; set; }
        IPGPKeyring SigningKeys { get; set; }
        IList Passphrases { get; }
        void Encrypt(Stream source, Stream destination, long count);
        void EncryptAndSign(Stream source, Stream destination, long count);
        event TSBPGPKeyPassphraseEvent OnKeyPassphrase;
        void Encrypt(string origin_path, string dest_path);


        void EncryptAndSign(string origin_path, string dest_path);
    }
    public class PGPWriter : TElPGPWriter, IPGPWriter
    {
        public new IPGPKeyring EncryptingKeys
        {
            get { return base.EncryptingKeys as IPGPKeyring; }
            set { base.EncryptingKeys = (TElPGPKeyring) value; }
        }

        public new IPGPKeyring SigningKeys
        {
            get { return base.SigningKeys as IPGPKeyring; }
            set { base.SigningKeys = (TElPGPKeyring)value; }
        }

        public new IList Passphrases
        {
            get { return base.Passphrases; }
        }

        public void Encrypt(string origin_path, string dest_path)
        {
            EncryptionType = TSBPGPEncryptionType.etPublicKey;
            Filename = origin_path;
            var inF = new FileStream(origin_path, FileMode.Open);

            var outF = new FileStream(dest_path, FileMode.Create);
            Armor = true;
            Encrypt(inF, outF, 0);
            inF.Close();
            outF.Close();
        }

        public void EncryptAndSign(string origin_path, string dest_path)
        {
            EncryptionType = TSBPGPEncryptionType.etPublicKey;
            Filename = origin_path;
            var inF = new FileStream(origin_path, FileMode.Open);

            var outF = new FileStream(dest_path, FileMode.Create);
            Armor = true;
            EncryptAndSign(inF, outF, 0);
            inF.Close();
            outF.Close();
        }
    }
}