// 24.04.2018   -View-  HauptFenster.cs 
// Tja, wenn man die Grundlagen nicht lernen will, stolpert man halt ständig beim Ausprobieren.
// Wenn du eine DataTable an ein DG bindest, spiegelt der DefaultView der DT die Daten wieder. Mit allen Filter- und Sort-Angaben.
// 16.11.2014 Ser/Deserialize 'Wertpapiere' zu/von Xml-Datei. 
// 07.08.2015 neustart: erst sichern.
// 11.2016 Login
// 27.11.2016 In KontenSynchronisieren..... BankÜbersichtsDaten.xml erstellen. 
// 27.10.2017 Ordneranpassung fürs Notebook D :/....
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Threading;
using Subsembly.FinTS;
using DataSetAdminNS;
using Subsembly.FinTS.Admin;
using MeineFinanzen.Model;
using MeineFinanzen.ViewModel;
using MeineFinanzen.Helpers;
namespace MeineFinanzen.View {
    public partial class HauptFenster : Window {
        internal string strSplash = "MeineFinanzen... loading";
        //private SplashWindow splash = null;
        internal List<FinContact> liContacte = new List<FinContact>();
        internal DgBanken _dgBanken;
        internal KontenSynchronisierenHBCI4j _kosyHBCI4j;
        internal VMKontenSynchronisierenSubsembly _kosySubsembly;
        internal KontenSynchronisierenInt _kosyInt;
        internal KontenSynchronisierenInt2 _kosyInt2;
        internal Konten_Knotenliste_Erstellen _kosyErstellen;         
        internal StreamWriter swLog;
        internal DateTime _Datum;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool _isGroup = true;
        internal bool _boAktualisieren = false;
        internal TabWertpapiere _tabwertpapiere = new TabWertpapiere();
        private TabKontoumsätze _tabKtoGes = new TabKontoumsätze();
        // NOCH internal ViewModel.TabGuckMalHier _tabguck = null;        
        internal bool tabWertGefüllt = false;
        private Stopwatch _stopwatch = new Stopwatch();        // Eine StopUhr
        private static UmsätzeHolen umsHolen;
        private Process[] processes;
        DirectoryInfo rootDir = null;      
        public HauptFenster() {
            //splash = new SplashWindow(this);
            // NOCH splash.Show();
            InitializeComponent();
            processes = Process.GetProcesses();
            ConWrLi("---- -1a- In HauptFenster()");
            GlobalRef.g_mw = this;
            GlobalRef.g_Ein = new Einstellungen();
            GlobalRef.g_User = new User();
            GlobalRef.g_CollUser = new CollUser();
            GlobalRef.g_WP = new CollWertpapiere();
            GlobalRef.g_WPHBCI = new WertpapHBCI4j();
            GlobalRef.g_KoHBCI = new CollKontenaufstellung();
            string[] drives = Environment.GetLogicalDrives();
            GlobalRef.g_dgBanken = new DgBanken();
            DirectoryInfo[] subDirs = null;
            foreach (string dr in drives) {
                DriveInfo di = new DriveInfo(dr);
                if (!di.IsReady)
                    continue;
                subDirs = di.RootDirectory.GetDirectories();
                foreach (DirectoryInfo dirInfo in subDirs) {
                    if (dirInfo.Name.Equals("MeineFinanzen")) {
                        Console.WriteLine("---- " + dirInfo.Name);
                        rootDir = dirInfo;
                        break;
                    }
                }
            }
            // FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));            
            // string strxxx = Helpers.GlobalRef.g_Ein + @"\" + Assembly.GetExecutingAssembly().GetName().Name;
            // MeineFinanzen.Model.Einstellungen\MeineFinanzen            
            GlobalRef.g_Ein.DeSerializeReadEinstellungen(rootDir.FullName + @"\MyDepot\Einstellungen\EinstellungsDaten.xml", out GlobalRef.g_Ein);
            ConWrLi("---- -1c- In HauptFenster()");
            GlobalRef.g_Ein.SerializeWriteEinstellungen(GlobalRef.g_Ein.strEinstellungen, GlobalRef.g_Ein);
            ConWrLi("---- -1d- In HauptFenster()");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ConWrLi("---- -2a- Beginn in Window_Loaded()");
            dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            DateTime dt7 = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt7 == null) {
                MessageBox.Show("MeineFinbanzen HauptFenster.xaml.cs HauptFenster() Fehler bei HolenAusXml() DataSetAdmin");
                this.Close();
                }
            ConWrLi("---- -2b- Nach DataSetAdmin.HolenAusXml()");
            var counters = new List<PerformanceCounter>();
            foreach (FinContact aContact in FinAdmin.DefaultFolder) { // FinContactFolder.Default) {  
                ConWrLi("---- -2b+- FinContact: " + aContact.BankCode + "/" + aContact.ContactName);
                liContacte.Add(aContact);           // Banken    zwischenspeichern. Wg. Blockade.
                }
            ConWrLi("---- -2c- In Window_Loaded()");
            _stopwatch.Start();
            _dgBanken = new DgBanken();
            _dgBanken.Machdgbanken();               // Mit DeSerialize banken also Read                   
            
            GlobalRef.g_KoHBCI.Kontenaufstellung_ReadXml();
            //_kontenaufHBCI4j = new KontenaufstellungHBCI4j();
            ConWrLi("---- -4- Nach machdgbanken()");
            wbAktuelles.Navigate(GlobalRef.g_Ein.strUrlIndizes);
            dgFinanzübersicht.ItemsSource = null;
            dgFinanzübersicht.ItemsSource = DgBanken.banken;
            Console.WriteLine("---- --01-- HauptFenster _dgBanken.banken.Count {0}", DgBanken.banken.Count);
            ConWrLi("---- -5- Nach _tabFinanzen.maches()");
            umsHolen = new Helpers.UmsätzeHolen(this);
            Console.WriteLine("---- --02-- HauptFenster _dgBanken.banken.Count {0}", DgBanken.banken.Count);
            // NOCH dtKontoumsätze sofort erstellen, beim download.
            // Lade dtKontoumsätze und _mw.Resources["kontoumsätzeGesamt"] aus g_Ein.myDepotPfad+\Log\Umsätze-"+sKtoNr+.csv                       
            // NOCH Zahlungen zah = new Zahlungen(this);    // ---> .dtPortFolBew. Aus CollKontoumsätzeGesamt Zahlungen extrahieren.
            // NOCH zah.ShowDialog();  
            tabControl1.SelectedItem = tabWertpapiere;
            tabWertpapiere.Visibility = Visibility.Visible;
            dgFinanzübersicht.EnableRowVirtualization = false;
            DrawChart();
            WertPapStart();
            //RowDef1.Height = new GridLength(1, GridUnitType.Star);
            //var xxx = dgFinanzübersicht.FindName("GridInnereDatagrid2");//.RowDef2.Height = new GridLength(1, GridUnitType.Star);
            //Console.WriteLine("");
            //RowDef3.Height = new GridLength(1, GridUnitType.Star);
            Console.WriteLine("---- --03-- HauptFenster _dgBanken.banken.Count {0}", DgBanken.banken.Count);
            }
        private void CbKonten_Loaded(object sender, RoutedEventArgs e) {
            cbKonten.Text = "";
            cbKonten.Items.Add("Alle Konten");
            cbKonten.SelectedIndex = 0;

            foreach (Model.BankÜbersicht fin in DgBanken.banken) {
                if (fin.OCBankKonten.Count > 0)
                    if (fin.OCBankKonten[0].KontoNr8.Length > 0) {
                        //ConWrLi("==== Bankname {0} /{1}/ {2} {3}", fin.Bankname, fin.Kontonummer, fin.Kontoname, fin.SortFeld);
                        if (DBNull.Value.Equals(fin.OCBankKonten[0].KontoArt8) || (fin.OCBankKonten[0].KontoArt8 == ""))
                            continue;
                        cbKonten.Items.Add(fin.OCBankKonten[0].KontoNr8);
                        }
                }
            try {
                } catch (Exception ex) {
                MessageBox.Show("Fehler in cbKonten_Loaded: " + ex);
                }
            Console.WriteLine("---- --04-- HauptFenster _dgBanken.banken.Count {0}", DgBanken.banken.Count);
            }
        private void CbNeueVerbindung_Loaded(object sender, RoutedEventArgs e) {
            cbNeueVerbindung.Text = "";
            cbNeueVerbindung.Items.Add("Bankverbindungen");
            cbNeueVerbindung.Items.Add("Neues Bank/Konto A");
            cbNeueVerbindung.Items.Add("Neues Bank/Konto B");
            cbNeueVerbindung.Items.Add("Neues Bank/Konto C");
            cbNeueVerbindung.SelectedIndex = 0;
            }
        private void CbNeueVerbindung_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (cbNeueVerbindung.Items.CurrentItem == null)
                return;
            string text = (sender as ComboBox).SelectedItem as string;
            }
        private void WbAktuelles_LoadCompleted(object sender, NavigationEventArgs e) {
            ConWrLi("---- -16g- wbAktuelles_LoadCompleted()");
            WebBrowser wb = (WebBrowser)sender;
            mshtml.HTMLDocument htmlDoc = wb.Document as mshtml.HTMLDocument;
            htmlDoc.parentWindow.scroll(0, 260);
            WindowState = WindowState.Maximized;
            ConWrLi("---- -16h- wbAktuelles_LoadCompleted()");
            // NOCH splash.Close();
            Console.WriteLine("---- --05-- HauptFenster _dgBanken.banken.Count {0}", DgBanken.banken.Count);           
            }
        private void CPUSpeedAnzeigen() {
            var counters = new List<PerformanceCounter>();
            foreach (Process process in processes) {
                var counter1 = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                var counter2 = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                counter1.NextValue();
                counters.Add(counter1);
                }
            int i = 0;
            foreach (var counter in counters) {
                string proName = processes[i].ProcessName;
                var xx1 = (Math.Round(counter.NextValue(), 1));
                var xx2 = (Math.Round(counter.NextValue(), 1));
                var xx3 = (Math.Round(counter.NextValue(), 1));
                if (proName.Contains("MeineFinanzen"))
                    txbCPU.Text = "MeiFi " + xx1.ToString();
                if (proName.Contains("Idle"))
                    txbCPU.Text += ", Idle" + xx1.ToString();
                ++i;
                }
            }
        //private IEnumerable<CollZahlungen> _zahlungen;     
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        private static void ExecuteInForeground() {
            DateTime start = DateTime.Now;
            var sw = Stopwatch.StartNew();
            //Debug.WriteLine("Thread {0}: {1}, Priority {2}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState, Thread.CurrentThread.Priority);
            do {
                //Debug.WriteLine("Thread {0}: Elapsed {1:N2} seconds", Thread.CurrentThread.ManagedThreadId, sw.ElapsedMilliseconds / 1000.0);
                Thread.Sleep(500);
                } while (sw.ElapsedMilliseconds <= 5000);
            sw.Stop();
            }
        private void DrawChart() {
            Helpers.ChartDatenHolen dh = new Helpers.ChartDatenHolen();
            DataTable dtWochen = dh.ChartDatenHolenX();
            graphchart.GeneriereGraph(dtWochen);
            }
        internal void NeuStarten() {
            ConWrLi("---- -8h- in neuStarten-1");
            DateTime dt = DataSetAdminNS.DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt == null) {
                MessageBox.Show("MeineFinanzen HauptFenster.xaml.cs HauptFenster() Fehler bei HolenAusXml() DataSetAdmin");
                this.Close();
                }
            WertPapStart();
            ConWrLi("---- -8i- in neuStarten-2");
            }
        private void DispatcherTimer_Tick(object sender, EventArgs e) {
            TimeSpan ts = _stopwatch.Elapsed;   // Die vergangene Zeit
            CPUSpeedAnzeigen();
            if (ts.Seconds > 180) {
                wbAktuelles.Navigate("http://kurse.boerse.ard.de/ard/indizes_einzelkurs_uebersicht.htn?bigIndex=0&i=159096&sektion=einzelwerte&sortierung=descriptionShort&ascdesc=ASC");
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                dispatcherTimer.Start();
                _stopwatch.Restart();
                }
            if (_boAktualisieren) {                         // Wenn gestartet
                if (!PrüfeKurseAktualisierenLäuft()) {      // und nicht mehr läuft               
                    _boAktualisieren = false;
                    KontenSynchronisierenInt_Fertig();
                    }
                }
            }
        private bool PrüfeKurseAktualisierenLäuft() {
            foreach (Process p in Process.GetProcesses()) {
                //Console.WriteLine("{0}", p.MainWindowTitle);
                if (p.MainWindowTitle.Contains("KontenSynchronisierenInt")) {
                    return true;
                    }
                }
            return false;
            }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = sender as TabControl;
            var selected = item.SelectedItem as TabItem;
            //this.Title = selected.Header.ToString();
            }
        private void TabFinanzen() {
            // _tabFinanzen.maches(this);
            //GridGesamtvermögen.Visibility = Visibility.Visible;
            //cbNeueVerbindung.Visibility = Visibility.Visible;
            }
        private void TabWertpap() {
            _tabwertpapiere = new TabWertpapiere();
            _tabwertpapiere.FelderLöschen();
            _tabwertpapiere.ErstelleWertpapiere(this);  // Aus dtPortFol

            tabWertGefüllt = true;
            CPUSpeedAnzeigen();
            }
        private void WertPapStart() {
            AlleTabsHidden();
            cbGraph.Visibility = Visibility.Visible;
            cbBrowser.Visibility = Visibility.Visible;
            //StackCheckBoxen.Visibility = System.Windows.Visibility.Visible;
            tabControl1.SelectedItem = tabWertpapiere;
            tabWertpapiere.Visibility = Visibility.Visible;
            ConWrLi("---- -8a- in WertPapStart vor tabWertpap");
            TabWertpap();
            ConWrLi("---- -8b- in WertPapStart nach tabWertpap");
            dgWertpapiere.UpdateLayout();
            ConWrLi("---- -8c- in WertPapStart nach dgWertpapiere.UpdateLayout()");
            }
        private void BtKategorien_Click(object sender, RoutedEventArgs e) {
            Kategorien myKat = new Kategorien(this);
            myKat.ShowDialog();    // Window_Loaded                
            myKat.Close();
            }
        private void AlleTabsHidden() {
            //StackCheckBoxen.Visibility = System.Windows.Visibility.Hidden;
            cbGraph.Visibility = Visibility.Collapsed;
            cbBrowser.Visibility = Visibility.Collapsed;
            //cbNeueVerbindung.Visibility = Visibility.Hidden;
            //GridGesamtvermögen.Visibility = Visibility.Hidden;
            //btFinanzübersicht.BorderThickness = new Thickness(1.0);
            //btWertpapiere.BorderThickness = new Thickness(1.0);
            //btKontoumsätzeGesamt.BorderThickness = new Thickness(1.0);
            //btWertpapiereGesamt.BorderThickness = new Thickness(1.0);
            //btKategorien.BorderThickness = new Thickness(1.0);
            //btGuckMalHier.BorderThickness = new Thickness(1.0);

            tabFinanzübersicht.Visibility = Visibility.Hidden;
            tabWertpapiere.Visibility = Visibility.Hidden;
            tabKontoumsätze.Visibility = Visibility.Hidden;
            tabKontoumsätze.Visibility = Visibility.Hidden;
            tabWertpapiereGesamt.Visibility = Visibility.Hidden;
            tabKategorien.Visibility = Visibility.Hidden;
            tabGuckMalHier.Visibility = Visibility.Hidden;
            }
        private void GridWertpapiere_LoadingRow(object sender, DataGridRowEventArgs e) { }
        private void BtWertpapiereGesamt_Click(object sender, RoutedEventArgs e) {
            AlleTabsHidden();
            tabControl1.SelectedItem = tabWertpapiereGesamt;
            tabWertpapiereGesamt.Visibility = Visibility.Visible;
            //btWertpapiereGesamt.BorderThickness = new Thickness(6.0);
            gridWertpapiereGesamt.UpdateLayout();
            }
        private void BtKontoumsatz_Click(object sender, RoutedEventArgs e) {
            AlleTabsHidden();
            tabControl1.SelectedItem = tabKontoumsätze;
            tabKontoumsätze.Visibility = Visibility.Visible;
            //btKontoumsatz.BorderThickness = new Thickness(6.0);
            ViewModel.TabKontoumsätze tabkto = new ViewModel.TabKontoumsätze(this);
            }
        private void BtGuckMalHier_Click(object sender, RoutedEventArgs e) {
            AlleTabsHidden();
            tabControl1.SelectedItem = tabGuckMalHier;
            tabGuckMalHier.Visibility = Visibility.Visible;
            //btGuckMalHier.BorderThickness = new Thickness(6.0);
            //tagesw = new Tageswerte(this);
            //tagesw.Show();            
            //Close();
            }
        private void UngroupButton_Click(object sender, RoutedEventArgs e) {
            ICollectionView cvWertpapiere = CollectionViewSource.GetDefaultView(dgWertpapiere.ItemsSource);
            if (cvWertpapiere != null && cvWertpapiere.CanGroup == true) {
                _isGroup = false;
                cvWertpapiere.GroupDescriptions.Clear();
                }
            }
        private void GroupButton_Click(object sender, RoutedEventArgs e) {            
            e.Handled = true;
            ICollectionView cvWertpapiere = CollectionViewSource.GetDefaultView(dgWertpapiere.ItemsSource);
            if (cvWertpapiere != null && cvWertpapiere.CanGroup == true) {
                _isGroup = true;
                cvWertpapiere.GroupDescriptions.Clear();
                cvWertpapiere.GroupDescriptions.Add(new PropertyGroupDescription("AKName"));
                //cvWertpapiere.GroupDescriptions.Add(new PropertyGroupDescription("ISIN"));
                }
            }
        private void ISINFilter_Changed(object sender, RoutedEventArgs e) {
            // Refresh the view to apply filters.
            CollectionViewSource.GetDefaultView(dgWertpapiere.ItemsSource).Refresh();
            }
        private void CollectionViewSource_Filter(object sender, FilterEventArgs e) {
            /* So filtern Sie Elemente in einem DataGrid.
         * Fügen Sie einen Handler für das CollectionViewSource.Filter-Ereignis hinzu.
         * Definieren Sie im Filter-Handler die Filterlogik.
         * Der Filter wird jedes Mal angewendet, wenn die Ansicht aktualisiert wird.
         * Alternativ können Sie Elemente in einem DataGrid filtern, indem Sie eine Methode erstellen,
         * die die Filterlogik bereitstellt, und die CollectionView.Filter-Eigenschaft zum Anwenden des Filters festlegen.
         * Ein Beispiel für diese Methode finden Sie unter Gewusst wie: Filtern von Daten in einer Ansicht. */
            Model.Wertpapier t = e.Item as Model.Wertpapier;
            //if (t.BisDatum.Year == 1980)

            if (t != null)
            // If filter is turned on, filter ISINd items.
            {
                //if (this.cbISINFilter.IsChecked == true && t.ISIN == "1004902")
                //    e.Accepted = false;
                //else
                e.Accepted = true;
                }
            }
        private void CollectionViewSourceFinanz_Filter(object sender, FilterEventArgs e) {
            Model.BankÜbersicht t = e.Item as Model.BankÜbersicht;
            if (t != null)
            // If filter is turned on, filter ISINd items.
            {
                //if (this.cbISINFilter.IsChecked == true && t.ISIN == "1004902")
                //    e.Accepted = false;
                //else
                e.Accepted = true;
                }
            }
        private void GridWertpapiere_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Windows.Controls.DataGrid dg = sender as System.Windows.Controls.DataGrid;
            /*
            DataRow dtr = ((System.Data.DataRowView)(dg.SelectedValue)).Row;
            string dat = dtr["AbDatum"].ToString();
            if (dat == "01.01.1980 00:00:00")
            {
               Console.WriteLine("kein Datum vor : {0}", dtr["AbDatum"]);
                dtr["AbDatum"] = DBNull.Value;
               Console.WriteLine("kein Datum nach: {0}", dtr["AbDatum"]);
            } */
            //Now to get the cell value just write dtr[0], dtr["ID"], etc.
            }
        private void CbKonten_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (cbKonten.Items.Count == 1)
                return;
            if (tabControl1.SelectedItem != tabKontoumsätze)
                return;
            AlleTabsHidden();
            tabControl1.SelectedItem = tabKontoumsätze;
            tabKontoumsätze.Visibility = Visibility.Visible;
            //btKontoumsätzeGesamt.BorderThickness = new Thickness(6.0);
            }
        private void Abmelden_Click(object sender, RoutedEventArgs e) {
            App.Current.Shutdown();
            }
        private void Konten_Knotenliste_Erstellen_Click(object sender, RoutedEventArgs e) {
            /* string strDir = @"C :\U sers\LuKe\Documents\Visual Studio 2015\Projects\MeineFinanzen Projekte\SynchronisierenInit\bin\Debug";            
             Directory.SetCurrentDirectory(strDir);
             ProcessStartInfo startInfo = new ProcessStartInfo("SynchronisierenInit");
             startInfo.Arguments = null;
             startInfo.UseShellExecute = false;
             startInfo.RedirectStandardOutput = true;
             startInfo.CreateNoWindow = false;
             startInfo.WindowStyle = ProcessWindowStyle.Hidden;
             _myProcess = null;
             _myProcess = Process.Start(startInfo);
             {
                 if (!_myProcess.HasExited) {
                     _myProcess.Refresh();
                 }
             } */
            _kosyErstellen = new Konten_Knotenliste_Erstellen(this);
            _kosyErstellen.ShowDialog();
            NeuStarten();
            }
        private void KontenSynchronisierenInt_Click(object sender, RoutedEventArgs e) {
            if (_boAktualisieren)
                return;
            _boAktualisieren = true;
            _kosyInt = new KontenSynchronisierenInt();  // ===> Wertpapiere Internet ---> dtPortFol Kurs Änd% ÄndDat Sharpe 
            _kosyInt.Ausführen(this, true);             // Kehrt SOFORT zurück. Wenn fertig: Rest starten s.u.       
            }
        private void KontenSynchronisierenInt2_Click(object sender, RoutedEventArgs e) {
            _kosyInt2 = new KontenSynchronisierenInt2();
            _kosyInt2.Ausführen();
        }
        private void KontenSynchronisierenInt_Fertig() {
            WertPapStart();
            GlobalRef.g_dgBanken.Machdgbanken();
            string s = Convert.ToString(DateTime.Now).Trim();
            string filename = GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten\PortFolInt_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
            GlobalRef.g_WP.SerializeWertpapiere(filename, DgBanken._wertpapiere);
            ConWrLi("---- -xx- Nach SerializeWertpapiere Int()");
            _boAktualisieren = false;
        }
        private void KontenSynchronisierenSubsembly_Click(object sender, RoutedEventArgs e) {
            if (_boAktualisieren)
                return;
            _kosySubsembly = new VMKontenSynchronisierenSubsembly();
            _kosySubsembly.Ausführen(this, true);       // false = nicht laden.
            GlobalRef.g_dgBanken.Machdgbanken();
            do { }
            while (!_kosySubsembly.WertpapSubsemblyToPortFol());
            WertPapStart();            
            string s = Convert.ToString(DateTime.Now).Trim();
            string filename = GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten\PortFol_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
            GlobalRef.g_WP.SerializeWertpapiere(filename, DgBanken._wertpapiere); 
            ConWrLi("---- -xx- Nach SerializeWertpapiere Subsembly()");
        }
        private void KontenSynchronisierenHBCI4j_Click(Object sender, RoutedEventArgs e) {
            if (_boAktualisieren)
                return;
            _kosyHBCI4j = new KontenSynchronisierenHBCI4j();
            _kosyHBCI4j.Ausführen(this, true);
            GlobalRef.g_dgBanken.Machdgbanken();
            do { }
            while (!_kosyHBCI4j.WertpapHBCI4jToPortFol());
            WertPapStart();
            string s = Convert.ToString(DateTime.Now).Trim();
            string filename = GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten\PortFolHBCI_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
            GlobalRef.g_WP.SerializeWertpapiere(filename, DgBanken._wertpapiere);
            ConWrLi("---- -xx- Nach SerializeWertpapiere HBCI()");
        }
        internal Single USDtoEuro(Single Kurs) {
            Single USKurs = 0;
            if (USKurs == 0) {
                USKurs = HoleUSDKurs();
                if (USKurs == 0) {
                    //DialogResult result = MessageBox.Show("Nicht in dvIntKurse-3 ", /*+ strisin, */
                    //"Ablaufkontrolle", MessageBoxButtons.YesNo);
                    //if (result == DialogResult.Yes)
                    //{
                    USKurs = (float)1.12;
                    //}
                    //else
                    //{
                    //    USKurs = 0.01m;
                    //}
                    }
                }
            return Kurs * (1 / USKurs);   //' z.Zt. 1/1.35 ca. 0.74 Euro pro Dollar
            }
        private Single SGDtoEuro(Single Kurs) {
            Single SGDKurs = 0;  //'2.03072
            SGDKurs = HoleSGDKurs();
            if (SGDKurs == 0) {
                /* mDialogResult result = MessageBox.Show("SGDKurs ist 0.00 !!!!", //+ strisin, 
                "Ablaufkontrolle", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    SGDKurs = 0.5m;
                }
                else
                {
                    SGDKurs = 1.00m;
                } */
                }
            return Kurs * (1 / SGDKurs);
            }
        private Single HoleUSDKurs() {
            Single USDKurs = 0;
            int i0, i1;
            string str;
            string[] Zeilen = File.ReadAllLines(Helpers.GlobalRef.g_Ein.myDataPfad + @"MyDepot\Daten\USD.html");
            string Zeile = "";
            int nz = -1;
            if (Zeilen.Length == 0)
                return USDKurs;
            Zeile = Zeilen[++nz];
            while (nz < Zeilen.Length) {
                Zeile = Zeilen[nz];
                i0 = Zeile.IndexOf("title=");
                if (i0 != -1) {
                    i0 = Zeile.IndexOf("Euro");
                    if (i0 != -1) {
                        //Debug.WriteLine("Zeile: {0}", Zeile);
                        Zeile = Zeilen[++nz];                       // nächste Zeile
                        i1 = Zeile.IndexOf("EUR/USD");
                        if (i1 > 0) {
                            nz++;
                            Zeile = Zeilen[++nz];                       // übernächste Zeile
                            i1 = Zeile.IndexOf(",");
                            str = Zeile.Substring(i1 - 1, 6);
                            USDKurs = Convert.ToSingle(str);
                            return USDKurs;
                            }
                        } else {
                        //MessageBox.Show("HoleUSDKurs-Encoding-Fehler.");
                        }
                    }
                nz++;
                if (nz >= Zeilen.Length)
                    return USDKurs;
                Zeile = Zeilen[nz];
                }
            return USDKurs;
            }
        private Single HoleSGDKurs() {
            Single SGDKurs = 0;
            int i1, i2, i3;
            string str;
            string Zeile = File.ReadAllText(Helpers.GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten\SGD.html");
            if (Zeile.Length == 0)
                return SGDKurs;
            i1 = Zeile.IndexOf(@"""kurs""");
            if (i1 > 0) {
                i2 = Zeile.IndexOf(">", i1 + 12);
                i3 = Zeile.IndexOf("<", i2);
                str = Zeile.Substring(i2 + 1, i3 - i2 - 1);
                SGDKurs = Convert.ToSingle(str);
                } else
                MessageBox.Show("HoleSGDKurs-Encoding-Fehler.");
            return SGDKurs;
            }
        private int FindRowIndex(DataGridRow row) {
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);
            return index;
            }
        private void GridWertpapiere_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridCell cell1 = dep as DataGridCell;
            DataGridRow row1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            row1 = dep as DataGridRow;
            DataGridCell cell2 = null;
            string isi = null;
            string nam = null;
            int nro = -1;
            int nwp = -1;
            if (row1 != null) {
                try {
                    isi = ((Wertpapier)(row1.Item)).ISIN;
                    nam = ((Wertpapier)(row1.Item)).Name;
                    } catch (Exception) {
                    //MessageBox.Show("gridWertpapiere_PreviewMouseDown() Fehler: " + ex);
                    return;
                    }
                nro = FindRowIndex(row1);
                int lfd = -1;
                foreach (Wertpapier wp in DgBanken._wertpapiere) {
                    ++lfd;
                    if (wp.ISIN == isi) {
                        nwp = lfd;
                        break;
                        }
                    }
                cell2 = GetCell(nro, 0);
                if (e.RightButton == MouseButtonState.Pressed) {
                    GridKlick gk = new GridKlick(this, isi, nro, nwp);
                    gk.ShowDialog();
                    } else if (e.LeftButton == MouseButtonState.Pressed) {
                    if (_isGroup) {
                        _isGroup = false;
                        return;
                        }
                    if (row1.DetailsVisibility == Visibility.Collapsed) {
                        row1.DetailsVisibility = Visibility.Visible;
                        } else {
                        row1.DetailsVisibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        private DataGridCell GetCell(int row, int column) {
            DataGridRow rowContainer = GetRow(row);
            if (rowContainer != null) {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null) {
                    dgWertpapiere.ScrollIntoView(rowContainer, dgWertpapiere.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    }
                return cell;
                }
            return null;
            }
        private DataGridRow GetRow(int index) {
            DataGridRow row = (DataGridRow)dgWertpapiere.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null) {
                dgWertpapiere.UpdateLayout();
                dgWertpapiere.ScrollIntoView(dgWertpapiere.Items[index]);
                row = (DataGridRow)dgWertpapiere.ItemContainerGenerator.ContainerFromIndex(index);
                }
            return row;
            }
        private static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                    }
                if (child != null) {
                    break;
                    }
                }
            return child;
            }
        private void GridWertpapiere_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e) {
            //Detail.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));           
            }
        private void AlsTextRestore_Click(object sender, RoutedEventArgs e) {

            }
        private void AlsTextSpeichern_Click(object sender, RoutedEventArgs e) {

            }
        private void Einstellungen_Click(object sender, RoutedEventArgs e) {
            EinstellungenView ein = new EinstellungenView();
            ein.ShowDialog();
            }
        private void _Beenden_Click(object sender, RoutedEventArgs e) {
            App.Current.Shutdown();
            }
        private void Zahlungen_Click(object sender, RoutedEventArgs e) {
            Zahlungen zah = new Zahlungen(this);
            zah.ShowDialog();
            }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            string s = Convert.ToString(DateTime.Now).Trim();
            string filename = Helpers.GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten\PortFol_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
            string obok = DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (obok != null) {
                MessageBox.Show("MeineFinanzen HauptFenster.xaml.cs HauptFenster() Fehler DatasetSichernInXlm() in WindowClose() DataSetAdmin: " + obok);
                this.Close();
                }
            GlobalRef.g_Ein.SerializeWriteEinstellungen(GlobalRef.g_Ein.strEinstellungen, GlobalRef.g_Ein);
            Properties.Settings.Default.Save();
            App.Current.Shutdown();
            }
        private void InnereDatagrid_PreviewMouseDown(object sender, MouseEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return; 
            DataGridRow row1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            row1 = dep as DataGridRow;
            string xxx = row1.Item.ToString();
            if (xxx == "{NewItemPlaceholder}")
                return;
            //Title = ((Model.BankKonto)(row1.Item)).KontoName 8;
            string kontoArt = ((Model.BankKonten)(row1.Item)).KontoArt8;
            if (kontoArt.Contains("Portfolio")) {
                AlleTabsHidden();
                tabControl1.SelectedItem = tabWertpapiere;
                tabWertpapiere.Visibility = Visibility.Visible;
                TabWertpap();
                dgWertpapiere.UpdateLayout();
                } else if (kontoArt.Contains("Giro")) {
                AlleTabsHidden();
                tabControl1.SelectedItem = tabKontoumsätze;
                tabKontoumsätze.Visibility = Visibility.Visible;
                ViewModel.TabKontoumsätze tabkto = new ViewModel.TabKontoumsätze(this);
                } else if (kontoArt.Contains("CreditCard")) {

                } else
                MessageBox.Show("InnereDatagrid_PreviewMouseDown() Fehler: Unbekannte KontoArt : " + kontoArt);
            }
        internal void OpenLogFile() {
            swLog = new StreamWriter(Helpers.GlobalRef.g_Ein.myDepotPfad + @"\Log\logKontoUmsätzeHolen.txt");
            swLog.WriteLine("Start KontoumsätzeHolen(): ---------------------------------------- " + DateTime.Now);
            swLog.Flush();
            }
        private void ÄnderungDurchClick() {
            TextBox1.Clear();
            TextBox1.AppendText(Environment.NewLine + "Start Kategorien anzeigen.");
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
                } catch (Exception ex) {
                MessageBox.Show("Fehler: " + ex);
                }
            }
        private void BtTest_Click(object sender, RoutedEventArgs e) {
            //MeineFinanzen.Helpers.Extended.DiesUndDas.test7777(GridTabitemKontoumsatz, 1, 0);
            ÄnderungDurchClick();
            //GridFilterKategorienAnzeige.Height = 20;
            }
        private void CmbSelectionChangedBetragVon(object sender, SelectionChangedEventArgs e) {

            }
        private void Initialized_cbBetragVon(object sender, EventArgs e) {

            }
        private void CmbSelectionChangedBetragBis(object sender, SelectionChangedEventArgs e) {

            }
        private void Initialized_cbBetragBis(object sender, EventArgs e) {

            }
        private void Initialized_GridFilterKategorienAnzeige(object sender, EventArgs e) {
            GridFilterKategorienAnzeige.Height = 100;
            Grid grid = sender as Grid;
            //Debug.WriteLine("{0} {1}", grid.Name, grid.ActualHeight);
            }
        private void CbFilter2_Loaded(object sender, RoutedEventArgs e) {
            cbFilter2.Items.Clear();
            cbFilter2.Text = "InfoXX";
            cbFilter2.Items.Add("Nicht gesetzt");
            cbFilter2.Items.Add("Datum von/bis");
            cbFilter2.Items.Add("Aktueller Monat(August)");
            cbFilter2.Items.Add("Letzter Monat(juli)");
            cbFilter2.Items.Add("Aktuelles Jahr(2016)");
            cbFilter2.Items.Add("Letztes Jahr(2015)");
            cbFilter2.Items.Add("Aktuelles Quartal(3)");
            cbFilter2.Items.Add("Letztes Quartal(2)");
            cbFilter2.SelectedIndex = 0;
            }
        private void CmbSelectionChangedFilter2(object sender, SelectionChangedEventArgs e) {
            if (cbFilter2 == null || cbFilter2.SelectedItem == null)
                return;
            string str = cbFilter2.SelectedItem.ToString();
            //Debug.WriteLine("{0}", str);      // Aktuelles Jahr(2015)
            }
        private void OnOpened(object sender, RoutedEventArgs e) { }
        private void OnClosed(object sender, RoutedEventArgs e) { }
        /* private void dgBankenÜbersicht_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e) {
           This event occurs one time for each row to which a new row details template is applied.
                A new details template is applied to a row in one of the following cases:
                The row DetailsTemplate property value changes.
                The row DetailsTemplate property value is null and the RowDetailsTemplate property value changes.  
            // this.selectedOrder.ProdGrid = (DataGrid)e.DetailsElement.FindName("JobProdGrid");
        } */
        /* private void dgBankenÜbersicht_LoadingRow(object sender, DataGridRowEventArgs e) {
               string str = e.Row.Item.ToString();
               Console.WriteLine("dgBankenÜbersicht_LoadingRow() Row.Item: {0}", str);
               if ((str == "{NewItemPlaceholder}") || (str == "MeineFinanzen.Model.BankÜbersicht")) {
                   DataGridRow r = e.Row;
                   Console.WriteLine("r.Data-Context: {0}", r.Data-Context);
                   for (int i = 0; i < dgBankenÜbersicht.Items.Count - 1; i++) {
                       DataGridRow row = (DataGridRow)dgBankenÜbersicht.ItemContainerGenerator.ContainerFromIndex(i);
                       if (row == null)
                           continue;
                       Model.BankÜbersicht dataitem1 = (Model.BankÜbersicht)row.Data-Context;                                
                       if (dataitem1.OCBankKonten.Count == 0) {
                           Console.WriteLine("machRowDetails(1) Visibility.Collapsed Bankname:{0} Konten:{1}",
                               dataitem1.BankName7, dataitem1.OCBankKonten.Count);
                           dgFinanzübersicht.SetDetailsVisibilityForItem(dataitem1, Visibility.Collapsed);
                       }
                       else {
                           Console.WriteLine("machRowDetails(2) Visibility.Visible Bankname:{0} Konten:{1}",
                               dataitem1.BankName7, dataitem1.OCBankKonten.Count);
                           dgFinanzübersicht.SetDetailsVisibilityForItem(dataitem1, Visibility.Visible);
                       }                   
                   }
               }
           } */
        private void DgBankenÜbersicht_SelectionChanged(object sender, SelectionChangedEventArgs e) {  }
        private void DgBankenÜbersicht_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridRow row1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            row1 = dep as DataGridRow;
            //var strxxx = (Model.BankKonten)row1.Item;
            //Console.WriteLine("BankName: " + strxxx.KontoName 8);
            string strType = row1.Item.GetType().ToString();            // ist 'MeineFinanzen.Model.BankKonto'.
            try {
                if (strType == "MeineFinanzen.Model.BankKonto") {
                    //Title = ((Model.BankKonto)(row1.Item)).KontoName 8;
                    }
                //Title = ((Model.BankÜbersicht)(row1.Item)).BankName7;   // ist BankKonto. Kann nicht in BankÜbersicht umgew werden.
                AlleTabsHidden();
                tabControl1.SelectedItem = tabFinanzübersicht;
                //tabFinanzübersicht.Visibility = Visibility.Visible;
                //GridGesamtvermögen.Visibility = Visibility.Visible;
                TabFinanzen();
                //dgFinanzübersicht.UpdateLayout();
                } catch (Exception ex) {
                MessageBox.Show("dgBankenÜbersicht_PreviewMouseDown Fehler: " + ex);
                }
            }
        /*  private void dgFinanzÜbersicht_LoadingRow(object sender, DataGridRowEventArgs e) {
              int ind = e.Row.GetIndex();
              string str = e.Row.Item.ToString();
              if (str == "{NewItemPlaceholder}")
                  return;
              DataGridRow row = (DataGridRow)dgFinanzübersicht.ItemContainerGenerator.ContainerFromIndex(ind);
              if (row == null)
                  return;            
              Model.BankÜbersicht item = (Model.BankÜbersicht)row.Data-Context;            
              Console.Write("===>(ind:{0}) {1,-40} {2,-30}", ind, str, item.BankName7);
              if (item.OCBankKonten.Count == 0) {
                  dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Collapsed);
                  Console.Write(" {0,-20}=Collapsed", item.BankName7);
                  row.DetailsVisibility = Visibility.Collapsed;
                  //e.Row.Foreground = new SolidColorBrush(Colors.DarkTurquoise);
                  //e.Row.Background = new SolidColorBrush(Colors.DeepPink);
              }
              else {
                  dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Visible);
                  Console.Write(" Visible");
              }
              Console.WriteLine(); 
          }   
           private void InnereDatagridFinanzen2_LoadingRow(object sender, DataGridRowEventArgs e) {
                  int ind = e.Row.GetIndex();
                  string str = e.Row.Item.ToString();
                  if (str == "{NewItemPlaceholder}")
                      return;
                  DataGridRow row = (DataGridRow)dgFinanzübersicht.ItemContainerGenerator.ContainerFromIndex(ind);
                  if (row == null)
                      return;            
                  Model.BankÜbersicht item = (Model.BankÜbersicht)row.Data-Context;
                  Console.Write("--->(ind:{0}) {1,-40} {2,-30}", ind, str, item.BankName7);            
                  if (item.OCBankKonten.Count == 0) {
                      dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Collapsed);
                      Console.Write(" {0,-20}=Collapsed", item.BankName7);
                      e.Row.Background = new SolidColorBrush(Colors.DeepPink);
                  }
                  else {
                      dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Visible);
                      foreach (Model.BankKonto ko in item.OCBankKonten) {                    
                          if (ko.OCUmsätze.Count > 0) {
                              dgFinanzübersicht.SetDetailsVisibilityForItem(ko, Visibility.Visible);
                              Console.Write(" {0,-20}=Visible   Ums={1}", ko.KontoName 8, ko.OCUmsätze.Count);
                          }
                          else {
                              dgFinanzübersicht.SetDetailsVisibilityForItem(ko, Visibility.Collapsed);
                              Console.Write(" {0,-20}=Collapsed Ums=null", ko.KontoName 8);
                          }
                          //e.Row.Background = new SolidColorBrush(Colors.Olive);
                          //e.Row.DetailsVisibility = Visibility.Collapsed;
                      }
                  }
                  Console.WriteLine(); 
          } */
        /* private void InnereDatagridBanken3_LoadingRow(object sender, DataGridRowEventArgs e) {
            int ind = e.Row.GetIndex();
            string str = e.Row.Item.ToString();
            if (str == "{NewItemPlaceholder}")
                return;
            DataGridRow row = (DataGridRow)dgFinanzübersicht.ItemContainerGenerator.ContainerFromIndex(ind);
            if (row == null)
                return;
            str = row.Item.ToString();
            if (str == "{NewItemPlaceholder}")
                return;            
            Model.BankÜbersicht item = (Model.BankÜbersicht)row.Data-Context;
            //Console.Write("...>(ind:{0}) {1,-40} {2,-30}", ind, str, item.BankName7);           
            if (item.OCBankKonten.Count == 0) {
                dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Collapsed);
                //Console.Write(" {0,-20}=Collapsed", item.BankName7);
            }
            else {
                dgFinanzübersicht.SetDetailsVisibilityForItem(item, Visibility.Visible);
                foreach (Model.BankKonto ko in item.OCBankKonten) {
                    if (ko.OCUmsätze.Count > 0) {
                        dgFinanzübersicht.SetDetailsVisibilityForItem(ko, Visibility.Visible);
                        //Console.Write(" {0,-20}=Visible   Ums:{1}", ko.KontoName 8, ko.OCUmsätze.Count);
                    }
                    else {
                        dgFinanzübersicht.SetDetailsVisibilityForItem(ko, Visibility.Collapsed);
                        //Console.Write(" {0,-20}=Collapsed Ums:null", ko.KontoName 8);
                    }
                }
            }
            //Console.WriteLine();
        } */
        /* private void dgFinanzÜbersicht_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e) {
             int ind = e.Row.GetIndex();
             DataGridRow dgrow = e.Row as DataGridRow;
             Model.BankÜbersicht item = (Model.BankÜbersicht)dgrow.Data-Context;            
             //Console.WriteLine("+++>(ind:{0} Bank:{1,-40} Konten:{2}   dgFinanzÜbersicht_LoadingRowDetails", ind, item.BankName7, item.OCBankKonten.Count);
         } */
        private void DgFinanzÜbersicht_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            }
        private void DgFinanzÜbersicht_Loaded(object sender, RoutedEventArgs e) {
            }
        private void Jahreswechsel_Click(object sender, RoutedEventArgs e) {
            JahreswechselView jw = new JahreswechselView();
            jw.ShowDialog();
            }
        private void HistorieGesamt_Click(object sender, RoutedEventArgs e) {
            HistorieGesamt hg = new HistorieGesamt(this);
            hg.ShowDialog();
            }
        private void TestWPAktualisieren_Click(Object sender, RoutedEventArgs e) {
            foreach (Wertpapier wp in DgBanken._wertpapiere) {
                if (!wp.Name.Contains("#")) {
                    wp.Name += "#";
                    break;
                    }
                }
            }
        private void TextWPXlöschen_Click(Object sender, RoutedEventArgs e) {
            DataTable dtPortFol = new DataTable();
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];
            foreach (DataRow dtrow in dtPortFol.Rows) {
                dtrow["WPXZeitX"] = "0";
                dtrow["WPXZeitY"] = "0";
                dtrow["WPXKursX"] = "0";
                dtrow["WPXKursY"] = "0";
                }
            DataSetAdmin.dtPortFol = dtPortFol;
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            Close();
            }
        private void KontenaufstellungHBCI4j_Click(Object sender, RoutedEventArgs e) {
            Console.WriteLine("---- --10-- HauptFenster _dgBanken.ko4js.Count {0}", DgBanken.ko4js.Count);
            KontenaufstellungHBCI4j koauf = new KontenaufstellungHBCI4j();
            koauf.dgKontenaufstellung.ItemsSource = null;
            koauf.dgKontenaufstellung.ItemsSource = DgBanken.ko4js;
            koauf.dgKontenaufstellung.EnableRowVirtualization = false;
            koauf.Show();
            }
        private void URLsVerwalten_Click(object sender, RoutedEventArgs e) {
            URLsVerwalten _urlsverwalten = new URLsVerwalten();
            _urlsverwalten.Show();             // Nicht Modal, kehrt zurück.
        }
        /* private void InnereDatagridBanken3_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e) {
DataGridRow dgrow = e.Row as DataGridRow;
Model.Umsatz ums = (Model.Umsatz)dgrow.Data-Context;
Console.WriteLine("InnereDatagridBanken3_RowDetailsVisibilityChanged:{0} Name1:{1}", ums.Kontonummer, ums.Name1);
}
private void InnereDatagridFinanzen2_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e) {
DataGridRow dgrow = e.Row as DataGridRow;
Model.BankKonto kon = (Model.BankKonto)dgrow.Data-Context;
//Console.WriteLine("InnereDatagridBanken2_RowDetailsVisibilityChanged:KontoName 8:{0} KontoNr8:{1}", kon.KontoName 8, kon.KontoNr8);
}
private void dgFinanzübersicht1_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e) {
DataGridRow dgrow = e.Row as DataGridRow;
Model.BankÜbersicht ban = (Model.BankÜbersicht)dgrow.Data-Context;
//Console.WriteLine("InnereDatagridBanken1_RowDetailsVisibilityChanged:BankName7:{0} BankValue7:{1}", ban.BankName7, ban.BankValue7);
} */
        /* private void treeView1_SelectedItemChanged(object sender,
RoutedPropertyChangedEventArgs<object> e)
{
TreeViewItem itemOld = e.OldValue as TreeViewItem;
TreeViewItem itemNew = e.NewValue as TreeViewItem;
if (e.OldValue != null)
{
string txtOld = itemOld.Header.ToString();
string txtNew = itemNew.Header.ToString();
}
}

private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
{
TreeViewItem tvItem = sender as TreeViewItem;
if (tvItem != null)
{
if (e.RightButton == MouseButtonState.Pressed)
{
tvItem.IsExpanded = true;
Expand(tvItem);
}
}
}

private void Expand(TreeViewItem item)
{
foreach (TreeViewItem node in item.Items)
{
node.IsExpanded = true;
if (node.Items.Count > 0) Expand(node);
}
}
private void btnAdd_Click(object sender, RoutedEventArgs e)
{
//if (treeView1.Items.Count == 0 || chkBox1.IsChecked == true) {
//  treeView1.Items.Add(new TreeViewItem { Header = txtNewItem.Text });
return;
} */
        //TreeViewItem selectedItem = (TreeViewItem)treeView1.SelectedItem;
        //if (selectedItem != null)
        //  selectedItem.Items.Add(new TreeViewItem { Header = txtNewItem.Text });
        /*
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem del = treeKategorie.SelectedItem as TreeViewItem;
            if (del != null)
            {
                TreeViewItem parent = del.Parent as TreeViewItem;
                if (parent != null)
                    parent.Items.Remove(del);
                else
                    treeKategorie.Items.Remove(del);
            }
        }
         * */
        /*
        private void treeKategorie_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
           Console.WriteLine(string.Format("e.NewValue.Header  = {0}", (e.NewValue as TreeViewItem).Header).ToString());  // z.B. Arbeit                               
            string strOld = "nix";
            string strNew = "";
            if (e.OldValue != null)
                strOld = (e.OldValue as TreeViewItem).Header.ToString();            
            strNew = (e.NewValue as TreeViewItem).Header.ToString();
           Console.WriteLine("Old = {0} New = {1}", strOld, strNew);
            string KatName = ((TreeViewItem)e.NewValue).Header.ToString(); */
        /* try
        {
            if (e.NewValue != null)
            {                    
                KatName = ((MeineFinanzen.Kategorie)e.NewValue).KategorieName.ToString();
                object zweiteKategorien = ((MeineFinanzen.Kategorie)e.NewValue).ToString();
                object katname = ((MeineFinanzen.Kategorie)e.NewValue).KategorieName;
                object obj2 = zweiteKategorien.GetType();
               Console.WriteLine("kategorie mit ZweiteKategorien: {0} {1}", katname, obj2);
            }
        }
        catch
        {
            try
            {
                KatName = ((MeineFinanzen.KategorieType)e.NewValue).KatType.ToString();
                object kategorien = ((MeineFinanzen.KategorieType)e.NewValue).Kategorien;
                object kattyp = ((MeineFinanzen.KategorieType)e.NewValue).KatType;
                object obj2 = kategorien.GetType();
               Console.WriteLine("kategorietype mit Kategorien: {0} {1}", kattyp, obj2);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Fehler2 in treeKategorie_SelectedItemChanged(): " + ex2);
            }
        }
            object obj = e.Source.ToString();
           Console.WriteLine("e.Source.ToString(): {0,-14}", obj);           
            object selItem = treeKategorie.SelectedItem;
            string selItemStr = selItem.GetType().Name.ToLower();
            switch (selItemStr)
            {
                case "kategorietype":
                   Console.WriteLine("Level1: {0,-14} / {1}", selItemStr, KatName);
                    break;
                case "kategorie":
                   Console.WriteLine("Level2: {0,-14} / {1}", selItemStr, KatName);
                    break;
                case "treeviewitem":
                   Console.WriteLine("selItemStr: {0,-14} / {1}", selItemStr, KatName);
                    break;
                default:
                    break;
            }
        }        
        private void treeKategorie_SelectedValuePath(object sender, RoutedPropertyChangedEventArgs<object> e)
        {    }
        private void cmbSelectionChangedDepot()
        { }       
        private void CheckBox_Click(object sender, RoutedEventArgs e)       
        //{ OnCheck(); }
        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        { OnCheck();       }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {        }
        private void CheckBox_Indeterminate(object sender, RoutedEventArgs e)
        {        }
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //private void treeKategorie_Initialized(object sender, EventArgs e)
        //{          //OnCheck();       }
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        private void treeKategorie_SourceUpdated(object sender, DataTransferEventArgs e)
        { }
        private string GetIconFolder()
        {
            string execName = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string currentFolder = System.IO.Path.GetDirectoryName(execName);
            string icons = System.IO.Path.Combine(currentFolder, "icons");
            return icons;       // C :\U sers\LuKe\Documents\Visual Studio 2010\Projects\MeineFinanzen\bin\Release\icons
        }        
        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            //string str = GetIconFolder();
            treeKategorie.Items.Clear();
            treeKategorie.Items.Add(TreeView_CreateKategorieItem());        // später bei bedarf TreeViewItem_Expanded
            ButtonExpandNeu();
        }
        TreeViewItem TreeView_CreateKategorieItem()
        {
            TreeViewItem kategorie1 = new TreeViewItem { Header = "Kategorien", IsExpanded = true };
            try
            {                
                foreach (var kat in _KategorieTypes)
                {
                    TreeViewItem katItem1 = new TreeViewItem();
                    katItem1.Header = String.Format("{0}", kat.KatType);
                    if (kat.Kategorien.Count > 0)
                    {
                        foreach (var kat2 in kat.Kategorien)
                        {
                            TreeViewItem katItem2 = new TreeViewItem();
                            katItem2.Header = String.Format("{0}", kat2.KategorieName);
                            //katItem2.Expanded += TreeViewItem_Expanded;
                            //katItem1.Items.Add(katItem2);
                        }

                        katItem1.Items.Add(null);

                    }
                    katItem1.Tag = kat.KatType;
                    katItem1.Expanded += TreeViewItem_Expanded;
                    kategorie1.Items.Add(katItem1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex);
            }
            return kategorie1;
        }
        void Expandieren(TreeViewItem rootItem)
        {
            try
            {
                if (rootItem.Items.Count == 1 && rootItem.Items[0] == null)
                {
                    rootItem.Items.Clear();
                    KategorieType kattyp = _KategorieTypes.Find(delegate(KategorieType bk) // KatType 'Verkehrsmittel' hat 9 Kategorien.
                    {
                        return bk.KatType.ToString() == rootItem.Tag.ToString();
                    });
                    if (kattyp == null)
                    {
                        foreach (KategorieType katt in _KategorieTypes)
                        {
                            //Debug.Write("\r{0,-16}", katt.KatType);
                            foreach (Kategorie kat in katt.Kategorien)
                            {
                                //Debug.Write("\r    {0, -16}", kat.KategorieName);
                                //if (rootItem.Tag.ToString() == kat.KategorieName)
                                {
                                    foreach (Kategorie kat2 in kat.ZweiteKategorien)
                                    {
                                        //Debug.Write("\r        {0}", kat2.KategorieName);
                                        TreeViewItem subItem = new TreeViewItem();
                                        //subItem.Name = kat2.KategorieName;
                                        subItem.Header = kat2.KategorieName;
                                        subItem.Tag = kat2.KategorieName;
                                        subItem.Expanded += TreeViewItem_Expanded;
                                        rootItem.Items.Add(subItem);
                                    }
                                }
                            }
                        }
                        return;
                    }
                    foreach (var kat in kattyp.Kategorien)
                    {
                        TreeViewItem subItem = new TreeViewItem();
                        subItem.Header = kat.KategorieName;
                        subItem.Tag = kat.KategorieName;
                        if (kat.ZweiteKategorien.Count > 0)
                            subItem.Items.Add(null);
                        subItem.Expanded += TreeViewItem_Expanded;
                        rootItem.Items.Add(subItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex);
            }
        }
        bool Expanded = false;
        // The event subscription method (for a button click)
        private void ButtonExpandNeu()
        {
            Expanded = true;// !Expanded;
            Style Style = new Style
            {
                TargetType = typeof(TreeViewItem)
            };

            Style.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, Expanded));
            treeKategorie.ItemContainerStyle = Style;
        } 
        void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem rootItem = (TreeViewItem)sender;
            Expandieren(rootItem);
        }
        private void SaveKategorieXml_Click(object sender, RoutedEventArgs e)
        {
            var FileNameXml = GlobalRef.g_Ein.myDataPfad + @ "MyDepot\Settings\Settings-" + "Kategorien" + ".xml";
            //treeKategorie.Items.xpand
            exportToXml(treeKategorie, FileNameXml);
        }
        public void exportToXml(TreeView tv, string filename)
        {           
            sr = new StreamWriter(filename, false, System.Text.Encoding.UTF8);            
            sr.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");            
            sr.WriteLine("<ROOT>");
            foreach (TreeViewItem node in tv.Items)
            {                
                saveNode(node.Items.OfType<TreeViewItem>().ToArray());                
            }           
            sr.WriteLine("</ROOT>");
            sr.Close();
        }
        private void saveNode(TreeViewItem[] tnc)
        {
           Console.WriteLine("saveNode tnc.Length: {0} ", tnc.Length);
            foreach (TreeViewItem node in tnc)
            {
                //If we have child nodes, we'll write a parent node, then iterrate through the children
                if (node.Items.Count > 0)
                {
                   Console.WriteLine("node.Items.Count: {0} Header: {1}", node.Items.Count, node.Header);
                    sr.WriteLine("<" + node.Header + ">");                   
                    saveNode(node.Items.OfType<TreeViewItem>().ToArray());
                    sr.WriteLine("</" + node.Header + ">");
                }
                else //No child nodes, so we just write the text
                    sr.WriteLine(node.Header);
            }
        }  */
    }
    [ValueConversion(typeof(Boolean), typeof(String))]
    public class CompleteConverter : IValueConverter {
        // This converter changes the value of a XTas ksX Complete status from true/false to a string value of
        // "Complete"/"Active" for use in the row group header.
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool complete = (bool)value;
            if (complete)
                return "Complete";
            else
                return "Active";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string strComplete = (string)value;
            if (strComplete == "Complete")
                return true;
            else
                return false;
        }
    }
    public class IconPathConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value is string && ((string)value).Length == 0)
                return (ImageSource)null;
            if (!File.Exists((string)value)) {
                return (ImageSource)null;
                }
            return value;
            }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
            }
        }
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateConverterShort : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime date = (DateTime)value;
            return date.ToShortDateString();
            }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime)) {
                return resultDateTime;
                }
            return DependencyProperty.UnsetValue;
            }
        }
    /*  public class CustomItem
     {
         public string Name { get; set; }
         public bool IstChecked { get; set; }
         public ObservableCollection<CustomItem> Children { get; set; }
     } */
    /* public _advancedFormat = Visibility.Visible; //(whatever you start with)

 public Visibility AdvancedFormat
     {
         get { return _advancedFormat; }
         set
         {
             _advancedFormat = value;
             //raise property changed here
         }
     } */
    public class NameToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int input;
            try {
                //DataGridCell dgc = (DataGridCell)value;
                var dgc2 = value;
                //Debug.WriteLine("{0}", dgc2);
                //                System.Data.DataRowView rowView = (System.Data.DataRowView)dgc2.Data  Context;
                //                input = (int)rowView.Row.ItemArray[dgc2.Column.DisplayIndex];
                if (dgc2.ToString() == "ING-DiBa")
                    input = 66;
                else
                    input = 99;
                } catch (InvalidCastException e) {
                MessageBox.Show("in NameToBrushConverter Fehler: " + e);
                return DependencyProperty.UnsetValue;
                }
            switch (input) {
                case 0:
                    return Brushes.Red;
                case 1:
                    return Brushes.White;
                case 2:
                    return Brushes.Blue;
                default:
                    return DependencyProperty.UnsetValue;
                }
            /*
            string input2 = value as string;
            switch (input2)
            {
                case "John":
                    return Brushes.LightGreen;
                default:
                    return DependencyProperty.UnsetValue;             
            } * */
            }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
            }
        }
    public class BoolToGridRowHeightConverter3 : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString() == "{NewItemPlaceholder}")
                return 0;
            var dg = (Model.BankKonten)value;
            if (dg.OCUmsätze == null)
                return null;
            //Console.WriteLine("\tBoolToGridRowHeightConverter3 {0,-24} {1,-20} {2,-20} {3,16} {4} OCUmsätze.Count:{5}",
            //    dg.KontoName 8, dg.KontoArt8, dg.KontoNr8, dg.KontoValue8, dg.KontoDatum8, dg.OCUmsätze.Count);
            if (dg.OCUmsätze.Count > 0) {
                int ro = dg.OCUmsätze.Count;
                if (ro > 10)
                    return new GridLength(10 * 16);
                else
                    return new GridLength(1, GridUnitType.Star);        // new GridLength(ro * 16); 
                } else
                return new GridLength(0);
            }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {    // Don't need any convert back
            return null;
            }
        }
    public class BoolToGridRowHeightConverter2 : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString() == "{NewItemPlaceholder}")
                return null;
            var dg = (Model.BankÜbersicht)value;
            Console.WriteLine("{0,-20} {1,16} {2} OCBankKonten.Count:{3}",
                dg.BankName7, dg.BankValue7, dg.Datum7, dg.OCBankKonten.Count);
            if (dg.OCBankKonten.Count > 0) {
                int ro = dg.OCBankKonten.Count;
                return new GridLength(1, GridUnitType.Star);   //new GridLength(ro * 110);
                } else
                return new GridLength(0);   //new GridLength(1, GridUnitType.Star);    // new GridLength(0);
            }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {    // Don't need any convert back
            return null;
            }
        }
    public class DateConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString() == String.Empty)
                return null;
            if (value.ToString().StartsWith("01.01.1980"))
                return Brushes.Transparent;
            if (DateTime.Parse(value.ToString()) < (System.Convert.ToDateTime(parameter)))
                return Brushes.Blue;
            else
                return Brushes.Black;
            }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
            }
        }
    public class DateConvertFi : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;
            if (value.ToString() == String.Empty)
                return null;
            if (value.ToString().StartsWith("01.01.1980"))
                return Brushes.Transparent;
            //if (DateTime.Parse(value.ToString()) < (System.Convert.ToDateTime(parameter)))
            //    return Brushes.Blue;
            //else
            return Brushes.Black;
            }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
            }
        }
    public class SignedConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;
            if (value.ToString() == String.Empty)
                return null;
            double val = 0;
            try {
                val = double.Parse(value.ToString());
                } catch (FormatException) { }
            if (val == 0)
                return Brushes.Transparent;          //Brushes.Pink;
            /*
            if (parameter == "AktKurs")
            {
                double kursVorher = TabWertpapiere.wpVorher[0].AktKurs;
                if (val < kursVorher)
                    return Brushes.Red;
            }
            */
            return (val < 0) ? Brushes.Red : Brushes.Black;
            }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
            }
        }
    public class SignedConvertFi : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString() == String.Empty)
                return null;
            double val = 0;
            try {
                val = double.Parse(value.ToString());
                } catch (FormatException) { }
            if (val == 0)
                return Brushes.Transparent;
            /*
            if (parameter == "AktKurs")
            {
                double kursVorher = TabWertpapiere.wpVorher[0].AktKurs;
                if (val < kursVorher)
                    return Brushes.Red;
            }
            */
            if (val < 0)
                return Brushes.Red;
            else
                //{
                //    if (this.xxx == 0)
                //        return Brushes.Black;
                //    else
                return Brushes.Black;
            // }
            }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotSupportedException();
            }
        }
    //[ValueConversion(typeof(Boolean), typeof(String))]
    public class ISINConverter : IValueConverter {
        // This converter changes the value of a Wertpapiere ISIN status from true/false to a string value of
        // "ISIN"/"Active" for use in the row group header.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string ISIN = (string)value;
            if (ISIN != "")
                return "ISIN";
            else
                return "Active";
            }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string strISIN = (string)value;
            if (strISIN == "ISIN")
                return true;
            else
                return false;
            }
        }
    /* class VacationSpots : ObservableCollection<string>
    {
        public VacationSpots()
        {
            Add("Spain");
            Add("France");
            Add("Peru");
            Add("Mexico");
            Add("Italy");
        }
    } */
    }