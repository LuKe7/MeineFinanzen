// 03.12.2018 XPathVerwalten.xaml.cs
// DataGrids aufbauen.
using MeineFinanzen.Helpers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace MeineFinanzen.View {
    public partial class XPathVerwalten : Window {
        public static Wertpap wertpap = new Wertpap();
        public static UrlTeil urlteil = new UrlTeil();
        public static List<UrlTeil> liUrlTeile = new List<UrlTeil>();
        public static List<Wertpap> liWertpaps = new List<Wertpap>();
        public CollWertpapSynchro _wertpapsynchro = null;
        public DataGridRow dgRow1;
        string _ColHeader = "";             // durch dgvUrls_PreviewMouseDown() gesetzt.
        string _curName = null;
        string _curUrl = null;
        string _curIsin = null;
        string _curUrlSharpe = null;
        // NOCH Konten_Knotenliste_Erstellen _mw = null;

        bool _navigiert = false;
        System.Windows.Forms.HtmlElement _elem1 = null;

        public XPathVerwalten() {
            InitializeComponent();
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
        }
        private void Window_Loaded(Object sender, RoutedEventArgs e) { }
       // public void Ausführen(Konten_Knotenliste_Erstellen mw) {
       //     _mw = mw;
       //     XPathVerwaltenAusführen();                         // liUrlTeile und liUrlIndices erstellen.                          
       // }
        private void XPathVerwaltenAusführen() {
            // Liste URLTeile.
            // Zu jedem WP zusätzlich n Einträge mit Verweis in die URL-Teile-Tabelle.
            // XPath kommt aus: node = doc.GetElementbyId(uniqueId); 
            HauptFenster _mw = GlobalRef.g_mw;
            string strUrl = null;
            string[] separators = { "/" };
            string[] strsplit;
            int n;
            liUrlTeile.Clear();
            liWertpaps = new List<Wertpap>();
            string strIndices;
            _wertpapsynchro = (CollWertpapSynchro)Resources["wertpapsynchro"];
            foreach (WertpapSynchro wps in _wertpapsynchro) {   // Loop Wertpapiere
                strUrl = wps.WPSURL;                // https://www.finanzen.net/fonds/sharperatio/spsw_-_whc_global_discovery
                Console.WriteLine("{0,-120} ", strUrl);
                if (strUrl.Length < 31) {
                    Console.WriteLine("URL-Länge < 31 !!!");
                    continue;
                }
                strIndices = "";
                strsplit = strUrl.Split(separators, StringSplitOptions.None);
                foreach (string teil in strsplit) {                // Loop UrlTeile
                    n = SucheInUrl(teil);
                    if (n >= 0) {
                        Console.Write("Drin:{0} {1} ", n, teil);
                    } else {
                        urlteil = new UrlTeil {
                            Index = liUrlTeile.Count.ToString(),
                            Teil = teil,
                            Color = "3"
                        };
                        liUrlTeile.Add(urlteil);
                        n = SucheInUrl(teil);
                        Console.Write("Add:{0} {1} ", n, teil);
                    }
                    strIndices += n.ToString() + "/";
                }
                wertpap = new Wertpap {
                    WPISIN = wps.WPSISIN,
                    WPName = wps.WPSName,
                    WPType = wps.WPSType.ToString(),
                    WPURL = strUrl,
                    WPXPathKurs = wps.WPXPathKurs,
                    WPXPathAend = wps.WPXPathAend,
                    WPXPathZeit = wps.WPXPathZeit,
                    WPXPathSharp = wps.WPXPathSharp,
                    WPURLIndices = strIndices,
                    WPColor = "1"
                };
                liWertpaps.Add(wertpap);
            }
            Console.WriteLine();
            dgvUrls.ItemsSource = liWertpaps;
            Console.WriteLine("liWertpaps an dgvUrls gebunden: {0}", liWertpaps.Count);
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();

            dgvTeile.ItemsSource = liUrlTeile;
            Console.WriteLine("liUrlTeile an dgvTeile gebunden: {0}", liUrlTeile.Count);
            dgvTeile.EnableRowVirtualization = false;
            dgvTeile.UpdateLayout();
        }
        private int SucheInUrl(string str) {
            int n = -1;
            foreach (UrlTeil ut in liUrlTeile) {
                n++;
                if (ut.Teil.Equals(str)) {
                    return n;
                }
            }
            return -1;
        }
        private void DgvUrls_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridCell cell1 = dep as DataGridCell;
            dgRow1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1) as DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            Console.WriteLine("cell1.Column.Header: {0}", cell1.Column.Header); // ist z.B. WPURLSharp
            _ColHeader = cell1.Column.Header.ToString();
            _curName = ((Wertpap)item).WPName;
            _curIsin = ((Wertpap)item).WPISIN;
            _curUrl = ((Wertpap)item).WPURL;
            _curUrlSharpe = ((Wertpap)item).WPURLSharp;
            NavigiereZu(_curUrl);
        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e) { }
        private void NavigiereZu(string address) {
            if (string.IsNullOrEmpty(address))
                return;
            if (address.Equals("about:blank"))
                return;
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
                address = "http://" + address;
            _navigiert = true;
            wb1.Navigate(new Uri(address));
        }
        private void Wb1_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) { }
        private void Wb1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) {
            AddTextStr("wb1_DocumentCompleted.");// + _elem1.InnerText);
            if (wb1.Document != null && _navigiert) {
                //SetFontSize(webBrowser1.Document.Body);
                wb1.Document.Click += new System.Windows.Forms.HtmlElementEventHandler(Wb1_Document_Click);
                _navigiert = false;
            }
        }
        private void Wb1_DocumentTitleChanged(object sender, EventArgs e) { }
        private void Wb1_Document_Click(Object sender, System.Windows.Forms.HtmlElementEventArgs e) {
            if (e.ClientMousePosition.IsEmpty) {
                _elem1 = null;
                //posx = -1;
                //posy = -1;
            } else {
                //posx = e.ClientMousePosition.X;
                //posy = e.ClientMousePosition.Y;
                _elem1 = wb1.Document.GetElementFromPoint(e.ClientMousePosition); // Ruft das an den angegebenen Clientkoordinaten befindliche HTML-Element ab.                            
                if (_elem1 != null)
                    AddTextStr("wb1_Document_Click: " + _elem1.InnerText); // wb1_Document_Click: 376,36 EUR -1,72 EUR -0,45%                
                else
                    AddTextStr("wb1_Document_Click: null");
            }
        }
        private string[] SearchWebPage() {
            if (wb1.Document == null)
                return null;
            System.Windows.Forms.HtmlElementCollection elemColl = null;
            System.Windows.Forms.HtmlDocument doc = wb1.Document;
            if (doc != null) {
                AddTextStr("--- Start ---");
            }
            string strText = "";
            foreach (System.Windows.Forms.HtmlElement elem in elemColl) {
                if (elem.InnerHtml.Contains("row quotebox")) {
                    strText = elem.InnerText;
                    strText = Regex.Replace(strText, "[\x00-\x1F]+", "/");
                    string[] strarr = strText.Split('/');
                    Console.WriteLine("In 'row quotebox' Children: {0,2} elem.InnerHtml.Length: {1,5}", elem.Children.Count, elem.InnerHtml.Length);
                    int nn = 0;
                    foreach (string split in strarr) {
                        if (split.StartsWith("Kurs")) {
                            Console.WriteLine("{0,3} split:{1}", ++nn, split);
                            AddTextStr(split);
                        }
                    }
                    Console.WriteLine("---------------------------------------------------------------");
                    return strarr;
                }                   // end if                 
            }                   // foreach HtmlElement
            return null;    // Wenn kein 'row quotebox' gefunden.
        }   
        private void BtPrintDom_Click(object sender, RoutedEventArgs e) {
            SearchWebPage();
        }
        private void AddTextStr(string str) {
            txtBox.AppendText(Environment.NewLine + str);
            txtBox.ScrollToEnd();
            txtBox.InvalidateVisual();
        }
        public void TxtWrLi(string str1) {
            try {
                string str = string.Format("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
                txtBox.AppendText(Environment.NewLine + str);
                txtBox.ScrollToEnd();
                txtBox.InvalidateVisual();
            } catch (Exception ex) {
                MessageBox.Show("Fehler TxtWrLi()" + ex);
            }
        }
    }
    public class UrlTeil {
        public string Index { get; set; }
        public string Teil { get; set; }
        public string Color { get; set; }
    }
    public class Wertpap {
        public string WPName { get; set; }
        public string WPURL { get; set; }
        public string WPType { get; set; }
        public string WPURLIndices { get; set; }
        public string WPISIN { get; set; }
        public double WPKurs { get; set; }
        public double WPProzentAenderung { get; set; }
        public Single WPharpe { get; set; }
        public string WPXPathKurs { get; set; }
        public string WPXPathAend { get; set; }
        public string WPXPathZeit { get; set; }
        public string WPXPathSharp { get; set; }
        public string WPURLSharp { get; set; }
        public string WPColor { get; set; }
    }
}