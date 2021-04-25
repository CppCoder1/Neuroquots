using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
using Microsoft.Data.Analysis;
using ner;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Reflection.Emit;

namespace NeuroN
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            }

        static void ShiftLeft(double[] Arr)
        {
            double temp = Arr[0];
            for (int i = 0; i < Arr.Length - 1; i++)
                Arr[i] = Arr[i + 1];
            Arr[Arr.Length - 1] = temp;
        }

        private void WinMove(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public class AVConnection
        {
            private readonly string _apiKey;

            public AVConnection(string apiKey)
            {
                this._apiKey = apiKey;
            }

            public void SaveCSVFromURL(string symbol)
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://" + $@"www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={this._apiKey}&datatype=csv");

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                StreamReader sr = new StreamReader(resp.GetResponseStream());
                string results = sr.ReadToEnd();
                sr.Close();
                File.WriteAllText("stockdata.csv", results);
            }
        }

        private void SearchQ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SearchQ.Text != "")
            {

                Greeting1.Visibility = Visibility.Hidden;
                Greeting2.Visibility = Visibility.Hidden;
                SearchQ.Visibility = Visibility.Hidden;
                QuotName.Visibility = Visibility.Visible;
                QuotName.Text ="Результат по тикеру " + SearchQ.Text;
                Graphics.Visibility = Visibility.Visible;
                AVConnection conn = new AVConnection("31LKAVO0B8NLOPVH");
                conn.SaveCSVFromURL(SearchQ.Text);
                DataFrame df = DataFrame.LoadCsv("stockdata.csv");
                var Values = new ChartValues<double>();
                var values = new List<double>();
                var Points = new List<double>();
                var results = new ChartValues<double>();
                for (int i = 0; i < 13; i++)
                {
                    Values.Add(double.Parse(df[i, 3].ToString().Replace(".", ",")));
                }
                
                for (int i = 0; i < 12; i++)
                {
                    values.Add(Values[i] - Values[i + 1]);
                }
                Points.Add(values[11]);
                
                for (int k = 0; k < 2; k++)
                {
                    var inputs = new double[1, 11];
                    
                    for (int i = 0; i < 11; i++)
                    {
                        inputs[0, i] = values[i];
                    }
                    var Inputs = new double[inputs.Length];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        Inputs[i] = inputs[0, i];
                    }
                    var output = new double[1];
                    output[0] = values[11];
                    for (int i = 0; i < inputs.Length; i++) inputs[0, i] *= 10;
                    var topology = new Topology(11, 1, 0.1, 14, 6);
                    var net = new NeuronNet(topology);
                    net.Learn(output, inputs, 1000);
                    for (int i = 0; i < inputs.Length; i++) inputs[0, i] /= 10;
                    
                    for (int i = 0; i < output.Length; i++)
                    {
                        var row = NeuronNet.GetRow(inputs, i);
                        var res = net.FeedForward(row).Output;
                        results.Add(res / 10);
                    }

                    ShiftLeft(Inputs);
                    Inputs[Inputs.Length - 1] = results[results.Count - 1];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        inputs[0, i] = Inputs[i];
                    }
                    Points.Add(results[results.Count - 1]);
                }

                var AllResults = new ChartValues<double>();
                for(int i = 0; i < Values.Count; i++)
                {
                    AllResults.Add(Values[i]);
                }
                
                    AllResults.Add(Values[Values.Count - 1] +results[0]);
                    AllResults.Add(Values[Values.Count - 1] +results[0] + results[1]);

                SeriesCollection = new SeriesCollection();
                var line = new LineSeries();
                line.Values = AllResults;
                line.Title = "Произошедшие изменения";
                SeriesCollection.Add(line);
                Graphics.Series = SeriesCollection;
                var Dates = new string[15];
                for(int i = 0; i < 13;i++)
                {
                    Dates[i] = DateTime.Now.AddDays(i-12).Day.ToString() + "." + DateTime.Now.AddDays(i - 12).Month.ToString();
                }
                for (int i = 13; i < 15; i++)
                {
                    Dates[i] = DateTime.Now.AddDays(i-11).Day.ToString() + "." + DateTime.Now.AddDays(i-11).Month.ToString();
                }
                Labels = Dates;

                DataContext = this;
            }
        }

        

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter&& Search.Text !="")
            {
                Greeting1.Visibility = Visibility.Hidden;
                Greeting2.Visibility = Visibility.Hidden;
                QuotName.Text = "";
                QuotName.Visibility =Visibility.Visible;
                QuotName.Text = "Результаты по тикеру " + Search.Text;
                Graphics.Series.Clear();
                
                AVConnection conn = new AVConnection("31LKAVO0B8NLOPVH");
                conn.SaveCSVFromURL(Search.Text);
                DataFrame df = DataFrame.LoadCsv("stockdata.csv");
                var Values = new ChartValues<double>();
                var values = new List<double>();
                var Points = new List<double>();
                var results = new ChartValues<double>();
                for (int i = 0; i < 13; i++)
                {
                    Values.Add(double.Parse(df[i, 3].ToString().Replace(".", ",")));
                }

                for (int i = 0; i < 12; i++)
                {
                    values.Add(Values[i] - Values[i + 1]);
                }
                Points.Add(values[11]);

                for (int k = 0; k < 2; k++)
                {
                    var inputs = new double[1, 11];

                    for (int i = 0; i < 11; i++)
                    {
                        inputs[0, i] = values[i];
                    }
                    var Inputs = new double[inputs.Length];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        Inputs[i] = inputs[0, i];
                    }
                    var output = new double[1];
                    output[0] = values[11];
                    for (int i = 0; i < inputs.Length; i++) inputs[0, i] *= 10;
                    var topology = new Topology(11, 1, 0.1, 14, 6);
                    var net = new NeuronNet(topology);
                    net.Learn(output, inputs, 1000);
                    for (int i = 0; i < inputs.Length; i++) inputs[0, i] /= 10;

                    for (int i = 0; i < output.Length; i++)
                    {
                        var row = NeuronNet.GetRow(inputs, i);
                        var res = net.FeedForward(row).Output;
                        results.Add(res / 10);
                    }

                    ShiftLeft(Inputs);
                    Inputs[Inputs.Length - 1] = results[results.Count - 1];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        inputs[0, i] = Inputs[i];
                    }
                    Points.Add(results[results.Count - 1]);
                }

                var AllResults = new ChartValues<double>();
                for (int i = 0; i < Values.Count; i++)
                {
                    AllResults.Add(Values[i]);
                }
                for (int i = 0; i < results.Count; i++)
                {
                    AllResults.Add(Values[Values.Count - 1] + results[i]);
                }

                SeriesCollection = new SeriesCollection();
                var line = new LineSeries();
                line.Values = AllResults;
                line.Title = "Произошедшие изменения";
                SeriesCollection.Add(line);
                Graphics.Series = SeriesCollection;
                var Dates = new string[15];
                for (int i = 0; i < 13; i++)
                {
                    Dates[i] = DateTime.Now.AddDays(i - 12).Day.ToString() + "." + DateTime.Now.AddDays(i - 12).Month.ToString();
                }
                for (int i = 13; i < 15; i++)
                {
                    Dates[i] = DateTime.Now.AddDays(i - 11).Day.ToString() + "." + DateTime.Now.AddDays(i - 11).Month.ToString();
                }
                Labels = Dates;

                DataContext = this;
            }
        }

        private void SearchQ_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://ru.investing.com/equities/");
        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
    }
}
