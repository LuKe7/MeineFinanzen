// Zahlungen.cs 07.10.2016
// Aus CollKontoumsätzeGesamt Zahlungen extrahieren.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Data;
using DataSetAdminNS;
namespace MeineFinanzen.View {
    public partial class Zahlungen : Window {
        List<ISIN> isins = new List<ISIN>();
        public Model.CollZahlungen _zahlungen = null;
        public Model.CollKontoumsätze _kontoumsätze = null;
        HauptFenster _mw;
        Random rand = new Random();
        public Zahlungen() {
            conWrLi("---- -80- Zahlungen()");
        }
        public Zahlungen(HauptFenster mw) {
            conWrLi("---- -81- Zahlungen (HauptFenster mw)");
            _mw = mw;
            InitializeComponent();
            conWrLi("---- -82- Zahlungen nach InitializeComponent()");
            _zahlungen = (Model.CollZahlungen)Resources["zahlungen"];
            _kontoumsätze = (Model.CollKontoumsätze)mw.Resources["kontoumsätze"];
            conWrLi("---- -83- Zahlungen nach Kto Ums holen");
        }
        public void conWrLi(string str1) {
           Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            conWrLi("---- -84- Zahlungen Window_Loaded");
            int anz = 0;
            double wert = 0;
            //string[] sArr = new string[] { "nix", "ZINSEN/DIVIDENDE", "WERTPAPIERZAHLUNG", "STORNO" };
            foreach (Model.Wertpapier wp in _mw._tabwertpapiere._wertpapiere) {
                if (wp.ISIN.Length != 12)
                    continue;
                if (wp.ISIN.Contains("7483612"))
                    Console.WriteLine("wp: {0} DE0007483612", wp.Name);
                foreach (Model.Kontoumsatz ku in _kontoumsätze) {
                    if (!ku.PaymtPurpose.Contains(wp.ISIN))
                        continue;
                    if (ku.PaymtPurpose.Contains("7483612"))
                        Console.WriteLine("\tku: {0} DE0007483612 knr: {1} name1: {2} name2: {3}", ku.BankCode, ku.Kontonummer, ku.Name1, ku.Name2);
                    //Debug.WriteLine("---- -85-1- Zahlungen ISIN: " + wp.ISIN + " EntryText: " + ku.EntryText + " PaymtPurpose: " + ku.PaymtPurpose);
                    if ((!ku.EntryText.Contains("ZINSEN/DIVIDENDE"))
                     && (!ku.EntryText.Contains("WERTPAPIERZAHLUNG"))
                     && (!ku.EntryText.Contains("STORNO"))
                     && (!ku.EntryText.Contains("WERTP. ABRECHN."))
                     && (!ku.EntryText.Contains("WERTPAPIERE"))
                     && (!ku.PaymtPurpose.Contains("Wertpapierertrag"))
                     && (!ku.PaymtPurpose.Contains("STEUERAUSGLEICH"))
                     && (!ku.PaymtPurpose.Contains("WERTP. ABRECHN."))
                     && (!ku.PaymtPurpose.Contains("Wertp.Abrechn."))    ) {
                        // DEPOT     700617681|WERTP. ABRECHN.   25.03.15|000006030110100  WKN A0YJMG|GESCH.ART  KV|WHC - GLOBAL DISCOVERY|DE000A0YJMG1
                        // Depot 0700617681|Wertp.Abrechn. 22.09.2016|000001067448600 WKN A1JRQD|Gesch.Art KV|4Q-SPECIAL INCOME EUR(R)|ISIN DE000A1JRQD1
                        continue;
                    }
                    //Console.WriteLine("++++ -85-2- Zahlungen ISIN: " + wp.ISIN + " EntryText: " + ku.EntryText + " PaymtPurpose: " + ku.PaymtPurpose);
                    ++anz;
                    wert = Convert.ToDouble(ku.Value);
                    _zahlungen.Add(new Model.Zahlung {
                        Anzahl = wp.Anzahl.ToString(),
                        Isin = wp.ISIN,
                        Name = wp.Name,
                        EntryDate = ku.EntryDate,
                        ValueDate = ku.ValueDate,
                        Value = ku.Value,
                        AcctNo = ku.AcctNo,
                        BankCode = ku.BankCode,
                        Name1 = ku.Name1,
                        Name2 = ku.Name2,
                        PaymtPurpose = ku.PaymtPurpose,
                        EntryText = ku.EntryText,
                        PrimaNotaNo = ku.PrimaNotaNo,
                        TranTypeIdCode = ku.TranTypeIdCode,
                        ZkaTranCode = ku.ZkaTranCode,
                        TextKeyExt = ku.TextKeyExt,
                        BankRef = ku.BankRef,
                        OwnerRef = ku.OwnerRef,
                        SupplementaryDetails = ku.SupplementaryDetails
                    });
                    DateTime dat = Convert.ToDateTime(ku.ValueDate);
                    string strDate = dat.ToString("dd.MM.yy");
                    isins.Add(new ISIN { Isin = wp.ISIN, Name = wp.Name, eingefügt = false, Datum = strDate, Anzahl = anz, Wert = wert, EntryText = ku.EntryText, PaymtPurpose = ku.PaymtPurpose });
                    if (wp.ISIN.Contains("DE0007483612"))
                        Console.WriteLine("B-DE0007483612: " + wert + " " + strDate + " " + wp.Name);
                }
                if (anz > 0)
                    isins.Add(new ISIN { Isin = "", Name = "------", Anzahl = 0, Wert = 0 });
                anz = 0;
                wert = 0;
            }
            ICollectionView cvZahlungen = CollectionViewSource.GetDefaultView(gridZahlungen.ItemsSource);
            cvZahlungen.GroupDescriptions.Clear();
            gridZahlungen.ItemsSource = _zahlungen;
            gridWP.ItemsSource = isins;
            foreach (ISIN isi in isins) {
                if (isi.Name == "------")
                    continue;
                bool einfügen = true;
                foreach (DataRow dr in DataSetAdmin.dtPortFolBew.Rows) {
                    if (System.DBNull.Value.Equals(dr["isin"]))
                        continue;
                    if ((string)dr["isin"] != isi.Isin)
                        continue;
                    foreach (DataRow dr2 in DataSetAdmin.dtPortFolBew.Rows) {
                        if (dr2["ISIN"].ToString() == isi.Isin) {
                            if (dr2["Datum"].ToString() == isi.Datum) {
                                einfügen = false;
                                break;
                            }
                        }
                    }
                }
                if (einfügen) {
                    DataRow newRow = DataSetAdmin.dtPortFolBew.NewRow();
                    newRow["ID"] = rand.Next();
                    newRow["ISIN"] = isi.Isin;
                    newRow["Name"] = isi.Name;
                    newRow["Datum"] = isi.Datum;
                    newRow["Betrag"] = isi.Wert;
                    newRow["IDvomGiroKto"] = 0;
                    newRow["Feld1"] = isi.EntryText;
                    newRow["IDvomWP"] = 0;
                    newRow["Text1"] = isi.PaymtPurpose;
                    try {
                        DataSetAdmin.dtPortFolBew.Rows.Add(newRow);
                        isi.eingefügt = true;
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Fehler in .Rows.Add:" + ex);
                        Console.WriteLine("{0} {1} {2} {3} {4}", isi.Isin, isi.Name, isi.eingefügt, isi.Datum, isi.Wert);
                        break;
                    }
                }
            }
            DataSetAdmin.DatasetSichernInXml("MeineFinanzen");
            this.Close();
        }
        public void undLos() {
            //foreach (isin isi in ISINs)
            {
                /* if (strLine.Contains(isi))
                {
                    strLine = zeileKürzen(strLine);
                    strArray = csvParser(strLine, ';');
                    if (strArray[0] == "2013-02-29")
                        strArray[0] = "2013-02-28";
                    if (strArray[1] == "2013-02-29")
                        strArray[1] = "2013-02-28";
                    if (strArray[0] == "2014-02-29")
                        strArray[0] = "2014-02-28";
                    if (strArray[1] == "2014-02-29")
                        strArray[1] = "2014-02-28";
                    //strArray = strLine.Split(seperator);   
                 */
                // CollZahlungen.
                //.Rows.Add(strArray);
                //break;
            }
        }
        public class ISIN {
            public string Isin { get; set; }
            public string Name { get; set; }
            public bool eingefügt { get; set; }
            public string Datum { get; set; }
            public int Anzahl { get; set; }
            public double Wert { get; set; }
            public string EntryText { get; set; }
            public string PaymtPurpose { get; set; }
        }
    }
}