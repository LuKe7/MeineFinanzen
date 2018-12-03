// 27.11.2018 KontenSynchronisierenHBCI4j.cs
// In 'KursDaten\Depot-aus-hbci4j' werden alte Sätze NICHT gelöscht!!!
// Daher Datum Heute abfragen!!
// C:\Users\LuKe\Downloads\hbci4java-master(2)\hbci4java-master
// C:\Users\LuKe\eclipse-workspace\hbci4java-master.zip_expanded\hbci4java-master
// In D:\MeineFinanzen\MyDepot\KursDaten\Depot-aus-hbci4j\ :
//  Kontenaufstellung_1004902.xml
//  Kontenaufstellung_5497047602149346.xml
//  Kontenaufstellung_189227812.xml
//  GiroUmsatz_DE85213522400001004902.xml
//  GiroUmsatz_DE83213522400189227812.xml
//  DepotUmsatz_700617681.xml
//  Wertpapier_DE000A0NEKQ8.xml             für alle Wertpapiere(Alte WP enhalten!!)
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DataSetAdminNS;
using MeineFinanzen.Model;
using MeineFinanzen.Helpers;
using MeineFinanzen.ViewModel;
using System.Windows.Threading;
using System.Threading;
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenHBCI4j : Window {
        //bool isApplicationActive;
        public WertpapHBCI4j wp4j = new WertpapHBCI4j();
        public List<WertpapHBCI4j> wp4js = new List<WertpapHBCI4j>();
        //internal DgBanken _b;
        public static DepotHolen _depHolen;
        public KontenSynchronisierenHBCI4j() {
            InitializeComponent();
            PrintText("---- KontenSynchronisierenHBCI4j()");
            ConWrLi("---- KontenSynchronisierenHBCI4j()");
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Ausführen();
        }
        public void Ausführen() {
            PrintText("---- KontenSynchronisierenHBCI4j Window_Loaded()");
            ConWrLi("---- -50- KontenSynchronisierenHBCI4j Window_Loaded()");
            /* Verzeichnis: @"\KursDaten\Depot-aus-hbci4j\"                                                                      
            * --funktion--            -was-           -wohin-                                                                                
            * KontoStändeFinCmd       Kontostände     \Kontenstände-sKontoNr-DateTime.csv               
            * KontoUmsätzeFinCmdStmt  Kontoumsätze    \Umsätze-KontoNr-DateTime.csv     
            *                                         \logKontoUmsätzeHolen.txt                                                                     
            * DepotHolen_ausführen    WertpapierDepot dtWertpapHBCI4j (Mit angepasstem hbci4j geholt)   */
            string propDir = GlobalRef.g_Ein.strHBCI4j; // C:\Users\LuKe/hbci4j-core/hbci4j-core-3.0.10/
            string datenDir = GlobalRef.g_Ein.myDepotPfad + @"\KursDaten\Depot-aus-hbci4j\";
            //List<string> props = new List<string>();
            //props.Clear();
            // ---- Mit HBCI4j mit Java DepotAbrufTest.bat
            // -----    nach Wertpap_ISIN.xml           
            PrintText("Start HBCI");
            ConWrLi("---- -51-Start HBCI");
            foreach (var ban in DgBanken.banken) {
                if (string.Compare(ban.SortFeld7, "888", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                // ---- props füllen.
                DirectoryInfo ParentDirectory = new DirectoryInfo(propDir);
                FileInfo[] fis = ParentDirectory.GetFiles();
                foreach (FileInfo fi in fis) {
                    string strExt = fi.Extension;
                    string line = null;
                    if (string.Compare(strExt, ".properties") != 0)
                        continue;
                    //Console.WriteLine("{0}", fi.FullName);                      
                    if (fi.Length < 500 || fi.Length > 1600)
                        continue;
                    StreamReader file = new StreamReader(fi.FullName);
                    ConWrLi("---- -51-properties suchen: " + fi.FullName);                   
                    while ((line = file.ReadLine()) != null) {
                        if (line.Contains("client.passport.default=")) {    // C:\Users\LuKe\hbci4j-core\hbci4j-core-3.0.10\hbci-ING-DiBa.properties
                            string strPfad = fi.FullName.Substring(2);
                            strPfad = strPfad.Replace(@"\", "/");
                            if (!fi.FullName.Contains(ban.BankName7))
                                continue;
                            if (ban.BankName7.Contains("ING"))
                                continue;
                            // NOCH wieder raus wenn ING läuft.                                   
                            // ---- Java-Pgm schreibt nach datenDir.
                            //string argumentText = string.Format(@"{0}", propDir);   //, ausgabeDir);
                            Directory.SetCurrentDirectory(GlobalRef.g_Ein.strEclipseHbci4jClasses);
                            PrintText("---- - 51 - Vor process2.Start() HBCI Bank: " + ban.BankName7);
                            ConWrLi("---- -51-Vor process2.Start() HBCI Bank: " + ban.BankName7);
                            Process process2 = new Process();
                            process2.StartInfo.FileName = GlobalRef.g_Ein.myDepotPfad + @"\DepotAbrufTest.bat";
                            process2.StartInfo.Arguments = strPfad + " " + datenDir;
                            process2.StartInfo.UseShellExecute = false;
                            process2.StartInfo.RedirectStandardOutput = true;
                            process2.StartInfo.CreateNoWindow = true;
                            DoEvents();
                            process2.Start();
                            string strMeldung = process2.StandardOutput.ReadToEnd();    // Lange Wartezeit. NOCH strMeldung untersuchen ...                       
                            PrintText("---- - 51 - Vor process2.WaitForExit()");
                            ConWrLi("---- -51-Vor process2.WaitForExit()");
                            process2.WaitForExit();
                            PrintText("---- - 51 - Nach process2.WaitForExit()");
                            ConWrLi("---- -51-Nach process2.WaitForExit()");
                        }
                    }   // while Zeilen file
                    file.Close();
                }   // foreach fi in fis
            }   // foreach ban in DGBanken.banken
            ConWrLi("---- -51- nach foreach ban in DGBanken.banken");
            PrintText("---- -51- nach foreach ban in DGBanken.banken");
            // ---- In List WertpapHBCI4j importieren
            DirectoryInfo ParentDirectory2 = new DirectoryInfo(datenDir);
            FileInfo[] fis2 = ParentDirectory2.GetFiles();
            ConWrLi("----------- Nur anzeigen " + fis2[0].DirectoryName + " ----------");
            // s.u. DataSet dsHier = new DataSet();
            wp4js.Clear();
            foreach (FileInfo fi in fis2) { // D:\MeineFinanzen\MyDepot\KursDaten\Depot-aus-hbci4j\DepotUmsatz_700617681.xml
                string strExt = fi.Extension;
                string strName = fi.Name;
                string strDatum = fi.LastWriteTime.ToShortDateString(); // 08.07.2018
                string strHeute = DateTime.Today.ToShortDateString();
                if ((string.Compare(strExt, ".xml") != 0) || (!strName.StartsWith("Wertpapier_")))
                    continue;
                if ((strDatum != strHeute))
                    continue;
                //Console.Write("{0,-80} ", fi.FullName);
                wp4j = null;
                GlobalRef.g_WPHBCI.DeserializeReadWertpapHBCI4j(fi.FullName, out wp4j);
                // wird nicht gebraucht: dsHier.ReadXml(fi.FullName, XmlReadMode.Auto);  
                //D:\MeineFinanzen\MyDepot\KursDaten\Depot-aus-hbci4j\Wertpapier_DE0006791809.xml KANAM GRUNDINVEST FONDS INHABER-ANTEILE  DE0006791809          13,37    06.07.18 20:37:34
                //D:\MeineFinanzen\MyDepot\KursDaten\Depot-aus-hbci4j\Wertpapier_DE0007483612.xml DEKA-IMMOBILIENGLOBAL INHABER-ANTEILE  DE0007483612          55,03    06.07.18 20:37:34
                string strForm = string.Format("{0,-62} {1,-12} {2,10} {3,18:dd/MM/yy H:mm:ss} KuWä:{4} DeWä:{5}", wp4j.Name, wp4j.ISIN, wp4j.Kurs, wp4j.KursZeit, wp4j.KursWaehrung, wp4j.DepotWaehrung);
                ConWrLi(strForm);
                PrintText(strForm);
                wp4js.Add(wp4j);
            }
            WertpapHBCI4jToPortFol();
            PrintText("hbci fertig");
        }
        public bool WertpapHBCI4jToPortFol() {  // Update von List WertpapHBCI4j wp4js nach dtPortFol.          
            if (wp4js.Count == 0)
                return true;
            //DataView dvWertpapiereGesamt = new DataView(DataSetAdmin.dtWertpapSubsembly, "", "ISIN", DataViewRowState.CurrentRows);
            DataTable dtt2 = new DataTable();
            DataSetAdmin.dvPortFol.Sort = "WPISIN";
            DataSetAdmin.dtPortFol.DefaultView.Sort = "WPISIN ASC";
            dtt2 = DataSetAdmin.dtPortFol.DefaultView.ToTable();
            DataSetAdmin.dtPortFol = dtt2;
            float quantity = -1;       // Anzahl
            float wpanzahl = -1;
            string wpname = "";
            string wpisin = "";
            string HoldingCurrency = ""; // EUR
            double kurs;
            // ---- Prüfen ob WP in wp4js auch in dtPortFol sind.
            PortFolDatensatz portfolNeu = new PortFolDatensatz();
            foreach (WertpapHBCI4j HBCI4j in wp4js) {
                int nPortfol = DataSetAdmin.dvPortFol.Find(HBCI4j.ISIN);
                if (nPortfol < 0) {           // ISIN nicht in dtPortFol, also in dtPortFol neu einfügen.
                    DataRow newRow = DataSetAdmin.dtPortFol.NewRow();
                    newRow = portfolNeu.dtPortFolAusdtNull(newRow);
                    newRow["WPName"] = HBCI4j.Name;
                    newRow["WPIsin"] = HBCI4j.ISIN;
                    newRow["WPAnzahl"] = HBCI4j.Saldo;
                    newRow["WPKurs"] = HBCI4j.Kurs;
                    int l = HBCI4j.Name.ToString().Length;
                    if (l > 8)
                        l = 8;
                    newRow["WPKurz"] = HBCI4j.Name.Substring(0, l);
                    newRow["WPAktWert"] = HBCI4j.DepotWert;
                    newRow["WPStand"] = DateTime.Now;
                    DataSetAdmin.dtPortFol.Rows.Add(newRow);
                }
            }
            // ---- Prüfen ob WP aus dtPortFol auch in wp4js sind.
            foreach (DataRow rowPortFol in DataSetAdmin.dtPortFol.Rows) {
                wpname = (string)rowPortFol["WPName"];
                wpisin = (string)rowPortFol["WPIsin"];
                string wpid = rowPortFol["WPTypeID"].ToString();
                if ((rowPortFol["WPTypeID"].ToString() == "80") ||
                    (rowPortFol["WPTypeID"].ToString() == "10"))
                    continue;
                if (wpisin.Length < 9)
                    continue;
                WertpapHBCI4j HBCI4j = SucheISIN(wp4js, wpisin);
                if (HBCI4j == null) {
                    // Nicht, also in dtPortFol löschen.
                    MessageBox.Show("WertpapWertpapHBCI4jToPortFol() Löschen: " + wpname + " " + wpisin);
                    try {
                        DataSetAdmin.dtPortFol.Rows.Remove(rowPortFol);
                    } catch (Exception ex) {
                        MessageBox.Show("" + ex);
                    }
                    return false;
                }
                kurs = HBCI4j.Kurs;
                wpanzahl = Convert.ToSingle(rowPortFol["WPAnzahl"]);
                quantity = Convert.ToSingle(HBCI4j.Saldo);
                rowPortFol["WPAktWert"] = HBCI4j.DepotWert;
                double wpkaufsumme = Convert.ToDouble(rowPortFol["WPKaufsumme"]);
                double wpkurs = Convert.ToDouble(rowPortFol["WPKurs"]);
                double kaufsumme = HBCI4j.DepotWert;
                //Debug.WriteLine("wpkaufsumme: {0} CostPriceRate: {1} Kaufsumme: {2}", wpkaufsumme, CostPriceRate, CostPriceRate * wpanzahl);               
                if (rowPortFol["WPTypeID"].ToString() == "70")
                    quantity /= 100;
                HoldingCurrency = HBCI4j.EinstandsPreisWaehrung;
                if (wpanzahl != quantity) {
                    //Debug.WriteLine("w:{0, -27} {1, -12} {2, -5} {3, -4}", wpname, wpisin, wpanzahl, wpges);
                    //Debug.WriteLine("g:{0, -27} {1, -12} {2, -5} {3, -4}", securityName, ISIN, quantity, wpges);
                    MessageBox.Show("NEU:  WPAnzahl (" + wpname + ") wird übernommen! Alt: " + wpanzahl + " Neu: " + quantity);
                    rowPortFol["WPAnzahl"] = quantity;
                }
                if (HBCI4j.KursWaehrung == string.Empty)
                    HBCI4j.KursWaehrung = "EUR";
                if (HBCI4j.KursWaehrung == "USD") {                 
                    kurs = GlobalRef.g_mw.USDtoEuro((float)HBCI4j.Kurs); // 51.5064 = 55.04      83.41 = 
                    string strForm = string.Format("WertpapHBCI4jToPortFol() HBCI4j.KursWaehrung == 'USD' {0} = {1} / {2}", kurs, kaufsumme, quantity);
                    ConWrLi(strForm);
                } else if (HBCI4j.KursWaehrung == "EUR") { } else if (HBCI4j.KursWaehrung == "%") { } else {
                    MessageBox.Show("KontenSynchronisierenHBCI4j() WertpapHBCI4jToPortFol() Fehler: KursWaehrung: " + HBCI4j.KursWaehrung);
                    continue;
                }
                if (wpkurs != kurs) {       // NOCH prüfen +- 10%
                    ConWrLi("WPKurs (" + wpname + ") wird übernommen! Alt: " + wpkurs + " Neu: " + kurs);
                    rowPortFol["WPKurs"] = kurs;
                }
                DateTime stand = HBCI4j.KursZeit;
                DateTime wpStand = Convert.ToDateTime(rowPortFol["WPStand"]);
                if (wpStand < stand) {
                    rowPortFol["WPStand"] = stand;
                    rowPortFol["WPKursVorher"] = wpkurs;
                    rowPortFol["WPStandVorher"] = wpStand;
                }
                rowPortFol["WPKaufsumme"] = wpkaufsumme;
                DateTime kaufdatum1 = (DateTime)rowPortFol["WPBisDatum"];
                if (kaufdatum1 != new DateTime(1980, 1, 1))
                    rowPortFol["WPBisDatum"] = kaufdatum1;
            }
            return true;
        }
        public WertpapHBCI4j SucheISIN(List<WertpapHBCI4j> hbci, string isin) {
            foreach (WertpapHBCI4j wp in hbci)
                if (wp.ISIN == isin)
                    return wp;
            return null;
            throw new NotImplementedException();
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e) { }
        void PrintText(string str) {
            txbHBCI.AppendText(Environment.NewLine + str);
            txbHBCI.ScrollToEnd();
            txbHBCI.InvalidateVisual();
            DoEvents();
        }
        protected void DoEvents() {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }       
        private void App_Deactivated(object sender, EventArgs e) {
            ConWrLi("---- --App_Deactivated()");
            //isApplicationActive = false;
        }
        private void App_Activated(object sender, EventArgs e) {
            //isApplicationActive = true;
            ConWrLi("---- --App_Activated()");
        }
    }
}