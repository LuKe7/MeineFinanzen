// 03.10.2016   -View-  Kategorien.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using DataSetAdminNS;
using System.Diagnostics;

namespace MeineFinanzen.View {
    public partial class Kategorien : Window {
        public List<Kategorie> lika;
        public Kategorie kat = null;
        public OCKategorien OCK = null;
        public View.HauptFenster _mw;
        public Kategorien(View.HauptFenster mw) {
            _mw = mw;
            InitializeComponent();
            OCK = new OCKategorien();
            lika = OCK.maches();
            kat = lika[0] as Kategorie;
            base.CommandBindings.Add(
                new CommandBinding(
                    ApplicationCommands.Undo,
                    (sender, e) => // Execute
                    {
                        e.Handled = true;
                        kat.IsChecked = false;
                        treeKategorie.Focus();
                    },
                    (sender, e) => // CanExecute
                    {
                        e.Handled = true;
                        e.CanExecute = (kat.IsChecked != false);
                    }));
        }
        private void btTest_Click(object sender, RoutedEventArgs e) {
            ÄnderungDurchClick();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            treeKategorie.ItemsSource = lika;
        }
        private void ÄnderungDurchClick() {
            TextBox1.Clear();
            TextBox1.AppendText("Start Kategorien anzeigen.");
            try {
                ItemCollection items = treeKategorie.Items;
                foreach (Kategorie kat1 in items) {
                    if (!kat1.IsChecked == null)
                        if (kat1.IsChecked == true)
                            TextBox1.AppendText(Environment.NewLine + kat1.KatName);
                    foreach (Kategorie kat2 in kat1.KatChildren) {
                        if (kat2.IsChecked == true)
                            TextBox1.AppendText(Environment.NewLine + "  " + kat2.KatName);
                        foreach (Kategorie kat3 in kat2.KatChildren) {
                            if (kat3.IsChecked == true)
                                TextBox1.AppendText(Environment.NewLine + "   " + kat3.KatName);
                            foreach (Kategorie kat4 in kat3.KatChildren) {
                                if (kat4.IsChecked == true)
                                    TextBox1.AppendText(Environment.NewLine + "    " + kat4.KatName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler: " + ex);
            }
        }
        private void cmbSelectionChangedAuswertenals(object sender, SelectionChangedEventArgs e) {
            if (cbAuswertenals == null || cbAuswertenals.SelectedItem == null)
                return;
        }
        /*
        bearb = new Bearbeiten(mw, isi, nro, nwp);
        bearb.Show();
            this.Close();
        */
    }
    public class OCKategorien : ObservableCollection<Kategorie> {
        public OCKategorien() { }
        public List<Kategorie> maches() {
            //myKategorie my = new myKategorie();
            return Kategorie.CreateKategorien();
        }
    }
    public class Kategorie : INotifyPropertyChanged, IEditableObject {
        bool? _isChecked = false; // ===================================================
        Kategorie _parent;
        public List<Kategorie> KatChildren { get; private set; }
        //public bool? IsChecked
        public bool IsInitiallySelected { get; private set; }
        public string KatName { get; private set; }
        public bool? IstExpanded { get; set; }
        //void OnPropertyChanged(string prop) ==========================================             
        public static List<Kategorie> CreateKategorien() {
            //mw.TextBox1.Clear();
            List<Kategorie> liKategorie = new List<Kategorie>();
            Kategorie Kategorie0 = null;
            List<Kategorie> lkuXX = KategorienFüllen(liKategorie, ref Kategorie0);
            return lkuXX;
        }
        public static List<Kategorie> KategorienFüllen(List<Kategorie> liKategorie, ref Kategorie Kategorie0) {
            Kategorie0 = new Kategorie("Kategorien");
            Kategorie0.IsInitiallySelected = true;
            Kategorie0.IstExpanded = true;
            Kategorie Kategorie1 = null;
            Kategorie Kategorie2 = null;
            Kategorie Kategorie3 = null;
            int ID1 = -7;
            int ID2 = -7;
            DataTable dtKat = new DataTable();
            dtKat = DataSetAdmin.dtKategorien.DefaultView.ToTable();
            liKategorie.Clear();
            string strHeader = "";
            foreach (DataRow row1 in dtKat.Rows) {
                ID1 = Convert.ToInt32(row1["ID"]);
                int VorläuferID;
                if (DBNull.Value.Equals(row1["VorläuferID"]))
                    VorläuferID = 0;
                else
                    VorläuferID = Convert.ToInt32(row1["VorläuferID"]);
                if (VorläuferID > 0)
                    continue;
                strHeader = row1["Header"].ToString();
                //Debug.WriteLine("{0, -2} {1}", ID1, strHeader);
                Kategorie1 = new Kategorie(strHeader);
                Kategorie1.IstExpanded = true;
                string strHeader2 = "";
                foreach (DataRow row2 in dtKat.Rows) {
                    ID2 = Convert.ToInt32(row2["ID"]);
                    int VorläuferID2;
                    if (DBNull.Value.Equals(row2["VorläuferID"]))
                        VorläuferID2 = 0;
                    else
                        VorläuferID2 = Convert.ToInt32(row2["VorläuferID"]);
                    if (ID1 != VorläuferID2)
                        continue;
                    strHeader2 = row2["Header"].ToString();
                    //Debug.WriteLine("     {0, -2} {1}", ID2, strHeader2);
                    Kategorie2 = new Kategorie(strHeader2);
                    Kategorie2.IsChecked = true;
                    Kategorie2.IstExpanded = true;
                    Kategorie1.KatChildren.Add(Kategorie2);

                    string strHeader3 = "";
                    foreach (DataRow row3 in dtKat.Rows) {
                        int ID3 = Convert.ToInt32(row3["ID"]);
                        int VorläuferID3;
                        if (DBNull.Value.Equals(row3["VorläuferID"]))
                            VorläuferID3 = 0;
                        else
                            VorläuferID3 = Convert.ToInt32(row3["VorläuferID"]);
                        if (ID2 != VorläuferID3)
                            continue;
                        strHeader3 = row3["Header"].ToString();
                        //Debug.WriteLine("         {0, -2} {1}", ID3, strHeader3);
                        Kategorie3 = new Kategorie(strHeader3);
                        Kategorie3.IstExpanded = true;
                        Kategorie2.KatChildren.Add(Kategorie3);
                    }
                }
                Kategorie0.KatChildren.Add(Kategorie1);
            }
            Kategorie0.Initialize();
            liKategorie.Add(Kategorie0);
            /* liKategorie.ForEach(delegate(Kategorie fo)
            {
               Console.WriteLine(fo.Children.Count);
            });
            foreach (Kategorie KategorieVM in liKategorie)
            {
               Console.WriteLine("{0, -20} {1} {2}", KategorieVM.Name, KategorieVM.Children.Count, KategorieVM.IsInitiallySelected);
            }  */
            return liKategorie;
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
        public Kategorie() { }
        public Kategorie(string name) {
            this.KatName = name;
            this.KatChildren = new List<Kategorie>();
        }
        void Initialize() {
            foreach (Kategorie child in this.KatChildren) {
                child._parent = this;
                child.Initialize();
            }
        }
        #region IsChecked
        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child KategorieViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent) {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.KatChildren.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState() {
            //IntPtr windowHandle = new WindowInteropHelper(Application.Current.HauptFenster).Handle;
            //windowHandle.
            /*
            VerifyCheckState()
            i:0 Count: 7 Name: Banken
            == 0       i: 0 Count:  7 Name: Banken
            i:1 Count: 7 Name: Gebühren
            i:2 Count: 7 Name: Rente 
            VerifyCheckState()
            i: 0 Count:9 Name: Arbeit
            == 0       i: 0 Count: 9 Name: Arbeit
            i:1 Count: 9 Name: Bürokratie
            ----OnPropertyChanged Kategorien IsChecked
            */
           Console.WriteLine("---- 1.VerifyCheckState()");
            bool? state = null;
            for (int i = 0; i < this.KatChildren.Count; ++i) {
                bool? current = this.KatChildren[i].IsChecked;
               Console.WriteLine("---- i:{0} Count:{1} Name:{2}", i, this.KatChildren.Count, this.KatChildren[i].KatName);
                if (i == 0) {
                    state = current;
                   Console.WriteLine("---- == 0       i:{0} Count:{1} Name:{2}", i, this.KatChildren.Count, this.KatChildren[i].KatName);
                }
                else if (state != current) {
                    state = null;
                    //Debug.WriteLine("!= current i:{0} Count:{1} Name:{2}", i, this.Children.Count, this.Children[i].Name);
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
           Console.WriteLine("---- Count:{0} Name[0]:{1}", this.KatChildren.Count, this.KatChildren[0].KatName);
        }
        #endregion // IsChecked

        #region INotifyPropertyChanged Members
        void OnPropertyChanged(string prop) {
            if (this.PropertyChanged != null) {
               Console.WriteLine("----OnPropertyChanged {0} {1}", this.KatName, prop);
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}