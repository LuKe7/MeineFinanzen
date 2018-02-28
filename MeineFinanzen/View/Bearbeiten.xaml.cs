// 01.01.2017 BearbeitenView.cs
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using DataSetAdminNS;
namespace MeineFinanzen.View {
    public partial class BearbeitenView : Window {
        public Model.CollWertpapiere _wertpap = null;
        public ObservableCollection<string> TypeList = new ObservableCollection<string>();
        public ObservableCollection<string> DepotList = new ObservableCollection<string>();
        DataRow _rowPortFol;
        DataRow _rowAnlKat;
        DataRow _rowDepot;
        string _isin;
        HauptFenster _mw;
        int _nro;
        int _nwp;
        public BearbeitenView() { }
        public BearbeitenView(HauptFenster mw, string isin, int nro, int nwp) {
            //Point location = new Point(0, 0);
            //Left = location.X;
            //Top = location.Y;
            _mw = mw;
            _isin = isin;
            _nro = nro;                 // Row-Nr in dgWertpapiere
            _nwp = nwp;                 // Nr in Wertpapiere
            InitializeComponent();
            _wertpap = (Model.CollWertpapiere)mw.Resources["wertpapiere"];
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (_isin == "") {
                MessageBox.Show("Ein neues Wertpapier wird angelgt. NOCH");
                return;
            }
            Model.Wertpapier wp = findWP(_isin);

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

            _rowDepot = findDepotID(wp.DepotID.ToString());
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

            DataContext = _wertpap[_nwp];
        }
        public DataRow findDepotID(string depotID) {
            for (int ir = 0; ir < DataSetAdmin.dvDepot.Count; ir++) {
                DataRow dr = DataSetAdmin.dvDepot[ir].Row;
                if (dr["DepotID"].ToString() == depotID.ToString())
                    return dr;
            }
            return null;
        }
        public DataRow findDepotName(string name) {
            for (int ir = 0; ir < DataSetAdmin.dvDepot.Count; ir++) {
                DataRow dr = DataSetAdmin.dvDepot[ir].Row;
                if (dr["DepotName"].ToString() == name)
                    return dr;
            }
            return null;
        }
        public DataRow findTypeName(string name) {
            for (int ir = 0; ir < DataSetAdmin.dvAnlKat.Count; ir++) {
                DataRow dr = DataSetAdmin.dvAnlKat[ir].Row;
                //Debug.WriteLine("{0} {1}", dr["AKName"].ToString(), name);
                if (dr["AKName"].ToString() == name)
                    return dr;
            }
            return null;
        }
        public Model.Wertpapier findWP(string isin) {
            foreach (Model.Wertpapier wp in _wertpap) {
                if (wp.isSumme)
                    continue;
                if (wp.ISIN != isin)
                    continue;
                return wp;
            }
            return null;
        }
        private void btAbbrechen_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
        private void btOk_Click(object sender, RoutedEventArgs e) {
            // In _wertpap[nwp] stehen die Änderungen. 
            if ((string)_rowPortFol["WPName"] != _wertpap[_nwp].Name)
                MessageBox.Show("Der WPName wird übernommen! Alt: " + _rowPortFol["WPName"] + " Neu: " + _wertpap[_nwp].Name);
            _rowPortFol["WPName"] = _wertpap[_nwp].Name;
            if ((double)_rowPortFol["WPKaufsumme"] != _wertpap[_nwp].Kaufsumme)
                MessageBox.Show("WPKaufsumme wird übernommen!" + _wertpap[_nwp].Name + " Alt: " + _rowPortFol["WPKaufsumme"] + " Neu: " + _wertpap[_nwp].Kaufsumme);
            _rowPortFol["WPKaufsumme"] = _wertpap[_nwp].Kaufsumme;
            _rowPortFol["WPKaufDatum"] = _wertpap[_nwp].KaufDatum;
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            this.Close();
        }
        private void cmbSelectionChangedType(object sender, SelectionChangedEventArgs e) {
            if (cbType == null || cbType.SelectedItem == null)
                return;
            _rowAnlKat = findTypeName(cbType.SelectedItem.ToString());
            if (_rowAnlKat["AKName"].ToString() == cbType.SelectedItem.ToString())
                _rowPortFol["WPTypeID"] = _rowAnlKat["AKID"];
        }
        private void cmbSelectionChangedDepot(object sender, SelectionChangedEventArgs e) {
            if (cbDepot == null || cbDepot.SelectedItem == null)
                return;
            _rowDepot = findDepotName(cbDepot.SelectedItem.ToString());
            if (_rowDepot["DepotName"].ToString() == cbDepot.SelectedItem.ToString())
                _rowPortFol["WPDepotID"] = Convert.ToInt32(_rowDepot["DepotID"]);
        }
    }
}