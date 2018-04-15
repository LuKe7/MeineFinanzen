// 14.04.2018 VorgabeInt2.xaml.cs
// VorgabeParameter für KontenSynchronisierung über Internet 2.Version(Über Textsuche in Elementen...).
// Die Wertpapiere suchen sich ihren Parametersatz selbst. Über Url1 + Url2 !!!!
// Erstellen dieser Sätze, falls sie nicht vorhanden sind.
// Ergänzer dieser Sätze manuell.
/*  <tblVorgabeInt2 diffgr:id="tblVorgabeInt21" msdata:rowOrder="0" diffgr:hasChanges="inserted">
      <Url1>https://www.finanzen.net/</Url1>
      <Url2>fonds</Url2>
      <Boxanfang>row quotebox</Boxanfang>
      <Ausschluss1>Kurse</Ausschluss1>
      <Wert1>Kursdatum</Wert1>
      <Wert2>Kurszeit</Wert2>
      <Wert3>Kurs</Wert3>
      <Wert4 />
    </tblVorgabeInt2>
    <tblVorgabeInt2 diffgr:id="tblVorgabeInt22" msdata:rowOrder="1" diffgr:hasChanges="inserted">
      <Url1>https://www.finanzen.net/</Url1>
      <Url2>etf</Url2>
      <Boxanfang />
      <Ausschluss1 />
      <Wert1 />
      <Wert2 />
      <Wert3 />
      <Wert4 />
    </tblVorgabeInt2>
    <tblVorgabeInt2 diffgr:id="tblVorgabeInt23" msdata:rowOrder="2" diffgr:hasChanges="inserted">
      <Url1>https://www.finanzen.net/</Url1>
      <Url2>anleihen</Url2>
      <Boxanfang />
      <Ausschluss1 />
      <Wert1 />
      <Wert2 />
      <Wert3 />
      <Wert4 />
    </tblVorgabeInt2> */
using DataSetAdminNS;
using MeineFinanzen.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace MeineFinanzen.View {
    public partial class VorgabeInt2 : Window {
        public string strAnzeige { get; set; }
        public static List<Wertpap> liWertpaps = new List<Wertpap>();
        public CollWertpapSynchroNeu _wertpapsynchroneu = null;
        public static Model.VorgabeInt2 Vorg = new Model.VorgabeInt2();
        public static List<Model.VorgabeInt2> liVorg = new List<Model.VorgabeInt2>();
        DataTable dtPortFol = new DataTable();
        DataTable dtVorgabe = new DataTable();
        DataTable dtVorgabeLoop = new DataTable();
        public System.Windows.Controls.DataGridRow dgRow1 = null;               // Diese Zeile in dgvUrl wurde angeklickt.      
        string _Url1, _Url2, _BoxAnfang, _TxtKurse, _TxtKurszeit, _TxtKursdatum, _TxtKurs; // Diese Zeile in dgvVorgabe wurde angeklickt.
        string _ColHeaderVorgabe;                       // Diese Spalte in dgvVorgabe wurde angeklickt.
        public VorgabeInt2() {
            InitializeComponent();
            //txtAnzeige.Multiline = true;
            strAnzeige = "Hier stehen Anweisungen und so ....";
            DataContext = this;
            // Get a reference to the CollWertpapSynchro collection.
            _wertpapsynchroneu= (CollWertpapSynchroNeu)Resources["wertpapsynchron"];
            wb1.ScriptErrorsSuppressed = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
            wb1.GoHome();
            wb1.Navigate(new Uri("https://www.google.de/"));        // Löst kein wb1-DocumentCompleted aus
            VorgabeParameterBearbeiten();
        }
        public void VorgabeParameterBearbeiten() {
            liVorg.Clear();
            DateTime dt = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt == null) {
                System.Windows.Forms.MessageBox.Show("MeineFinanzen VorgabeParameterBearbeiten.xaml.cs Fehler HolenAusXml() DataSetAdmin");
                System.Windows.Forms.MessageBox.Show("MeineFinanzen Fehler!!  Dateien nicht geladen!!!!");
                Close();
            }
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // dtPortFol geholt.                
            foreach (DataRow dr in dtPortFol.Rows) {                    // _wertpapsynchroneu erstellen
                int typeid = (int)dr["WPTypeID"];
                if (typeid <= 10 || typeid >= 80)
                    continue;
                if (dr["WPISIN"].ToString().Length != 12)
                    continue;
                _wertpapsynchroneu.Add(new WertpapSynchroNeu {
                    WPVAnzahl = Convert.ToSingle(dr["WPAnzahl"]),
                    WPVName = dr["WPName"].ToString(),
                    WPVKursZeit = Convert.ToDateTime(dr["WPStand"]),
                    WPVISIN = dr["WPISIN"].ToString(),

                    WPVURL = dr["WPUrlText"].ToString(),
                    WPVKurs = Convert.ToDouble(dr["WPKurs"]),
                    WPVProzentAenderung = Convert.ToDouble(dr["WPProzentAenderung"]),
                    WPVType = Convert.ToInt32(dr["WPTypeID"]),
                    WPVSharpe = Convert.ToSingle(dr["WPSharpe"]),

                    WPVURLSharp = dr["WPUrlSharpe"].ToString(),
                    WPVColor = "1"
                });
            }
            dgvUrls.ItemsSource = _wertpapsynchroneu;
            dgvUrls.UpdateLayout();
            dtVorgabe = DataSetAdmin.dsHier.Tables["tblVorgabeInt2"];   // dtVorgabeInt2 geholt. 
            // dtVorgabe.Clear();
            dtVorgabe.PrimaryKey = new DataColumn[] { dtVorgabe.Columns["Url1"], dtVorgabe.Columns["Url2"] };
            // ---- Datensätze erstellen falls sie nicht vorhanden sind in liVorg. ----
            if (dtVorgabe.Rows.Count == 0) {
                DataRow newRow = DataSetAdmin.dtVorgabeInt2.NewRow();   // Standard Basissatz erstellen.
                newRow["Url1"] = "https://www.finanzen.net/";
                newRow["Url2"] = "fonds";
                newRow["Boxanfang"] = "row quotebox";
                newRow["Ausschluss1"] = "Kurse";
                newRow["Wert1"] = "Kursdatum";
                newRow["Wert2"] = "Kurszeit";
                newRow["Wert3"] = "Kurs";
                newRow["Wert4"] = "";
                dtVorgabe.Rows.Add(newRow);
            }
            // ---- liVorg für dgvVorgabeInt2 erstellen ----
            for (int i = dtVorgabe.Rows.Count - 1; i >= 0; i--) {
                DataRow dr = dtVorgabe.Rows[i];
                //foreach (DataRow dr in dtVorgabeLoop.Rows) {
                Vorg = new Model.VorgabeInt2 {
                    Url1 = dr["Url1"].ToString(),
                    Url2 = dr["Url2"].ToString(),
                    Boxanfang = dr["Boxanfang"].ToString(),
                    Ausschluss1 = dr["Ausschluss1"].ToString(),
                    Wert1 = dr["Wert1"].ToString(),
                    Wert2 = dr["Wert2"].ToString(),
                    Wert3 = dr["Wert3"].ToString(),
                    Wert4 = dr["Wert4"].ToString()
                };
                liVorg.Add(Vorg);
            }
            // ---- In dgvUrl Color auf 'Eingefügt' setzen, wenn kein Vogabesatz vorhanden.
            string[] splitUrls = null;
            string[] splitVorg = null;
            foreach (WertpapSynchroNeu wps in _wertpapsynchroneu) {
                splitUrls = wps.WPVURL.Split('/');
                bool gef = false;
                foreach (Model.VorgabeInt2 vg in liVorg) {
                    splitVorg = vg.Url1.Split('/');            // https leer www.finanzen.net leer                      
                    if (splitUrls[2] == splitVorg[2] && splitUrls[3] == vg.Url2) {
                        // www.finanzen.net == dto   &&        fonds == fonds
                        gef = true;
                        break;
                    }
                }
                if (!gef) {
                    // ---- Und jetzt fehlenden Satz in liVorg erstellen. ----                    
                    string[] splitUrls2 = wps.WPVURL.Split('/');
                    Vorg = new Model.VorgabeInt2 {
                        Url1 = splitUrls2[0] + "//" + splitUrls2[2] + "/",
                        Url2 = splitUrls2[3],
                        Boxanfang = "",
                        Ausschluss1 = "",
                        Wert1 = "",
                        Wert2 = "",
                        Wert3 = "",
                        Wert4 = "",
                        Vg2Color = "Eingefügt"
                    };
                    liVorg.Add(Vorg);
                    AddliVorgTodtVorgabe(liVorg, Vorg);
                }
            }
            dgvUrls.ItemsSource = null;
            dgvUrls.ItemsSource = _wertpapsynchroneu;
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();
            dgvVorgabeInt2.ItemsSource = null;
            dgvVorgabeInt2.ItemsSource = liVorg;
            dgvVorgabeInt2.EnableRowVirtualization = false;
            dgvVorgabeInt2.UpdateLayout();
        }
        public class TEST_TermineList : List<Model.VorgabeInt2> {        // Kann weg
            public void DeleteTermineForDate(DateTime dt) {
                for (int i = this.Count - 1; i >= 0; i--) {
                    if (this[i].Url1 == "xxxxx") {
                        this.RemoveAt(i);
                    }
                }
            }
        }
        private void AddliVorgTodtVorgabe(List<Model.VorgabeInt2> liVorg, Model.VorgabeInt2 Vorg) {
            DataRow newRow = DataSetAdmin.dtVorgabeInt2.NewRow();
            newRow["Url1"] = Vorg.Url1;
            newRow["Url2"] = Vorg.Url2;
            newRow["Boxanfang"] = Vorg.Boxanfang;
            newRow["Ausschluss1"] = Vorg.Ausschluss1;
            newRow["Wert1"] = Vorg.Wert1;
            newRow["Wert2"] = Vorg.Wert2;
            newRow["Wert3"] = Vorg.Wert3;
            newRow["Wert4"] = Vorg.Wert4;
            try {
                dtVorgabe.Rows.Add(newRow);
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler in AddliVorgTodtVorgabe(): " + ex);
                Console.WriteLine("Fehler in AddliVorgTodtVorgabe(): " + ex);
            }
        }
        private void UpdatedtVorgabe(string sooo_gehts_nicht) {
            foreach (Model.VorgabeInt2 livor in liVorg) {
                foreach (DataRow dtrow in dtVorgabe.Rows) {
                    try {
                        dtrow["Url1"] = livor.Url1;
                        dtrow["Url2"] = livor.Url2;
                        dtrow["Boxanfang"] = livor.Boxanfang;
                        dtrow["Ausschluss1"] = livor.Ausschluss1;
                        dtrow["Wert1"] = livor.Wert1;
                        dtrow["Wert2"] = livor.Wert2;
                        dtrow["Wert3"] = livor.Wert3;
                        dtrow["Wert4"] = livor.Wert4;
                    } catch (Exception ex) {
                        System.Windows.MessageBox.Show("Fehler in UpdatedtVorgabe(): " + ex);
                    }
                }
            }
        }
        private void Close_Window(object sender, System.ComponentModel.CancelEventArgs e) {
            DataSetAdmin.dtVorgabeInt2 = dtVorgabe;
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
        }
        private void dgvUrls_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            _ColHeaderVorgabe = null;                   // Muß noch geklickt werden.
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            System.Windows.Controls.DataGridCell cell1 = dep as System.Windows.Controls.DataGridCell;
            dgRow1 = dep as System.Windows.Controls.DataGridRow;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as System.Windows.Controls.DataGridRow;
            if (dgRow1 == null)
                return;
            string strType = dgRow1.Item.GetType().ToString();
            if (strType != "MeineFinanzen.Model.WertpapSynchroNeu") {
                strAnzeige = "Bitte dgvUrl anklicken!";
                DataContext = null;
                DataContext = this;
                return;
            }
            System.Windows.Controls.DataGrid dataGrid = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(dgRow1) as System.Windows.Controls.DataGrid;
            WertpapSynchroNeu wpsn = (WertpapSynchroNeu)dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            Console.WriteLine("cell1.Column.Header: {0} Color: {1}", cell1.Column.Header, wpsn.WPVColor);            
            if (wpsn.WPVColor == "3") {
                strAnzeige = "Diese Zeile hat schon eine Vorgabe!";
            } else {
                strAnzeige = "Für diese Zeile eine Vorgabe bearbeiten oder erstellen.";
            }
            _Url1 = wpsn.WPVURL;
            if (dgRow1.DetailsVisibility == Visibility.Visible) {           
                dgRow1.DetailsVisibility = Visibility.Collapsed;
                dgRow1.Background = new SolidColorBrush(Colors.BlanchedAlmond);
                DataContext = null;
                DataContext = this;
                return;
            }
            dgRow1.DetailsVisibility = Visibility.Visible;
            dgRow1.Background = new SolidColorBrush(Colors.LightYellow);
            DataContext = null;
            DataContext = this;
            //wb1.LoadCompleted += (s, e) =>  {
            //the page has been loaded here. Do your thing...   };
            wb1.Navigate(new Uri(_Url1));
            while (wb1.IsBusy || wb1.ReadyState != WebBrowserReadyState.Complete) {
                DoEvents();
            }           
            _BoxAnfang = "row quotebox";
            _TxtKurse = "Kurse";
            _TxtKurszeit = "Kurszeit";
            _TxtKursdatum = "Kursdatum";
            _TxtKurs = "Kurs";
            bool bol = SearchWebPage(_Url1, _BoxAnfang, _TxtKurse, _TxtKurszeit, _TxtKursdatum, _TxtKurs, wpsn);
            if (bol) {
                AddTextStr("Ok, hat geklappt.");
            } else
                AddTextStr("!!!! Nee, hat nicht geklappt!!!!");
        }
        private bool SearchWebPage(string url1, string boxanfang, string txtkurse, string txtkurszeit, string txtkursdatum,
            string txtkurs, WertpapSynchroNeu wpsneu) {
            if (wb1.Document == null)
                return false;
            String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
            String pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999
            String pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";              // 99:99:99
            HtmlElementCollection elemColl = null;
            HtmlDocument doc = wb1.Document;
            if (doc != null)
                AddTextStr("--- Start ---" + wb1.Document.Url);            
            elemColl = doc.GetElementsByTagName("body");
            string strInnerText = "";
            string strKursdatum = "";
            string strKurszeit = "";
            string strKurs = "";
            string strZeilePlus = "";
            string[] strarr1;
            string[] strarr2;
            string[] strarr3;
            string strf = "";           
            foreach (HtmlElement elem in elemColl) {                    // Ein Element (Node)
                if (!elem.InnerHtml.Contains(boxanfang))
                    continue;
                //DoEvents();
                strInnerText = elem.InnerText;                               // Beginn Box 'row quotebox'
                strInnerText = Regex.Replace(strInnerText, "[\x00-\x1F]+", "/");
                string[] strZeilenTeile = strInnerText.Split('/');
                Console.Write("---- {0,2} {1,5} {2,-80} BoxAnf:{3}", elem.Children.Count, elem.InnerHtml.Length, wb1.Document.Url, boxanfang);                
                int nn = 0;
                strKursdatum = "";
                strKurszeit = "";
                strKurs = "";                                                       // Und +-EUR, +-% 
                char[] charSeparators = new char[] { ' ' };
                foreach (string strZeile in strZeilenTeile) {                       // Zeilen in der Box.
                    strf = "";
                    ++nn;
                    if (strZeile.StartsWith(txtkurse))
                        continue;
                    strZeilePlus = "";
                    if (strZeile.StartsWith(txtkursdatum)) {
                        strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile);
                        foreach (Match m in Regex.Matches(strZeile, pattDatum))
                            strKursdatum += String.Format("{0} ", m.Value);
                    }
                    if (strZeile.StartsWith(txtkurszeit)) {
                        strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile);
                        foreach (Match m in Regex.Matches(strZeile, pattZeit))
                            strKurszeit += String.Format("{0} ", m.Value);
                    }
                    if (strZeile.StartsWith(txtkurs)) {
                        strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile);
                        foreach (Match m in Regex.Matches(strZeile, pattBetrag))
                            strKurs += String.Format("{0} ", m.Value);
                    }
                    if (strZeilePlus.Length > 0) {
                        strarr1 = strKurs.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        strarr2 = strKursdatum.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        strarr3 = strKurszeit.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                        strf = String.Format("@1 {0,-40}-->(K:{1} {2}) (D:{3} {4}) (Z:{5} {6})", strZeilePlus, strKurs,
                            strarr1.Length, strKursdatum, strarr2.Length, strKurszeit, strarr1.Length);
                        AddTextStr(strf);
                        Console.WriteLine("{0}", strf);
                    }
                }                   // foreach (string strZeile in strZeilenTeile)
                strf = String.Format("@2 K:{0} D:{1} Z:{2}", strKurs, strKursdatum, strKurszeit);
                if (strf.Length == 0)
                    return false;
                string[] strarrx = strKurs.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (strKurs.Length > 0) {
                    wpsneu.WPVKursNeu = Convert.ToDouble(strarrx[0]);
                    // 128,96 2,33 1,84
                }
                if (strarrx.Length > 1)
                    wpsneu.WPVProzentAenderung = Convert.ToDouble(strarrx[1]);
                if (strarrx.Length > 2)
                    wpsneu.WPVProzentAenderung = Convert.ToDouble(strarrx[2]);
                strarrx = strKursdatum.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                wpsneu.WPVKursZeit = Convert.ToDateTime(strarrx[0]);
                Console.WriteLine("§{0,7} {1} {2}", wpsneu.WPVKursNeu, wpsneu.WPVProzentAenderung, wpsneu.WPVKursZeit);
                //Progress++;
                return true;
            }                   // foreach (HtmlElement elem in elemColl)
            System.Windows.MessageBox.Show("Fehler. Auf dieser WebSeite kein 'row quotebox' gefunden!!!");
            //SetDataRowColor("R");
            return false;    // Wenn kein 'row quotebox' gefunden.
        }
        private void dgvVorgabeInt2_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            System.Windows.Controls.DataGridCell cell1 = dep as System.Windows.Controls.DataGridCell;
            dgRow1 = dep as System.Windows.Controls.DataGridRow;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as System.Windows.Controls.DataGridRow;
            if (dgRow1 == null)
                return;
            string strType = dgRow1.Item.GetType().ToString();
            if (strType != "MeineFinanzen.Model.VorgabeInt2") {
                strAnzeige = "Bitte VorgabeIn2 anklicken!";
                DataContext = null;
                DataContext = this;
                return;
            }
            System.Windows.Controls.DataGrid dataGrid = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(dgRow1) as System.Windows.Controls.DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            _ColHeaderVorgabe = cell1.Column.Header.ToString();
            _Url1 = ((Model.VorgabeInt2)item).Url1;
            _Url2 = ((Model.VorgabeInt2)item).Url2;
            _BoxAnfang = ((Model.VorgabeInt2)item).Boxanfang;
            if (dgRow1.DetailsVisibility == Visibility.Collapsed) {
                dgRow1.DetailsVisibility = Visibility.Visible;
            } else {
                dgRow1.DetailsVisibility = Visibility.Collapsed;
            }
            Console.WriteLine("_ColHeaderVorgabe: {0,12} {1,20} {2,20} _BoxAnfang: {3,20}", _ColHeaderVorgabe, _Url1, _Url2, _BoxAnfang);
            if (((Model.VorgabeInt2)item).Vg2Color == "Eingefügt") {
                strAnzeige = "xxxxxxxxxxEingefügtxxxxxxxxxxxxxx";
            } else {
                strAnzeige = "yyyyyyyyyyyyyyyyyyyyyyyyyyy " + cell1.Column.Header;
                strAnzeige += Environment.NewLine + "mach was";
            }
            DataContext = null;
            DataContext = this;
        }
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) { }
        private void wb1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) { }
        private void wb1_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) { }
        private void _Beenden_Click(object sender, RoutedEventArgs e) { }
        private void Abmelden_Click(object sender, RoutedEventArgs e) { }
        private void dgvVorgabeInt2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }
        protected void DoEvents() {
            if (System.Windows.Application.Current != null)
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        private void AddTextStr(string str) {          
            txtAnzeige.AppendText(Environment.NewLine + str);
            txtAnzeige.ScrollToEnd();
            txtAnzeige.InvalidateVisual();
            DoEvents();
        }
    }
}