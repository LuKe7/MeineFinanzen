// 29.10.2018 KontenSynchronisierenInt.xaml.cs
// _wertpapsynchro wird aus dtPortFol gefüllt.
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
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using MessageBox = System.Windows.MessageBox;
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenInt : Window, INotifyPropertyChanged, IEditableObject {
        public CollWertpapSynchro _wertpapsynchro = null;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        internal void NotifyPropertyChanged(string propertyName) {
            Console.WriteLine("NotifyPropertyChanged: " + propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
        string _strFile = Helpers.GlobalRef.g_Ein.myDocPfad + @"\MeineFinanzen\MyDepot\Daten";
        // NOCH Helpers.GlobalRef.g_Ein.myDepotPfad + @"Daten\";       
        DataTable dtPortFol = new DataTable();
        public PortFol _portfol = new PortFol();
        private int lfdPoFo = -1;
        WertpapSynchro wpsyn = new WertpapSynchro();       
        private int n2;
        private delegate void EmptyDelegate();
        SearchValues SearchV = new SearchValues();
        public KontenSynchronisierenInt() {
            InitializeComponent();           
            _wertpapsynchro = (CollWertpapSynchro)Resources["wertpapsynchro"];
            PrintTxtUnten("start -Statustext-           ");
            DataContext = this;
        }
        public void Ausführen() {
            txtUnten.Clear();
            txtUnten.FontSize = 10;
            txtUnten.FontFamily = new FontFamily("Courier New, Verdana");
            PrintTxtUnten("Start -KontenAusInternetSynchronisieren-2-");
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
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // dtPortFol geholt.                
            foreach (DataRow dr in dtPortFol.Rows) {
                Wertpapierklasse typeid = (Wertpapierklasse)dr["WPTypeID"];

                //if (typeid != Wertpapierklasse.Anleihe)
                //    continue;

                if (typeid < Wertpapierklasse.MinWertpap || typeid > Wertpapierklasse.MaxWertpap)
                    continue;
                if (dr["WPISIN"].ToString().Length != 12)
                    continue;
                _wertpapsynchro.Add(new WertpapSynchro {
                    WPSAnzahl = Convert.ToSingle(dr["WPAnzahl"]),
                    WPSName = dr["WPName"].ToString(),
                    WPSKursZeit = Convert.ToDateTime(dr["WPStand"]),
                    WPSISIN = dr["WPISIN"].ToString(),

                    WPSURL = dr["WPUrlText"].ToString(),
                    WPSKurs = Convert.ToSingle(dr["WPKurs"]),
                    WPSProzentAenderung = Convert.ToSingle(dr["WPProzentAenderung"]),
                    WPSType = (Wertpapierklasse)Convert.ToInt32(dr["WPTypeID"]),
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
            HtmlDocument doc = Fx_read_Page(new Uri(wpsyn.WPSURL));
            SearchWebPage(doc, wpsyn);
            n2 = 0;
            return true;
        }
        private SearchValues SetSearchValues(WertpapSynchro wpsyn) {            
            if (wpsyn.WPSType == Wertpapierklasse.Anleihe) {
                SearchV.SelectNodesValue1 = "box table-quotes pricebox";
                SearchV.textValue1 = "Aktueller Kurs";
                SearchV.textValue2 = "Kurs";
                SearchV.textSharpeRatio1 = "box";
                SearchV.textSharpeRatio2 = "Kupondaten";

            } else if (wpsyn.WPSType >= Wertpapierklasse.MinWertpap && wpsyn.WPSType <= Wertpapierklasse.MaxWertpap) {
                SearchV.SelectNodesValue1 = "box";
                SearchV.textValue1 = "aktueller Kurs";
                SearchV.textValue2 = "Kurs";
                SearchV.textSharpeRatio1 = "box";
                SearchV.textSharpeRatio2 = "Sharpe Ratio";
            } else {
                MessageBox.Show("Fehler in KontenSynchronisieren2 GetSearchVaues(): Fehlerhafter WPSType: " + wpsyn.WPSType);
            }
            return SearchV;
        }
        private void SearchWebPage(HtmlDocument doc, WertpapSynchro wpsyn) {
            // Finden einer beliebigen dezimalzahlen Ganzzahl mit einem optionalen Vorzeichen:
            // [+-]?\b[0-9]+\b
            // ok : string pattBetrag1 = @"(\d+)([,])(\d+)(\d+)";                        //     123,99
            string pattBetrag1 = @"(-?)([0-9]+)([,])([0-9]+)([0-9]+)";                   //     123,99
            string pattBetrag2 = @"(\d+)([.])(\d+)([,])(\d+)(\d+)";                      //   1.239,99
            string pattDatum = @"(\d+)(\d+)([.])(\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";    // 99.99.9999
            string pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";
            Regex regBetrag1 = new Regex(pattBetrag1, RegexOptions.IgnoreCase);
            Regex regBetrag2 = new Regex(pattBetrag2, RegexOptions.IgnoreCase);
            Regex regDatum = new Regex(pattDatum, RegexOptions.IgnoreCase);
            Regex regZeit = new Regex(pattZeit, RegexOptions.IgnoreCase);
            Match matchZeit = null;
            SetSearchValues(wpsyn); // NOCH in xml-Datei...

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='" + SearchV.SelectNodesValue1 + "']");

            string strTitel = string.Format("========== " + wpsyn.WPSName + " nodes.Count: " + nodes.Count);
            Console.WriteLine(strTitel);
            PrintTxtUnten(strTitel);
            PrintTxtLinks(strTitel);
            string strReg = null;
            bool nixmehr = false;
            foreach (HtmlNode node1 in nodes) {
                if (nixmehr)
                    continue;
                if (!node1.InnerText.Contains(SearchV.textValue1))  // 'aktueller Kurs'
                    continue;
                matchZeit = null;
                foreach (HtmlNode node2 in node1.SelectNodes(".//tr")) {   // Nodes 'tr' in der Klasse 'box' mit dem Text 'aktueller Kurs'
                                                                           // The <tr> tag defines a row in an HTML table.
                                                                           // A <tr> element contains one or more<th> or<td> elements.
                                                                           // Kurs
                                                                           // 106,56 EUR - 0,51 EUR - 0,48 %
                                                                           /* <tr>
                                                                               <td><strong>Kurs</strong></td>
                                                                               <td class="text-right">236,18 EUR <span class="colorGreen">0,68 EUR</span> <span class="colorGreen">0,29%</span></td>
                                                                             </tr>
                                                                           </table>
                                                                           <table class="table table-small">
                                                                           <tr>
                                                                               <td><strong>Kurszeit</strong></td>
                                                                               <td class="text-right">12:00:31</td>
                                                                           </tr>
                                                                           <tr>
                                                                               <td><strong>Kursdatum</strong></td>
                                                                               <td class="text-right">24.10.2018</td>
                                                                           </tr> */
                    if (nixmehr)
                        continue;
                    strReg = Regex.Replace(node2.InnerText, "[\x00-\x20]+", "/");   // /Kurs/106,56/EUR/-0,51/EUR/-0,48%/
                    //if (strReg != "")
                        //Console.WriteLine("InnerText: " + strReg);      // 2.mal: /Kursdatum/05.10.2018/  oder Kurszeit                                                                       
                    char[] charSeparators = new char[] { '/' };
                    string[] StrArr = strReg.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (wpsyn.WPSType == Wertpapierklasse.Anleihe) {         // ---- Anleihe ----                    
                        if (StrArr[0].StartsWith("Kurszeit")) {
                            nixmehr = true;
                            string[] StrDatum = StrArr[0].Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                            Match match0 = regDatum.Match(StrDatum[0].Substring(8));
                            if (match0.Success) {
                                wpsyn.WPSKursZeit = Convert.ToDateTime(match0.Value);
                                PrintTxtUnten("Anleihe Datum  : " + wpsyn.WPSKursZeit);
                            } else {
                                MessageBox.Show("Fehler match0 KursDatum.!!!!!!");
                            }
                        } else if (StrArr[0].StartsWith("Kurs")) {
                            Match match1 = regBetrag1.Match(StrArr[0]);
                            if (match1.Success) {
                                Single kurs = Convert.ToSingle(match1.Value);
                                wpsyn.WPSKurs = kurs;
                                PrintTxtUnten("Anleihe Kurs: " + kurs);
                            }
                            // NOCH %
                        }
                    } else {
                        if (StrArr[0] == "Kurs") {                          // 1.mal
                            string StrBetrag = StrArr[1];
                            Match match0 = regBetrag2.Match(StrBetrag);
                            if (match0.Success) {
                                wpsyn.WPSKurs = Convert.ToSingle(match0.Value);
                                string Währung = "EUR";
                                if (StrArr.Length > 1)
                                    if (StrArr[2] == "USD")
                                        Währung = "USD";
                                PrintTxtUnten("Kurs: " + Währung + " " + wpsyn.WPSKurs);
                            } else {
                                match0 = regBetrag1.Match(StrBetrag);
                                if (match0.Success) {
                                    wpsyn.WPSKurs = Convert.ToSingle(match0.Value);
                                    string Währung = "EUR";
                                    if (StrArr.Length > 1)
                                        if (StrArr[2] == "USD")
                                            Währung = "USD";
                                    PrintTxtUnten("Kurs(2): " + Währung + " " + wpsyn.WPSKurs);
                                } else {
                                    MessageBox.Show("Fehler Währung!!!!!!!!!!!!! ");
                                }
                            }
                            Match match1 = regBetrag1.Match(StrArr[3]);
                            if (match1.Success) {
                                Single diffEur = Convert.ToSingle(match1.Value);
                                PrintTxtUnten("Diff-EUR: " + diffEur);
                            } else {
                                MessageBox.Show("Fehler Diff-EUR!!!!!!!!!!!!! ");
                            }
                            Match match2 = regBetrag1.Match(StrArr[5]);
                            if (match2.Success) {
                                Single diffProz = Convert.ToSingle(match2.Value);
                                wpsyn.WPSProzentAenderung = diffProz;
                                PrintTxtUnten("Diff-%: " + diffProz);
                            } else {
                                MessageBox.Show("Fehler Diff-%!!!!!!!!!!!!! ");
                            }
                        } else if (StrArr[0] == "Kursdatum") {
                            Match match3 = regDatum.Match(StrArr[1]);
                            if (match3.Success) {
                                wpsyn.WPSKursZeit = Convert.ToDateTime(match3.Value);
                                if (matchZeit != null)
                                    wpsyn.WPSKursZeit = Convert.ToDateTime(match3.Value + " " + matchZeit.Value);
                                PrintTxtUnten(" Datum  : " + wpsyn.WPSKursZeit);
                                continue;
                            } else {
                                MessageBox.Show("Fehler match3 KursDatum.!!!!!!");
                                continue;
                            }
                        } else if (StrArr[0] == "Kurszeit") {
                            matchZeit = regZeit.Match(strReg);
                            if (matchZeit.Success) {   // Zeit
                                DateTime dt = wpsyn.WPSKursZeit;
                                wpsyn.WPSKursZeit = Convert.ToDateTime(dt.Day + "." + dt.Month + "." + dt.Year + " " + matchZeit.Value);
                                PrintTxtUnten(" Zeit: " + wpsyn.WPSKursZeit);
                                continue;
                            } else
                                MessageBox.Show("Fehler match4 Kurszeit.!!!!!!");
                        }   // Kurszeit
                    }   // !Anleihe
                }   // foreach node2
            }   // foreach node1
            n2 = 0;
            float Sharpe = GetSharpeRatio(doc, SearchV.textSharpeRatio1, SearchV.textSharpeRatio2);
            wpsyn.WPSSharpe = Sharpe;
        }
        private float GetSharpeRatio(HtmlDocument doc, string SelectNodesValue1, string textValue) {
            string pattSharpe = @"(\d+)([,])(\d+)(\d+)";                                 //       9,99
            Regex regSharpe = new Regex(pattSharpe, RegexOptions.IgnoreCase);
            n2 = 0;
            string[,] StrSharpeArr = new string[12, 2];
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='" + SelectNodesValue1 + "']");
            foreach (HtmlNode node1 in nodes) {                
                ++n2;
                if (!node1.InnerText.Contains(textValue))
                    continue;
                string strTitel = string.Format("---------- Sharpe: Die Suche nach 'class={0}' hat {1} Nodes ergeben.  Die {2}.node2 mit '{3}'.",
                    SelectNodesValue1, nodes.Count, n2, textValue);
                PrintTxtUnten(strTitel);
                Console.WriteLine(strTitel);
                int nn = -1;
                try {
                    foreach (HtmlNode node2 in node1.SelectNodes(".//tr")) {       
                        if (node2.ChildNodes.Count > 1) {
                            char[] charSeparators = new char[] { '/' };
                            string StrInner = Regex.Replace(node2.InnerText, "[\x00-\x1F]+", "/");                            
                            string[] StrArr = StrInner.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);    // Kupon in %4,750
                            string strZeile = string.Format("{0} == ", StrArr[0]);
                            StrSharpeArr[++nn, 0] = StrArr[0];
                            //Console.Write(strZeile);
                            if (textValue.Contains("Sharpe")) {
                                if (StrArr.Length > 1) {
                                    StrSharpeArr[nn, 1] = StrArr[1];
                                    strZeile += string.Format("{0}", StrArr[1]);
                                    //Console.Write("{0}", StrArr[1]);
                                } else {
                                    StrSharpeArr[nn, 1] = "---";
                                    strZeile += "---";
                                    //Console.Write("---");
                                }

                            } else {
                                //Console.WriteLine("Kupondaten");
                            }
                            PrintTxtUnten(strZeile);
                            //Console.WriteLine();
                        }
                    }
                } catch (NullReferenceException ex) {
                    MessageBox.Show("---- Fehler node2 in node1" + ex);
                }
            }
            if (textValue.Contains("Sharpe")) {
                /*  6 Monate == ---
                    1 Jahr  == -4,03
                    3 Jahre == 0,36
                    5 Jahre == 0,51
                   10 Jahre == ---*/
                Match match7 = regSharpe.Match(StrSharpeArr[2, 1]);
                if (match7.Success) {
                    Single sharpe = Convert.ToSingle(match7.Value);
                    wpsyn.WPSSharpe = sharpe;
                    PrintTxtUnten("Sharpe: " + sharpe);
                } else {
                    Match match8 = regSharpe.Match(StrSharpeArr[3, 1]);
                    if (match8.Success) {
                        Single sharpe = Convert.ToSingle(match8.Value);
                        wpsyn.WPSSharpe = sharpe;
                        PrintTxtUnten("Sharpe: " + sharpe);
                    } else
                        wpsyn.WPSSharpe = 0;
                    // MessageBox.Show("Sharpe Fehler !!!!!!!!!!!!! ");
                }
            } else {                // Kupondaten

            }
            return wpsyn.WPSSharpe;
        }
        private void SetDataRowColor(string strColor) {
            wpsyn.WPSColor = strColor;
            dgvUrls.ItemsSource = null;
            dgvUrls.ItemsSource = _wertpapsynchro;
            dgvUrls.EnableRowVirtualization = false;
            dgvUrls.UpdateLayout();
        }
        private HtmlDocument Fx_read_Page(Uri sURL) {
            WebRequest objRequest = WebRequest.Create(sURL);
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            Stream objDataStream = objResponse.GetResponseStream();
            StreamReader TextReader = new StreamReader(objDataStream, Encoding.Default);
            string sHTML = TextReader.ReadToEnd();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(sHTML);
            return doc;
        }
        private void AcceptButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e) {
            MessageBoxResult result = MessageBox.Show("Mit UpDate?", " Nur Beenden",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question,
                              MessageBoxResult.No);
            lfdPoFo = _wertpapsynchro.Count + 7777;
            if (result == MessageBoxResult.Yes) {
                UpdatedtPortFol();               
            }
        }
        private void UpdatedtPortFol() {
            // WPKurs WPStand WPProzentAenderung WPSharpe
            Console.WriteLine("----  UpdatedtPortFol()   WPKurs  WPStand  WPProzentAenderung  WPSharpe  ----");
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];
            string strISIN = "";
            foreach (WertpapSynchro wps in _wertpapsynchro) {
                strISIN = wps.WPSISIN;
                if (strISIN.Length != 12)
                    continue;
                DataRow dtrow = dtPortFol.Rows.Find(strISIN);
                if (wps.WPSKursZeit != (DateTime)dtrow["WPStand"]) {
                    Console.WriteLine("---- KursZeit alt: {0,-12} neu: {1,-12}", (DateTime)dtrow["WPStand"], wps.WPSKursZeit);
                    dtrow["WPStand"] = wps.WPSKursZeit;
                }
                if (wps.WPSKurs != (float)dtrow["WPKurs"]) {
                    Console.WriteLine("---- Kurs     alt: {0,-12} neu: {1,-12}", (float)dtrow["WPKurs"], wps.WPSKurs);
                    dtrow["WPKurs"] = wps.WPSKurs;
                }
                if (wps.WPSProzentAenderung != (float)dtrow["WPProzentAenderung"]) {
                    Console.WriteLine("---- %Änd     alt: {0,-12} neu: {1,-12}", (float)dtrow["WPProzentAenderung"], wps.WPSProzentAenderung);
                    dtrow["WPProzentAenderung"] = wps.WPSProzentAenderung;
                }
                if (wps.WPSSharpe != (float)dtrow["WPSharpe"]) {
                    Console.WriteLine("---- Sharpe   alt: {0,-12} neu: {1,-12}", (float)dtrow["WPSharpe"], wps.WPSSharpe);
                    dtrow["WPSharpe"] = wps.WPSSharpe;
                }
            }
            DataSetAdmin.dtPortFol = dtPortFol;
            DataSetAdmin.DatasetSichernInXml(@"D:\MeineFinanzen");
        }
        protected void DoEvents() {
            //  if (System.Windows.Application.Current != null)
            //      System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
            //* Diese Funktion uebernimmt die Unterbrechnung zur Anzeige und Eventbearbeitung in C#, WPF beim langen Loop Berechnungen
            //* mit einer Dispatcher
            //* EmptyDelegate im Header definieren
            //* using System.Windows.Threading; im Header festlegen   
        }      
        private void PrintTxtUnten(string str) {
            DoEvents();
            txtUnten.AppendText(Environment.NewLine + str);
            txtUnten.ScrollToEnd();
            txtUnten.InvalidateVisual();
            DoEvents();
        }
        private void PrintTxtLinks(string str) {
            txtLinks.AppendText(Environment.NewLine + str);
            txtLinks.ScrollToEnd();
            txtLinks.InvalidateVisual();
            DoEvents();
        }        
    }
    public class SearchValues {
        public string SelectNodesValue1;    
        public string textValue1;
        public string textValue2;
        public string textSharpeRatio1;  // box
        public string textSharpeRatio2;  // Sharpe Ratio

    }
}