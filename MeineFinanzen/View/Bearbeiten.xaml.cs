// 04.07.2018 Bearbeiten.cs
// https://www.finanzen.net/suchergebnis.asp?strSuchString=LU0171293920     &inCnt=0&strKat=alles
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using DataSetAdminNS;
using System.Windows.Threading;
using System.Threading;
namespace MeineFinanzen.View {
    public partial class Bearbeiten : Window {
        public Model.CollWertpapiere _wertpap = null;
        public ObservableCollection<string> TypeList = new ObservableCollection<string>();
        public ObservableCollection<string> DepotList = new ObservableCollection<string>();
        private static DataRow _rowPortFol;
        private static DataRow _rowAnlKat;
        private static DataRow _rowDepot;
        public string _isin;
        public HauptFenster _mw;
        public int _nro;
        public int _nwp;
        public Bearbeiten() {
            InitializeComponent();
            ConWrLi("---- -60- Konstruktor Bearbeiten()");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            MachWas();
        }
        private void MachWas() {
            ConWrLi("---- -62- Bearbeiten  Window_Loaded()");
            Point location = new Point(0, 0);
            Left = location.X + 300;
            Top = location.Y;
            _wertpap = (Model.CollWertpapiere)_mw.Resources["wertpapiereXXX"];
            if (_isin == "") {
                System.Windows.MessageBox.Show("Ein neues Wertpapier wird angelgt. NOCH");
                return;
            }
            Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
            wb1.GoHome();
            wb1.Navigate(new Uri("https://www.google.de/"));

            Model.Wertpapier wp = FindWP(_isin);
            DataTable dtt1 = new DataTable();
            DataSetAdmin.dvPortFol.Sort = "WPISIN";
            dtt1 = DataSetAdmin.dtPortFol.DefaultView.ToTable();
            DataSetAdmin.dtPortFol = dtt1;
            DataColumn[] keys1 = new DataColumn[1];
            keys1[0] = DataSetAdmin.dtPortFol.Columns["WPIsin"];
            DataSetAdmin.dtPortFol.PrimaryKey = keys1;
            _rowPortFol = DataSetAdmin.dtPortFol.Rows.Find(_isin);
            DataSetAdmin.dvAnlKat.Sort = "AKID";
            DataColumn[] keys2 = new DataColumn[1];
            keys2[0] = DataSetAdmin.dtAnlKat.Columns["AKID"];
            DataSetAdmin.dtAnlKat.PrimaryKey = keys2;

            DataTable dtt2 = new DataTable();
            DataSetAdmin.dvDepot.Sort = "DepotID";
            dtt2 = DataSetAdmin.dtDepot.DefaultView.ToTable();
            DataSetAdmin.dtDepot = dtt2;
            DataColumn[] keys3 = new DataColumn[1];
            keys3[0] = DataSetAdmin.dtDepot.Columns["DepotID"];
            DataSetAdmin.dtDepot.PrimaryKey = keys3;

            _rowDepot = FindDepotID(wp.DepotID.ToString());
            if (_rowDepot != null)
                DepotList.Add(_rowDepot["DepotName"].ToString());
            for (int ir = 0; ir < DataSetAdmin.dvDepot.Count; ir++)
                DepotList.Add((string)DataSetAdmin.dvDepot[ir]["DepotName"].ToString());
            cbDepot.ItemsSource = DepotList;
            cbDepot.SelectedIndex = 0;

            TypeList.Add(wp.AKName);
            for (int ir = 0; ir < DataSetAdmin.dvAnlKat.Count; ir++)
                TypeList.Add((string)DataSetAdmin.dvAnlKat[ir]["AKName"].ToString());
            cbType.ItemsSource = TypeList;
            cbType.SelectedIndex = 0;

            string url = _wertpap[_nwp].URL;
            if (url.Length > 0)
                wb1.Navigate(new Uri(url));
            else {
                url = @"https://www.finanzen.net/suchergebnis.asp?strSuchString=" + _wertpap[_nwp].ISIN;
                wb1.Navigate(new Uri(url));
            }
            DataContext = _wertpap[_nwp];
            txtKaufKurs.Text = (_wertpap[_nwp].Kaufsumme / _wertpap[_nwp].Anzahl).ToString("#.##0,00");
            DoEvents();
        }
        public DataRow FindDepotID(string depotID) {
            for (int ir = 0; ir < DataSetAdmin.dvDepot.Count; ir++) {
                DataRow dr = DataSetAdmin.dvDepot[ir].Row;
                if (dr["DepotID"].ToString() == depotID.ToString())
                    return dr;
            }
            return null;
        }
        public DataRow FindDepotName(string name) {
            for (int ir = 0; ir < DataSetAdmin.dvDepot.Count; ir++) {
                DataRow dr = DataSetAdmin.dvDepot[ir].Row;
                if (dr["DepotName"].ToString() == name)
                    return dr;
            }
            return null;
        }
        public DataRow FindTypeName(string name) {
            for (int ir = 0; ir < DataSetAdmin.dvAnlKat.Count; ir++) {
                DataRow dr = DataSetAdmin.dvAnlKat[ir].Row;
                //Debug.WriteLine("{0} {1}", dr["AKName"].ToString(), name);
                if (dr["AKName"].ToString() == name)
                    return dr;
            }
            return null;
        }
        public Model.Wertpapier FindWP(string isin) {
            foreach (Model.Wertpapier wp in _wertpap) {
                if (wp.isSumme)
                    continue;
                if (wp.ISIN != isin)
                    continue;
                return wp;
            }
            return null;
        }
        private void BtOk_Ende(object sender, RoutedEventArgs e) {
            Close();
        }
        private void BtAbbrechen_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void BtSpeichern_Click(object sender, RoutedEventArgs e) {
            // In _wertpap[nwp] stehen die Änderungen. NOCH weitere Änderungen übernehmen
            bool geändert = false;
            if ((string)_rowPortFol["WPName"] != _wertpap[_nwp].Name) {
                MessageBox.Show("Der WPName  wird übernommen! " + _wertpap[_nwp].Name + " Alt: " + _rowPortFol["WPName"] + " Neu: " + _wertpap[_nwp].Name);
                _rowPortFol["WPName"] = _wertpap[_nwp].Name;
                geändert = true;
            }
            if ((double)_rowPortFol["WPKaufsumme"] != _wertpap[_nwp].Kaufsumme) {
                System.Windows.MessageBox.Show("WPKaufsumme wird übernommen! " + _wertpap[_nwp].Name + " Alt: " + _rowPortFol["WPKaufsumme"] + " Neu: " + _wertpap[_nwp].Kaufsumme);
                _rowPortFol["WPKaufsumme"] = _wertpap[_nwp].Kaufsumme;
                geändert = true;
            }
            if ((DateTime)_rowPortFol["WPKaufDatum"] != _wertpap[_nwp].KaufDatum) {
                MessageBox.Show("WPKaufDatum wird übernommen! " + _wertpap[_nwp].Name + " Alt: " + _rowPortFol["WPKaufDatum"] + " Neu: " + _wertpap[_nwp].KaufDatum);
                _rowPortFol["WPKaufDatum"] = _wertpap[_nwp].KaufDatum;
                geändert = true;
            }
            if ((string)_rowPortFol["WPUrlText"] != _wertpap[_nwp].URL) {
                MessageBox.Show("WPUrlText   wird übernommen! " + _wertpap[_nwp].Name + " Alt: " + _rowPortFol["WPUrlText"] + " Neu: " + _wertpap[_nwp].URL);
                _rowPortFol["WPUrlText"] = _wertpap[_nwp].URL;
                geändert = true;
            }
            if (geändert == true) {
                MessageBox.Show("---- Bearbeiten: DatasetSichernInXml()");
                DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.MyDataPfad);
            }
        }
        private void CmbSelectionChangedType(object sender, SelectionChangedEventArgs e) {
            if (cbType == null || cbType.SelectedItem == null)
                return;
            _rowAnlKat = FindTypeName(cbType.SelectedItem.ToString());
            if (_rowAnlKat["AKName"].ToString() == cbType.SelectedItem.ToString())
                _rowPortFol["WPTypeID"] = _rowAnlKat["AKID"];
        }
        private void CmbSelectionChangedDepot(object sender, SelectionChangedEventArgs e) {
            if (cbDepot == null || cbDepot.SelectedItem == null)
                return;
            _rowDepot = FindDepotName(cbDepot.SelectedItem.ToString());
            if (_rowDepot["DepotName"].ToString() == cbDepot.SelectedItem.ToString())
                _rowPortFol["WPDepotID"] = Convert.ToInt32(_rowDepot["DepotID"]);
        }
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        private void TxtKaufKurs_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) {
            MessageBox.Show("txtKaufKurs_SourceUpdated() Aktualisierung durchgeführt.");
        }
        private void TxtKaufKurs_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) {
            MessageBox.Show("txtKaufKurs_TargetUpdated() Aktualisierung durchgeführt.");
        }
        private void Wb1_DocumentTitleChanged(object sender, EventArgs e) {
            Title = Title = "-Bearbeiten- " + (sender as System.Windows.Forms.WebBrowser).DocumentTitle;
        }
        private void Wb1_StatusTextChanged(object sender, EventArgs e) {
            if (wb1.Url != null) {
                TxtWebUrl.Text = wb1.Url.ToString();
                //TxtWebUrl.InvalidateVisual();
            }
            TxtStatus(wb1.StatusText);
        }
        private void Wb1_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) {
        }
        private void URL_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Uri url = wb1.Document.Url;
            txtUrl.Text = url.ToString();
        }
        protected void DoEvents() {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        public void TxtStatus(string str1) {
            try {
                if (str1.Length >= 3) {
                    if (!str1.Equals("Fertig")) {
                        string str = string.Format("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
                        TxtWebStatus.AppendText(Environment.NewLine + str);
                        TxtWebStatus.ScrollToEnd();
                        TxtWebStatus.InvalidateVisual();
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler TxtWrLi()" + ex);
            }
        }
        private void CmbSelectionChangedUrl1(object sender, SelectionChangedEventArgs e) {
            if (cbUrl1 == null || cbUrl1.SelectedItem == null)
                return;
            // _rowDepot = findDepotxxxxxxxxxxxxx NOCH Name(cbUrl1.SelectedItem.ToString());
            if (_rowDepot["DepotName"].ToString() == cbUrl1.SelectedItem.ToString())
                _rowPortFol["WPDepotID"] = Convert.ToInt32(_rowDepot["DepotID"]);
        }
        private void CbUrl1_Loaded(object sender, RoutedEventArgs e) {
            cbUrl1.Text = "";
            cbUrl1.Items.Add("Alle Konten");
            cbUrl1.SelectedIndex = 0;

            foreach (Model.BankÜbersicht fin in ViewModel.DgBanken.banken) {
                if (fin.OCBankKonten.Count > 0)
                    if (fin.OCBankKonten[0].KontoNr8.Length > 0) {
                        //ConWrLi("==== Bankname {0} /{1}/ {2} {3}", fin.Bankname, fin.Kontonummer, fin.Kontoname, fin.SortFeld);
                        if (DBNull.Value.Equals(fin.OCBankKonten[0].KontoArt8) || (fin.OCBankKonten[0].KontoArt8 == ""))
                            continue;
                        cbUrl1.Items.Add(fin.OCBankKonten[0].KontoNr8);
                    }
            }
            try {
            } catch (Exception ex) {
                MessageBox.Show("Fehler in cbUrl1_Loaded: " + ex);
            }
            Console.WriteLine("---- -04-- Bearbeiten.xaml.cs cbUrl1_Loaded {0}", ViewModel.DgBanken.banken.Count);
        }
    }
}