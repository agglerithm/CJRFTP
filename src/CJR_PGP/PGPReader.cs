namespace CJR_PGP
{
    using SBPGP;
    using SBPGPKeys;
    using SBPGPStreams;

    public interface IPGPReader
    {
        event TSBPGPPassphraseEvent OnPassphrase;
        string KeyPassphrase { get; set; }
        IPGPKeyring VerifyingKeys { get; set; }
        IPGPKeyring DecryptingKeys { get; set; }
        string OutputFile { get; set; }
        void DecryptAndVerifyFile(string origin_path, string dest_path);
    }

    public class PGPReader : TElPGPReader, IPGPReader
    {
        public new IPGPKeyring VerifyingKeys
        {
            get { return base.VerifyingKeys as IPGPKeyring; }
            set { base.VerifyingKeys = value as TElPGPKeyring; }
        }

        public new IPGPKeyring DecryptingKeys
        {
            get { return base.DecryptingKeys as IPGPKeyring; }
            set { base.DecryptingKeys = value as TElPGPKeyring; }
        }

        public void DecryptAndVerifyFile(string origin_path, string dest_path)
        {
            OutputFile = dest_path;
            DecryptAndVerifyFile(origin_path);
        }
    }
}