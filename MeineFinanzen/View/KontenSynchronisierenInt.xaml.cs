/* 08.03.2018 KontenSynchronisierenInt.xaml.cs
 * Never access UI objects on a thread that didn't create them.
 * Screen Scraping
 * 4 Werte in dtPortFol setzen: WPKurs WPStand WPProzentAenderung WPSharpe
 * 13.06.2016 ... Microsoft Richtlinie hältst und alle Aktionen die länger als 20ms dauern in einen separaten Thread packst....
 * 14.06.2016 DownloadAsync()
 * 29.09.2016 wb1.Navigate...
 * 04.01.2018 Füt dtPortFol liPortFol.
 * 09.02.2018 threshold auf 50000.
 * Generally, when the WebBrowser.ReadyState property becomes WebBrowserReadyState.Complete, the DocumentCompleted event is fired
 * to notice us that the page has been completely loaded.
 * But when the page has multiple frame pages, this event will be fired multiple times.
 * So it seems not feasible to use the DocumentCompleted event to check the loading completion of the page.
 * http://msdn.microsoft.com/en-us/library/system.windows.forms.webbrowserdocumentcompletedeventargs.url(v=VS.100).aspx.
 * If the DocumentCompleted event is fired by a frame page, the value of the WebBrowserDocumentCompletedEventArgs.
 * URL property is the URL of the frame page but not the top page, the frame pages will always be loaded before the top page is loaded.
 * So what do you think? Here we can check if the value of the WebBrowserDocumentCompletedEventArgs.
 * URL property is the same with the top page's URL, if so, this indicates that the top page has been completely loaded:
 * private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
 * // Use this way to decide if the whole page document has been completely downloaded.
 *  if (e.Url == webBrowser1.Document.Url) {
 *      MessageBox.Show("Top page been loaded.")
 *      }    }  
 * Call a method on the WatchDog class that updates a common lastPacketReceived value time a packet is received.
 * Then you only need to start a single timer one time in the WatchDog class that ticks once per timeout interval
 * and compares the current time to the lastPacketReceived value. */
using System;
using System.IO;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using System.Threading.Tasks;
using DataSetAdminNS;
using System.Xml.XPath;
using System.Diagnostics;
using System.Timers;
using System.ComponentModel;
using System.Collections.Generic;
using MeineFinanzen.Model;
using System.Runtime.CompilerServices;
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenInt : Window, INotifyPropertyChanged, IEditableObject {
        public List<PortFol> liPortFol = new List<PortFol>();
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
        private double _progress1;
        public double Progress1 {
            get { return _progress1; }
            set {
                _progress1 = value;
                RaisePropertyChanged("Progress1");
            }
        }
        private double _maximum1;
        public double Maximum1 {
            get { return _maximum1; }
            set {
                _maximum1 = value;
                RaisePropertyChanged("Maximum1");
            }
        }
        private double _minimum1;
        public double Minimum1 {
            get { return _minimum1; }
            set {
                _minimum1 = value;
                RaisePropertyChanged("Minimum1");
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
        //static long lastPacketReceived = 0;
        static Stopwatch stopWatch = new Stopwatch();
        static System.Timers.Timer myTimer;
        static long threshold = 50000;
        int _lfdUrl = 0;
        int _nverarb = 0;
        GetFromXpath getxp;
        bool boKurs = false;
        bool boZeit = false;
        bool boAend = false;
        bool boSharp = false;
        bool nixSharpe = false;
        bool navigateGestartet = false;
        bool pgmaus = false;
        bool bo_ZeitSchwelle = false;
        //DataTable dtWertPap = new DataTable();  ???
        public PortFol _portfol = new PortFol();
        int nInteractive = 0;
        string strFillB = "                                                                                                                                              ";
        string strFill = "------------------------------------------------------------------------------------------------------";
        int nichtGefundendene = 0;
        internal int _nAufrufe = -1;
        public string _isin = "", _url = "", _UrlSharpe = "", _name = "";
        string _strFile = Helpers.GlobalRef.g_Ein.myDocPfad + @"\MeineFinanzen\MyDepot\Daten"; // NOCH Helpers.GlobalRef.g_Ein.myDepotPfad + @"Daten\";       
        string _txtVorKurs;
        DataTable dtPortFol = new DataTable();
        public string myText2 { get; set; }
        public Uri uri_url = null;
        public KontenSynchronisierenInt() {
            InitializeComponent();
            progrBar = new System.Windows.Controls.ProgressBar();
            DataContext = this;
            PrintTxtUnten("start -Statustext-           ");
            getxp = new GetFromXpath();
        }
        public void Ausführen(HauptFenster mw, bool laden) {
            txtOben.Clear();
            wb1.ScriptErrorsSuppressed = true;
            txtOben.FontSize = 10;
            txtOben.FontFamily = new FontFamily("Courier New, Verdana");
            PrintTxtOben("Start -KontenAusInternetSynchronisieren-");
            txtUnten.Clear();
            txtUnten.FontSize = 10;
            txtUnten.FontFamily = new FontFamily("Courier New, Verdana");
            DateTime dt = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (dt == null) {
                System.Windows.MessageBox.Show("MeineFinanzen HauptFenster.xaml.cs HauptFenster() Fehler HolenAusXml() DataSetAdmin");
                System.Windows.MessageBox.Show("MyPortfolio Fehler!!  Dateien nicht geladen!!!!");
                Close();
            }
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
            DataTable dttableNew = dtPortFol.Clone();
            foreach (DataRow dr in dtPortFol.Rows) {
                int typeid = (int)dr["WPTypeID"];
                if (typeid > 10 && typeid < 80)
                    dttableNew.ImportRow(dr);
            }
            liPortFol = dttableNew.ToCollection<PortFol>();              // liPortFol erstellen.
            progrBar.Maximum = liPortFol.Count;
            _portfol = liPortFol[0];
            Progress1 = 1;
            Minimum1 = 0;
            Maximum1 = liPortFol.Count;
            LosGehts(_lfdUrl, false);       // false = nicht nurSharpe.
        }
        internal void LosGehts(int lfdUrl, bool nurSharpe) {
            if (pgmaus)
                return;
            if (nurSharpe) {
                boSharp = false;
                _url = (_portfol.WPUrlSharpe).Substring(0);
            } else {
                boKurs = false;
                boZeit = false;
                boAend = false;
                boSharp = false;
                if (lfdUrl >= liPortFol.Count) {
                    PrintTxtOben(Environment.NewLine + "  ----------------------- Fertig ------------------------------ ");
                    if (nichtGefundendene > 0)
                        PrintTxtOben(Environment.NewLine + "   ANZAHL FEHLER: " + nichtGefundendene);
                    pgmaus = true;
                    Close();
                    return;
                }
                _portfol = liPortFol[lfdUrl];
                _name = _portfol.WPName;
                _url = (_portfol.WPUrlText).Substring(0);
                _txtVorKurs = _portfol.WPTextVorKurs;
                _UrlSharpe = (_portfol.WPUrlSharpe).Substring(0);
                string strx1 = strFill.Substring(0, 30) + _name + strFill;
                string strx2 = strx1.Substring(0, 80) + DateTime.Now;
                PrintTxtUnten(strx2);
                string str = "-" + lfdUrl + " " + _name + strFill;
                PrintTxtOben(Environment.NewLine + str.Substring(0, 36));
                if (nixSharpe)
                    boSharp = true;
                else {
                    if (_UrlSharpe == "")
                        boSharp = true;
                }
            }
            wb1.GoHome();
            nInteractive = 0;
            navigateGestartet = true;
            bo_ZeitSchwelle = false;
            //lastPacketReceived = stopWatch.ElapsedMilliseconds;
            progrBar = new System.Windows.Controls.ProgressBar();
            Progress = 0;
            Minimum = 0;
            Maximum = 1000;
            stopWatch.Start();
            myTimer = new System.Timers.Timer(2000);
            myTimer.Elapsed -= new ElapsedEventHandler(myTimer_Elapsed);
            myTimer.Elapsed += new ElapsedEventHandler(myTimer_Elapsed);
            myTimer.Start();
            wb1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
            //Console.WriteLine("---- vor  Navigate: " + _url);
            uri_url = new Uri(_url);
            wb1.Navigate(uri_url);                          // Löst wb1_DocumentCompleted aus.              
                                                            //Console.WriteLine("---- nach Navigate: " + _url);
        }
        private void wb1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            if (bo_ZeitSchwelle) {
                TxtWrLi("bo_ZeitSchwelle");
                VerarbeiteDokument();
            } else if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath) {
                TxtWrLi("Urls !=");
            } else if (!navigateGestartet) {
                TxtWrLi("navigateNICHTGestartet: " + wb1.ReadyState);      // nach dem Letzten WP.              
            } else if (wb1.ReadyState == WebBrowserReadyState.Complete) {
                TxtWrLi("Complete");
                VerarbeiteDokument();
            } else if (wb1.ReadyState == WebBrowserReadyState.Interactive) {
                nInteractive++;
                TxtWrLi("Interactive" + nInteractive);
                if (nInteractive > 16) {
                    TxtWrLi("Interactive > 16");
                    VerarbeiteDokument();
                }
            } else if (wb1.ReadyState == WebBrowserReadyState.Loading) {
                TxtWrLi("Loading");
            } else {
                TxtWrLi("wb1-DocumentCompleted: " + wb1.ReadyState + " unbekannt!!!");
            }
        }
        private void VerarbeiteDokument() { // Evtl. mehrere Durchläufe, evtl. wg. Sharpe/Fehler. 
            stopWatch.Stop();
            stopWatch.Reset();
            wb1.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
            bool nurSharpe = false;
            _nverarb++;                     // Anzahl Verarbeitungen pro WP. 1.mal von 0 auf 1.
            navigateGestartet = false;
            if (!boKurs)
                boKurs = updKursHier();
            if (!boZeit)
                boZeit = updZeitHier();
            if (!boAend)
                boAend = updAendHier();
            if (!boSharp) {
                if (_url == _UrlSharpe) {
                    PrintTxtUnten("URLScharpe ==");
                    boSharp = updSharpeHier();
                } else {
                    PrintTxtUnten("URLScharpe <>");
                    _url = _UrlSharpe;
                    nurSharpe = true;
                }
            }
            if ((boKurs & boZeit & boAend & boSharp) || (_nverarb >= 3)) {
                Progress1 += 1;
                ++_lfdUrl;
                _nverarb = 0;
            }
            LosGehts(_lfdUrl, nurSharpe);
        }
        public bool updKursHier() {
            if (wb1.Document == null)
                return false;
            Single kursVor = _portfol.WPKurs;
            string str1 = string.Format("{0,8:###0.00 ;###0.00-;0.00 }", kursVor) + strFillB;
            PrintTxtUnten("KursVor: " + str1.Substring(0, 8));
            PrintTxtOben("K1" + str1.Substring(0, 8));
            string xpath = _portfol.WPXPathKurs;
            float kurs = getxp.GetPriceFromXpath(xpath, wb1, uri_url, _portfol);
            string str3 = string.Format("{0,8:###0.00 ;###0.00-;0.00 }", kurs) + strFillB;
            PrintTxtUntenNoCR(" Kurs: " + str3.Substring(0, 8));
            PrintTxtOben("K2" + str3.Substring(0, 8));
            if (kurs > 0) {
                _portfol.WPKurs = kurs;
                return true;
            }
            PrintTxtOben(" Fehler-Kurs");
            return false;
        }
        public bool updZeitHier() {
            if (wb1.Document == null)
                return false;
            string xpath = _portfol.WPXPathZeit;
            if (xpath.Length > 10) {
                DateTime zeit = getxp.GetZeitFromXpath(xpath, wb1, uri_url, _portfol);
                string str3 = string.Format("{0}", zeit.ToString("dd.MM.yy")) + strFillB;
                PrintTxtOben(str3.Substring(0, 8));
                PrintTxtUntenNoCR(" Stand: " + str3.Substring(0, 8));
                _portfol.WPStand = zeit;
            } else {
                PrintTxtUnten("kein XPathZeit!!");
                PrintTxtOben(" Fehler-Zeit");
                return false;
            }
            return true;
        }
        public bool updAendHier() {
            if (wb1.Document == null)
                return false;
            Single aendVor = -1;
            //DataColumn[] keys = new DataColumn[1];
            //keys[0] = DataSetAdmin.dtPortFol.Columns["WPIsin"];
            //DataSetAdmin.dtPortFol.PrimaryKey = keys;
            //_foundRow = DataSetAdmin.dtPortFol.Rows.Find(isin);
            if (_portfol.WPProzentAenderung.ToString() == "")
                aendVor = 0;
            else
                aendVor = (float)_portfol.WPProzentAenderung;
            string str1 = String.Format("{0,6:#0.00 ;#0.00-;0.00 }", aendVor) + strFillB;
            PrintTxtOben(" Ä1:" + str1.Substring(0, 6));
            string xpath = _portfol.WPXPathAend;
            float aend = getxp.GetAendFromXpath(xpath, wb1, uri_url, _portfol);
            string str3 = String.Format("{0,6:#0.00 ;#0.00-;0.00 }", aend) + strFillB;
            PrintTxtOben("Ä2:" + str3.Substring(0, 6));
            PrintTxtUntenNoCR(" Ä2:" + str3.Substring(0, 6));
            if (aend != -1) {
                _portfol.WPProzentAenderung = aend;
                return true;
            }
            PrintTxtOben(" Fehler-%Änd");
            return false;
        }
        public bool updSharpeHier() {
            if (wb1.Document == null) {
                return false;
            }
            Single sharpVor = -1;
            if (_portfol.WPSharpe.ToString() == "")         // wg. null !!
                sharpVor = 0;
            else
                sharpVor = _portfol.WPSharpe;
            string str1 = String.Format("{0,6:#0.00 ;#0.00-;0.00 }", sharpVor) + strFillB;
            PrintTxtOben("S1" + str1.Substring(0, 6));
            string xpath = _portfol.WPXPathSharp;
            if (xpath.Length < 20) {
                PrintTxtOben(Environment.NewLine + "!!!!!!!!!!!!!!!!! xpathlLenght < 20:" + _url);
                return false;
            }
            float sharp = getxp.GetSharpFromXpath(xpath, wb1, uri_url, _portfol);
            string str3 = String.Format("{0,6:#0.00 ;#0.00-;0.00 }", sharp) + strFillB;
            PrintTxtUntenNoCR(" Sharpe:" + str3.Substring(0, 6) + " ReadyState: " + wb1.ReadyState);
            PrintTxtOben("S2" + str3.Substring(0, 6));
            if (sharp != -1) {
                _portfol.WPSharpe = sharp;
                return true;
            }
            PrintTxtOben(" Fehler-Sharpe");
            return false;
        }
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) {
            //Title = "-KontenSynchronisierenInt- " + (sender as System.Windows.Forms.WebBrowser).DocumentTitle;
        }
        private void acceptButton_Click(object sender, RoutedEventArgs e) {
            // Accept the dialog and return the dialog result
            this.DialogResult = true;
        }
        private void wb1_Navigating(object sender, WebBrowserNavigatingEventArgs e) { }
        private void wb1_StatusTextChanged(object sender, EventArgs e) {
            if (wb1.StatusText.Length < 3)
                return;
        }
        private void btEnde_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Grid1_Unloaded(object sender, RoutedEventArgs e) {
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
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
        private void PrintTxtUntenNoCR(string str) {
            txtUnten.AppendText(str);
            txtUnten.ScrollToEnd();
            txtUnten.InvalidateVisual();
        }
        internal void myTimer_Elapsed(object sender, ElapsedEventArgs e) {
            Progress = stopWatch.ElapsedMilliseconds;
            //ConWrLi("myTimer_Elapsed:" + string.Format("Progress:{0,6}", Progress));
            if (stopWatch.ElapsedMilliseconds > threshold) {
                ConWrLi("---- myTimer_Elapsed bo_ZeitSchwelle:" + string.Format("Progress:{0,6}", Progress));
                //lastPacketReceived = stopWatch.ElapsedMilliseconds;
                bo_ZeitSchwelle = true;
                //wb1.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
                VerarbeiteDokument();
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e) {
            //DataSetAdmin.DatasetSichernInXml(D :xxx MeineFinanzen");
            MessageBoxResult result = System.Windows.MessageBox.Show("Schließen?", "Beenden",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Question,
                                      MessageBoxResult.No);
            if (result == MessageBoxResult.No)
                e.Cancel = true;
            UpdatedtPortFol();
        }
        private void UpdatedtPortFol() {    // WPKurs WPStand WPProzentAenderung WPSharpe
            // liPortFol-Daten nach DataSetAdmin.dsHier.Tables["tblPortFol"];
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // nochmal rausziehen, da evtl. geändert.
            string strISIN = "";
            foreach (DataRow dtrow in dtPortFol.Rows) {
                strISIN = (string)dtrow["WPISIN"];
                if (strISIN.Length != 12)
                    continue;
                PortFol lirow = liPortFol.Find(x => x.WPISIN.Contains(strISIN));
                if (lirow.WPStand != (DateTime)dtrow["WPStand"]) {
                    Console.WriteLine("++++ Stand alt: {0,-12} neu: {1,-12}", (DateTime)dtrow["WPStand"], lirow.WPStand);
                    dtrow["WPStand"] = lirow.WPStand;
                }
                if (lirow.WPKurs != (float)dtrow["WPKurs"]) {
                    Console.WriteLine("UpdatedtPortFol() Kurs alt: {0,-12} neu: {1,-12}", (float)dtrow["WPKurs"], lirow.WPKurs);
                    dtrow["WPKurs"] = lirow.WPKurs;
                }
                if (lirow.WPProzentAenderung != (float)dtrow["WPProzentAenderung"])
                    dtrow["WPProzentAenderung"] = lirow.WPProzentAenderung;

                if (dtrow["WPSharpe"] != dtrow["WPSharpe"])
                    dtrow["WPSharpe"] = lirow.WPSharpe;
            }
            DataSetAdmin.dtPortFol = dtPortFol;
            DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
        }
        private void warte(int ms) {
            Stopwatch sw = Stopwatch.StartNew();
            var delay = Task.Delay(ms).ContinueWith(_ => { sw.Stop(); return sw.ElapsedMilliseconds; });
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-80} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
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
    }
    public class GetFromXpath {
        HtmlAgilityPack.HtmlDocument hdoc = new HtmlAgilityPack.HtmlDocument();
        public GetFromXpath() { }
        public Single GetPriceFromXpath(string xpath, WebBrowser wb1, Uri url, PortFol datarow) {
            Single price = -1;
            XPathNavigator node = GetFromXpathAlle(xpath, wb1);
            if (node == null) {
                System.Windows.MessageBox.Show("!!! Fehler GetPriceFromXpath() Node aus SelectSingleNode ist null!!! " + " " + url);
                return price;
            }
            string str2 = null;
            try {
                str2 = node.TypedValue.ToString();   // Elemeniere alle /t und /n
                string str3 = str2.Trim(new Char[] { ' ', '\n', '\t', 'E', 'U', 'R' });
                str2 = str3;
                int i1 = -1;
                i1 = str2.IndexOf('&');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf(' ');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf('%');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf(' ');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                price = Convert.ToSingle(str2);   // 331,19&nbsp;€             
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("GetPriceFromXpath() catch Fehler: " + ex + " " + str2 + " " + url);
                return -1;
            }
            return price;
        }
        public DateTime GetZeitFromXpath(string xpath, WebBrowser wb1, Uri url, PortFol datarow) {
            DateTime dt = new DateTime(1980, 1, 1);
            XPathNavigator node = GetFromXpathAlle(xpath, wb1);
            if (node == null) {
                System.Windows.MessageBox.Show("!!! Fehler GetZeitFromXpath() Node aus SelectSingleNode ist null!!! " + " " + url);
                return dt;
            }
            int i1;
            string str2 = "";
            string str1 = "";
            try {
                hdoc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml);
                XPathNavigator docNav = hdoc.CreateNavigator();
                node = docNav.SelectSingleNode(xpath);
                if (node == null) {
                    Console.WriteLine("GetZeitFromXpath node==null!!!:" + url);
                    Console.WriteLine("GetZeitFromXpath hdoc.DocumentNode:" + hdoc.DocumentNode.ToString());
                    // NOCH System.Windows.MessageBox.Show("!!! Fehler GetZeitFromXpath() Node aus SelectSingleNode ist null!!! " + " " + url);
                    return dt;
                }
                str2 = node.TypedValue.ToString().Trim();        // 06.03.2015
                str1 = str2;
                //Console.WriteLine("GetZeitFromXpath xpath:{0} url:{1} str1:{2} str2:{3} ", xpath, url, str1, str2);
                //if (url.Contains("Aramea")) {
                //    Console.WriteLine("GetZeitFromXpath Aramea");
                //}
                if (url.ToString().Contains("maxblue"))               // 06.03.15               
                {
                    //Console.WriteLine("GetZeitFromXpath maxblue");
                    i1 = str2.IndexOf('.');
                    if (i1 >= 0)
                        str2 = str2.Substring(i1 - 2);
                    if (str2.Length == 8) {
                        str1 = str2.Substring(0, 6) + "20" + str2.Substring(6);
                        str2 = str1;
                    }
                } else if (url.ToString().Contains("/snapshot")) {
                    //Console.WriteLine("GetZeitFromXpath /snapshot");
                } else if (url.ToString().Contains("/preise"))                   // 11,17&nbsp;€ 10:45 Uhr (09.03.)                
                  {
                    //Console.WriteLine("GetZeitFromXpath /preise");
                    i1 = str2.IndexOf('.');
                    if (i1 >= 0)
                        str2 = str2.Substring(i1 - 2);              // 09.03.) 
                    i1 = str2.IndexOf(')');
                    if (i1 >= 0) {
                        string str = DateTime.Now.ToString();
                        str2 = str2.Substring(0, i1) + str.Substring(6, 4);
                        i1 = str1.IndexOf(':');
                        str2 += " " + str1.Substring(i1 - 2, 5);              // 10:45
                    }
                } else if (url.ToString().Contains("/zertifikate"))              // Boerse Online  10:29 (26.03.2015)               
                  {
                    //Console.WriteLine("GetZeitFromXpath /zertifikate");
                    i1 = str2.IndexOf('.');
                    if (i1 >= 0)
                        str2 = str2.Substring(i1 - 2, 10) + " " + str2.Substring(0, 5);
                } else if (url.ToString().Contains("boersennews.de/markt/anleihen/")) {
                    //Console.WriteLine("GetZeitFromXpath boersennews.de/markt/anleihen/ url-->" + url);
                    // ALT: NORDDEUTSCHE LANDESBANK -GZ- NACHR.-MTN… Anleihenkurs:&nbsp;12:08 (Hannover, 15 Min. verzögert)
                    // NEU: NORDDEUTSCHE LANDESBANK -GZ- NACHR.-MTN… Anleihenkurs:&nbsp;30.10. (Stuttgart, 15 Min. verzögert)
                    // http://www.boersennews.de/markt/anleihen/norddeutsche-landesbank-girozentrale-anleihe-de000nlb2hc4/66328231/profile
                    if (str2.Contains("Hannover")) {
                        int i2 = str2.IndexOf(':');
                        i1 = str2.IndexOf(':', i2 + 1);
                        if (i1 >= 0) {
                            string strHH = str2.Substring(i1 - 2, 2);  // 12:08
                            string strmm = str2.Substring(i1 + 1, 2);  //    08                        
                            string strDateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");          // "dd MMM HH:mm:ss", 
                            str2 = strDateTime.Substring(0, 10) + " " + strHH + ":" + strmm + ":00";    // 2015.09.29 12:22:00                       
                        }
                    }
                    if (str2.Contains("Stuttgart"))     // Anleihenkurs:&nbsp;30.10. (Stuttgart,
                    {
                        int i2 = str2.IndexOf(':');
                        i1 = str2.IndexOf('.', i2 + 1);     // Zwischen 30 und 10
                        if (i1 >= 0) {
                            string strDateTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
                            string stryyyy = strDateTime.Substring(0, 4) + ".";     // 2015
                            string strMM = str2.Substring(i1 + 1, 2);               // 10                        
                            string strdd = str2.Substring(i1 - 2, 2);               // 30                        
                            str2 = strDateTime.Substring(0, 10) + " " + "12" + ":" + "00" + ":00";    // 2015.09.29 12:00:00                       
                        }
                    }
                } else if (url.ToString().Contains("www.sbroker.de")) {      // 02.08.16 / 15:15
                    int i2 = str2.IndexOf('/');
                    if (i2 >= 0) {
                        string str3 = str2.Substring(0, i2 - 1) + str2.Substring(i2 + 1);
                        str2 = str3;
                    }
                }
                dt = Convert.ToDateTime(str2);
            } catch (Exception ex) {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!GetZeitFromXpath() Fehler.  str1:{0} str2:{1} url:{2}", str1, str2, url.ToString());
                System.Windows.MessageBox.Show("GetZeitFromXpath() catch Fehler: " + ex + " " + url);
            }
            return dt;
        }
        public Single GetAendFromXpath(string xpath, WebBrowser wb1, Uri url, PortFol datarow) {
            Single aend = -1; ;
            XPathNavigator node = GetFromXpathAlle(xpath, wb1);
            if (node == null) {
                System.Windows.MessageBox.Show("!!! Fehler GetAendFromXpath() Node aus SelectSingleNode ist null!!! " + " " + url);
                return aend;
            }
            string str2 = "";
            try {
                str2 = (node.TypedValue.ToString()).Trim();       // +0,18 (+0,16%)&nbsp;  Oder: 0,21 EUR / 0,20%
                string str1 = str2;
                int i1 = -1;
                i1 = str2.IndexOf('±');
                if (i1 >= 0)
                    str2 = str2.Substring(i1 + 1);
                i1 = str2.IndexOf(" / ");
                if (i1 >= 0)
                    str2 = str2.Substring(i1 + 3);
                i1 = str2.IndexOf('&');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf(' ');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf('%');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf(' ');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                aend = Convert.ToSingle(str2);      // ±
            } catch (Exception ex) {
                //Console.WriteLine("{0} {1} {2}", ex, url, str2);
                System.Windows.MessageBox.Show("GetAendFromXpath() catch Fehler: " + ex + " " + url);
            }
            return aend;
        }
        public XPathNavigator GetFromXpathAlle(string xpath, WebBrowser wb1) {
            XPathNavigator node = null;
            try {
                hdoc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml);
                XPathNavigator docNav = hdoc.CreateNavigator();
                node = docNav.SelectSingleNode(xpath);
                // /body[1]/div[2]/div[3]/div[1]/div[7]/div[1]/div[2]/div[1]/table[1]/tbody[1]/tr[1]/th[1]
                if (node == null) {
                    Console.WriteLine("GetFromXpathAlle node==null!!!:" + wb1.DocumentTitle);
                    Console.WriteLine("GetFromXpathAlle hdoc.DocumentNode:" + hdoc.DocumentNode.ToString());
                    System.Windows.MessageBox.Show("!!! Fehler GetFromXpathAlle() Node aus SelectSingleNode ist null!!! " + " " + wb1.Url);
                    return node;
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("GetFromXpathAlle() catch Fehler: " + ex);
                return node;
            }
            return node;
        }
        public Single GetSharpFromXpath(string xpath, WebBrowser wb1, Uri url, PortFol datarow) {
            Single sharp = -1;
            XPathNavigator node = GetFromXpathAlle(xpath, wb1);
            if (node == null) {
                System.Windows.MessageBox.Show("!!! Fehler GetSharpFromXpath() Node aus SelectSingleNode ist null!!! " + " " + url);
                return sharp;
            }
            try {
                string str2 = node.TypedValue.ToString();   // +1,14%
                string str1 = str2;
                int i1 = str2.IndexOf('&');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf(' ');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                i1 = str2.IndexOf('%');
                if (i1 >= 0)
                    str2 = str2.Substring(0, i1);
                try {
                    sharp = Convert.ToSingle(str2);             // 331,19&nbsp;€
                } catch (Exception ex) {
                    System.Windows.Forms.MessageBox.Show("Fehler in GetSharpFromXpath: " + ex);
                    sharp = 0;
                }
                //txtBox.AppendText(Environment.NewLine + "GetSharpFromXpath() sharp: " + sharp.ToString());
                // }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("GetSharpFromXpath() Fehler: " + ex);
            }
            return sharp;
        }
    }
}