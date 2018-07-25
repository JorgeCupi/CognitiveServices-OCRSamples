using System;
using System.Collections.Generic;
using System.Linq;

namespace WPF
{
    static class ConversionHelper
    {
        internal static string GetResultOrderedByRegions(List<Region> regions)
        {   
            int regionCount = 1;
            string finalResult = String.Empty;
            foreach (var region in regions)
            {
                finalResult += $"----------\nNew region {regionCount}\n----------\n";
                finalResult += ConstructStringFromLines(region.Lines);
                regionCount++;
            }

            return finalResult;
        }

        internal static string GetResultOrderedByLines(List<Region> regions)
        {
            var words = SetLimitsInWords(regions);
            var linesByLeftLimit = GetLinesByLeftLimit(words);
            return ConstructStringFromLines(linesByLeftLimit);
        }

        static string ConstructStringFromLines(List<Line> lines)
        {
            string finalResult = string.Empty;
            foreach (var line in lines)
            {
                foreach (var word in line.Words)
                    finalResult += word.Text + " ";
                finalResult += "\n";
            }
            return finalResult;
        }

        private static List<Word> SetLimitsInWords(List<Region> regions)
        {
            var words = new List<Word>();
            string[] separators = new string[] { "," };
            
            foreach (var region in regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        string[] limits = word.BoundingBox.Split(separators,
                            StringSplitOptions.None);
                        word.Left = int.Parse(limits[0]);
                        word.Top = int.Parse(limits[1]);
                        words.Add(word);
                    }
                }
            }
            return words;
        }

        static List<Line> GetLinesByLeftLimit(List<Word> words)
        {
            var linesWithOrderedWords = new List<Line>();
            var wordsByLeftLimit = new List<Word>();
            words = words.OrderBy(c => c.Top).ToList(); ;
            int currentPixel = words.First().Top;
            
            foreach (var word in words)
            {
                bool wordIsWithinBottomLimit = (word.Top - 10 <= currentPixel);
                bool wordIsWithinTopLimit = (currentPixel <= word.Top + 10);
                if (!wordIsWithinBottomLimit || !wordIsWithinTopLimit)
                {
                    currentPixel = word.Top;

                    wordsByLeftLimit =
                        wordsByLeftLimit.OrderBy(c => c.Left).ToList();

                    linesWithOrderedWords.Add(
                        new Line { Words = wordsByLeftLimit });

                    wordsByLeftLimit = new List<Word>();
                }
                wordsByLeftLimit.Add(word);
            }
            wordsByLeftLimit = wordsByLeftLimit.OrderBy(c => c.Left).ToList();
            linesWithOrderedWords.Add(new Line { Words = wordsByLeftLimit });

            return linesWithOrderedWords;
        }
    }
}