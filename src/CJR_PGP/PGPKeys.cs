namespace CJR_PGP
{
    using SBPGPKeys;

    public class PGPKeys : IPGPKeys
    {
        private readonly IPGPKeyring _public_keyring;
        private readonly IPGPKeyring _private_keyring;
        private readonly IPGPKeyring _master_keyring;

        public PGPKeys(IPGPKeyring public_keyring, IPGPKeyring private_keyring, IPGPKeyring master_keyring)
        {
            _public_keyring = public_keyring;
            _master_keyring = master_keyring;
            _private_keyring = private_keyring;
            _master_keyring.ArmorBoundary = "";
        }

        public void LoadMasterKeyring(string pub_path, string sec_path)
        {
            _master_keyring.Load(pub_path, sec_path, true);
            var pub_key = _master_keyring.get_PublicKeys(0);
            //_master_keyring.AddPublicKey(pub_key);
            _public_keyring.AddPublicKey(pub_key);
            var sec_key = _master_keyring.get_SecretKeys(0);
            _private_keyring.AddSecretKey(sec_key) ;
            //_master_keyring.AddSecretKey(sec_key);
//            _public_keyring.AddPublicKey(GeneratePublicKey(pub_path));
//            _private_keyring.AddSecretKey(GeneratePrivateKey(sec_path));
        }

//        private IPGPSecretKey GeneratePrivateKey(string sec_path)
//        {
//            var encrypt_key = new PGPSecretKey();
//            encrypt_key.LoadFromFile(sec_path);
//            AddPrivateKey(encrypt_key);
//            return encrypt_key;
//        }

        public void AddPrivateKey(IPGPSecretKey encrypt_key)
        {
            _private_keyring.Clear();
            _private_keyring.AddSecretKey(encrypt_key);
        }

        public IPGPKeyring Master
        {
            get { return _master_keyring; }
        }

        public IPGPPublicKey GeneratePublicKey(string pubkey)
        {
            var encrypt_key = new PGPPublicKey(); 
            encrypt_key.LoadFromFile(pubkey); 
            AddPublicKey(encrypt_key);
            return encrypt_key;
        }

        public IPGPKeyring Public
        {
            get { return _public_keyring; }
        }

        public IPGPKeyring Private
        {
            get { return _private_keyring; }
        }

        public void AddPublicKey(IPGPPublicKey encrypt_key)
        {
            _public_keyring.Clear();
            _public_keyring.AddPublicKey(encrypt_key);
        }

        public void RemovePublicKey(bool remove_secret_key)
        {
            _public_keyring.RemovePublicKey(_public_keyring.get_PublicKeys(0),remove_secret_key);
        }


        public class PGPSecretKey :   IPGPSecretKey
        {
            public PGPSecretKey(TElPGPSecretKey key)
            {
                Key = key; 
            }

            public PGPSecretKey()
            {
                Key = new TElPGPSecretKey();
            }

            public TElPGPSecretKey Key { get; private set; }

            public void LoadFromFile(string sec_path)
            {
                Key.LoadFromFile(sec_path);
            }
        }
        public class PGPPublicKey :  IPGPPublicKey
        {
            public PGPPublicKey(TElPGPPublicKey key)
            {
                Key = key; 
            }
            public PGPPublicKey( )
            {
                Key = new TElPGPPublicKey();
            }

            public TElPGPPublicKey Key { get; private set; }

            public void LoadFromFile(string their_public_key)
            {
                Key.LoadFromFile(their_public_key);
            }
        }
    }
}