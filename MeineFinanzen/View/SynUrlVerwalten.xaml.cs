// 15.03.2018 SynUrlVerwalten.xaml.cs
// Verwaltung der URLs für die Synchronisation mit Daten aus dem Internet.
// 1. Aufteilung der Url in bis zu 5 Teile wird in eine Liste/Tabelle gespeichert.
//    Dabei werden keine doppelten Einträge gemacht.
// 2. Nutzung dieser Liste zur vereinfachten XPath-Anpassung.
//    Bei Änderung der Position des aktuellen Kurses im HTML-Dokument, wird XPathNavigator node geändert.
//    Die Änderung der node wird nun in allen betroffenen URLs vollzogen.
//    Betroffene URLs sind solche, die den Kurs an gleicher Position haben.
// In diesem Program anlegen:
// A: Liste aller URL-Teile.
// B: In jedem WP zusätzlich 5 Felder mit Verweis in die URL-Teile-Tabelle.
using MeineFinanzen.Model;
using System;
using System.Data;
using System.Windows;
namespace MeineFinanzen.View {
    public partial class SynUrlVerwalten : Window {
        public CollWertpapSynchro _wertpapsynchro = null;
        HauptFenster _mw;
        public PortFol _foundRow = new PortFol();
        public PortFol _foundRow_Vor = new PortFol();
        DataTable dtPortFol = new DataTable();
        public SynUrlVerwalten() {
            InitializeComponent();
        }
        public SynUrlVerwalten(HauptFenster mw) {
            ConWrLi("---- -xx- SynUrlVerwalten");
            _mw = mw;
            _wertpapsynchro = (CollWertpapSynchro)mw.Resources["wertpapsynchro"];
            InitializeComponent();
            //getxp = new GetFromXpath();
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }
        private void btPathDoppeln_Click(object sender, RoutedEventArgs e) {
            // Der XPath aus _foundRow_Vor auf andere vergeben. XPath kommt aus: node = doc.GetElementbyId(uniqueId);
            string xpath = _foundRow_Vor.WPXPathKurs;
            Console.WriteLine("1: {0,-120} {1,-80} ", _foundRow_Vor.WPUrlText, _foundRow_Vor.WPXPathKurs);
            foreach (DataRow pofo in dtPortFol.Rows) {
                if (pofo["WPISIN"].ToString().Length < 12)
                    continue;
                pofo["WPUrlText1"] = "1";
                pofo["WPUrlText2"] = "2";
                pofo["WPUrlText3"] = "3";
                pofo["WPUrlText4"] = "4";
                pofo["WPUrlText5"] = "5";
                if (pofo["WPISIN"].Equals(_foundRow_Vor.WPISIN))    // dieser nicht
                    continue;
                if (_foundRow_Vor.WPUrlText.Length < 31 || pofo["WPUrlText"].ToString().Length < 31)
                    continue;
                if (_foundRow_Vor.WPUrlText.Substring(0, 31) == pofo["WPUrlText"].ToString().Substring(0, 31)) {
                    Console.WriteLine("2: {0,-120} {1,-80} ", pofo["WPUrlText"], pofo["WPXPathKurs"]);
                }
            }
        }
        private void wb1_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e) {

        }    
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) {

        }
        private void wb1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) {

        }
        private void wb1_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) {

        }
        private void _Beenden_Click(object sender, RoutedEventArgs e) {

        }
        private void Abmelden_Click(object sender, RoutedEventArgs e) {

        }
        private void KontenaufstellungHBCI4j_Click(object sender, RoutedEventArgs e) {

        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void dgvUrls_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}