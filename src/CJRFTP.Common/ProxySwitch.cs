using Microsoft.Win32;

namespace AFPFTP.Common
{
    public class ProxySwitch
    {
        public static void On()
        { 
            var key = get_key(); 
            //key.SetValue("ProxyEnable", 1);
        }

        public static void Off()
        {
            var key = get_key();
            key.SetValue("ProxyEnable", 0);  
        }

        private static RegistryKey get_key()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
        }
    }
}