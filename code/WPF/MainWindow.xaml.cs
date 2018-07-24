using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
        }

        private void DrawRegionsOverImage(List<Region> regions)
        {
            string[] separators = new string[] { "," };
            foreach (var region in regions)
            {
                string[] values = region.BoundingBox.Split(separators,
                            StringSplitOptions.None);

                int left = int.Parse(values[0]);
                int top = int.Parse(values[1]);
                int width = int.Parse(values[2]);
                int height = int.Parse(values[3]);

                Rectangle rect = new Rectangle();
                rect.Width = width;
                rect.Height = height;
                rect.Stroke = new SolidColorBrush(Colors.Black);

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, top);

                canvas.Children.Add(rect);
            }
        }

        private async void txtbImageUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                while (canvas.Children.Count > 1)
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", credentials.Key);

                var content = new StringContent(
                    $"{{\"url\":\"{txtbImageUrl.Text}\"}}",
                    Encoding.UTF8, "application/json");

                var httpResult = await client.PostAsync(credentials.Uri, content);
                string result = await httpResult.Content.ReadAsStringAsync();
                var ocrResult = JsonConvert.DeserializeObject<OCRResult>(result);


                string rawResult = ConversionHelper.GetResultOrderedByRegions(ocrResult.Regions);
                string linesFromOCR =
                    ConversionHelper.GetResultOrderedByLines(ocrResult.Regions);

                DrawRegionsOverImage(ocrResult.Regions);
                txtbRawResult.Text = rawResult;
                txtbResult.Text = linesFromOCR;
            }
            catch(Exception ex)
            {
                txtbRawResult.Text = ex.ToString();
            }
        }

        private async void btnUpload(object sender, RoutedEventArgs e)
        {
            try
            {
                while (canvas.Children.Count > 1)
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", credentials.Key);

                FileStream fileStream = File.OpenRead(txtUpload.Text);
                var content = new StreamContent(fileStream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                

                var httpResult = await client.PostAsync(credentials.Uri, content);
                string result = await httpResult.Content.ReadAsStringAsync();
                var ocrResult = JsonConvert.DeserializeObject<OCRResult>(result);


                string rawResult = ConversionHelper.GetResultOrderedByRegions(ocrResult.Regions);
                string linesFromOCR =
                    ConversionHelper.GetResultOrderedByLines(ocrResult.Regions);

                DrawRegionsOverImage(ocrResult.Regions);
                txtbRawResult.Text = rawResult;
                txtbResult.Text = linesFromOCR;
            }

            catch (Exception ex)
            {
                txtbRawResult.Text = ex.ToString();
            }
        }
    }
}