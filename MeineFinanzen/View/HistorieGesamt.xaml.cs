// 04.01.2017   -Model-   HistorieGesamt.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
namespace MeineFinanzen.View {
    public partial class HistorieGesamt : Window {
        HauptFenster _mw = null;
        string strKursdaten = Helpers.GlobalRef.g_Ein.myDepotPfad + @"KursDaten\";
        static private string[] arrFiles;
        DataTable dtTagessummen;
        const double margin = 10;
        double xmin;
        double xmax;
        double ymax;
        double maxData = 0;
        double minData = 9999999;
        public int JedenNtenTag = 50;
        public HistorieGesamt() { }
        public HistorieGesamt(HauptFenster mw) {
            InitializeComponent();
            _mw = mw;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            dtTagessummen = new DataTable("tblTagessummen");
            HoleDaten();
            MachDenGraph();
        }
        public void MachDenGraph() {
            graphArea.Children.Clear();            
            double d = 0;
            foreach (DataRow dr in dtTagessummen.Rows) {
                d = Convert.ToDouble(dr["Tageswert"]);
                if (d > 0) {
                    if (maxData < d)
                        maxData = d;        // den Größten Tageswert
                    if (minData > d)
                        minData = d;        // den Kleinsten Tageswert                    
                }
            }
            xmin = margin;
            double x = xmin + 46;
            xmax = graphArea.Width - margin;
            ymax = graphArea.Height - margin;
            double y = ymax - margin - 50;
            PointCollection points = new PointCollection();
            double max6tel = (maxData - minData) / 9;
            double min6 = minData;
            double min = y / 10;
            double proz = 0;
            for (int i = 0; i < 10; i++) {
                AddMarkerTextToChart(y - 10, min6, proz);   // Setze links senkrecht 10 Tageswerte + % + Linien
                AddLineToChart(x, y - 10, xmax);            // Und die Linien
                min6 += max6tel;
                y -= min;
            }
            double twert = 0;
            double minwert = 0;
            int ntest = 0;
            int nTage = 0;
            string strdt;
            foreach (DataRow dr in dtTagessummen.Rows) {
                nTage++;
                ntest++;
                if (ntest != JedenNtenTag)
                    continue;
                ntest = 0;
                strdt = Convert.ToDateTime(dr["Datum"]).ToString("yyyy.MM.dd");
                DrawXAxisLabel(strdt, x);
                twert = (int)Convert.ToInt32(dr["Tageswert"]);
                minwert = twert - minData;
                y = ymax - (minwert * (ymax - margin) / (maxData - minData));
                y = Convert.ToInt32(y);
                points.Add(new Point(x, y));                        // Die Verbindungslinien in Punkten
                Line linie = new Line();
                linie.X1 = x;
                linie.Y1 = y;
                linie.X2 = linie.X1 + 4;
                linie.Y2 = linie.Y1 + 4;
                linie.Stroke = new SolidColorBrush(Colors.Red);     // Die Punkte
                linie.StrokeThickness = 6;
                graphArea.Children.Add(linie);
                //Console.WriteLine("{0,5} {1,7} X1:{2,5} Y1:{3,5} {4}", nTage, Convert.ToInt32(dr["Tageswert"]), linie.X1, linie.Y1, strdt);                          
                x += 30;
            }
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 2;
            polyline.Stroke = Brushes.Black;
            polyline.Points = points;
            graphArea.Children.Add(polyline);                       // Die Verbindungslinien            
            setText(80.00, 8.00, "Depotwerte", Colors.Black);
        }
        private void AddMarkerTextToChart(double top, double markerTextValue, double proz) {
            TextBlock markText = new TextBlock();
            markText.Text = markerTextValue.ToString("###,##0");
            markText.Width = 40;
            markText.FontSize = 10;
            markText.HorizontalAlignment = HorizontalAlignment.Right;
            markText.TextAlignment = TextAlignment.Right;
            //markText.Background = new SolidColorBrush(Colors.Linen);
            graphArea.Children.Add(markText);
            Canvas.SetTop(markText, top - 10);                      // adjust y location
            Canvas.SetLeft(markText, 10);
        }
        private void DrawXAxisLabel(string strdt, double x) {
            TextBlock markText = new TextBlock();
            markText.Text = strdt;
            markText.FontFamily = new FontFamily("Century Gothic");
            markText.FontSize = 12;
            //markText.Background = new SolidColorBrush(Colors.Red);
            markText.FontWeight = FontWeights.Bold;
            Transform st = new SkewTransform(0, 20);
            markText.RenderTransform = st;
            Canvas.SetTop(markText, ymax);
            Canvas.SetLeft(markText, x);
            graphArea.Children.Add(markText);
        }
        private void setText(double x, double y, string text, Color color) {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            textBlock.FontSize = 10;
            //textBlock.Background = new SolidColorBrush(Colors.LightYellow);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            graphArea.Children.Add(textBlock);
        }
        private Line AddLineToChart(double x, double y, double xrechts) {
            Line linie = new Line();
            linie.X1 = x;
            linie.Y1 = y;
            linie.X2 = xrechts;
            linie.Y2 = y;
            linie.Stroke = new SolidColorBrush(Colors.Black);
            //linie.Fill = new SolidColorBrush(Colors.Yellow);
            linie.StrokeThickness = 0.9;
            graphArea.Children.Add(linie);
            return linie;
        }
        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (textBoxSlider == null)
                return;
            var slider = sender as Slider;
            textBoxSlider.Text = "zoomSlider: " + slider.Value.ToString("0.0") + ", max: " + slider.Maximum;
        }
        public void HoleDaten() {
            dtTagessummen.Columns.Add("Datum", typeof(string));
            dtTagessummen.Columns.Add("Tageswert", typeof(double));
            DataColumn[] keys6 = new DataColumn[1];
            keys6[0] = dtTagessummen.Columns["Datum"];
            dtTagessummen.PrimaryKey = keys6;
            arrFiles = new string[9999];
            string searchPattern = "*";
            string[] files = HoleFiles(strKursdaten, searchPattern, SearchOption.TopDirectoryOnly);
            foreach (string file in files) {
                FileInfo fsi = new FileInfo(file);
                if (fsi.Name.Substring(0, 8) == "PortFol_")     // if (fsi.Name.Contains(".xml"))                    
                    AddToDatatable(file, fsi.Name);
            }
        }
        public static string[] HoleFiles(string path, string searchPattern, SearchOption searchOption) {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        private void AddToDatatable(string pfad, string fname) {
            bool gefunden;
            string strDat;
            if (fname == null)
                return;
            Model.CollWertpapiere wp;
            if (File.Exists(pfad)) {
                if (pfad.EndsWith("txt")) {
                    DateTime dt = File.GetLastWriteTime(pfad);
                    using (StreamReader sr = new StreamReader(pfad)) {
                        string line;
                        string[] strArr;
                        string st7;
                        while ((line = sr.ReadLine()) != null) {
                            if (line.Contains("Summe Gesamt")) {
                                // / Summe Gesamt///316.863,18*/307.046,26*/9.816,92*/3,20*////////
                                strArr = line.Split('/');
                                double dbl1 = 0.00;
                                double dbl2 = 0.00;
                                foreach (string st in strArr) {
                                    if (st.Length < 7 || st.Length > 12)
                                        continue;
                                    if (st.Contains(",") && st.Contains(".")) {
                                        //316.863,18 *
                                        st7 = st;
                                        if (st.Contains("*")) {
                                            st7 = st.Remove(st.IndexOf("*"));
                                        }
                                        dbl1 = Convert.ToDouble(st7);
                                        if (dbl1 > dbl2)
                                            dbl2 = dbl1;
                                    }
                                }
                                //Console.WriteLine("{0,-16} {1,16}", dt, dbl2);
                                dtTagessummen.Rows.Add(dt, dbl2);
                            }
                        }
                    }
                }
                else {
                    XmlSerializer ser = new XmlSerializer(typeof(Model.CollWertpapiere));
                    try {
                        using (Stream rd = new FileStream(pfad, FileMode.Open)) {
                            wp = (Model.CollWertpapiere)ser.Deserialize(rd);
                        }
                        if (wp.Count > 0) {
                            gefunden = false;
                            foreach (Model.Wertpapier f1 in wp) {
                                if (f1.Name == "Summe Gesamt") {
                                    gefunden = true;
                                    string sAktWert = String.Format("{0:###,##0.00 ;#0.00-;' '}", f1.AktWert);
                                    double dbl = Convert.ToDouble(sAktWert);
                                    strDat = fname.Substring(14, 2) + "." + fname.Substring(12, 2) + "." + fname.Substring(8, 4);
                                    //Console.WriteLine("{0,16} {1,16}", strDat, sAktWert);
                                    dtTagessummen.Rows.Add(strDat, dbl);
                                    break;
                                }
                            }
                            if (!gefunden)
                                MessageBox.Show("HistorieGesamt() Summe Gesamt nicht gefunden: " + fname);
                        }
                    }
                    catch (Exception) {
                        //MessageBox.Show("HistorieGesamt Exception. pfad: " + pfad + Environment.NewLine + ex);
                    }
                }
            }
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        private void OnKeyDownHandler4(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                MessageBox.Show("4) Enter " + txt4.Text);
            }
        }
        private void OnKeyDownHandler3(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                MessageBox.Show("3) Enter " + txt3.Text);
            }
        }
        private void OnKeyDownHandler2(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                MessageBox.Show("2) Enter " + txt2.Text);
            }
        }
        private void OnKeyDownHandler1(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                //MessageBox.Show("1) Enter " + txt1.Text);
                JedenNtenTag = Convert.ToInt32(txt1.Text);
                MachDenGraph();
            }
        }
    }
}