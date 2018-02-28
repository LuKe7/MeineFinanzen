// 13.02.2018 DgBanken.cs 
// 28.11.2016 KontenSynchronisieren() hier.
// 08.01.2017 Keine Extra-Zeilenende in Umsätze_....
// 08.06.2017 depHolen._bank ohn Blanks.
// 23.11.2017 xml-daten, von hbci4j erstellt, einlesen.
// 06.12.2017 Anzahl und quantity sind Single nnicht int!!!
// 28.12.2017 dtWertpapSubsembly hier löschen. Evtl. mehrere WPDepots.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DataSetAdminNS;
using MeineFinanzen.Model;
using MeineFinanzen.Helpers;
namespace MeineFinanzen.ViewModel {
    public class DgBanken {
        public static BankÜbersicht bank = new BankÜbersicht();
        public static BankKonten konto = new BankKonten();
        public static Kontoumsatz umsatz = new Kontoumsatz();
        public static Wertpapier wertpap = new Wertpapier();
        public static List<BankÜbersicht> banken = new List<BankÜbersicht>();
        public static List<BankKonten> konten = new List<BankKonten>();
        public static List<Kontoumsatz> umsätze = new List<Kontoumsatz>();
        public static List<Wertpapier> wertpaps = new List<Wertpapier>();
        public static Kontenaufstellung ko4j = new Kontenaufstellung();
        public static List<Kontenaufstellung> ko4js = new List<Kontenaufstellung>();
        public View.HauptFenster mw;
        public string sKontoNr;
        public double Betrag = 0.00;
        public static Helpers.DepotHolen _depHolen;
        //public string path MeineBilder = "";
        public DgBanken() {
            mw = GlobalRef.g_mw;
            //path MeineBilder = GlobalRef.g_Ein.strBilderPfad + @"\";
            //_depHolen = new DepotHolen();    // In FinKontenÜbersicht Grundwerte setzen und ini depHolen.
            }
        public void machdgbanken() {            
            //ConWrLi("---- -x- DgBanken vor Deserialisierung -read-");
            GlobalRef.g_Büb = new BankÜbersicht();
            GlobalRef.g_Büb.DeserializeReadBankÜbersicht(GlobalRef.g_Ein.myDepotPfad + @"\Daten\BankÜbersichtsDaten.xml", out banken);
            mw.dgBankenÜbersicht.ItemsSource = null;
            mw.dgBankenÜbersicht.ItemsSource = banken;
            mw.tabControl1.SelectedItem = mw.tabFinanzübersicht;
            mw.dgBankenÜbersicht.EnableRowVirtualization = false;
            testBankAnzeige();
            }
        public double SummeGeschlFonds() {
            double geschlfonds = 0.00;
            foreach (DataRow dr in DataSetAdmin.dtPortFol.Rows)
                if (dr["WPTypeID"].ToString() == "80")
                    geschlfonds += Convert.ToDouble(dr["WPKurs"].ToString());
            return geschlfonds;
            }
        public List<Kontoumsatz> KontoumsatzFüllen(string strKtoNr) {
            umsätze.Clear();
            DataTable dtKonto = new DataTable();
            dtKonto = DataSetAdmin.dtKontoumsätze.DefaultView.ToTable();
            try {
                string expression = "Kontonummer='" + strKtoNr + "'";
                string sortOrder = "ValueDate DESC";
                DataRow[] liste = dtKonto.Select(expression, sortOrder);
                string strPaym;
                foreach (DataRow row in liste) {
                    strPaym = row["PaymtPurpose"].ToString();
                    if (strPaym.Length > 80)
                        strPaym = strPaym.Substring(0, 80);
                    umsätze.Add(new Model.Kontoumsatz {
                        Kontonummer = row["Kontonummer"].ToString(),
                        ValueDate = String.Format("{0:yyyy/MM/dd}", row["ValueDate"]),
                        Value = String.Format("{0:###,##0.00 }", Convert.ToSingle(row["Value"])),
                        AcctNo = row["AcctNo"].ToString(),
                        BankCode = row["BankCode"].ToString(),
                        Name1 = row["Name1"].ToString(),
                        Name2 = row["Name2"].ToString(),
                        PaymtPurpose = strPaym,
                        EntryText = row["EntryText"].ToString()
                        });
                    }
                } catch { }
            return umsätze;
            }
        /* dtWPGes pif.Name:ISIN                               GetValue:DE000A0NEKQ8             PropertyType:System.String
        dtWPGes pif.Name:WKN                                GetValue:A0NEKQ                   PropertyType:System.String
        dtWPGes pif.Name:Quantity                           GetValue:385                      PropertyType:System.Decimal
        dtWPGes pif.Name:QuantityType                       GetValue:UNIT                     PropertyType:Subsembly.Swift.SwiftQuantityType
        dtWPGes pif.Name:QuantityCurrency                   GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:OriginOfPrice                      GetValue:LMAR                     PropertyType:System.String
        dtWPGes pif.Name:OriginOfPriceText                  GetValue:OTCO                     PropertyType:System.String
        dtWPGes pif.Name:SecurityName                       GetValue:ARAMEA RENDITE PLUS A    PropertyType:Subsembly.Swift.SwiftTextLines
        dtWPGes pif.Name:Price                              GetValue:187,26                   PropertyType:System.Decimal
        dtWPGes pif.Name:PriceCurrency                      GetValue:EUR                      PropertyType:System.String
        dtWPGes pif.Name:PriceType                          GetValue:ACTU                     PropertyType:Subsembly.Swift.SwiftPriceType
        dtWPGes pif.Name:QuotationDateOfPrice               GetValue:20171222                 PropertyType:Subsembly.Swift.SwiftDate
        dtWPGes pif.Name:QuotationTimeOfPrice               GetValue:000000                   PropertyType:Subsembly.Swift.SwiftTime
        dtWPGes pif.Name:TypeOfSecurity                     GetValue:811                      PropertyType:System.String
        dtWPGes pif.Name:SectorCode                         GetValue:00028                    PropertyType:System.String
        dtWPGes pif.Name:IssuerCountry                      GetValue:DE                       PropertyType:System.String
        dtWPGes pif.Name:BuyingDate                         GetValue:20170914                 PropertyType:Subsembly.Swift.SwiftDate
        dtWPGes pif.Name:MaturityDate                       GetValue:00000000                 PropertyType:Subsembly.Swift.SwiftDate
        dtWPGes pif.Name:CostPriceRate                      GetValue:187,2116                 PropertyType:System.Decimal
        dtWPGes pif.Name:CostPriceRateCurrency              GetValue:EUR                      PropertyType:System.String
        dtWPGes pif.Name:InterestRate                       GetValue:0                        PropertyType:System.Decimal
        dtWPGes pif.Name:FutureContractSymbol               GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractKey                  GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractExpiryDate           GetValue:00000000                 PropertyType:Subsembly.Swift.SwiftDate
        dtWPGes pif.Name:FutureContractBasicPrice           GetValue:0                        PropertyType:System.Decimal
        dtWPGes pif.Name:FutureContractBasicPriceCurrency   GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractVersion              GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractSize                 GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractUnderlyingWKN        GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:FutureContractUnderlyingISIN       GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:NumberOfDaysAccrued                GetValue:0                        PropertyType:System.Int32
        dtWPGes pif.Name:AccruedInterestCurrency            GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:AccruedInterestAmount              GetValue:0                        PropertyType:System.Decimal
        dtWPGes pif.Name:HoldingCurrency                    GetValue:EUR                      PropertyType:System.String
        dtWPGes pif.Name:HoldingValue                       GetValue:72095,1                  PropertyType:System.Decimal
        dtWPGes pif.Name:ExchangeRateFirstCurrency          GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:ExchangeRateSecondCurrency         GetValue:                         PropertyType:System.String
        dtWPGes pif.Name:ExchangeRate                       GetValue:0                        PropertyType:System.Decimal
        dtWPGes pif.Name:NumberOfSubBalances                GetValue:1                        PropertyType:System.Int32*/
        public string[] KontoStändeFinCmd(string cmdLine, string kto) {
            ConWrLi("---- -29e- In KontoumsatzFüllen " + kto);
            string[] strResult2;
            Directory.SetCurrentDirectory(Helpers.GlobalRef.g_Ein.strSubsemblyAPI);
            ProcessStartInfo startInfo = new ProcessStartInfo("FinCmd");
            startInfo.Arguments = cmdLine;
            mw.swLog.WriteLine("{0}", startInfo.Arguments);
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.WorkingDirectory = GlobalRef.g_Ein.myDataPfad + @ "FinTS";
            string result1 = "";
            string result2 = "";
            // balance -contactname ING-DiBa -pin xxxxxx -bankcode 50010517 -acctno 5591152146
            // balance -contactname Sparkasse Holstein -pin xxxxx -bankcode 21352240 -acctno 189227812
            try {
                using (Process process = Process.Start(startInfo)) {
                    using (StreamReader reader = process.StandardOutput) {
                        result1 = reader.ReadLine();
                        result2 = reader.ReadLine();
                        mw.swLog.WriteLine(result1);     // BalanceType; BankCode;   AcctNo;     Date;       Currency; Value
                        mw.swLog.WriteLine(result2);     // BOOKED;      50010517;   5591152146; 2012-01-18; EUR;      19300,99                                        
                        if (result1 == null)
                            return null;
                        char[] splitter = { ';' };
                        string[] strResult1 = result1.Split(splitter);
                        if (strResult1[0] != "BalanceType" | result2 == "") {
                            mw.swLog.WriteLine("performFinCmd() Fehler/Keine Daten: performFinCmd: {0}", result1);
                            return null;
                            }
                        strResult2 = result2.Split(splitter);
                        sKontoNr = strResult2[2];
                        string sValue = strResult2[5];
                        string strDateTime = strResult2[3].Replace(".", "-");
                        mw._Datum = Convert.ToDateTime(strDateTime);
                        int anzAendern = 0;
                        foreach (DataRow updRow in DataSetAdmin.dtPortFol.Rows) {
                            if (updRow["WPKontoNr"].ToString() == sKontoNr) {
                                ConWrLi("---- -29h- In KontoStändeFinCmd " + kto);
                                anzAendern += 1;
                                Betrag = Convert.ToDouble(sValue);
                                updRow["WPStand"] = mw._Datum;
                                updRow["WPKurs"] = Betrag;
                                mw.swLog.WriteLine("KontoumsätzeHolen() performFinCmd updRow KontoNr:{0} Betrag:{1}", sKontoNr, Betrag);
                                string strDateTime2 = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");  //"dd MMM HH:mm:ss",                             
                                using (StreamWriter sw = new StreamWriter(Helpers.GlobalRef.g_Ein.myDepotPfad + @"\Log\Kontenstände -" + sKontoNr + "-" + strDateTime2 + ".csv")) {
                                    int ii = 0;
                                    foreach (string zei in strResult2) {       // BOOKED;21352240;1004902;2016-09-12;EUR;1552,40
                                        if (ii > 0)
                                            sw.Write(";");
                                        sw.Write(zei);
                                        ii++;
                                        }
                                    sw.WriteLine();
                                    }
                                break;
                                }
                            }
                        }
                    }
                ConWrLi("---- -29j- In KontoStändeFinCmd " + kto);
                return strResult2;
                } catch (Exception ex) {
                MessageBox.Show("KontenÜbersicht, GetDataTable Fehler: " + ex);
                }
            return null;
            }
        public void KontoUmsätzeFinCmdStmt(string cmdLine, string ktoNr) {
            Directory.SetCurrentDirectory(Helpers.GlobalRef.g_Ein.strSubsemblyAPI);
            ProcessStartInfo startInfo = new ProcessStartInfo("FinCmd");
            startInfo.Arguments = cmdLine;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            string result1 = "";
            using (Process process = Process.Start(startInfo)) {
                using (StreamReader reader = process.StandardOutput) {
                    result1 = reader.ReadToEnd();
                    mw.swLog.WriteLine(result1);
                    char[] splitter = { '\n' };
                    string[] strResult1 = result1.Split(splitter);
                    string strDateTime = DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss");  //"dd MMM HH:mm:ss", 
                    strDateTime = strDateTime.Replace(".", "-");
                    if (sKontoNr != null) {
                        using (StreamWriter sw = new StreamWriter(Helpers.GlobalRef.g_Ein.myDepotPfad + @"\Log\Umsätze -" + sKontoNr + "-" + strDateTime + ".csv")) {
                            foreach (string zei in strResult1)
                                sw.Write(zei);
                            }
                        ConWrLi("---- -29f- In KontoUmsätzeFinCmdStmt " + ktoNr);
                        ErstelledtKontoumsätzeGesamt(';', strResult1, ktoNr);
                        }
                    }
                }
            }
        public void ErstelledtKontoumsätzeGesamt(char seperator, string[] strResult1, string ktoNr) {
            string[] strArray;
            string zeile;
            string Line = strResult1[0].Trim().Replace("\"", "");
            Line = zeileKürzen("Kontonummer;" + Line);
            string[] strArrayTitel = Line.Split(seperator);
            try {
                if (DataSetAdmin.dtKontoumsätze.Columns.Count == 0) {
                    foreach (string value in strArrayTitel) {
                        if (value.Trim() == "ValueDate")
                            DataSetAdmin.dtKontoumsätze.Columns.Add(value.Trim(), typeof(DateTime));
                        else if (value.Trim() == "EntryDate")
                            DataSetAdmin.dtKontoumsätze.Columns.Add(value.Trim(), typeof(DateTime));
                        else
                            DataSetAdmin.dtKontoumsätze.Columns.Add(value.Trim(), typeof(string));
                        }
                    }
                foreach (string strLine in strResult1) {
                    if ((strLine.Length <= 2) || (strLine.Contains("EntryDate")))
                        continue;
                    zeile = zeileKürzen(ktoNr + ";" + strLine);
                    strArray = csvParser(zeile, ';');
                    //Console.WriteLine("strLine: {0}", strLine);
                    if (strArray[1] == "2013-02-29")
                        strArray[1] = "2013-02-28";
                    if (strArray[2] == "2013-02-29")
                        strArray[2] = "2013-02-28";
                    if (strArray[1] == "2014-02-29")
                        strArray[1] = "2014-02-28";
                    if (strArray[2] == "2014-02-29")
                        strArray[2] = "2014-02-28";
                    if (strArray[2] == "2015-02-29")
                        strArray[2] = "2015-02-28";
                    if (strArray[2] == "2016-02-29")
                        strArray[2] = "2016-02-28";
                    if (strArray[2] == "2016-02-30")
                        strArray[2] = "2016-02-28";
                    if (strArray[2] == "2017-02-29")
                        strArray[2] = "2017-02-28";
                    //strArray = strLine.Split(seperator);  

                    DataSetAdmin.dtKontoumsätze.Rows.Add(strArray);
                    }
                } catch (Exception ex) {
                MessageBox.Show("KontenÜbersicht, GetDataTable Fehler: " + ex);
                }
            ConWrLi("---- -29h- ErstelledtKontoumsätzeGesamt " + ktoNr);
            return;
            }
        public string[] csvParser(string csv, char separator = ',') {
            List<string> parsed = new List<string>();
            string[] temp = csv.Split(separator);
            int counter = 0;
            string data = string.Empty;
            while (counter < temp.Length) {
                data = temp[counter].Trim().Replace("\"", "");
                parsed.Add(data);
                counter++;
                }
            return parsed.ToArray();
            }
        public string zeileKürzen(string strLine) {
            for (int i = strLine.Length - 1; i >= 0; i--)
                if (strLine.Substring(i, 1) == ";") {
                    if (i < strLine.Length - 1)
                        break;
                    strLine = strLine.Substring(0, strLine.Length - 1);
                    }
            return strLine;
            }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        public void testBankAnzeige() {
            Console.WriteLine("============ TestAnzeige aus DgBanken.cs  BankÜbersicht ============");
            Console.WriteLine("-ArrayOfBankÜbersicht");
            foreach (var ban in banken) {
                Console.WriteLine("\t-BankÜbersicht");
                Console.WriteLine("\t\t+BankPfad7   : {0,-12} ", ban.BildPfad7);
                Console.WriteLine("\t\t+BankName7   : {0,-12} ", ban.BankName7);
                Console.WriteLine("\t\t+BankValue   : {0,-12} ", ban.BankValue7);
                Console.WriteLine("\t\t-OCBankKonten");
                foreach (Model.BankKonten bako in ban.OCBankKonten) {
                    Console.WriteLine("\t\t\t-BankKonto");
                    Console.WriteLine("\t\t\t\t+KontoName 8: {0,-12} ", bako.KontoName8);
                    Console.WriteLine("\t\t\t\t+KontoArt8  : {0,-12} ", bako.KontoArt8);
                    Console.WriteLine("\t\t\t\t+KontoNr8   : {0,-12} ", bako.KontoNr8); 
                    Console.WriteLine("\t\t\t\t+KontoDatum8: {0,-12} ", bako.KontoDatum8);
                    if (bako.OCUmsätze != null) {
                        //foreach (Model.Umsatz ums in bako.OCUmsätze) {
                        //    Console.WriteLine("\t\t\t\tOCUmsätze, Kontonummer : {0,-12} {1}", ums.Kontonummer, ums.Name1 + ums.Name2);
                        //}
                        }
                    }
                }
            }
        }
    }