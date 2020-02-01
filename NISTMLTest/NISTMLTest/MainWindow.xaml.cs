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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using System.Threading;

using Microsoft.ML;
using NISTMLTestML.Model;

namespace NISTMLTest
{
    public partial class MainWindow : Window
    {
        private delegate void UpdateDelegate(float targetValue, ModelOutput result);
        private int currentSampleIndex = 0;
        private List<byte[]> testvalues;
        private MLContext mlContext;
        private string modelPath = "";
        private ITransformer mlModel;
        private PredictionEngine<ModelInput, ModelOutput> predEngine;
        private IEnumerable<ModelInput> testInputs;
        private float correctPercentage = 0;
        private List<byte[]> failedSamples;
        private List<float> failedTargets;
        private List<ModelOutput> failedResults;
        private int samplecount;

        public MainWindow()
        {
            InitializeComponent();

            // Create MLContext
            mlContext = new MLContext();
            // Load model & create prediction engine
            modelPath = AppDomain.CurrentDomain.BaseDirectory + "MLModel.zip";
            mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
            //Load the default test Data.
            LoadTestData("mnist_test.csv");
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnTestData_Click(object sender, RoutedEventArgs e)
        {
            //Use a FileOpenDialog to select the TestData
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Text Files (*.csv)|*.csv",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (openFileDialog1.ShowDialog() == true)
            {
                LoadTestData(openFileDialog1.FileName);
            }
        }

        private void LoadTestData(string testData)
        {
            TxtTestData.Text = testData;
            //Create the ModelInputs
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                            path: testData,
                            hasHeader: true,
                            separatorChar: ',',
                            allowQuoting: true,
                            allowSparse: false);
            testInputs = mlContext.Data.CreateEnumerable<ModelInput>(dataView, false);
            //Read the testData strings and convert them to a list of Byte[] for display in a Canvas
            //The values are inverted to show black letters on a white background.
            StreamReader sr = new StreamReader(testData);
            string[] stringData;
            byte[] byteData;
            char[] seperator = { ',' };
            testvalues = new List<byte[]>();
            sr.ReadLine();  //Skip the first line
            while (!sr.EndOfStream)
            {
                stringData = sr.ReadLine().Split(seperator);
                byteData = new byte[stringData.Length];
                byteData[0] = Convert.ToByte(stringData[0]);
                for (int I = 1; I < stringData.Length; I++)
                {
                    byteData[I] = Convert.ToByte(255 - Convert.ToInt32(stringData[I]));
                }
                testvalues.Add(byteData);
            }
            samplecount = testInputs.Count();
            currentSampleIndex = 0;
            TxtSampleNr.Text = "1";
            ShowImage(currentSampleIndex);
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            // Use model to make prediction on input data
            ModelInput sample = testInputs.ElementAt(currentSampleIndex);
            ModelOutput result = predEngine.Predict(sample);
            ShowResult(sample.Number, result);        
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            currentSampleIndex++;
            if (currentSampleIndex < samplecount )
            {
                TxtSampleNr.Text = (currentSampleIndex + 1).ToString();
                txtResult.Text = "";
                TxtOutput0.Text = "";
                TxtOutput1.Text = "";
                TxtOutput2.Text = "";
                TxtOutput3.Text = "";
                TxtOutput4.Text = "";
                TxtOutput5.Text = "";
                TxtOutput6.Text = "";
                TxtOutput7.Text = "";
                TxtOutput8.Text = "";
                TxtOutput9.Text = "";
                ShowImage(currentSampleIndex);
            }
        }

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateDelegate updateDel = new UpdateDelegate(ShowResult);
            ModelInput input;
            ModelOutput output;
            failedSamples = new List<byte[]>();
            failedTargets = new List<float>();
            failedResults = new List<ModelOutput>();
            float correct = 0;
            for (int I = 0; I < samplecount ; I++ )
            {
                currentSampleIndex = I;
                TxtSampleNr.Text = (I+1).ToString();
                input = testInputs.ElementAt(I);
                output = predEngine.Predict(input);
                if (output.Prediction == input.Number)
                {
                    correct++;
                }
                else
                {
                    failedSamples.Add(testvalues[I]);
                    failedTargets.Add(input.Number);
                    failedResults.Add(output);
                }
                correctPercentage  = 100 * correct / (I + 1);
                Dispatcher.Invoke(updateDel, DispatcherPriority.ApplicationIdle, input.Number, output);
            }
        }

        private void ShowResult(float targetvalue, ModelOutput result)
        {
            ShowImage(currentSampleIndex);
            txtTarget.Text = targetvalue.ToString();
            txtResult.Text = result.Prediction.ToString();
            txtPercentOK.Text = correctPercentage.ToString("F2");
            if (result.Prediction == targetvalue)
            {
                txtResult.Foreground = Brushes.Green;
            }
            else
            {
                txtResult.Foreground = Brushes.Red;
            }
            TxtOutput0.Text = result.Score[1].ToString("F4");
            TxtOutput1.Text = result.Score[3].ToString("F4");
            TxtOutput2.Text = result.Score[5].ToString("F4");
            TxtOutput3.Text = result.Score[6].ToString("F4");
            TxtOutput4.Text = result.Score[2].ToString("F4");
            TxtOutput5.Text = result.Score[0].ToString("F4");
            TxtOutput6.Text = result.Score[7].ToString("F4");
            TxtOutput7.Text = result.Score[8].ToString("F4");
            TxtOutput8.Text = result.Score[9].ToString("F4");
            TxtOutput9.Text = result.Score[4].ToString("F4");
        }

        private void ShowImage(int index)
        {
            WriteableBitmap writeBmp = new WriteableBitmap(28, 28, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);
            int stride = (writeBmp.PixelWidth * writeBmp.Format.BitsPerPixel / 8);
            Int32Rect intrect = new Int32Rect(0, 0, writeBmp.PixelWidth, writeBmp.PixelHeight);
            writeBmp.WritePixels(intrect, testvalues[index], stride, 0);
            Brush imgBrush = new ImageBrush(writeBmp);
            SampleCanvas.Background = imgBrush;
            txtTarget.Text = testvalues[index][0].ToString();
        }

        private void BtnFailed_Click(object sender, RoutedEventArgs e)
        {
            FailedSamples fs = new FailedSamples(failedSamples, failedTargets, failedResults);
            fs.Show();
        }
    }
}
