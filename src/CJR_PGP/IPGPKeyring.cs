namespace CJR_PGP
{
    public interface IPGPKeyring
    {
        void Load(string pub_path, string sec_path, bool clear);
        void AddPublicKey(IPGPPublicKey publicKey);
        IPGPPublicKey get_PublicKeys(int i);
        IPGPSecretKey get_SecretKeys(int i);
        void AddSecretKey(IPGPSecretKey secretKey);
        void Clear();
        void RemovePublicKey(IPGPPublicKey key, bool remove_secret_key);
        string ArmorBoundary { get; set; }
    }
}