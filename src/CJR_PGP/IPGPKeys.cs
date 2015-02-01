namespace CJR_PGP
{
    using SBPGPKeys;

    public interface IPGPSecretKey
    {
        TElPGPSecretKey Key { get; }
        void LoadFromFile(string our_private_key);
    }

    public interface IPGPPublicKey
    {
        void LoadFromFile(string their_public_key);
        TElPGPPublicKey Key { get; }
    }

    public interface IPGPKeys
    {
        void LoadMasterKeyring(string pub_path, string sec_path);
        IPGPKeyring Master { get; }
        IPGPKeyring Public { get; }
        IPGPKeyring Private { get; }
        IPGPPublicKey GeneratePublicKey(string pubkey);
        void AddPublicKey(IPGPPublicKey encrypt_key);
        void RemovePublicKey(bool remove_secret_key);
        void AddPrivateKey(IPGPSecretKey encrypt_key);
    }
}