namespace WPF
{
    public sealed class CredentialsHelper
    {
        public string Key { get; set; }
        public string Uri { get; set; }

        private static readonly CredentialsHelper instance 
            = new CredentialsHelper();

        private CredentialsHelper()
        {

        }
        
        public static CredentialsHelper Instance
        {
            get
            {
                return instance;
            }
        }
    }
}