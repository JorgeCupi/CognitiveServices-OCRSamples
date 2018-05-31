using System.Net.Http;
using System.Text;
using System.Windows;

namespace WPF
{

    public partial class MainWindow : Window
    {
        CognitiveHelper credentials;
        public MainWindow()
        {
            InitializeComponent();

            LoadCredentials();
            credentials.UriBase = credentials.UriBase.Replace("%26", "&");

            btnUpload.Click += BtnUpload_Click;
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", credentials.Key);

            StringContent content = new StringContent(
                $"{{\"url\":\"{txtbImageUrl.Text}\"}}",
                Encoding.UTF8,
                "application/json");

            
            var result = await client.PostAsync(credentials.UriBase, content);
            txtbResult.Text = await result.Content.ReadAsStringAsync();
        }

        private void LoadCredentials()
        {
            credentials = CognitiveHelper.Instance;
            txtbKey.Text= credentials.Key;
            txtbUri.Text = credentials.UriBase;
        }
    }
}
