// 30.01.2017  -View-   GraphChart.xaml.cs 
// Es fehlen Wochen!!!! 
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
namespace MeineFinanzen.View {
    public partial class GraphChart : UserControl {
        const double margin = 10;
        double xmin;
        double xmax;
        double ymin;
        double ymax;        
        double maxData = 0;
        double minData = 9999999;
        int abWoche = 0;
        public GraphChart() {
            InitializeComponent();
        }
        // Draw a simple graph. Creates the chart based on the ChartDataTable.        
        internal void GeneriereGraph(DataTable dtWoche) {
            ConWrLi("---- -40- GeneriereGraph()");
            if (dtWoche.Rows.Count > 34)
                abWoche = dtWoche.Rows.Count - 34;            
            double d = 0;
            int nWochen = 0;
            foreach (DataRow dr in dtWoche.Rows) {
                if (Convert.ToInt32(dr["Woche"]) >= abWoche) {
                    nWochen++;
                    d = Convert.ToDouble(dr["Tageswert"]);
                    if (d > 0) {
                        if (maxData < d)
                            maxData = d;        // den Größten Tageswert
                        if (minData > d)
                            minData = d;        // den Kleinsten Tageswert                    
                    }
                }
            }
            xmin = margin;
            double x = xmin + 46;
            xmax = graphArea.Width - margin;
            ymin = margin;
            ymax = 200 - margin;
            double last_y = 0;
            double y = ymax - margin - 50;        
            PointCollection points = new PointCollection();
            double max6tel = (maxData - minData) / 5;
            double min6 = minData;
            double min = ((y - 0) / 6);                       
            //double xrechts = x + nWochen * ;
            //Debug.WriteLine("GraphChart.GeneriereGraph() Wochen:{0} abWoche:{1} minData:{2} maxData:{3}",
            //dtWoche.Rows.Count, abWoche, minData.ToString("###,##0"), maxData.ToString("###,##0"));
            for (int i = 0; i < 6; i++) {
                //Debug.WriteLine("AddMarkerTextToChart ({0}, {1}) {2}", (y - 10).ToString("###0"), min6.ToString("###,##0"), i);
                AddMarkerTextToChart(y - 10, min6);     // Setze links senkrecht 6 Tageswerte   782.184  bis 864.372        
                AddLineToChart(x, y - 10, xmax - 40);     // Und die Linien
                min6 += max6tel;
                y -= min;
            }           
            double wert = 0;
            foreach (DataRow dr in dtWoche.Rows) {
                if (Convert.ToInt32(dr["Woche"]) >= abWoche) {
                    DrawXAxisLabel(dr, x);
                    try {
                        wert = (int)Convert.ToInt32(dr["Tageswert"]);
                    }
                    catch (Exception ex) {
                        MessageBox.Show("wert: " + ex);
                        wert = 0;
                    }
                    wert -= minData;
                    double yyy = 100 - (wert * 100) / (maxData - minData) * 0.8;
                    last_y = yyy;
                    if (last_y < ymin)
                        last_y = (int)ymin;
                    if (last_y > ymax)
                        last_y = (int)ymax;
                    points.Add(new Point(x, last_y));                   // Die Verbindungslinien in Punkten
                    Line linie = new Line();
                    linie.X1 = x;
                    linie.Y1 = last_y - 2;
                    linie.X2 = linie.X1 + 4;
                    linie.Y2 = linie.Y1 + 4;
                    linie.Stroke = new SolidColorBrush(Colors.Blue);     // Die Punkte
                    linie.StrokeThickness = 6;
                    graphArea.Children.Add(linie);                   
                    //scale = (((float)yMarkerValue * 100 / maxData)) * (canGraph.Height - 100 - 10) / 100;
                    //Debug.WriteLine("Woche:{0} Tageswert:{1} x:{2} last_y:{3}", Convert.ToInt32(dr["Woche"]), 
                        //Convert.ToDouble(dr["Tageswert"]).ToString("###,##0"), x.ToString("###0"), last_y.ToString("###0"));
                    x += nWochen - 8;
                    /* 
GraphChart.GeneriereGraph() Wochen:90 abWoche:56 minData:782.184 maxData:864.372
AddMarkerTextToChart (80, 782.184) 0
AddMarkerTextToChart (65, 798.622) 1
AddMarkerTextToChart (50, 815.059) 2
AddMarkerTextToChart (35, 831.497) 3
AddMarkerTextToChart (20, 847.934) 4
AddMarkerTextToChart (5, 864.372) 5
Woche:56 x:806.203 last_y:56
Woche:57 x:814.735 last_y:86
Woche:58 x:822.686 last_y:116
Woche:59 x:804.400 last_y:146
Woche:60 x:782.184 last_y:176
Woche:61 x:805.964 last_y:206
Woche:62 x:810.579 last_y:236
Woche:63 x:826.304 last_y:266
Woche:65 x:828.049 last_y:296
Woche:66 x:827.066 last_y:326
Woche:67 x:826.367 last_y:356
Woche:68 x:823.687 last_y:386
Woche:69 x:838.053 last_y:416
Woche:70 x:842.117 last_y:446
Woche:71 x:836.238 last_y:476
Woche:72 x:828.060 last_y:506
Woche:73 x:831.012 last_y:536
Woche:75 x:840.943 last_y:566
Woche:76 x:837.333 last_y:596
Woche:77 x:836.060 last_y:626
Woche:78 x:829.158 last_y:656
Woche:79 x:833.352 last_y:686
Woche:80 x:842.106 last_y:716
Woche:81 x:840.071 last_y:746
Woche:82 x:853.371 last_y:776
Woche:83 x:856.912 last_y:806
Woche:84 x:857.718 last_y:836
Woche:85 x:855.733 last_y:866
Woche:86 x:857.958 last_y:896
Woche:87 x:856.702 last_y:926
Woche:88 x:856.309 last_y:956
Woche:89 x:859.896 last_y:986
Woche:90 x:863.972 last_y:1016
Woche:91 x:855.827 last_y:1046
Woche:92 x:855.320 last_y:1076
Woche:93 x:864.372 last_y:1106
Woche:94 x:860.669 last_y:1136
Woche:95 x:860.262 last_y:1166   */
                }
            }
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 2;
            polyline.Stroke = Brushes.Black;
            polyline.Points = points;
            graphArea.Children.Add(polyline);                       // Die Verbindungslinien            
            setText(80.00, 8.00, "Depotwerte wöchentlich", Colors.Black);
            //CanvasXXX(); 
            ConWrLi("---- -41- GeneriereGraph()");
        }
        private void AddMarkerTextToChart(double top, double markerTextValue) {
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
        private void DrawXAxisLabel(DataRow row, double x) {
            // Setup XAxis label
            TextBlock markText = new TextBlock();
            markText.Text = row["Datum"].ToString();                // 'Region'
            markText.FontFamily = new FontFamily("Century Gothic");
            markText.FontSize = 10;
            //markText.Background = new SolidColorBrush(Colors.LightYellow);
            markText.FontWeight = FontWeights.Bold;
            Transform st = new SkewTransform(0, 20);
            markText.RenderTransform = st;
            Canvas.SetTop(markText, 90);                            // this.Height - 10);  // adjust y location
            Canvas.SetLeft(markText, x);                            // 10 + 10 / 2);
            graphArea.Children.Add(markText);
            //Debug.WriteLine("markText.Text: {0}", markText.Text);
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
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}