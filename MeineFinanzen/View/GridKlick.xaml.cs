﻿using System.Windows;
using System.Diagnostics;
namespace MeineFinanzen.View {   
    public partial class GridKlick : Window {
        internal static BearbeitenView bearb = null;
        internal static string isi = null;
        internal static HauptFenster mw;
        internal int nro = -1;
        internal int nwp = -1;
        public GridKlick() {
            InitializeComponent();
        }
        public GridKlick(HauptFenster mw1, string isi1, int nro1, int nwp1) {
            InitializeComponent();
            mw = mw1;
            isi = isi1;
            nro = nro1;
            nwp = nwp1;
        }
        private void btSpalten_auswählen_Click(object sender, RoutedEventArgs e) {
        }
        private void lbExportAlle_Click(object sender, RoutedEventArgs e) {
        }
        private void btBearbeiten_Click(object sender, RoutedEventArgs e) {
            bearb = new BearbeitenView(mw, isi, nro, nwp);
            bearb.Show();
            this.Close();
        }
        private void btLöschen_Click(object sender, RoutedEventArgs e) {
        }
        private void btExport_Click(object sender, RoutedEventArgs e) {
        }
        private void btInternet_Click(object sender, RoutedEventArgs e) {
            if (isi == "") {
                return;
            }
            foreach (Model.Wertpapier wp in mw._tabwertpapiere._wertpapiere) {
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
    }
}