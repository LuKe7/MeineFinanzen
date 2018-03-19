// 18.03.2018 XPathVerwalten.xaml.cs
// DataGrid aufbauen.
// 
using MeineFinanzen.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace MeineFinanzen.View {
    public partial class XPathVerwalten : Window {
        public static Wertpap       wertpap    = new Wertpap();
        public static UrlTeil       urlteil    = new UrlTeil();
        public static UrlIndex      urlindex   = new UrlIndex();
        public static List<UrlTeil> liUrlTeile = new List<UrlTeil>();             
        public static List<Wertpap> liWertpaps = new List<Wertpap>();
        public DataGridRow dgRow1;
        Konten_Knotenliste_Erstellen _mw = null;
        public XPathVerwalten() {
            InitializeComponent();
        }
        private void Window_Loaded(Object sender, RoutedEventArgs e) {        }
        public void Ausführen(Konten_Knotenliste_Erstellen mw) {
            _mw = mw;
            XPathVerwaltenAusführen();                         // liUrlTeile und liUrlIndices erstellen.                          
        }
        private void XPathVerwaltenAusführen() {
            // 2 Listen erstellen, immer;
            // Liste aller URL - Teile.
            // Zu jedem WP zusätzlich n Einträge mit Verweis in die URL-Teile-Tabelle.
            // XPath kommt aus: node = doc.GetElementbyId(uniqueId);                      
            int teilnr;
            string strUrl = null;
            string[] separators = { "/" };
            string[] strsplit, strsplit1;
            int n;
            liUrlTeile.Clear();           
            liWertpaps = new List<Wertpap>();
            // _mw._foundRow_Vor
            int nn = -1;
            foreach (WertpapSynchro wps in _mw._wertpapsynchro) {
                strUrl = wps.WPSURL;                // https://www.finanzen.net/fonds/sharperatio/spsw_-_whc_global_discovery
                Console.WriteLine("{0,-120} ", strUrl);
                if (strUrl.Length < 31) {
                    Console.WriteLine("URL-Länge < 31 !!!");
                    continue;
                }
                urlindex = new UrlIndex();
                strsplit = strUrl.Split(separators, StringSplitOptions.None);
                foreach (string teil7 in strsplit) {
                    n = SucheInUrl(teil7);
                    if (n >= 0) {
                        Console.Write("Drin:{0} {1} ", n, teil7);
                    } else {                        
                        urlteil = new UrlTeil { Teil = teil7 };
                        liUrlTeile.Add(urlteil);
                        n = SucheInUrl(teil7);
                        Console.Write("Add:{0} {1} ", n, teil7);
                    }
                    urlindex.Index = n.ToString();
                }                                    
                UrlTeil uteile = new UrlTeil();                
                strsplit1 = strUrl.Split(separators, StringSplitOptions.None);               
                foreach (string split in strsplit1) {                // URL
                    teilnr = SucheInUrl(split);
                    Console.Write("{0}={1}/", teilnr, split);
                    uteile = new UrlTeil {
                        Index = (++nn).ToString(),
                        Teil = split,
                        Color = "3"
                    };           
                }
                liUrlTeile.Add(uteile);
                wertpap = new Wertpap {
                    WPISIN = wps.WPSISIN,
                    WPName = wps.WPSName,
                    WPURL = strUrl,
                    WPXPathKurs = wps.WPXPathKurs,
                    WPURLIndices = urlindex.Index,
                    WPURLTeile = uteile.Teil,
                    WPColor = "1"
                };
                liWertpaps.Add(wertpap);
                Console.WriteLine("{0}Add Url wertpaps: {1}", Environment.NewLine, wps.WPSName);
            }
            Console.WriteLine();
            dgvUrls.ItemsSource = liWertpaps;
            Console.WriteLine("wertpaps an dgvUrls gebunden: {0}", liWertpaps.Count);
            //dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();

            dgvTeile.ItemsSource = liUrlTeile;
            Console.WriteLine("liTeile an dgvTeile gebunden: {0}", liUrlTeile.Count);
            //dgvTeile.EnableRowVirtualization = false;
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
        private void dgvUrls_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
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
            //Console.WriteLine("cell1.Column.Header: {0}", cell1.Column.Header); // ist z.B. WPURLSharp
            //string _ColHeader = cell1.Column.Header.ToString();
        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e) { }
    }
    public class UrlTeil {
        public string Index { get; set; }
        public string Teil { get; set; }
        public string Color { get; set; }
    }
    public class Wertpap {
        public string WPName { get; set; }
        public string WPURL { get; set; }
        public string WPURLIndices { get; set; }
        public string WPURLTeile { get; set; }
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
    public class UrlIndex {
        public string Index { get; set; }
    }
}