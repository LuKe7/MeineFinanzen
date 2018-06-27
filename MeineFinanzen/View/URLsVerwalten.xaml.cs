// 27.05.2018 URLsVerwalten.xaml.cs
// VorgabeParameter für KontenSynchronisierung über Internet 2.Version(Über Textsuche in Elementen...).
// Die Wertpapiere suchen sich ihren Parametersatz selbst. Über Url1 + Url2 !!!!
// Erstellen dieser Sätze, falls sie nicht vorhanden sind.
// Ergänzen dieser Sätze manuell.
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
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace MeineFinanzen.View {
    public partial class URLsVerwalten : Window {
        public ObservableCollection<UrlVerwalten> liVorgaben = new ObservableCollection<UrlVerwalten>();
        public static List<Wertpap> liWertpaps = new List<Wertpap>();
        public CollWertpapSynchroNeu _wertpapsynchroneu = null;
        public static UrlVerwalten Vorg = new UrlVerwalten();
        public static List<UrlVerwalten> liVorg = new List<UrlVerwalten>();
        DataTable dtPortFol = new DataTable();
        DataTable dtVorgabe = new DataTable();
        DataTable dtVorgabeLoop = new DataTable();
        public DataGridRow dgRow1 = null;               // Diese Zeile in dgvUrl wurde angeklickt.      
        string _Url1, _Url2, _BoxAnfang, _TxtKurse, _TxtKurszeit, _TxtKursdatum, _TxtKurs; // Diese Zeile in dgvVorgabe wurde angeklickt.
        string _ColHeaderVorgabe;                       // Diese Spalte in dgvVorgabe wurde angeklickt.
        int _posx = -1;      //  e.ClientMousePosition.X;
        int _posy = -1;
        HtmlElement _elem1 = null;
        public URLsVerwalten() {
            InitializeComponent();
            //txtAnzeige.Multiline = true;          
            DataContext = this;
            // Get a reference to the CollWertpapSynchro collection.
            Meldung("Anwendung gestartet.");
            _wertpapsynchroneu = (CollWertpapSynchroNeu)Resources["wertpapsynchron"];
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
            Meldung("Bitte ein Wertpapier wählen.");
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
                    WPVColor = "1",
                    WPVKursNeu = 0,
                    WPVKursZeitNeu = Convert.ToDateTime("01.01.1970"),
                    WPVProzentAenderungNeu = 0,
                    WPVSharpeNeu = 0
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
                newRow["Vg2Color"] = "2";
                dtVorgabe.Rows.Add(newRow);
            }
            // ---- liVorg für dgvVorgabeInt2 erstellen ----
            for (int i = dtVorgabe.Rows.Count - 1; i >= 0; i--) {
                DataRow dr = dtVorgabe.Rows[i];
                //foreach (DataRow dr in dtVorgabeLoop.Rows) {
                Vorg = new UrlVerwalten {
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
                foreach (UrlVerwalten vg in liVorg) {
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
                    Vorg = new UrlVerwalten {
                        Url1 = splitUrls2[0] + "//" + splitUrls2[2] + "/",
                        Url2 = splitUrls2[3],
                        Boxanfang = "",
                        Ausschluss1 = "",
                        Wert1 = "",
                        Wert2 = "",
                        Wert3 = "",
                        Wert4 = "",
                        Vg2Color = "0"
                    };
                    liVorg.Add(Vorg);
                    AddliVorgTodtVorgabe(liVorg, Vorg);
                }
            }
            Meldung("VorgabeParameter aufgebaut.");
            dgvUrls.ItemsSource = null;
            dgvUrls.ItemsSource = _wertpapsynchroneu;
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();
            dgvVorgabeInt2.ItemsSource = null;
            dgvVorgabeInt2.ItemsSource = liVorg;
            dgvVorgabeInt2.EnableRowVirtualization = false;
            dgvVorgabeInt2.UpdateLayout();
            DoEvents();
        }
        public class TEST_TermineList : List<UrlVerwalten> {        // Kann weg
            public void DeleteTermineForDate(DateTime dt) {
                for (int i = this.Count - 1; i >= 0; i--) {
                    if (this[i].Url1 == "xxxxx") {
                        this.RemoveAt(i);
                    }
                }
            }
        }
        private void AddliVorgTodtVorgabe(List<UrlVerwalten> liVorg, UrlVerwalten Vorg) {
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
            foreach (UrlVerwalten livor in liVorg) {
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
            //mel.Close();
        }
        private void ResetDgRow() {
            dgRow1.DetailsVisibility = Visibility.Collapsed;
            foreach (UrlVerwalten vg2 in liVorg) {
                vg2.Vg2Color = "0";
            }
            foreach (WertpapSynchroNeu sy in _wertpapsynchroneu) {
                sy.WPVColor = "1";
            }
            dgvVorgabeInt2.ItemsSource = null;
            dgvVorgabeInt2.ItemsSource = liVorg;
            dgvVorgabeInt2.EnableRowVirtualization = false;
            dgvVorgabeInt2.UpdateLayout();
        }
        private void dgvUrls_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            dgRow1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            if (e.RightButton == MouseButtonState.Pressed)
                return;
            ResetDgRow();
            _ColHeaderVorgabe = null;
            System.Windows.Controls.DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1)
                as System.Windows.Controls.DataGrid;
            WertpapSynchroNeu wpsn = (WertpapSynchroNeu)dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            //System.Windows.Controls.DataGridCell cell1 = dep as System.Windows.Controls.DataGridCell;
            //Console.WriteLine("cell1.Column.Header: {0} Color: {1}", cell1.Column.Header, wpsn.WPVColor);
            dgRow1.DetailsVisibility = Visibility.Visible;
            dgRow1.Background = new SolidColorBrush(Colors.LightYellow);
            _Url1 = wpsn.WPVURL;
            Meldung("Ok, ausgewählt: " + _Url1);
            wb1.Navigate(new Uri(_Url1));
            while (wb1.IsBusy || wb1.ReadyState != WebBrowserReadyState.Complete) {
                DoEvents();
            }
            // ---- Vorgabe-Parametersatz in dgvVorgabeInt2 suchen ----    
            int iSearch = 0;
            char[] charSeparators = new char[] { '/' };
            string[] url1split = _Url1.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            UrlVerwalten vor = null;
            Console.WriteLine("---- gesucht wird in DataGrid-Vorgabe: _Url1:{0} _Url2: {1}", _Url1, _Url2);
            foreach (UrlVerwalten vor1 in liVorg) {
                if (vor1.Url1 == url1split[0] + @"//" + url1split[1] + @"/" && vor1.Url2 == url1split[2]) {    // https://www.finanzen.net/  == https://www.finanzen.net/ etf leer
                    vor = vor1;
                    Console.WriteLine("---- Url1 gefunden: {0} Url2: {1} Wert1: {2} Wert2: {3}", vor.Url1, vor1.Url2, vor1.Wert1, vor1.Wert2);
                    if (vor1.Wert1 == string.Empty) {
                        Meldung("Wert1 muß eingegeben werden!");
                        iSearch = 4;
                    } else {
                        if (vor.Url2 == url1split[2]) {
                            AddTextStr("---- Url2 gefunden: " + vor.Url2);
                            _BoxAnfang = vor.Boxanfang;     // "row quotebox";                            
                            _TxtKurse = vor.Ausschluss1;    // "Kurse";
                            _TxtKursdatum = vor.Wert1;      // "Kursdatum";
                            _TxtKurszeit = vor.Wert2;       // "Kurszeit";                            
                            _TxtKurs = vor.Wert3;           // "Kurs";   
                            iSearch = SearchWebPageHIER(_Url1, _BoxAnfang, _TxtKurse, _TxtKurszeit, _TxtKursdatum, _TxtKurs, ref wpsn);
                            if (iSearch == 1) {
                                AddTextStr("Ok. 1.");
                            } else
                                Meldung("!!!! Nee, Die Suche nach Kurs... im WEB war nicht erfolgreich.  !!!! : " + iSearch);
                        }
                    }
                }
            } // foreach vor1    
            vor.Vg2Color = iSearch.ToString();
            //if (iSearch == 1)
            //    AddTextStr("Ok. 2.");
            //else
            //    Meldung("!!!! Nee Fehler 2.!!!!  FehlerNummer: " + iSearch);
            dgvVorgabeInt2.ItemsSource = null;
            dgvVorgabeInt2.ItemsSource = liVorg;
            dgvVorgabeInt2.EnableRowVirtualization = false;
            dgvVorgabeInt2.UpdateLayout();
            Meldung("Suchargumente anlegen/ändern.");
            borderCombo.Background = new SolidColorBrush(Colors.LightGreen);
        }
        private int SearchWebPageHIER(string url1, string boxanfang, string txtkurse, string txtkurszeit,
            string txtkursdatum, string txtkurs, ref WertpapSynchroNeu wpsneu) {
            if (wb1.Document == null)
                return 2;
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
            //string strSharpe = "";
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
                //strSharpe = "";
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
                    return 2;
                string[] strarrx = strKurs.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (strKurs.Length > 0) {
                    wpsneu.WPVKursNeu = Convert.ToDouble(strarrx[0]);
                    // 128,96 2,33 1,84
                }
                if (strarrx.Length > 1)
                    wpsneu.WPVProzentAenderungNeu = Convert.ToDouble(strarrx[1]);
                if (strarrx.Length > 2)
                    wpsneu.WPVProzentAenderungNeu = Convert.ToDouble(strarrx[2]);
                strarrx = strKursdatum.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (strarrx.Length > 0)
                    wpsneu.WPVKursZeitNeu = Convert.ToDateTime(strarrx[0]);
                Console.WriteLine("§{0,7} {1} {2}", wpsneu.WPVKursNeu, wpsneu.WPVProzentAenderungNeu, wpsneu.WPVKursZeitNeu);
                //Progress++;
                return 1;
            }                   // foreach (HtmlElement elem in elemColl)
            System.Windows.MessageBox.Show("Fehler. Auf dieser WebSeite kein 'row quotebox' gefunden!!!");
            //SetDataRowColor("R");
            return 3;    // Wenn kein 'row quotebox' gefunden.
        }
        private void dgvVorgabeInt2_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            /* var source = e.Source;
            if (e.RightButton == MouseButtonState.Pressed) {
                URLsVerwaltenContextMenu gk = new URLsVerwaltenContextMenu();
                gk.ShowDialog();
            } */
            base.OnMouseDown(e);
            e.Handled = true;
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            System.Windows.Controls.DataGridCell cell1 = dep as System.Windows.Controls.DataGridCell;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            //string strType = dgRow1.Item.GetType().ToString();  // MeineFinanzen.Model.VorgabeInt2                      
            System.Windows.Controls.DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1) as System.Windows.Controls.DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            if (item.ToString() == "{NewItemPlaceholder}")
                return;
            _ColHeaderVorgabe = cell1.Column.Header.ToString();
            _Url1 = ((UrlVerwalten)item).Url1;
            _Url2 = ((UrlVerwalten)item).Url2;
            _BoxAnfang = ((UrlVerwalten)item).Boxanfang;
            /* if (dgRow1.DetailsVisibility == Visibility.Collapsed) {
                 dgRow1.DetailsVisibility = Visibility.Visible;
            } else {
                //dgRow1.DetailsVisibility = Visibility.Collapsed;
            }
            Console.WriteLine("_ColHeaderVorgabe: {0,12} {1,20} {2,20} _BoxAnfang: {3,20}", _ColHeaderVorgabe, _Url1, _Url2, _BoxAnfang);
            if (((VorgabeInt2)item).Vg2Color == "Eingefügt") {
                strAnzeige = "xxxxxxxxxxEingefügtxxxxxxxxxxxxxx";
            } else {
                strAnzeige = "yyyyyyyyyyyyyyyyyyyyyyyyyyy " + cell1.Column.Header;
                strAnzeige += Environment.NewLine + "mach was";
            } */
            Meldung("Vorgabe " + _Url1 + " " + _Url2);
            DataContext = null;
            DataContext = this;
        }
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) {
            AddTextStr("wb1_DocumentTitleChanged");
        }
        private void wb1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            if (e.Url.AbsolutePath != (sender as System.Windows.Forms.WebBrowser).Url.AbsolutePath) {
                AddTextStr("Urls !=");
            } else if (wb1.ReadyState == WebBrowserReadyState.Complete) {
                AddTextStr("Complete");
                wb1.Document.Click += new HtmlElementEventHandler(wb1_Document_Click);
            } else if (wb1.ReadyState == WebBrowserReadyState.Loading) {
                AddTextStr("Loading");
            } else {
                AddTextStr("wb1-DocumentCompleted: " + wb1.ReadyState + " unbekannt!!!");
            }
        }
        private void cbBoxanfang_Loaded(object sender, RoutedEventArgs e) {
            //cbBoxanfang.Text = "BoxAnfang - Text";
            //cbBoxanfang.Items.Add("BoxAnfang-Text");
            //cbBoxanfang.SelectedIndex = 0;            
            try {
                //OCBoxanfang();
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler in cbBoxanfang_Loaded: " + ex);
            }
        }
        private void cbBoxanfang_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    cbBoxanfang_SelectionChanged");
        }
        private void cbAusschluss1_Loaded(object sender, RoutedEventArgs e) {
            //cbUrl2.Text = "URL2";
            //cbUrl2.Items.Add("URL2");
            //cbUrl2.SelectedIndex = 0;
            foreach (UrlVerwalten vg in liVorg) {
                try {
                    //cbUrl2.Items.Add(vg.Url2);
                } catch (Exception ex) {
                    System.Windows.MessageBox.Show("Fehler in cbUrl2_Loaded: " + ex);
                }
            }
        }
        private void cbAusschluss1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    cbAusschluss1_SelectionChanged");
        }
        private void cbWert1_Loaded(object sender, RoutedEventArgs e) {
            //System.Windows.Forms.MessageBox.Show("NOCH    cbWert1_Loaded");
        }
        private void cbWert1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    cbWert1_SelectionChanged");
        }
        private void wb1_Navigating(object sender, WebBrowserNavigatingEventArgs e) { }
        private void dgvUrls_Neu_Click(object sender, RoutedEventArgs e) {
            DatagridContextMenu.IsOpen = false;
            string xxx = DatagridContextMenu.ToString();
            string strType = dgRow1.Item.GetType().ToString();      // MeineFinanzen.Model.WertpapSynchroNeu
            System.Windows.Controls.DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1)
                as System.Windows.Controls.DataGrid;
            WertpapSynchroNeu wpsn = (WertpapSynchroNeu)dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);

            wb1.Navigate(new Uri("https://www.google.de/search?q=" + "Kurs " + wpsn.WPVISIN
                + "&ie=utf-8&oe=utf-8&client=firefox-b"));
            //https://www.google.com/search?q=XXYY&ie=utf-8&oe=utf-8&client=firefox-b
            //string browser = GetDefaultBrowser();
        }
        private void dgvUrls_bearbeiten_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    dgvUrls_bearbeiten_Click");
        }
        private void myMenuButton_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            //Console.WriteLine("intercepted!!!!");
            e.Handled = true;
        }
        private void searchButton_Click(object sender, EventArgs e) {
            wb1.GoSearch();
        }
        private void _Beenden_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    _Beenden_Click");
        }
        private void searchButton_Click(object sender, RoutedEventArgs e) {
            wb1.GoSearch();
        }
        private void homeButton_Click(object sender, RoutedEventArgs e) {
            wb1.GoHome();
        }
        private void wb1_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
            TxtUrl.Text = wb1.Url.ToString();
        }
        private void wb1_Document_Click(Object sender, HtmlElementEventArgs e) {
            if (e.ClientMousePosition.IsEmpty) {
                _elem1 = null;
                _posx = -1;
                _posy = -1;
            } else {
                _posx = e.ClientMousePosition.X;
                _posy = e.ClientMousePosition.Y;
                _elem1 = wb1.Document.GetElementFromPoint(e.ClientMousePosition);
                // Ruft das an den angegebenen Clientkoordinaten befindliche HTML-Element ab.                            
                if (_elem1 != null) {
                    AddTextStr("wb1_Document_Click: " + _elem1.InnerText); 
                    // 145,01EUR
                    // 145,01 < span class="currency-iso">EUR</span>

                      HtmlElementCollection elemColl = null;
                    HtmlDocument doc = wb1.Document;
                    if (doc != null)
                        AddTextStr("--- Start ---" + wb1.Document.Url);
                    elemColl = doc.GetElementsByTagName("body");
                    foreach (HtmlElement elem in elemColl) {                    // Ein Element (Node)
                        if (!elem.InnerHtml.Contains(_elem1.InnerText))
                            continue;
                        //DoEvents();
                        string strInnerText = elem.InnerText;                               // Beginn Box 'row quotebox'
                        strInnerText = Regex.Replace(strInnerText, "[\x00-\x1F]+", "/");
                        string[] strZeilenTeile = strInnerText.Split('/');
                        Console.WriteLine("---- {0,2} {1,5} {2,-80} BoxAnf:{3}", elem.Children.Count,
                            elem.InnerHtml.Length, wb1.Document.Url, "row quotebox");
                        //int nn = 0;
                    }
                } else {
                    AddTextStr("wb1_Document_Click: null");
                }
            } // else
        }
        private int SearchSuchbegriff(string url1, string boxanfang, string txtkurse, string txtkurszeit,
     string txtkursdatum, string txtkurs, ref WertpapSynchroNeu wpsneu) {
            if (wb1.Document == null)
                return 2;
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
            //string strSharpe = "";
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
                //strSharpe = "";
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
                    return 2;
                string[] strarrx = strKurs.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (strKurs.Length > 0) {
                    wpsneu.WPVKursNeu = Convert.ToDouble(strarrx[0]);
                    // 128,96 2,33 1,84
                }
                if (strarrx.Length > 1)
                    wpsneu.WPVProzentAenderungNeu = Convert.ToDouble(strarrx[1]);
                if (strarrx.Length > 2)
                    wpsneu.WPVProzentAenderungNeu = Convert.ToDouble(strarrx[2]);
                strarrx = strKursdatum.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (strarrx.Length > 0)
                    wpsneu.WPVKursZeitNeu = Convert.ToDateTime(strarrx[0]);
                Console.WriteLine("§{0,7} {1} {2}", wpsneu.WPVKursNeu, wpsneu.WPVProzentAenderungNeu, wpsneu.WPVKursZeitNeu);
                //Progress++;
                return 1;
            }                   // foreach (HtmlElement elem in elemColl)
            System.Windows.MessageBox.Show("Fehler. Auf dieser WebSeite kein 'row quotebox' gefunden!!!");
            //SetDataRowColor("R");
            return 3;    // Wenn kein 'row quotebox' gefunden.
        }
        private void btOk_Click(object sender, RoutedEventArgs e) {
            /* if (!boDgvRowAusgewählt)
                return;
            if (_foundRow == null)
                return;
            if (cbKeinSharpe.IsChecked == true) {
                _foundRow.WPUrlSharpe = "";
                _foundRow.WPSharpe = 0;
            }  */
            //AddTextStr("btOk() _foundRow[\"WPKurs\"]: " + _foundRow.WPKurs.ToString());
            //AllesReset();
            wb1.Document.Click -= new HtmlElementEventHandler(wb1_Document_Click);
        }
        private void dgvUrls_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            System.Windows.Controls.DataGridCell cell1 = dep as System.Windows.Controls.DataGridCell;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            //string strType = dgRow1.Item.GetType().ToString();  // MeineFinanzen.Model.VorgabeInt2                      
            System.Windows.Controls.DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1) as System.Windows.Controls.DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            if (item.ToString() == "{NewItemPlaceholder}")
                return;
            _ColHeaderVorgabe = cell1.Column.Header.ToString();
            //string strIsin = ((MeineFinanzen.Model.WertpapSynchroNeu)item).WPVISIN;

        }
        private void labKurs(object sender, MouseButtonEventArgs e) {
            string strKurs = null;
            if (_elem1 != null) {
                try {
                    String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
                    foreach (Match m in Regex.Matches(_elem1.InnerText, pattBetrag))
                        strKurs += String.Format("{0} ", m.Value);
                    if (strKurs == null)
                        strKurs = "FEHLER";
                    double db = Convert.ToDouble(strKurs);
                    Meldung("Kurs:  " + strKurs + " ist Ok");
                    txtKurs.Text = strKurs;
                } catch (Exception) {
                    //System.Windows.MessageBox.Show("Fehler in wb1_Document_Click 'Kurs': " + ex);
                    Meldung("Fehler Kurs: " + _elem1.InnerText);
                }
            }
        }
        private void labZeit(object sender, MouseButtonEventArgs e) {
            string strKurszeit = null;
            if (_elem1 != null) {
                try {
                    String pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";              // 99.99.99  
                    String pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999                   
                    foreach (Match m in Regex.Matches(_elem1.InnerText, pattDatum))
                        strKurszeit += String.Format("{0} ", m.Value);
                    if (strKurszeit == null)
                        strKurszeit = "FEHLER";
                    DateTime dt = Convert.ToDateTime(strKurszeit);
                    Meldung("Zeit:  " + strKurszeit + " ist Ok");
                    txtZeit.Text = strKurszeit;
                } catch (Exception) {
                    Meldung("Fehler Kurszeit: " + _elem1.InnerText);
                }
            }
        }
        private void labÄnder(object sender, MouseButtonEventArgs e) {
            string strÄnd = null;
            if (_elem1 != null) {
                try {
                    String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
                    foreach (Match m in Regex.Matches(_elem1.InnerText, pattBetrag))
                        strÄnd += String.Format("{0} ", m.Value);
                    if (strÄnd == null)
                        strÄnd = "FEHLER";
                    double db = Convert.ToDouble(strÄnd);
                    Meldung("%Änd:  " + strÄnd + " ist Ok");
                    txtÄnd.Text = strÄnd;
                } catch (Exception) {
                    Meldung("Fehler %Änd: " + _elem1.InnerText);
                }
            }
        }
        private void labSharpe(object sender, MouseButtonEventArgs e) {
            string strSharpe = null;
            if (_elem1 != null) {
                try {
                    String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
                    foreach (Match m in Regex.Matches(_elem1.InnerText, pattBetrag))
                        strSharpe += String.Format("{0} ", m.Value);
                    if (strSharpe == null)
                        strSharpe = "FEHLER";
                    double db = Convert.ToDouble(strSharpe);
                    Meldung("Sharpe:  " + strSharpe + " ist Ok");
                    txtSharpe.Text = strSharpe;
                } catch (Exception) {
                    Meldung("Fehler Sharpe: " + _elem1.InnerText);
                }
            }
        }
        /* private string GetDefaultBrowser() {
string browser = string.Empty;
RegistryKey key = null;
try {
key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");
// trim off quotes
if (key != null)
browser = key.GetValue(null).ToString().ToLower(); 
// "c:\program files (x86)\mozilla firefox\firefox.exe" -osint -url "%1"
// get rid of everything after the ".exe"
if (!browser.EndsWith("exe")) {
browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
browser = browser.Substring(1);
// c:\program files (x86)\mozilla firefox\firefox.exe
}
} finally {
if (key != null)
key.Close();
}
return browser;
}  */
        private void Abmelden_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    Abmelden_Click");
        }
        private void dgvVorgabeInt2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            System.Windows.Forms.MessageBox.Show("NOCH    dgvVorgabeInt2_SelectionChanged");
        }
        protected void DoEvents() {
            if (System.Windows.Application.Current != null)
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        private void AddTextStr(string str) {
            txtAnzeige.AppendText(Environment.NewLine + str);
            txtAnzeige.ScrollToEnd();
            txtAnzeige.InvalidateVisual();
            //DoEvents();
        }
        private void Meldung(string str) {
            TxtMeldung.AppendText(str + Environment.NewLine);
            TxtMeldung.ScrollToEnd();
            TxtMeldung.InvalidateVisual();
            DoEvents();
        }
        public ObservableCollection<UrlVerwalten> GetVorgaben() {
            liVorgaben.Add(new UrlVerwalten { Url1 = "uurrrllll1" });
            return liVorgaben;
        }
    }
    public class OCBoxanfang : ObservableCollection<string> {
        public OCBoxanfang() {
            Add("Spain");
            Add("France");
            Add("Peru");
            Add("Mexico");
            Add("Italy");
        }
    }
    public class OCAusschluss1 : ObservableCollection<string> {
        public OCAusschluss1() {
            Add("Spain");
            Add("France");
            Add("Peru");
            Add("Mexico");
            Add("Italy");
        }
    }
    public class OCWert1 : ObservableCollection<string> {
        public OCWert1() {
            Add("Spain22");
            Add("France");
            Add("Peru");
            Add("Mexico");
            Add("Italy");
        }
    }
}