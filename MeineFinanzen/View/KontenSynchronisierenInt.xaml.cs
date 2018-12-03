// 29.11.2018 KontenSynchronisierenInt.xaml.cs
// _wertpapsynchro wird aus dtPortFol gefüllt.
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using HtmlAgilityPack;
using DataSetAdminNS;
using MeineFinanzen.Model;
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
        private string _strFile = Helpers.GlobalRef.g_Ein.MyDocPfad + @"\MeineFinanzen\MyDepot\Daten\";
        private string sHTML = "";
        // NOCH Helpers.GlobalRef.g_Ein.myDepotPfad + @"Daten\";       
        DataTable dtPortFol = new DataTable();
        public PortFol _portfol = new PortFol();
        private int lfdPoFo = -1;
        WertpapSynchro wpsyn = new WertpapSynchro();
        private int n2;
        private delegate void EmptyDelegate();
        SearchValues SearchV = new SearchValues();
        private static string pattBetrag = @"\-?[0-9]*[\.]?[0-9]*[,][0-9]*";                         //  -1.234,56                                                                                      
        private static string pattDatumJJJJ = @"(\d+)(\d+)[.](\d+)(\d+)([.])(\d+)(\d+)(\d+)(\d+)";   // 99.99.9999
        private static string pattZeit = @"(\d+)(\d+)([:])(\d+)(\d+)([:])(\d+)(\d+)";                //   99:99:99
        private static string pattDatumJJ = @"(\d+)(\d+)[.](\d+)(\d+)([.])(\d+)(\d+)";               //   99.99.99
        Regex regBetrag = new Regex(pattBetrag, RegexOptions.IgnoreCase);
        Regex regDatumJJJJ = new Regex(pattDatumJJJJ, RegexOptions.IgnoreCase);
        Regex regDatumJJ = new Regex(pattDatumJJ, RegexOptions.IgnoreCase);
        Regex regZeit = new Regex(pattZeit, RegexOptions.IgnoreCase);
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
            DateTime dt = DataSetAdmin.HolenAusXml(Helpers.GlobalRef.g_Ein.MyDataPfad);
            if (dt == null) {
                MessageBox.Show("MeineFinanzen HauptFenster.xaml.cs HauptFenster() Fehler HolenAusXml() DataSetAdmin");
                MessageBox.Show("MyPortfolio Fehler!!  Dateien nicht geladen!!!!");
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
            //DateTime dtLastWriteTime = File.GetLastWriteTime(strx);            
            string _url = "";
            for (int lop = 0; lop < 3; lop++) {
                _url = _strurl[lop];
                string strFile = _strFile + _strWohin[lop] + ".html";
                WebClient wclient1 = new WebClient();
                wclient1.DownloadFile(new Uri(_url), strFile);
            }
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
            //if (wpsyn.WPSName.StartsWith("JPM-")) {
            HtmlDocument doc = Fx_read_Page(new Uri(wpsyn.WPSURL));
            SearchWebPage(doc, wpsyn);
            //}
            n2 = 0;
            return true;
        }
        private SearchValues SetSearchValues(WertpapSynchro wpsyn) {
            if (wpsyn.WPSType == Wertpapierklasse.Anleihe) {
                SearchV.SelectNodesValue1 = "col-sm-7";
                SearchV.SelectNodesValue2 = "row quotebox";
                SearchV.textSharpeRatio1 = "box";
                SearchV.textSharpeRatio2 = "Kupondaten";

            } else if (wpsyn.WPSType >= Wertpapierklasse.MinWertpap && wpsyn.WPSType <= Wertpapierklasse.MaxWertpap) {
                SearchV.SelectNodesValue1 = "col-sm-7";
                SearchV.SelectNodesValue2 = "row quotebox";
                SearchV.textSharpeRatio1 = "box";
                SearchV.textSharpeRatio2 = "Sharpe Ratio";
            } else {
                MessageBox.Show("Fehler in KontenSynchronisieren2 GetSearchVaues(): Fehlerhafter WPSType: " + wpsyn.WPSType);
            }
            return SearchV;
        }
        private void SearchWebPage(HtmlDocument doc, WertpapSynchro wpsyn) {                                 
            SetSearchValues(wpsyn); // NOCH in xml-Datei...
            Console.Write("==== {0}", wpsyn.WPSName);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='" + SearchV.SelectNodesValue1 + "']");
            string strReg0 = Regex.Replace(nodes[0].InnerText, "[\x00-\x20]+", "/");    // /103,26EUR/-1,04EUR/-1,00%/15.11.2018/NAV/NAV/
            Console.WriteLine(" nodes.Count: {0} .InnerText: {1} {2}", nodes.Count, strReg0, "//div[@class='" + SearchV.SelectNodesValue1 + "']");
            PrintTxtUnten("==== " + wpsyn.WPSName + " nodes.Count: " + nodes.Count + " .InnerText: " + strReg0 + "  //div[@class='" + SearchV.SelectNodesValue1 + "']");
            string strPrint = "";
            foreach (HtmlNode node1 in nodes) {
                string strReg1 = Regex.Replace(node1.InnerText, "[\x00-\x20]+", "/");
                Console.WriteLine("    node1.Name: {0} .ChildNodes.Count: {1} .Type: {2} .InnerText: {3}",
                    node1.Name, node1.ChildNodes.Count, node1.NodeType, strReg1);
                foreach (HtmlNode node2 in node1.ChildNodes) {
                    if (node2.NodeType == HtmlNodeType.Text)
                        continue;
                    string strReg2 = Regex.Replace(node2.InnerText, "[\x00-\x20]+", "/");
                    if (strReg2.Length <= 1)
                        continue;
                    Console.WriteLine("        node2.Name: {0} .ChildNodes.Count: {1} .Type: {2} .InnerText: {3}",
                        node2.Name, node2.ChildNodes.Count, node2.NodeType, strReg2);
                    char[] charSeparators = new char[] { '/' };
                    string[] StrArr = strReg2.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    // /98,50%/-0,16/-0,16%/09:42:40/Uhr/HAN/Berlin/Frankfurt/Hamburg/H....
                    if (wpsyn.WPSType == Wertpapierklasse.Anleihe) {                    // ---- Anleihe ----             
                        // /98,50%/-0,16/-0,16%/09:42:40/Uhr/HAN/Berlin/Frankfurt/Hambu
                        charSeparators = new char[] { '%' };
                        Match match1 = regBetrag.Match(StrArr[0]);
                        if (match1.Success) {
                            float kurs = Convert.ToSingle(match1.Value);
                            wpsyn.WPSKurs = kurs;
                            strPrint += "Anleihe Kurs: " + wpsyn.WPSKurs + " ";
                            PrintTxtUnten("Anleihe Kurs: " + kurs);
                        }                        
                        Match match0 = regDatumJJ.Match(StrArr[3]);
                        Match matchZeit = null;
                        if (match0.Success) {
                            wpsyn.WPSKursZeit = Convert.ToDateTime(match0.Value);       // KursDatum
                            strPrint += "Datum: " + wpsyn.WPSKursZeit + " ";
                            PrintTxtUnten("Anleihe Datum  : " + wpsyn.WPSKursZeit);
                        } else {
                            matchZeit = regZeit.Match(StrArr[3]);                       // KursZeit
                            if (matchZeit.Success) {
                                DateTime dt = wpsyn.WPSKursZeit;
                                wpsyn.WPSKursZeit = Convert.ToDateTime(dt.Day + "." + dt.Month + "." + dt.Year + " " + matchZeit.Value);
                                strPrint += "Zeit: " + wpsyn.WPSKursZeit + " ";
                                PrintTxtUnten("Anleihe Zeit  : " + wpsyn.WPSKursZeit);
                            } else
                                MessageBox.Show("Fehler match0 KursDatum.!!!!!!");
                        }
                        // NOCH %                       
                        Console.WriteLine(strPrint);
                        return;
                    } else {            // ---- Wertpapier ----
                        // /101,67EUR/0,18EUR/0,18%/22.11.2018/NAV/NAV/
                        string str = StrArr[0];                                         // /103,26EUR/-1,04EUR/-1,00%/15.11.2018/NAV/NAV/
                        string StrBetrag = str.Substring(0, str.Length - 3);
                        wpsyn.WPSWährung = "EUR";                                         // Währung                        
                        if (StrArr[0].Contains("USD"))
                            wpsyn.WPSWährung = "USD";
                        Match match0 = regBetrag.Match(StrBetrag);                     // Kurs Format 2
                        if (match0.Success) {
                            wpsyn.WPSKurs = Convert.ToSingle(match0.Value);
                            strPrint += "Kurs: " + wpsyn.WPSWährung + " " + wpsyn.WPSKurs + " ";
                        } else {
                            match0 = regBetrag.Match(StrBetrag);                       // Kurs Format 1
                            if (match0.Success) {
                                wpsyn.WPSKurs = Convert.ToSingle(match0.Value);
                                strPrint += "Kurs: " + wpsyn.WPSWährung + " " + wpsyn.WPSKurs + " ";
                            } else
                                MessageBox.Show("Fehler Kurs !!!!!!!!!!!!! ");
                        }
                        Match match1 = regBetrag.Match(StrArr[1]);                     // Diff Euro  (nicht gebraucht)
                        if (match1.Success) {
                            float diffEur = Convert.ToSingle(match1.Value);
                            strPrint += "Diff-EUR: " + diffEur + " ";
                        } else
                            MessageBox.Show("Fehler Diff-EUR!!!!!!!!!!!!! ");
                        Match match2 = regBetrag.Match(StrArr[2]);                     // Diff %
                        if (match2.Success) {
                            float diffProz = Convert.ToSingle(match2.Value);
                            wpsyn.WPSProzentAenderung = diffProz;
                            strPrint += "Diff-%: " + diffProz + " ";
                        } else {
                            MessageBox.Show("Fehler Diff-%!!!!!!!!!!!!! ");
                        }
                        Match match3 = regDatumJJJJ.Match(StrArr[3]);                       // KursDatum
                        Match matchZeit = null;
                        if (match3.Success) {
                            wpsyn.WPSKursZeit = Convert.ToDateTime(match3.Value);
                            if (matchZeit != null)
                                wpsyn.WPSKursZeit = Convert.ToDateTime(match3.Value + " " + matchZeit.Value);
                            strPrint += " Datum  : " + wpsyn.WPSKursZeit;
                        } else {
                            matchZeit = regZeit.Match(StrArr[3]);                       // KursZeit
                            if (matchZeit.Success) {
                                DateTime dt = wpsyn.WPSKursZeit;
                                wpsyn.WPSKursZeit = Convert.ToDateTime(dt.Day + "." + dt.Month + "." + dt.Year + " " + matchZeit.Value);
                                strPrint += "Zeit: " + wpsyn.WPSKursZeit + " ";
                            } else
                                MessageBox.Show("Fehler Match KursDatum/Zeit !!!!!!");
                        }   // else
                        PrintTxtUnten(strPrint);
                        strPrint = "";
                    }   // Anleihe / Wertpapier                        
                }   // foreach node2               
            }   // foreach node1
            n2 = 0;
            float Sharpe = GetSharpeRatio(doc, SearchV.textSharpeRatio1, SearchV.textSharpeRatio2);
            wpsyn.WPSSharpe = Sharpe;
        }
        private float GetSharpeRatio(HtmlDocument doc, string textSharpeRatio1, string textSharpeRatio2) {
            string pattSharpe = @"\-?[0-9]*[\.]?[0-9]*[,][0-9]*";                   // -1.234,56  oder 1,23         
            Regex regSharpe = new Regex(pattSharpe, RegexOptions.IgnoreCase);
            n2 = 0;
            string[,] StrSharpeArr = new string[12, 2];
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='" + textSharpeRatio1 + "']");
            foreach (HtmlNode node1 in nodes) {
                ++n2;
                if (!node1.InnerText.Contains(textSharpeRatio2))
                    continue;
                string strTitel = string.Format("Sharpe: Die Suche nach 'class={0}' hat {1} Nodes ergeben.  Die {2}.node2 mit '{3}'.",
                    textSharpeRatio1, nodes.Count, n2, textSharpeRatio2);
                PrintTxtUnten(strTitel);
                Console.WriteLine(strTitel);
                int nn = -1;
                string strZeileGesamt = string.Empty;
                try {
                    foreach (HtmlNode node2 in node1.SelectNodes(".//tr")) {
                        if (node2.ChildNodes.Count > 1) {
                            char[] charSeparators = new char[] { '/' };
                            string StrInner = Regex.Replace(node2.InnerText, "[\x00-\x1F]+", "/");
                            string[] StrArr = StrInner.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);    // Kupon in %4,750
                            string strZeile = string.Format("{0} == ", StrArr[0]);
                            StrSharpeArr[++nn, 0] = StrArr[0];
                            if (textSharpeRatio2.Contains("Sharpe")) {
                                if (StrArr.Length > 1) {
                                    StrSharpeArr[nn, 1] = StrArr[1];
                                    strZeile += string.Format("{0}", StrArr[1]);
                                } else {
                                    StrSharpeArr[nn, 1] = "---";
                                    strZeile += "---";
                                }

                            } else {
                                //Console.WriteLine("Kupondaten");
                            }
                            strZeileGesamt += strZeile + "  ";
                        }
                    }   // foreach node2
                    PrintTxtUnten(strZeileGesamt);
                } catch (NullReferenceException ex) {
                    MessageBox.Show("---- Fehler node2 in node1" + ex);
                }
            }      // foreach node1            
            if (textSharpeRatio2.Contains("Sharpe")) {
                /*  6 Monate == ---  1 Jahr  == -4,03 3 Jahre == 0,36  5 Jahre == 0,51 10 Jahre == ---*/
                Match match7 = regSharpe.Match(StrSharpeArr[2, 1]);
                if (match7.Success) {
                    float sharpe = Convert.ToSingle(match7.Value);
                    wpsyn.WPSSharpe = sharpe;
                    PrintTxtUnten("Sharpe: " + sharpe);
                } else {
                    Match match8 = regSharpe.Match(StrSharpeArr[3, 1]);
                    if (match8.Success) {
                        float sharpe = Convert.ToSingle(match8.Value);
                        wpsyn.WPSSharpe = sharpe;
                        PrintTxtUnten("Sharpe: " + sharpe);
                    } else {
                        wpsyn.WPSSharpe = 0;
                        // MessageBox.Show("Sharpe Fehler !!!!!!!!!!!!! ");
                    }
                }
            } else { }               // Kupondaten           
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
            sHTML = TextReader.ReadToEnd();
            HtmlDocument doc = new HtmlDocument();
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
            Console.WriteLine("----  KontenSynchronisierenInt.UpdatedtPortFol()   WPKurs  WPStand  WPProzentAenderung  WPSharpe  ----");
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];
            string strISIN = "";
            foreach (WertpapSynchro wps in _wertpapsynchro) {
                strISIN = wps.WPSISIN;
                if (strISIN.Length != 12)
                    continue;
                Console.Write("Änd: {0,-50} ", wps.WPSName);
                //if (wps.WPSName.StartsWith("JPM-"))
                //    Console.WriteLine();
                DataRow dtrow = dtPortFol.Rows.Find(strISIN);
                if (wps.WPSKursZeit != (DateTime)dtrow["WPStand"]) {
                    Console.Write("KursZeit alt: {0,-12} neu: {1,-12}", (DateTime)dtrow["WPStand"], wps.WPSKursZeit);
                    dtrow["WPStand"] = wps.WPSKursZeit;
                }
                if (wps.WPSWährung == null)
                    wps.WPSWährung = "EUR";
                if (wps.WPSWährung == "USD") {
                    wps.WPSKurs = Helpers.GlobalRef.g_mw.USDtoEuro(wps.WPSKurs); // 51.5064 = 55.04      83.41 =                
                } else if (wps.WPSWährung == "EUR") { } else if (wps.WPSWährung == "%") { } else {
                    MessageBox.Show("KontenSynchronisierenInt() UpdatedtPortFol() Fehler: wps.WPSWährung: " + wps.WPSWährung);
                    continue;
                }
                if (wps.WPSKurs != (float)dtrow["WPKurs"]) {
                    Console.Write("Kurs     alt: {0,-12} neu: {1,-12}", (float)dtrow["WPKurs"], wps.WPSKurs);
                    dtrow["WPKurs"] = wps.WPSKurs;
                }
                if (wps.WPSProzentAenderung != (float)dtrow["WPProzentAenderung"]) {
                    Console.Write("%Änd     alt: {0,-12} neu: {1,-12}", (float)dtrow["WPProzentAenderung"], wps.WPSProzentAenderung);
                    dtrow["WPProzentAenderung"] = wps.WPSProzentAenderung;
                }
                if (wps.WPSSharpe != (float)dtrow["WPSharpe"]) {
                    Console.Write("Sharpe   alt: {0,-12} neu: {1,-12}", (float)dtrow["WPSharpe"], wps.WPSSharpe);
                    dtrow["WPSharpe"] = wps.WPSSharpe;
                }
                Console.WriteLine();
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
        public string SelectNodesValue2;
        public string textSharpeRatio1;  // box
        public string textSharpeRatio2;  // Sharpe Ratio

    }
}