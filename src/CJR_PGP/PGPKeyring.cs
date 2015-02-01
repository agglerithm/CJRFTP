namespace CJR_PGP
{
    using SBPGPKeys;

    public class PGPKeyring   :TElPGPKeyring,IPGPKeyring
    {
        public void AddPublicKey(IPGPPublicKey publicKey)
        {
            AddPublicKey(publicKey.Key);
        }

        public new IPGPPublicKey get_PublicKeys(int ndx)
        {
            var key = base.get_PublicKeys(ndx);
            return new PGPKeys.PGPPublicKey (key);
        }

        public new IPGPSecretKey get_SecretKeys(int ndx)
        {
            var key = base.get_SecretKeys(ndx);
            return new PGPKeys.PGPSecretKey (key);
        }

        public void AddSecretKey(IPGPSecretKey secretKey)
        {
            AddSecretKey(secretKey.Key);
        }

        public void RemovePublicKey(IPGPPublicKey key, bool remove_secret_key)
        {
            RemovePublicKey(key.Key, remove_secret_key);
        }
    }
}