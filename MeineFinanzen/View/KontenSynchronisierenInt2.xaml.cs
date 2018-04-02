// 01.04.2018 KontenSynchronisierenInt2.xaml.cs
// _wertpapsynchro wird aus dtPortFol gefüllt.
// LosGehts - NavigiereZu - DocumentCompleted - SearchWebPage - LosGehts usw
// https://www.finanzen.net/        fonds/      spsw_-_whc_global_discovery
// BoxAnfang suchen mit 'row quotebox' in InnerHtml.
//      In der Box Zeilenanfang 'Kursdatum/Kurszeit/Kurs' suchen. Mehrere Zeilen jeweils möglich!
//          Mit Regex.Matches den Wert suchen und sammeln, da mehrere Werte vorhanden. Kurs,Änd,%Änd.
// Nach Boxende, gesammelte Werte wieder aufbröseln, mit Split.
// Es gibt mehrere VorgabeParameterSätze. Diese werden in der XML-VorgabeParameterDatei gespeichert.
// Ein VorgabeParameterSatz sieht wie folgt aus:
// Feldname     Inhalt
// .Url1        'https://www.finanzen.net/'
// .Url2        'fonds'
// .Boxanfang   'row quotebox'
// .Ausschluss1 'Kurse'
// .Wert1       'Kursdatum'
// .Wert2       'Kurszeit'
// .Wert3       'Kurs'
// .Wert4       ''
// Im Wertpapier wird also die Nr des VorgabeParameterSatzes und Url3 gespeichert.
using DataSetAdminNS;
using MeineFinanzen.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenInt2 : Window, INotifyPropertyChanged, IEditableObject {
        public CollWertpapSynchro _wertpapsynchro = null;
        public static VorgabeInt2 Vorg = new VorgabeInt2();
        public static List<VorgabeInt2> liVorg = new List<VorgabeInt2>();
        private double _progress;
        public double Progress {
            get { return _progress; }
            set {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }
        private double _maximum;
        public double Maximum {
            get { return _maximum; }
            set {
                _maximum = value;
                RaisePropertyChanged("Maximum");
            }
        }
        private double _minimum;
        public double Minimum {
            get { return _minimum; }
            set {
                _minimum = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(string name) {
            //Console.WriteLine("RaisePropertyChanged: " + name);
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        internal void NotifyPropertyChanged(string propertyName) {
            Console.WriteLine("NotifyPropertyChanged: " + propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
        string _strFile = Helpers.GlobalRef.g_Ein.myDocPfad + @"\MeineFinanzen\MyDepot\Daten"; // NOCH Helpers.GlobalRef.g_Ein.myDepotPfad + @"Daten\";       
        DataTable dtPortFol = new DataTable();
        public PortFol _portfol = new PortFol();
        private int lfdPoFo = -1;
        WertpapSynchro wpsyn = new WertpapSynchro();
        public KontenSynchronisierenInt2() {
            InitializeComponent();
            DataContext = this;
            _wertpapsynchro = (CollWertpapSynchro)Resources["wertpapsynchro"];
            PrintTxtUnten("start -Statustext-           ");
        }
        public void VorgabeParameterBearbeiten() {
            DateTime dt = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt == null) {
                System.Windows.MessageBox.Show("MeineFinanzen VorgabeParameterBearbeiten.xaml.cs Fehler HolenAusXml() DataSetAdmin");
                System.Windows.MessageBox.Show("MeineFinanzen Fehler!!  Dateien nicht geladen!!!!");
                Close();
            }
            string VorgabeFile = Helpers.GlobalRef.g_Ein.myDepotPfad +
                @"\Einstellungen\VorgabeParameterInt2.xml";

            VorgabeInt2 vg2 = new VorgabeInt2();
            vg2.DeserializeVorgabeInt2(VorgabeFile, out Vorg);
        }
        public void Ausführen() {           
            txtOben.Clear();
            wb1.ScriptErrorsSuppressed = true;
            txtOben.FontSize = 10;
            txtOben.FontFamily = new FontFamily("Courier New, Verdana");
            PrintTxtOben("Start -KontenAusInternetSynchronisieren-2-");
            txtUnten.Clear();
            txtUnten.FontSize = 10;
            txtUnten.FontFamily = new FontFamily("Courier New, Verdana");
            DateTime dt = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt == null) {
                System.Windows.MessageBox.Show("MeineFinanzen HauptFenster.xaml.cs HauptFenster() Fehler HolenAusXml() DataSetAdmin");
                System.Windows.MessageBox.Show("MyPortfolio Fehler!!  Dateien nicht geladen!!!!");
                Close();
            }
            string VorgabeFile = Helpers.GlobalRef.g_Ein.myDepotPfad + @"\Einstellungen\VorgabeParameterInt2.xml";
            VorgabeInt2 vg2 = new VorgabeInt2();
            vg2.DeserializeVorgabeInt2(VorgabeFile, out Vorg);
            PrintTxtUnten(dt.ToString());
            // DataColumn[] keys = new DataColumn[1];
            // keys[0] = DataSetAdmin.dtPortFol.Columns["WPIsin"];
            // DataSetAdmin.dtPortFol.PrimaryKey = keys;       // NOCH
            Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
            wb1.GoHome();
            wb1.Navigate(new Uri("https://www.google.de/"));        // Löst kein wb1-DocumentCompleted aus
            string[] _strurl = new string[3];
            _strurl[0] = "http://www.finanztreff.de";
            _strurl[1] = "http://waehrungen.finanztreff.de/devisen_uebersicht.htn";  // USD ...
            _strurl[2] = "http://waehrungen.finanztreff.de/devisen_einzelkurs_uebersicht,i,2079609.html"; // nur SGD
            string[] _strWohin = new string[3];
            _strWohin[0] = "MarktÜberblick";
            _strWohin[1] = "USD";
            _strWohin[2] = "SGD";
            string strx = _strFile + _strWohin[0] + ".html";
            if (!File.Exists(strx))
                File.Create(strx);
            DateTime dtLastWriteTime = File.GetLastWriteTime(strx);
            //nixSharpe = false;
            /*if (dtLastWriteTime.DayOfYear != DateTime.Now.DayOfYear)        // Nur 1mal am Tag!!  NOCH  // raus.
            {
                nixSharpe = false;
                for (int lop = 0; lop < 3; lop++) {
                    _url = _strurl[lop];
                    string strFile = _strFile + _strWohin[lop] + ".html";
                    PrintTxtOben("SonderSeitenHolen() " + strFile + Environment.NewLine);
                    WebClient wclient1 = new WebClient();
                    wclient1.DownloadFile(new Uri(_url), strFile);
                }
            } */
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // dtPortFol geholt.                
            foreach (DataRow dr in dtPortFol.Rows) {
                int typeid = (int)dr["WPTypeID"];
                if (typeid <= 10 || typeid >= 80)
                    continue;
                if (dr["WPISIN"].ToString().Length != 12)
                    continue;
                _wertpapsynchro.Add(new WertpapSynchro {
                    WPSAnzahl = Convert.ToSingle(dr["WPAnzahl"]),
                    WPSName = dr["WPName"].ToString(),
                    WPSKursZeit = Convert.ToDateTime(dr["WPStand"]),
                    WPSISIN = dr["WPISIN"].ToString(),

                    WPSURL = dr["WPUrlText"].ToString(),
                    WPSKurs = Convert.ToDouble(dr["WPKurs"]),
                    WPSProzentAenderung = Convert.ToDouble(dr["WPProzentAenderung"]),
                    WPSType = Convert.ToInt32(dr["WPTypeID"]),
                    WPSSharpe = Convert.ToSingle(dr["WPSharpe"]),

                    WPURLSharp = dr["WPUrlSharpe"].ToString(),
                    WPXPathKurs = dr["WPXPathKurs"].ToString(),
                    WPXPathAend = dr["WPXPathAend"].ToString(),
                    WPXPathZeit = dr["WPXPathZeit"].ToString(),
                    WPXPathSharp = dr["WPXPathSharp"].ToString(),
                    WPSColor = "0"
                });
            }
            progrBar.Maximum = _wertpapsynchro.Count;
            Progress = 1;
            Minimum = 0;
            Maximum = _wertpapsynchro.Count;
            LosGehts(true);
        }
        private void LosGehts(bool ergebnis) {
            if (ergebnis) {
                SetDataRowColor("3");
                lfdPoFo++;
            }
            if ((lfdPoFo + 1) > _wertpapsynchro.Count) {
                Close();
                return;
            }
            wb1.GoHome();
            wpsyn = _wertpapsynchro[lfdPoFo];
            wb1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
            //NavigiereZuAriva(@"http://www.ariva.de/carmignac_patrimoine_d_eur_inc-fonds");  //@"https://www.ariva.de/" + pofo.WPUrlText);
            NavigiereZu(wpsyn.WPSURL);
        }
        private void SetDataRowColor(string strColor) {
            wpsyn.WPSColor = strColor;
            dgvUrls.ItemsSource = null;
            dgvUrls.ItemsSource = _wertpapsynchro;
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();
        }
        private bool SearchWebPage() {  // Kommt von wb1_DocumentCompleted. Also eine WebPage.
            DoEvents();
            if (wb1.Document == null)
                return false;
            String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
            String pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999
            String pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";              // 99:99:99
            HtmlElementCollection elemColl = null;
            HtmlDocument doc = wb1.Document;
            if (doc != null) {
                addTextStr("--- Start ---" + wb1.Document.Url);
            }
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
                if (!elem.InnerHtml.Contains("row quotebox"))
                    continue;
                DoEvents();
                strInnerText = elem.InnerText;                               // Beginn Box 'row quotebox'
                strInnerText = Regex.Replace(strInnerText, "[\x00-\x1F]+", "/");
                string[] strZeilenTeile = strInnerText.Split('/');
                Console.Write("'row quotebox'{0,2} {1,5} {2,-80}", elem.Children.Count, elem.InnerHtml.Length, wb1.Document.Url);
                if (wpsyn.WPSURL.Contains("deka"))
                    SetDataRowColor("1");
                int nn = 0;
                strKursdatum = "";
                strKurszeit = "";
                strKurs = "";                                                       // Und +-EUR, +-% 
                char[] charSeparators = new char[] { ' ' };
                foreach (string strZeile in strZeilenTeile) {                       // Zeilen in der Box.
                    strf = "";
                    ++nn;
                    if (strZeile.StartsWith("Kurse"))
                        continue;
                    strZeilePlus = "";
                    if (strZeile.StartsWith("Kursdatum") || strZeile.StartsWith("Kurszeit")) {
                        strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile);
                        foreach (Match m in Regex.Matches(strZeile, pattDatum))
                            strKursdatum += String.Format("{0} ", m.Value);
                    }
                    if (strZeile.StartsWith("Kurszeit")) {
                        strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile);
                        foreach (Match m in Regex.Matches(strZeile, pattZeit))
                            strKurszeit += String.Format("{0} ", m.Value);
                    }
                    if (strZeile.StartsWith("Kurs")) {
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
                        addTextStr(strf);
                        Console.WriteLine("{0}", strf);
                    }
                }                   // foreach (string strZeile in strZeilenTeile)
                strf = String.Format("@2 K:{0} D:{1} Z:{2}", strKurs, strKursdatum, strKurszeit);
                if (strf.Length == 0)
                    return false;
                string[] strarrx = strKurs.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                wpsyn.WPSKurs = Convert.ToDouble(strarrx[0]);
                // 128,96 2,33 1,84
                if (strarrx.Length > 1)
                    wpsyn.WPSProzentAenderung = Convert.ToDouble(strarrx[1]);
                if (strarrx.Length > 2)
                    wpsyn.WPSProzentAenderung = Convert.ToDouble(strarrx[2]);
                strarrx = strKursdatum.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                wpsyn.WPSKursZeit = Convert.ToDateTime(strarrx[0]);
                Console.WriteLine("§{0,7} {1} {2}", wpsyn.WPSKurs, wpsyn.WPSProzentAenderung, wpsyn.WPSKursZeit);
                Progress++;
                return true;
            }                   // foreach (HtmlElement elem in elemColl)
            System.Windows.MessageBox.Show("Fehler. Auf dieser WebSeite kein 'row quotebox' gefunden!!!");
            SetDataRowColor("R");
            return false;    // Wenn kein 'row quotebox' gefunden.
        }
        private void NavigiereZu(string address) {
            if (string.IsNullOrEmpty(address))
                return;
            if (address.Equals("about:blank"))
                return;
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
                address = "http://" + address;
            //_navigiert = true;
            wb1.Navigate(new Uri(address));
        }
        private void NavigiereZuAriva(string address) {
            wb1.Navigate(new Uri(address));
        }
        private void wb1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            if (e.Url.AbsolutePath != (sender as System.Windows.Forms.WebBrowser).Url.AbsolutePath) {
                TxtWrLi("Urls !=");
            } else if (wb1.ReadyState == WebBrowserReadyState.Complete) {
                TxtWrLi("Complete");
                wb1.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
                bool ergebnis = SearchWebPage();
                LosGehts(ergebnis);
            } else if (wb1.ReadyState == WebBrowserReadyState.Interactive) {
                TxtWrLi("Interactive");
            } else if (wb1.ReadyState == WebBrowserReadyState.Loading) {
                TxtWrLi("Loading");
            } else {
                TxtWrLi("wb1-DocumentCompleted: " + wb1.ReadyState + " unbekannt!!!");
            }
        }
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) {
            Title = "-KontenSynchronisierenInt-2 " + (sender as System.Windows.Forms.WebBrowser).DocumentTitle;
        }
        private void wb1_StatusTextChanged(object sender, EventArgs e) {
            if (wb1.StatusText.Length < 3)
                return;
        }
        private void wb1_Navigating(object sender, WebBrowserNavigatingEventArgs e) { }
        void acceptButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e) {
            MessageBoxResult result = System.Windows.MessageBox.Show("Abgebrochen! Mit UpDate?", " Nur Beenden",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question,
                              MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) {
                lfdPoFo = _wertpapsynchro.Count + 777;
                DataSetAdmin.DatasetSichernInXml("MeineFinanzen");
                UpdatedtPortFol();
            }
        }
        private void UpdatedtPortFol() {
            // WPKurs WPStand WPProzentAenderung WPSharpe
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];
            string strISIN = "";
            foreach (WertpapSynchro wps in _wertpapsynchro) {
                strISIN = wps.WPSISIN;
                if (strISIN.Length != 12)
                    continue;
                DataRow dtrow = dtPortFol.Rows.Find(strISIN);
                if (wps.WPSKursZeit != (DateTime)dtrow["WPStand"]) {
                    Console.WriteLine("++++ Stand alt: {0,-12} neu: {1,-12}", (DateTime)dtrow["WPStand"], wps.WPSKursZeit);
                    dtrow["WPStand"] = wps.WPSKursZeit;
                }
                if (wps.WPSKurs != (float)dtrow["WPKurs"]) {
                    Console.WriteLine("UpdatedtPortFol() Kurs alt: {0,-12} neu: {1,-12}", (float)dtrow["WPKurs"], wps.WPSKurs);
                    dtrow["WPKurs"] = wps.WPSKurs;
                }
                if (wps.WPSProzentAenderung != (float)dtrow["WPProzentAenderung"])
                    dtrow["WPProzentAenderung"] = wps.WPSProzentAenderung;

                if (dtrow["WPSharpe"] != dtrow["WPSharpe"])
                    dtrow["WPSharpe"] = wps.WPSSharpe;
            }
            DataSetAdmin.dtPortFol = dtPortFol;
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
        }
        protected void DoEvents() {
            if (System.Windows.Application.Current != null)
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        private void PrintTxtOben(string str) {
            txtOben.AppendText(str);
            txtOben.ScrollToEnd();
            txtOben.InvalidateVisual();
        }
        private void PrintTxtUnten(string str) {
            txtUnten.AppendText(Environment.NewLine + str);
            txtUnten.ScrollToEnd();
            txtUnten.InvalidateVisual();
        }
        private void addTextStr(string str) {
            txtUnten.AppendText(Environment.NewLine + str);
            txtUnten.ScrollToEnd();
            txtUnten.InvalidateVisual();
        }
        public void TxtWrLi(string str1) {
            try {
                string str = string.Format("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
                txtUnten.AppendText(Environment.NewLine + str);
                txtUnten.ScrollToEnd();
                txtUnten.InvalidateVisual();
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler TxtWrLi()" + ex);
            }
        }
        private void dgvUrls_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        }
    }  
}