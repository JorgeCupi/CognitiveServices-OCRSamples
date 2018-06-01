using System.Configuration;
using System.Windows;

namespace WPF
{
    public partial class App : Application
    {
        public App()
        {
            ReadCredentials();
        }

        private void ReadCredentials()
        {
            var credentialsHelper = CredentialsHelper.Instance;
            credentialsHelper.Key = 
                ConfigurationManager.AppSettings["ComputerVisionKey"];
            credentialsHelper.Uri = 
                ConfigurationManager.AppSettings["ComputerVisionUri"];
        }
    }
}