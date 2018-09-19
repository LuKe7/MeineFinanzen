// 19.09.2018 KontenSynchronisierenInt2.xaml.cs
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
// NOCH ?? : Im Wertpapier wird also die Nr des VorgabeParameterSatzes und Url 3.Teil gespeichert.
// PrintDom...
using DataSetAdminNS;
using HtmlAgilityPack;
using MeineFinanzen.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenInt2 : Window, INotifyPropertyChanged, IEditableObject {
        public CollWertpapSynchro _wertpapsynchro = null;
        // public static UrlVerwalten Vorg = new UrlVerwalten();
        // public static List<UrlVerwalten> liVorg = new List<UrlVerwalten>();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        public void Ausführen() {
            txtOben.Clear();
            //wb1.ScriptErrorsSuppressed = true;
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
            PrintTxtUnten(dt.ToString());
            // DataColumn[] keys = new DataColumn[1];
            // keys[0] = DataSetAdmin.dtPortFol.Columns["WPIsin"];
            // DataSetAdmin.dtPortFol.PrimaryKey = keys;       // NOCH
            Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;
            //wb1.ScriptErrorsSuppressed = true;
            //wb1.ScrollBarsEnabled = true;
            //wb1.GoHome();
            //wb1.Navigate(new Uri("https://www.google.de/"));        // Löst kein wb1-DocumentCompleted aus
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
            do {              
            } while (LosGehts(true));
        }
        private bool LosGehts(bool ergebnis) {
            if (ergebnis) {
                SetDataRowColor("3");
                lfdPoFo++;
            }
            if ((lfdPoFo + 1) > _wertpapsynchro.Count) {
                Close();
                return false;
            }
            wpsyn = _wertpapsynchro[lfdPoFo];
            HtmlAgilityPack.HtmlDocument doc = fx_read_Page(new Uri(wpsyn.WPSURL));
            SearchWebPage(doc);
            return true;
        }
        private bool SearchWebPage(HtmlAgilityPack.HtmlDocument doc) {
            DoEvents();
            String pattSharpe = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
            String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
            String pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999
            String pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";              // 99:99:99                                 

            Regex regBetrag = new Regex(pattBetrag, RegexOptions.IgnoreCase);
            Regex regDatum = new Regex(pattDatum, RegexOptions.IgnoreCase);
            Regex regZeit = new Regex(pattZeit, RegexOptions.IgnoreCase);
            Regex regSharpe = new Regex(pattSharpe, RegexOptions.IgnoreCase);

            string LetzteInnerText = null;
            /* foreach (HtmlElement elm in wb 1.Document.All) {
                if (elm.GetAttribute("className") == "row quotebox") {
                    LetzteElement = elm;                    
                }
            } */

            //HtmlAgilityPack.HtmlDocument doc = fx_read_Page(sURL);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class=\"row quotebox\"]");

            //LetzteElement.InnerText = "114,24EUR \r\n\r\n-0,07EUR \r\n\r\n-0,06% \r\n\r\n\r\n09.08.2018 \r\n\r\nNAV \r\nNAV"            
            // string strInnerText = Regex.Replace(LetzteInnerText, "[\x00-\x1F]+", "/");
            // 111,62EUR 0,04EUR 0,04% 13.09.2018 
            // NAV NAV
/* ---- Mit den Eigenschaften eines XmlNode-Objekts navigieren ----
Eigenschaft 	Beschreibung
Attributes      Diese Eigenschaft ruft eine Auflistung vom Typ XmlAttributCollection ab, die die Attribute des aktuellen Knotens enthält.
ChildNodes      Ruft alle untergeordneten Knoten des Knotens ab.
FirstChild      Ruft das erste untergeordnete Element des aktuellen Knotens ab.
HasChildNodes   Ruft einen Wert ab, der angibt, ob dieser Knoten über untergeordnete Knoten verfügt.
Item            Ruft das erste untergeordnete Element mit dem angegebenen Bezeichner ab.
LastChild       Ruft das letzte untergeordnete Element des aktuellen Knotens ab.
NextSibling     Ruft den nächsten nebengeordneten Knoten ab, der dem aktuellen Knoten folgt.
ParentNode      Ruft das übergeordnete Element des aktuellen Knotens ab.
PreviousSibling Ruft den vorhergehenden nebengeordneten Knoten des aktuellen Knotens ab.*/
            Console.WriteLine("========== " + wpsyn.WPSName + " ==========");
            Console.WriteLine("nodes.Count: " + nodes.Count);
            foreach (HtmlNode node1 in nodes) {
                Console.WriteLine("Attributes   : " + node1.Attributes.ToString());
                Console.WriteLine("Id           : " + node1.Id);
                Console.WriteLine("NextSibling  : " + node1.NextSibling.InnerText);
                Console.WriteLine("FirstChild   : " + node1.FirstChild.ToString());
                Console.WriteLine("ChildNodes   : " + node1.ChildNodes.ToString());
                Console.WriteLine("LastChild    : " + node1.LastChild.ToString());
                Console.WriteLine("ParentNode   : " + node1.ParentNode.ToString());
                Console.WriteLine("PreviousSibl : " + node1.PreviousSibling.ToString());
                //string strInnerText1 = Regex.Replace(node1.InnerText, "[\x00-\x1F]+", "/");// 100,35 % -0,65 - 0,64 % 13:19:02 UhrSTU
                Console.Write("---- node1.ChildNodes.Count: " + node1.ChildNodes.Count);
                Console.WriteLine("---- " + Regex.Replace(node1.InnerText, "[\x00-\x1F]+", "/"));
                foreach (HtmlNode node2 in node1.ChildNodes) {
                    if (node2.ChildNodes.Count > 0) {
                        Console.Write("---- ---- node2.ChildNodes.Count: " + node2.ChildNodes.Count);
                        Console.WriteLine(" " + Regex.Replace(node2.InnerText, "[\x00-\x1F]+", "/"));
                    }
                    foreach (HtmlNode node3 in node2.ChildNodes) {
                        if (node3.ChildNodes.Count > 0) {
                            Console.Write("---- ---- ---- node3.ChildNodes.Count: " + node3.ChildNodes.Count);
                            Console.WriteLine(" " + Regex.Replace(node3.InnerText, "[\x00-\x1F]+", "/"));
                        }
                    }
                }
            }
            /*
            foreach (HtmlNode node1 in nodes) {
                LetzteInnerText = node1.InnerText;
                string strInnerText1 = Regex.Replace(LetzteInnerText, "[\x00-\x1F]+", "/");// 100,35 % -0,65 - 0,64 % 13:19:02 UhrSTU
                string[] strZeilenSplit1 = strInnerText1.Split('/');
                Console.WriteLine("strInnerText1         : " + strInnerText1);
                Console.WriteLine("node1.ChildNodes.Count: " + node1.ChildNodes.Count);
                foreach (HtmlNode node2 in node1.ChildNodes) {
                    Console.WriteLine("---- node2.InnerText:        " + node2.InnerText);
                    Console.WriteLine("---- node2.ChildNodes.Count: " + node2.ChildNodes.Count);
                }
                if (wpsyn.WPSType == 70) {
                    Console.Write("{0,-50} ", wpsyn.WPSName);
                    foreach (HtmlNode node2 in node1.ChildNodes) {
                        Console.WriteLine("node2.ChildNodes.Count: " + node2.ChildNodes.Count);
                    }
                }
            }  
            */

            string strInnerText = Regex.Replace(LetzteInnerText, "[\x00-\x1F]+", "/");// 100,35 % -0,65 - 0,64 % 13:19:02 UhrSTU
            string[] strZeilenSplit = strInnerText.Split('/');
          
            //Console.WriteLine("Kurs: {0,12} Diff-Euro: {1,12} Diff-%: {2,12} Datum: {3,14}",
            //      strZeilenSplit[0], strZeilenSplit[1], strZeilenSplit[2], strZeilenSplit[3]);           

            string Währung = "EUR";
            if (strZeilenSplit[1].Contains("USD"))

                // https://www.finanzen.net/anleihen/nlb2hc-norddeutsche-landesbank-girozentrale-anleihe

                Währung = "USD";

            Match match0 = regBetrag.Match(strZeilenSplit[1]);
            if (match0.Success) {
                wpsyn.WPSKurs = Convert.ToDouble(match0.Value);
                Console.Write("Kurs: {0} {1,-8}", Währung, wpsyn.WPSKurs);
            }
            Match match2 = regBetrag.Match(strZeilenSplit[3]);
            if (match2.Success) {
                wpsyn.WPSProzentAenderung = Convert.ToDouble(match2.Value);
                Console.Write(" Diff-%: {0,-5}", wpsyn.WPSProzentAenderung);
            }
            Match match3 = regDatum.Match(strZeilenSplit[4]);
            if (match3.Success) {
                Console.WriteLine(" Datum: " + match3.Value);
                wpsyn.WPSKursZeit = Convert.ToDateTime(match3.Value);
            } else {

                Match match4 = regZeit.Match(strZeilenSplit[3]);
                if (match4.Success) {
                    DateTime dt = DateTime.Now;
                    wpsyn.WPSKursZeit = Convert.ToDateTime(dt.Day + "." + dt.Month + "." + dt.Year + " " + match4.Value);
                    Console.WriteLine(" Zeit: " + wpsyn.WPSKursZeit);
                } else {
                    System.Windows.MessageBox.Show("Fehler KursDatum/-Zeit");
                }
            }
            return true;
        }
        private void SetDataRowColor(string strColor) {
            wpsyn.WPSColor = strColor;
            dgvUrls.ItemsSource = null;
            dgvUrls.ItemsSource = _wertpapsynchro;
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();
        }
        private HtmlAgilityPack.HtmlDocument fx_read_Page(Uri sURL) {
            WebRequest objRequest = WebRequest.Create(sURL);
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            Stream objDataStream = objResponse.GetResponseStream();
            StreamReader TextReader = new StreamReader(objDataStream);
            string sHTML = TextReader.ReadToEnd();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(sHTML);
            return doc;
        }
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
        /* private void PrintDomBegin(WebBrowser wb1, HtmlElement element) {
            if (wb1.Document != null) {
                HtmlElementCollection elemColl = null;
                System.Windows.Forms.HtmlDocument doc = wb1.Document;
                if (doc != null) {
                    StringBuilder str7 = new StringBuilder("--- Start ---");
                    addText(str7);
                    elemColl = doc.GetElementsByTagName("body");
                    //string str = PrintDom(elemColl, new System.Text.StringBuilder(), 0);
                    string str = PrintDomElem(element, new StringBuilder(), 0);
                    Console.WriteLine("{0}", str);
                }
            }
        }
        private string PrintDomElem(HtmlElement elem, StringBuilder returnStr, Int32 depth) {
            StringBuilder str = new StringBuilder("");
            int ll;
            string strText;
            //foreach (HtmlElement elem in elemColl)
            {
                string elemName;
                string elemId;
                string elemDiv;
                string elemlass;
                elemName = elem.GetAttribute("name");
                elemId = elem.GetAttribute("id");
                elemDiv = elem.GetAttribute("div");
                elemlass = elem.GetAttribute("class");
                str.Append(' ', depth * 4);
                if (elem.InnerHtml != null) {
                    ll = elem.InnerHtml.Length;
                    if (ll > 80)
                        ll = 80;
                    strText = elem.InnerHtml.Substring(0, ll);
                    strText = Regex.Replace(strText, "[\x00-\x1F]+", ".");
                } else {
                    strText = "";
                    ll = 0;
                }
                if (strText != "") {
                    str.Append(elemName + ":" + elemId + ":" + elemDiv + ":" + elemlass + ":" + elem.TagName + "(" + depth + ")      ---> " + strText);
                    returnStr.AppendLine(str.ToString());
                    addText(str);
                }
                if (elem.CanHaveChildren) {
                    PrintDom(elem.Children, returnStr, depth + 1);
                }
                str.Remove(0, str.Length);
            }
            return (returnStr.ToString());
        }
        private void PrintDomBegin() {
            if (wb1.Document != null) {
                HtmlElementCollection elemColl = null;
                System.Windows.Forms.HtmlDocument doc = wb1.Document;
                if (doc != null) {
                    elemColl = doc.GetElementsByTagName("body");    // HTML");
                    String strDom = PrintDom(elemColl, new StringBuilder(), 0);
                    //wb1.DocumentText = str;
                }
            }
        }
        private string PrintDom(HtmlElementCollection elemColl, System.Text.StringBuilder returnStr, Int32 depth) {
            StringBuilder str = new StringBuilder();
            foreach (HtmlElement elem in elemColl) {
                string elemName;

                elemName = elem.GetAttribute("ID");
                if (elemName == null || elemName.Length == 0) {
                    elemName = elem.GetAttribute("name");
                    if (elemName == null || elemName.Length == 0) {
                        elemName = "<nn>";
                    }
                }

                str.Append(' ', depth * 4);
                str.Append(elemName + ": " + elem.TagName + "(L " + depth + ")");
                returnStr.AppendLine(str.ToString());

                if (elem.CanHaveChildren) {
                    PrintDom(elem.Children, returnStr, depth + 1);
                }

                str.Remove(0, str.Length);
            }

            return (returnStr.ToString());
        }  */
        private void addText(StringBuilder str) {
            if (str.Length > 0)
                txtBox.AppendText(Environment.NewLine + str);
        }
        private void AddTextStr(string str) {
            if (str.Length > 0)
                txtBox.AppendText(Environment.NewLine + str);
        }
        /*private bool SearchWebPage_ALT() {  // Kommt von wb1_DocumentCompleted. Also eine WebPage.
                DoEvents();
                if (wb1.Document == null)
                    return false;
                //PrintDomBegin();
                //DisplayMetaDescription();
                String pattSharpe = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
                String pattBetrag = @"(\d+)([,])(\d+)(\d+)";                                // 9,99
                String pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999
                String pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";              // 99:99:99
                HtmlElementCollection elemColl = null;
                HtmlDocument doc = wb1.Document;
                if (doc != null) {
                    addTextStr("--- Start SearchWebPage() ---" + wb1.Document.Url);
                }
                elemColl = doc.GetElementsByTagName("body");    // von <body>  bis </body>
                string strInnerText = "";
                string strKursdatum = "";
                string strKurszeit = "";
                string strKurs = "";
                string strSharpe = "";
                string strZeilePlus = "";
                string[] strarr1;
                string[] strarr2;
                string[] strarr3;
                string strf = "";
                HtmlElement LetzteElement = null;
                foreach (HtmlElement elm in wb1.Document.All)
                    if (elm.GetAttribute("className") == "row quotebox")
                        LetzteElement = elm;
                //LetzteElement.InnerText = "114,24EUR \r\n\r\n-0,07EUR \r\n\r\n-0,06% \r\n\r\n\r\n09.08.2018 \r\n\r\nNAV \r\nNAV"
                strInnerText = LetzteElement.InnerText;                      
                strInnerText = Regex.Replace(strInnerText, "[\x00-\x1F]+", "/");
                string[] strZeilenTeile1 = strInnerText.Split('/');
                Console.Write("Kurs: {0} Diff-Euro: {1} Diff-%: {2} Datum: {3}", strZeilenTeile1[0], strZeilenTeile1[1], strZeilenTeile1[2], strZeilenTeile1[3]);
                //Console.Write("'row quotebox'{0,2} {1,5} {2,-80}", LetzteElement.Children.Count, LetzteElement.InnerHtml.Length, wb1.Document.Url);
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
                    / *foreach (string strZeile in strZeilenTeile) {                       // Zeilen in der Box.
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
                        if (strZeile.StartsWith("Sharpe Ratio")) {
                            // <h2 class="box-headline">Sharpe Ratio</h2>
                            Console.WriteLine("Sharpe Ratio: " + strZeile + " " + nn);
                            string strZeile2 = strZeilenTeile[nn++];                       
                            // 6 Monate - 1,94
                            strZeilePlus += String.Format(" nn:{0,3} {1} =", nn, strZeile2);
                            foreach (Match m in Regex.Matches(strZeile, pattSharpe))
                                strSharpe += String.Format("{0} ", m.Value);
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
                    }                   // foreach (string strZeile in strZeilenTeile)      * /
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
         * */
    }
}