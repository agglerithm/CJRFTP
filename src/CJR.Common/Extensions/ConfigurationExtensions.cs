namespace CJR.Common.Extensions
{
    public static class ConfigurationExtensions
    {
        public static bool KeepWindowOpen(this CJRConfiguration config)
        {
            return config.GetSettingBasedOnTestMode("KeepWindowOpen") != "0";
        }

        public static string Inbox(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "Inbox");
        }

        public static string Outbox(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "Outbox");
        }

        public static string TransferFolder(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "Transfer");
        }

        public static string Host(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "URL");
        }
        public static string Password(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "Password");
        }
        public static string User(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "User");
        }
        public static string LocalDownloadPath(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "LocalDownloadPath");
        }
        public static string LocalUploadPath(this CJRConfiguration config, string prefix)
        {
            return config.GetSettingBasedOnTestMode(prefix + "LocalUploadPath");
        }
    }
}
