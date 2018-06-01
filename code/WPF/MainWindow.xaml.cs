using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace WPF
{
    public partial class MainWindow : Window
    {
        CredentialsHelper credentials;
        public MainWindow()
        {
            InitializeComponent();
            SetCredentials();
        }

        void SetCredentials()
        {
            credentials = CredentialsHelper.Instance;
            credentials.Uri = credentials.Uri.Replace("%26", "&");
            txtbKey.Text = credentials.Key;
            txtbUri.Text = credentials.Uri;
        }

        async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", credentials.Key);

            var content = new StringContent(
                $"{{\"url\":\"{txtbImageUrl.Text}\"}}", 
                Encoding.UTF8, "application/json");

            var httpResult = await client.PostAsync(credentials.Uri, content);
            string result = await httpResult.Content.ReadAsStringAsync();
            var ocrResult = JsonConvert.DeserializeObject<OCRResult>(result);

            string linesFromOCR = 
                ConversionHelper.GetResultOrderedByLines(ocrResult.Regions);
            txtbResult.Text = linesFromOCR;
        }
    }
}