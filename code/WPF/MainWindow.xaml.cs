using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", credentials.Key);

            var content = new StringContent(
                $"{{\"url\":\"{txtbImageUrl.Text}\"}}",
                Encoding.UTF8,
                "application/json");

            
            var result = await client.PostAsync(credentials.UriBase, content);
            string httpResult = await result.Content.ReadAsStringAsync();
            var cognitiveResult = JsonConvert.DeserializeObject<CognitiveResult>(httpResult);
            string stringFromOCR = ConvertOCRToString(cognitiveResult.Regions);
            string linesFromOCR = ConvertOCRToLines(cognitiveResult.Regions);
            txtbResult.Text = linesFromOCR;
        }

        private string ConvertOCRToLines(List<Region> regions)
        {
            var words = new List<Word>();
            foreach (var region in regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        string[] parameters = new string[]{","};
                        string[] margins = word.BoundingBox.Split(parameters,StringSplitOptions.None);
                        word.Left = int.Parse(margins[0]);
                        word.Top = int.Parse(margins[1]);
                        words.Add(word);
                    }
                }
            }

            List<Word> wordsOrderedByTopMargin = OrderByTopMargin(words);
            List<Line> linesOrderedByLeftMargin = OrderByLeftMargin(wordsOrderedByTopMargin);
            string finalResult = String.Empty;
            foreach (var line in linesOrderedByLeftMargin)
            {
                foreach (var word in line.Words)
                {
                    finalResult += word.Text + " ";
                }
                finalResult += "\n";
            }

            return finalResult;
        }

        private List<Line> OrderByLeftMargin(List<Word> wordsOrderedByTopMargin)
        {
            List<Line> orderedWordsByLine = new List<Line>();
            
            string newLine = string.Empty;

            int margin = wordsOrderedByTopMargin.First().Top;
            List<Word> tempList = new List<Word>();
            foreach (var word in wordsOrderedByTopMargin)
            {
                if (word.Top-10 <= margin && margin <= word.Top+10)
                {
                    tempList.Add(word);
                }
                else
                {
                    margin = word.Top;
                    tempList = OrderWordsByLeftMargin(tempList);
                    orderedWordsByLine.Add(new Line { Words = tempList });
                    tempList = new List<Word>();
                    tempList.Add(word);
                }
            }
            return orderedWordsByLine;
        }

        private List<Word> OrderWordsByLeftMargin(List<Word> tempList)
        {
            return (from w in tempList select w).OrderBy(c => c.Left).ToList();
        }

        private List<Word> OrderByTopMargin(List<Word> words)
        {
            return (from w in words select w).OrderBy(c => c.Top).ToList();
        }

        private string ConvertOCRToString(List<Region> regions)
        {
            string finalResult = String.Empty;
            foreach (Region region in regions)
            {
                foreach (Line line in region.Lines)
                {
                    string newLine = String.Empty;
                    foreach (Word word in line.Words)
                    {
                        newLine += word.Text + " ";
                    }
                    newLine += "\n";
                    finalResult += newLine;
                }
            }
            return finalResult;
        }

        private void LoadCredentials()
        {
            credentials = CognitiveHelper.Instance;
            txtbKey.Text= credentials.Key;
            txtbUri.Text = credentials.UriBase;
        }
    }
}
