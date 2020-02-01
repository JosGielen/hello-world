using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NISTMLTestML.Model;

namespace NISTMLTest
{
    /// <summary>
    /// Interaction logic for FailedSamples.xaml
    /// </summary>
    public partial class FailedSamples : Window
    {
        private List<byte[]> failedSamples;
        private List<float> failedTargets;
        private List<ModelOutput> failedResults;
        private int sampleNr;

        public FailedSamples( List<byte[]> samples, List<float> targets, List<ModelOutput> results)
        {
            InitializeComponent();

            failedSamples = samples;
            failedTargets = targets;
            failedResults = results;
            sampleNr = 0;
            if (samples != null && sampleNr < failedSamples.Count)
            {
                ShowTest(sampleNr);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            sampleNr++;
            if (failedSamples != null && sampleNr < failedSamples.Count)
            {
                ShowTest(sampleNr);
            }
        }

        private void ShowTest(int index)
        {
            WriteableBitmap writeBmp = new WriteableBitmap(28, 28, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);
            int stride = (writeBmp.PixelWidth * writeBmp.Format.BitsPerPixel / 8);
            Int32Rect intrect = new Int32Rect(0, 0, writeBmp.PixelWidth, writeBmp.PixelHeight);
            writeBmp.WritePixels(intrect, failedSamples[index], stride, 0);
            Brush imgBrush = new ImageBrush(writeBmp);
            SampleCanvas.Background = imgBrush;
            TxtSampleNr.Text = index.ToString();
            txtTarget.Text = failedTargets[index].ToString();
            txtResult.Text = failedResults[index].Prediction.ToString();
            TxtOutput0.Text = failedResults[index].Score[1].ToString("F4");
            TxtOutput1.Text = failedResults[index].Score[3].ToString("F4");
            TxtOutput2.Text = failedResults[index].Score[5].ToString("F4");
            TxtOutput3.Text = failedResults[index].Score[6].ToString("F4");
            TxtOutput4.Text = failedResults[index].Score[2].ToString("F4");
            TxtOutput5.Text = failedResults[index].Score[0].ToString("F4");
            TxtOutput6.Text = failedResults[index].Score[7].ToString("F4");
            TxtOutput7.Text = failedResults[index].Score[8].ToString("F4");
            TxtOutput8.Text = failedResults[index].Score[9].ToString("F4");
            TxtOutput9.Text = failedResults[index].Score[4].ToString("F4");
        }
    }
}
