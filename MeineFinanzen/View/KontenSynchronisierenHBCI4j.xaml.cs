// 07.03.2018 KontenSynchronisierenHBCI4j.cs
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
namespace MeineFinanzen.View {
    public partial class KontenSynchronisierenHBCI4j : Window {
        public WertpapHBCI4j wp4j = new WertpapHBCI4j();
        public List<WertpapHBCI4j> wp4js = new List<WertpapHBCI4j>();
        //internal DgBanken _b;
        public static DepotHolen _depHolen;
        public KontenSynchronisierenHBCI4j() {
            InitializeComponent();
        }
        private void Window_Loaded(Object sender, RoutedEventArgs e) { }
        public void Ausführen(HauptFenster mw, bool laden) {
            /* Verzeichnis: @"\KursDaten\Depot-aus-hbci4j\"                                                                      
            * --funktion--            -was-           -wohin-                                                                                
            * KontoStändeFinCmd       Kontostände     \Kontenstände-sKontoNr-DateTime.csv               
            * KontoUmsätzeFinCmdStmt  Kontoumsätze    \Umsätze-KontoNr-DateTime.csv     
            *                                         \logKontoUmsätzeHolen.txt                                                                     
            * DepotHolen_ausführen    WertpapierDepot dtWertpapHBCI4j (Mit angepasstem hbci4j geholt)   */
            string propDir = Helpers.GlobalRef.g_Ein.strHBCI4j; // C:\Users\LuKe/hbci4j-core/hbci4j-core-3.0.10/
            string datenDir = Helpers.GlobalRef.g_Ein.myDepotPfad + @"\KursDaten\Depot-aus-hbci4j\";
            //List<string> props = new List<string>();
            //props.Clear();
            // ---- Mit HBCI4j mit Java DepotAbrufTest.bat
            // -----    nach Wertpap_ISIN.xml
            ConWrLi("---- -50- Start KontenSynchronisieren_HBCI4j");
            TxtWrLi("Start HBCI");
            foreach (var ban in DgBanken.banken) {
                if (String.Compare(ban.SortFeld7, "888", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    //Console.WriteLine("properties gefunden?: {0}", file);
                    while ((line = file.ReadLine()) != null) {
                        if (line.Contains("client.passport.default=")) {
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
                            TxtWrLi("HBCI läuft");
                            Process process2 = new Process();
                            process2.StartInfo.FileName = GlobalRef.g_Ein.myDepotPfad + @"\DepotAbrufTest.bat";
                            process2.StartInfo.Arguments = strPfad + " " + datenDir;
                            process2.StartInfo.UseShellExecute = false;
                            process2.StartInfo.RedirectStandardOutput = true;
                            process2.StartInfo.CreateNoWindow = true;
                            process2.Start();
                            string strMeldung = process2.StandardOutput.ReadToEnd();
                            // NOCH strMeldung untersuchen ...
                            Console.WriteLine(strMeldung);
                            process2.WaitForExit();
                        }
                    }   // Zeile in .properties
                    file.Close();
                }   // foreach .properties
            }   // foreach banken
            ConWrLi("---- -51- nach hbci4j");
            TxtWrLi("Update HBCI");
            // ---- In List WertpapHBCI4j importieren
            DirectoryInfo ParentDirectory2 = new DirectoryInfo(datenDir);
            FileInfo[] fis2 = ParentDirectory2.GetFiles();
            // s.u. DataSet dsHier = new DataSet();
            wp4js.Clear();
            foreach (FileInfo fi in fis2) {
                string strExt = fi.Extension;
                string strName = fi.Name;
                if ((string.Compare(strExt, ".xml") != 0) || (!strName.StartsWith("Wertpapier_")))
                    continue;
                Console.Write("{0,-80} ", fi.FullName);
                wp4j = null;
                GlobalRef.g_WPHBCI.DeserializeReadWertpapHBCI4j(fi.FullName, out wp4j);
                // wird nicht gebraucht: dsHier.ReadXml(fi.FullName, XmlReadMode.Auto);                    
                Console.WriteLine("{0,-28} {1,-16} {2,10} {3,20:dd/MM/yy H:mm:ss}", wp4j.Name, wp4j.ISIN, wp4j.Kurs, wp4j.KursZeit);
                wp4js.Add(wp4j);
            }
            WertpapHBCI4jToPortFol();
            TxtWrLi("hbci fertig");
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
            string PriceCurrency = "";  // EUR oder USD oder so         
            Single quantity = -1;       // Anzahl
            Single wpanzahl = -1;
            string wpname = "";
            string wpisin = "";
            string HoldingCurrency = ""; // EUR
            double kurs;
            // ---- Prüfen ob WP in wp4js auch in dtPortFol sind.
            PortFolDatensatz portfolNeu = new PortFolDatensatz();
            foreach (WertpapHBCI4j HBCI4j in wp4js) {
                int nPortfol = DataSetAdmin.dvPortFol.Find(HBCI4j.ISIN);
                if (nPortfol < 0) {           // Nicht, also in dtPortFol neu einfügen.
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
                    MessageBox.Show("WPAnzahl (" + wpname + ") wird übernommen! Alt: " + wpanzahl + " Neu: " + quantity);
                    rowPortFol["WPAnzahl"] = quantity;
                }
                if (PriceCurrency == "")
                    PriceCurrency = "EUR";
                if (PriceCurrency == "USD") {
                    //Price = _mw.USDtoEuro(Price);
                    kurs = wpkaufsumme / quantity;      // NOCH
                } else if (PriceCurrency == "EUR") { } else {
                    MessageBox.Show("Update_Kaufdatum_KtoKurs Fehler: PriceCurrency: " + PriceCurrency);
                    continue;
                }
                if (wpkurs != kurs) {       // NOCH prüfen +- 30%
                    //MessageBox.Show("WPKurs (" + wpname + ") wird übernommen! Alt: " + wpkurs + " Neu: " + kurs);
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
        private void CloseWindow(Object sender, System.ComponentModel.CancelEventArgs e) { }
        void TxtWrLi(string str) {
            //txbHBCI.Text += Environment.NewLine + str;
            try {
                //string str2 = string.Format("{0,-50} {1}", str, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
                txbHBCI.AppendText(Environment.NewLine + str);
                txbHBCI.ScrollToEnd();
                txbHBCI.InvalidateVisual();
            } catch (Exception ex) {
                MessageBox.Show("Fehler TxtWrLi()" + ex);
            }
        }
    }
}