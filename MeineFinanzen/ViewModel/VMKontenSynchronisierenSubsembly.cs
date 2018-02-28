// 28.02.2018 VMKontenSynchronisierenSubsembly.cs
// Get Passw verändert. NOCH verbessern.
// SortFeld7 < 888 sind Banken.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DataSetAdminNS;
using Subsembly.FinTS;
using MeineFinanzen.Model;
using System.Xml;
using MeineFinanzen.Helpers;
namespace MeineFinanzen.ViewModel {
    public class VMKontenSynchronisierenSubsembly {
        internal DgBanken _b;
        public static Helpers.DepotHolen _depHolen;
        string pathMeineBilder;
        public VMKontenSynchronisierenSubsembly() {
            _b = Helpers.GlobalRef.g_dgBanken;
            pathMeineBilder = Helpers.GlobalRef.g_Ein.strBilderPfad + @"\";
            _depHolen = new Helpers.DepotHolen();    // In FinKontenÜbersicht Grundwert
        }
        //public void KontenSynchronisieren_Subsembly(View.HauptFenster mw, bool laden) {  // false = nicht laden.
        public void KontenSynchronisieren_Subsembly(View.HauptFenster mw, bool laden) {
            /* Verzeichnis: D :\MeineFinanzen\MyDepot\Log                                                                       
            * --funktion--            -was-           -wohin-                                                                                
            * KontoStändeFinCmd       Kontostände     \Kontenstände - sKontoNr-DateTime.csv balance   -contactname...               
            * KontoUmsätzeFinCmdStmt  Kontoumsätze    \Umsätze-KontoNr-DateTime.csv         statement -contactname... 
            *                                         \logKontoUmsätzeHolen.txt                                                                     
            * DepotHolen_ausführen    WertpapierDepot dtWertpapSubsembly. (Mit angepasstem FinPadForm geholt)   */
            if (!laden)
                return;
            ConWrLi("---- -20- Start KontenSynchronisieren_Subsembly");
            mw._boKurseAktualisierenKo = true;
            string[] strResult;
            double BankBetrag = 0.00;
            double GesamtBetrag = 0.00;
            DgBanken.wertpaps.Clear();
            DgBanken.umsätze.Clear();
            DgBanken.konten.Clear();
            DgBanken.banken.Clear();                                                         // -ArrayOfBankÜbersicht                     
            mw.openLogFile();
            int anzBanken = 0;
            DataSetAdmin.dtKontoumsätze.Clear();
            DataSetAdmin.dtKontoumsätze.Columns.Clear();

            DataSetAdmin.dtWertpapSubsembly = new DataTable("tblWertpapSubsembly");
            DataSetAdmin.dtWertpapSubsembly.Columns.Clear();
            DataSetAdmin.dtWertpapSubsembly.Rows.Clear();
            DataSetAdmin.dtWertpapSubsembly.PrimaryKey = new DataColumn[] { DataSetAdmin.dtWertpapSubsembly.Columns["ISIN"] };

            foreach (FinContact aContact in mw.liContacte) {                        // Banken
                Debug.WriteLine("Bank: {0}", aContact.ContactName);
                DgBanken.bank = new BankÜbersicht();                                         // -BankÜbersicht
                DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
                DgBanken.banken.Add(DgBanken.bank);
                DgBanken.bank.SortFeld7 = (anzBanken++ + 300).ToString();                    // aContact.BankCode;   50010517
                DgBanken.bank.BankName7 = aContact.ContactName;                              // +BankName7         
                string strImagePath = "";
                if (aContact.ContactName.Contains("Spark"))
                    strImagePath += "Spk";
                else if (aContact.ContactName.Contains("DiBa"))
                    strImagePath += "DiBa";
                else
                    strImagePath += "nichts";
                strImagePath += ".png";
                DgBanken.bank.BildPfad7 = pathMeineBilder + strImagePath;                    // +BildPfad7 
                // D :\Visual Studio 2015\Projects\SubsemblyFinTS\DiBa.png
                DgBanken.bank.BLZ7 = aContact.BankCode;
                DgBanken.bank.UserID7 = aContact.UserID;
                DgBanken.bank.Datum7 = File.GetLastWriteTime(Helpers.GlobalRef.g_Ein.myDepotPfad + @"Log\BankKontoStand.xml");
                DgBanken.bank.Bearbeitungsart7 = "bearb...";
                DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";
                DgBanken.bank.Status7 = "sta";
                _depHolen._bank = aContact.ContactName;
                _depHolen._blz = aContact.BankCode;
                _depHolen._pin = GetPasswort(_depHolen._blz);
                foreach (FinAcctInfo aAcctInfo in aContact.UPD) {                   // Konten der Bank                          
                    Debug.WriteLine("   Konto: {0,-30} {1,-16} {2,-20}", aAcctInfo.AcctName, aAcctInfo.AcctTypeClass, aAcctInfo.AcctNo);
                    if (aAcctInfo.AcctTypeClass.ToString().Contains("Portfolio")) {
                        _b.Betrag = Convert.ToDouble(_depHolen.DepotHolen_ausführen()); // ===> Wertpapiere   Bank ---> dtWertpapSubsembly  <<Subsembly.FinTS>>                               
                        WertpapSubsemblyToPortFol();                    // dtWertpapSubsembly ---> dtPortFol.
                        mw._tabwertpapiere.ErstelleWertpapiere(mw);     // dtPortFol          ---> CollWertpapiere.                                    // aus dtWertpapSubsembly
                        CollWertpapiere liWP = (CollWertpapiere)mw.Resources["wertpapiere"];
                        if (aAcctInfo.AcctName == "Wertpapierdepot") {
                            if (liWP.Count > 0) {
                                DgBanken.konto.OCWertpap = new ObservableCollection<Wertpapier>(liWP);
                                mw.dgFinanzübersicht.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Visible;
                            }
                        }
                    } else if (aAcctInfo.AcctTypeClass.ToString().Contains("Giro")) {
                        _depHolen._ktoNr = aAcctInfo.AcctNo;
                        string sCmd = "";
                        string strBank = _depHolen._bank;
                        strBank.Replace(" ", "-");
                        sCmd = "balance" + " -contactname " + strBank + " -pin " + _depHolen._pin + " -bankcode " + _depHolen._blz + " -acctno " + _depHolen._ktoNr;
                        strResult = _b.KontoStändeFinCmd(sCmd, _depHolen._ktoNr);       // ===> Konto-Stände  Bank ---> @"Log\Kontenstände -" + sKontoNr
                        if (strResult == null) {
                            Console.WriteLine("strResult == null!!!!");
                            _b.Betrag = 0;
                        } else {
                            _b.Betrag = Convert.ToDouble(strResult[5]);
                            sCmd = "statement" + " -contactname " + _depHolen._bank + " -pin " + _depHolen._pin + " -acctno " + _depHolen._ktoNr;
                            _b.KontoUmsätzeFinCmdStmt(sCmd, _depHolen._ktoNr);          // ===> Konto-Umsätze Bank ---> @"Log\Umsätze -" + sKontoNr 
                        }
                    } else {
                        //Debug.WriteLine("nix von Beiden!!!!" + aAcctInfo.AcctTypeClass.ToString());
                        _b.Betrag = 0;
                    }
                    DgBanken.konto = new BankKonten();
                    DgBanken.konto.KontoName8 = aAcctInfo.AcctName;                          // +KontoName 8                      
                    DgBanken.konto.KontoArt8 = aAcctInfo.AcctTypeClass.ToString();           // +KontoArt8                    
                    DgBanken.konto.KontoNr8 = aAcctInfo.AcctNo;                              // +KontoNr8                     
                    DgBanken.konto.KontoValue8 = _b.Betrag;                                  // +KontoValue8                                  

                    DateTime dt = File.GetLastWriteTime(Helpers.GlobalRef.g_Ein.myDepotPfad + @"Log\BankKontoStand.xml");// +KontoDatum8
                    DgBanken.konto.KontoDatum8 = dt;

                    List<Kontoumsatz> liku = _b.KontoumsatzFüllen(aAcctInfo.AcctNo);  // aus dtKontoumsätze
                    if (liku.Count > 0) {
                        DgBanken.konto.OCUmsätze = new ObservableCollection<Kontoumsatz>(liku);
                        mw.dgFinanzübersicht.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Visible;
                    }

                    DgBanken.konten.Add(DgBanken.konto);
                    DgBanken.bank.OCBankKonten.Add(DgBanken.konto);
                    BankBetrag += _b.Betrag;
                    DgBanken.bank.BankValue7 = BankBetrag;                                   // +BankValue7
                    GesamtBetrag += _b.Betrag;
                    Console.WriteLine("GesamtBetrag: {0} BankBetrag: {1} Betrag: {2}", GesamtBetrag, BankBetrag, _b.Betrag);
                }   // loop AcctInfo = Konten der Bank
            }   //  loop Contact = Bank
            ConWrLi("---- -26- In KontenSynchronisieren_Subsembly");

            DgBanken.bank = new BankÜbersicht();                                             // -BankÜbersicht
            DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
            DgBanken.banken.Add(DgBanken.bank);
            DgBanken.bank.SortFeld7 = "888";
            DgBanken.bank.Bearbeitungsart7 = "bearb...";
            DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";
            DgBanken.bank.Datum7 = File.GetLastWriteTime(Helpers.GlobalRef.g_Ein.myDepotPfad + @"Log\BankKontoStand.xml");
            DgBanken.bank.BankName7 = "GeschlFonds";                                         // +BankName7     
            DgBanken.bank.BildPfad7 = pathMeineBilder + "Aktualisieren1.png";                // +BildPfad7 
            // @"C :\U sers\Public\Pictures\index.png";                 
            DgBanken.bank.BankValue7 = _b.SummeGeschlFonds();                                // +BankValue7

            DgBanken.bank = new BankÜbersicht();                                             // -BankÜbersicht
            DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
            DgBanken.banken.Add(DgBanken.bank);
            DgBanken.bank.SortFeld7 = "889";
            DgBanken.bank.Bearbeitungsart7 = "bearb...";
            DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";
            DgBanken.bank.Datum7 = File.GetLastWriteTime(Helpers.GlobalRef.g_Ein.myDepotPfad + @"Log\BankKontoStand.xml");
            DgBanken.bank.BankName7 = "Alle Konten";                                         // +BankName7     
            DgBanken.bank.BildPfad7 = pathMeineBilder + "Aktualisieren1.png";                // +BildPfad7 
            // @"C :\U sers\Public\Pictures\index.png";                
            DgBanken.bank .BankValue7 = GesamtBetrag + _b.SummeGeschlFonds();                 // +BankValue7  NOCH
            mw.swLog.Close();
            string str = DataSetAdmin.DatasetSichernInXml(Helpers.GlobalRef.g_Ein.myDataPfad);
            if (str != null) {
                MessageBox.Show("Fehler DatasetSichernInXml(): " + str);
            } else {
                Helpers.GlobalRef.g_Büb.SerializeWriteBankÜbersicht(Helpers.GlobalRef.g_Ein.myDepotPfad
                    + @"\Daten\BankÜbersichtsDaten.xml", DgBanken.banken);
                ConWrLi("---- -29- In KontenSynchronisieren_Subsembly");
            }

            mw.dgBankenÜbersicht.UpdateLayout();
            mw._boKurseAktualisierenKo = false;
            ConWrLi("---- -29b- In KontenSynchronisieren_Subsembly vor neuStart");
            mw.neuStarten();
            //testBankAnzeige();
            ConWrLi("---- -29c- Fertig KontenSynchronisieren_Subsembly nach neuStart");
        }
        public bool WertpapSubsemblyToPortFol() {  // Update von dtWertpapSubsembly ---> dtPortFol.          
            if (DataSetAdmin.dtWertpapSubsembly.Columns.Count == 0)
                return true;
            DataTable dtt1 = new DataTable();
            DataSetAdmin.dvWertpapSubsembly.Sort = "ISIN";
            DataSetAdmin.dtWertpapSubsembly.DefaultView.Sort = "ISIN ASC";
            dtt1 = DataSetAdmin.dtWertpapSubsembly.DefaultView.ToTable();
            DataSetAdmin.dtWertpapSubsembly = dtt1;
            DataView dvWertpapiereGesamt = new DataView(DataSetAdmin.dtWertpapSubsembly, "", "ISIN", DataViewRowState.CurrentRows);

            DataTable dtt2 = new DataTable();
            DataSetAdmin.dvPortFol.Sort = "WPISIN";
            DataSetAdmin.dtPortFol.DefaultView.Sort = "WPISIN ASC";
            dtt2 = DataSetAdmin.dtPortFol.DefaultView.ToTable();
            DataSetAdmin.dtPortFol = dtt2;

            string securityName = "";
            string ISIN = "";
            Single Price = -1;          // Kurs
            string PriceCurrency = "";  // EUR oder USD oder so         
            Single quantity = -1;       // Anzahl
            double CostPriceRate = -1;  // Kaufsumme
            string isinGes;
            Single wpanzahl = -1;
            string wpname = "";
            string wpisin = "";
            DateTime wpkaufdatum;
            double wpkurs = 0;
            double wpkaufsumme = 0;
            double wpaktwert = 0;
            Single HoldingValue = 0;
            string HoldingCurrency = ""; // EUR
            // Prüfen ob WP in dtWertpapSubsembly auch in dtPortFol sind.
            PortFolDatensatz portfolNeu = new PortFolDatensatz();
            foreach (DataRow rowGesamt in DataSetAdmin.dtWertpapSubsembly.Rows) {
                isinGes = (string)rowGesamt["ISIN"];
                int nPortfol = DataSetAdmin.dvPortFol.Find(isinGes);
                //Debug.WriteLine("isinGes:{0} nPortfol:{1} SecurityName: {2}", isinGes, nPortfol, rowGesamt["SecurityName"]);
                if (nPortfol < 0)           // Nicht, also in dtPortFol einfügen.
                {
                    DataRow newRow = DataSetAdmin.dtPortFol.NewRow();
                    newRow = portfolNeu.dtPortFolAusdtGesamt(newRow, rowGesamt);

                    newRow["WPName"] = rowGesamt["SecurityName"];
                    newRow["WPIsin"] = rowGesamt["ISIN"];

                    DataSetAdmin.dtPortFol.Rows.Add(newRow);
                }
            }
            // Prüfen ob WP in dtPortFol auch in dvWertpapiereGesamt sind.
            foreach (DataRow rowPortFol in DataSetAdmin.dtPortFol.Rows) {
                wpname = (string)rowPortFol["WPName"];
                wpisin = (string)rowPortFol["WPIsin"];
                string wpid = rowPortFol["WPTypeID"].ToString();
                if ((rowPortFol["WPTypeID"].ToString() == "80") ||
                    (rowPortFol["WPTypeID"].ToString() == "10"))
                    continue;
                if (wpisin.Length < 9)
                    continue;
                int wpges = dvWertpapiereGesamt.Find(wpisin);
                if (wpges == -1) {
                    // Nicht, also aus aus dtPortFol löschen.
                    MessageBox.Show("WertpapSubsemblyToPortFol() Löschen: " + wpname + " " + wpisin);
                    try {
                        DataSetAdmin.dtPortFol.Rows.Remove(rowPortFol);
                    } catch (Exception ex) {
                        MessageBox.Show("" + ex);
                    }
                    return false;
                }
                wpkaufdatum = (DateTime)rowPortFol["WPBisDatum"];
                wpanzahl = Convert.ToSingle(rowPortFol["WPAnzahl"]);
                quantity = Convert.ToSingle(dvWertpapiereGesamt[wpges]["quantity"]);

                HoldingValue = Convert.ToSingle(dvWertpapiereGesamt[wpges]["HoldingValue"]);
                wpaktwert = HoldingValue;
                rowPortFol["WPAktWert"] = wpaktwert;
                wpkaufsumme = Convert.ToDouble(rowPortFol["WPKaufsumme"]);
                wpkurs = Convert.ToDouble(rowPortFol["WPKurs"]);
                CostPriceRate = Convert.ToDouble(dvWertpapiereGesamt[wpges]["CostPriceRate"]);
                //Debug.WriteLine("wpkaufsumme: {0} CostPriceRate: {1} Kaufsumme: {2}", wpkaufsumme, CostPriceRate, CostPriceRate * wpanzahl);               
                if (rowPortFol["WPTypeID"].ToString() == "70")
                    quantity /= 100;
                securityName = (string)dvWertpapiereGesamt[wpges]["securityName"];
                ISIN = (string)dvWertpapiereGesamt[wpges]["ISIN"];
                Price = Convert.ToSingle(dvWertpapiereGesamt[wpges]["Price"].ToString());
                PriceCurrency = dvWertpapiereGesamt[wpges]["PriceCurrency"].ToString();
                HoldingCurrency = dvWertpapiereGesamt[wpges]["HoldingCurrency"].ToString();
                if (wpanzahl != quantity) {
                    //Debug.WriteLine("w:{0, -27} {1, -12} {2, -5} {3, -4}", wpname, wpisin, wpanzahl, wpges);
                    //Debug.WriteLine("g:{0, -27} {1, -12} {2, -5} {3, -4}", securityName, ISIN, quantity, wpges);
                    MessageBox.Show("WPAnzahl (" + wpname + ") wird übernommen! Alt: " + wpanzahl + " Neu: " + quantity);
                    rowPortFol["WPAnzahl"] = quantity;
                }
                /* if (wpname != securityName)
                {
                    MessageBox.Show("WPName wird übernommen! Alt: " + wpname + " Neu: " + securityName);
                    rowPortFol["WPName"] = securityName;
                } */
                if (CostPriceRate != 0) {
                    if (wpkaufsumme != CostPriceRate * wpanzahl) {
                        MessageBox.Show("WPKaufsumme (" + wpname + ") wird übernommen! Alt: " + wpkaufsumme + " Neu: " + CostPriceRate * wpanzahl);
                        wpkaufsumme = CostPriceRate * wpanzahl;
                    }
                }
                if (PriceCurrency == "")
                    PriceCurrency = "EUR";
                if (PriceCurrency == "USD") {
                    //Price = _mw.USDtoEuro(Price);
                    Price = Convert.ToSingle(HoldingValue) / quantity;      // NOCH
                } else if (PriceCurrency == "EUR") { } else {
                    MessageBox.Show("Update_Kaufdatum_KtoKurs Fehler: PriceCurrency: " + PriceCurrency);
                    continue;
                }
                if (wpkurs != Price) {
                    //MessageBox.Show("WPKurs (" + wpname + ") wird übernommen! Alt: " + wpkurs + " Neu: " + Price);
                    rowPortFol["WPKurs"] = Price;
                }
                DateTime standBank = Convert.ToDateTime(dvWertpapiereGesamt[wpges]["QuotationDateOfPrice"].ToString());
                DateTime dt = Convert.ToDateTime(rowPortFol["WPStand"]);
                if (dt < standBank) {
                    rowPortFol["WPStand"] = standBank;
                    rowPortFol["WPKursVorher"] = Price;
                    rowPortFol["WPStandVorher"] = standBank;
                }
                rowPortFol["WPKaufsumme"] = wpkaufsumme;
                DateTime kaufdatum2 = (DateTime)dvWertpapiereGesamt[wpges]["MaturityDate"];         // Fälligkeitstag
                if (kaufdatum2 == new DateTime(1980, 1, 1))
                    continue;
                //Debug.WriteLine("WPBisDatum        : {0} {1} {2} {3}", name1, isin1, kaufdatum1, kaufdatum2);
                if (wpkaufdatum == kaufdatum2)
                    continue;
                //Debug.WriteLine("WPBisDatum updaten: {0} {1} {2}", name1, kaufdatum1, kaufdatum2);
                rowPortFol["WPBisDatum"] = kaufdatum2;
            }
            return true;
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        public static string[] HoleFiles(string path, string searchPattern, SearchOption searchOption) {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        internal string GetPasswort(string blz) {
            string path = GlobalRef.g_Ein.myDepotPfad + @"\daten";           
            string searchPattern = "dwp_*|*_"; 
            string[] files = HoleFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            string dwp = null;
            string xmlblz = null;
            foreach (string file in files) {
                FileInfo fsi = new FileInfo(file);
                if (!fsi.Name.Contains(".xml"))
                    continue;
                XmlTextReader reader = new XmlTextReader(path + "/" + fsi.Name);
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("Passworte"))
                                continue;
                            xmlblz = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (xmlblz.Equals("BLZ" + blz))
                                dwp = reader.Value;
                            break;
                        case XmlNodeType.EndElement:
                            //Console.Write("</" + reader.Name + ">");
                            break;
                    }
                }
            }
            return dwp;
        }
    }
}