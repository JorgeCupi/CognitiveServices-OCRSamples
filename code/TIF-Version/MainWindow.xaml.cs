using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

		/// <summary>
		/// Toma el archivo Tiff seleccionado, convierte a 1 o múltiples png e invoca al OCR
		/// Pega el texto final en los controles del formulario
		/// (Esto último habría que llevarlo a un file directamente).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private async void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            string filePath = OpenTifFile();
            txtbImageUrl.Text = filePath;
			List<string> pngFiles = ConvertTifToPng(filePath);

			try
            {
				for (int p = 0; p < pngFiles.Count; p++)
				{
					OCRResult ocrResult = await GetOcrResult(pngFiles[p]);
					if (ocrResult.Regions.Count > 0)
					{
						string rawResult = ConversionHelper.GetResultOrderedByRegions(ocrResult.Regions);
						string linesFromOCR = ConversionHelper.GetResultOrderedByLines(ocrResult.Regions);

						DrawRegionsOverImage(ocrResult.Regions); 
						txtbRawResult.Text += rawResult;
						txtbResult.Text += linesFromOCR;
					}
				}
                WriteToTxtFile(txtbResult.Text, txtbImageUrl.Text);
            }

            catch (Exception ex)
            {
                txtbRawResult.Text = ex.ToString();
                mainGrid.Visibility = Visibility.Visible;
                messageGrid.Visibility = Visibility.Hidden;
            }
        }

        private async Task<OCRResult> GetOcrResult(string pngFile)
        {
            while (grid.Children.Count > 1)
                grid.Children.RemoveAt(grid.Children.Count - 1);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", credentials.Key);

            var fileStream = File.OpenRead(pngFile);
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var httpResult = await client.PostAsync(credentials.Uri, content);
            string result = await httpResult.Content.ReadAsStringAsync();
            var ocrResult = JsonConvert.DeserializeObject<OCRResult>(result);
            return ocrResult;
        }
        private void WriteToTxtFile(string text, string fileName)
        {
            string filePath = fileName.Replace(".tif", "Azure.txt");
            File.WriteAllText(filePath, text);
        }

        private string OpenTifFile()
        {
            mainGrid.Visibility = Visibility.Hidden;
            messageGrid.Visibility = Visibility.Visible;

            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var fileDialogResult = fileDialog.ShowDialog();

            switch (fileDialogResult)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    return file;
                default:
                    return "No se selecciono ningun archivo";
            }
        }

        private List<string> ConvertTifToPng(string filePath)
        {
            var converter = new TiffToPngConverter();

            List<string> pngFiles = converter.ConvertTiffToPngFiles(filePath);
            txtbImageUrl.Text = pngFiles[0];

            mainGrid.Visibility = Visibility.Visible;
            messageGrid.Visibility = Visibility.Hidden;

			return pngFiles;
		}

        private void DrawRegionsOverImage(List<Region> regions)
        {
            int regionCounter = 1;
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
                rect.Stroke = new SolidColorBrush(Colors.Red);

                TextBlock textRegion = new TextBlock();
                textRegion.Margin = new Thickness(left, top, 0, 0);
                textRegion.VerticalAlignment = VerticalAlignment.Top;
                textRegion.HorizontalAlignment = HorizontalAlignment.Left;
                textRegion.Text = regionCounter.ToString();
                textRegion.FontSize = 16;
                textRegion.Foreground = new SolidColorBrush(Colors.Red);

                rect.HorizontalAlignment = HorizontalAlignment.Left;
                rect.VerticalAlignment = VerticalAlignment.Top;
                rect.Margin = new Thickness(left, top, 0, 0);

                grid.Children.Add(textRegion);
                grid.Children.Add(rect);
                regionCounter++;
            }
        }
    }
}