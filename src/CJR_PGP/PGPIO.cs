namespace CJR_PGP
{
    using SBPGP;
    using SBPGPStreams;

    public interface IPGPIO
    {
        IPGPWriter GetEncrypter(IPGPKeyring  keys);
        IPGPReader GetDecrypter();
        IPGPReader SetDecrypter(string passphrase, IPGPKeyring master, string dest_path,
            TSBPGPPassphraseEvent onPassphrase);

        IPGPWriter SetEncrypter(IPGPKeyring their_public, 
            IPGPKeyring our_private, string passphrase, 
            TSBPGPKeyPassphraseEvent keyPassphrase);
    }

    public class PGPIO : IPGPIO
    {
        private readonly IPGPReader _decrypter;
        private readonly IPGPWriter _encrypter;

        public PGPIO(IPGPReader decrypter, IPGPWriter encrypter)
        {
            _decrypter = decrypter;
            _encrypter = encrypter;
        }

        public IPGPWriter GetEncrypter(IPGPKeyring keys)
        {
            _encrypter.EncryptingKeys = keys; 
            return _encrypter;
        }

        public IPGPReader GetDecrypter()
        {
            return _decrypter;
        }

        public IPGPReader SetDecrypter(string passphrase, IPGPKeyring master,
            string dest_path, TSBPGPPassphraseEvent onPassphrase)
        {
            _decrypter.KeyPassphrase = passphrase;
            _decrypter.VerifyingKeys = master;
            _decrypter.DecryptingKeys = master;
            _decrypter.OnPassphrase +=  onPassphrase;
            _decrypter.OutputFile = dest_path;
            return GetDecrypter();
        }

        public IPGPWriter SetEncrypter(IPGPKeyring their_public, IPGPKeyring our_private,
            string passphrase, TSBPGPKeyPassphraseEvent keyPassphrase)
        {
            _encrypter.OnKeyPassphrase +=   keyPassphrase;
            _encrypter.EncryptingKeys = their_public;
            _encrypter.SigningKeys = our_private;
            _encrypter.Passphrases.Clear();
            _encrypter.Passphrases.Add(passphrase);
            _encrypter.EncryptionType = TSBPGPEncryptionType.etPassphrase;
            return _encrypter;
        }
    }
}