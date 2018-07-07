// 28.06.2018 GridKlick.cs
// 03.03.2018 DgBanken.wertpap statt -wertpap.  NOCH
using System.Windows;
using System.Diagnostics;
using MeineFinanzen.Helpers;
using MeineFinanzen.ViewModel;
using System;
namespace MeineFinanzen.View {   
    public partial class GridKlick : Window {
        internal static string isi = null;
        internal static HauptFenster mw;
        internal int nro = -1;
        internal int nwp = -1;        
        public GridKlick() {
            ConWrLi("---- -xx- Konstruktor GridKlick()");
        }
        public GridKlick(HauptFenster mw1, string isi1, int nro1, int nwp1) {
            ConWrLi("---- -xx- Konstruktor GridKlick(HauptFenster mw1, string isi1, int nro1, int nwp1)");
            InitializeComponent();
            mw = mw1;
            isi = isi1;
            nro = nro1;
            nwp = nwp1;
        }
        private void BtSpalten_auswählen_Click(object sender, RoutedEventArgs e) {
        }
        private void LbExportAlle_Click(object sender, RoutedEventArgs e) {
        }
        private void BtBearbeiten_Click(object sender, RoutedEventArgs e) {
            ConWrLi("---- -xx- GridKlick BtBearbeiten_Click()");
            Bearbeiten bearb = new Bearbeiten {
                _mw = mw,
                _isin = isi,
                _nro = nro,                // Row-Nr in dgWertpapiere
                _nwp = nwp                 // Nr in Wertpapiere           
            };
            bearb.Show();
            Close();
        }
        private void BtLöschen_Click(object sender, RoutedEventArgs e) {
        }
        private void BtExport_Click(object sender, RoutedEventArgs e) {
        }
        private void BtInternet_Click(object sender, RoutedEventArgs e) {
            if (isi == "") {
                return;
            }
            foreach (Model.Wertpapier wp in DgBanken._wertpapiere) { // mw._tabwertpapiere._wertpapiere) {  // NOCH
                if (isi != wp.ISIN)
                    continue;               
                Process.Start(wp.URL);
                this.Close();
            }          
        }
        private void Datei_Click(object sender, RoutedEventArgs e) {
        }
        private void File_Click(object sender, RoutedEventArgs e) {
        }
        private void Save_Click(object sender, RoutedEventArgs e) {
        }
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}