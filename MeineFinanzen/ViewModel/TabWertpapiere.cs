// 10.03.2018   -ViewModel-  TabWertpapiere.cs DataGrid 'dgWertpapiere' erstellen.
// 19.02.2014 Text Zahlungen >= 8
// public class Wertpapiere : ObservableCollection<Wertpapier>
// Im folgenden Beispiel werden das Gruppieren, Sortieren und Filtern von Wertpapiere-Daten in einer
// CollectionViewSource und das Anzeigen der gruppierten, sortierten und gefilterten
// Wertpapier-Daten in einem DataGrid veranschaulicht.
// Die CollectionViewSource wird als ItemsSource für das DataGrid verwendet.
// Gruppierung, Sortierung und Filterung werden für die CollectionViewSource ausgeführt und
// in der DataGrid-Benutzeroberfläche angezeigt.
// 12.08.2014 _wertpapiere nur einmal füllen!!!
// 07.03.2015 Mit Summe Depot.
// 25.08.2015 Vortageswerte korr.
// 19.10.2015 Mit Summe Depot. Korrigiert. 
// 23.07.2016 Ren1J auf 12 Monate!!! berechnen.
// 16.10.2016 Bei Giro-Konten den Kontostand/-Datum aus liba holen.
// 03.03.2018 Ohne _wertpapiere, jetzt aus DgBanken...
// 07.03.2018 _wertpapiere nach DgBanken verlegt
// 10.03.2018 Mit CollectionViewSource.GetDefaultView(mw.dgWertpapiere.ItemsSource).Refresh();
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using MeineFinanzen.Model;
using DataSetAdminNS;
namespace MeineFinanzen.ViewModel {
    public class TabWertpapiere {      
        public double suZahlungenlfdJ = 0;
        public double suZahlungenAlle = 0;
        public double suErtrag = 0, suAktuWert = 0, suKaufWert = 0, su0101Wert = 0;     // Zeile
        public double suErtrag2 = 0, suAktuWert2 = 0, suKaufWert2 = 0, suZahlungen2 = 0, su0101Wert2 = 0;       // pro AKName
        public double suErtragWP = 0, suAktuWertWP = 0, suKaufWertWP = 0, suZahlungenWP = 0, su0101WertWP = 0;  // WertPapiere
        public double suErtrag3 = 0, suAktuWert3 = 0, suKaufWert3 = 0, suZahlungen3 = 0, su0101Wert3 = 0;       // Gesamt
        //double suErtragOK = 0, suAktuWertOK = 0, suKaufWertOK = 0, suZahlungenOK = 0, su0101WertOK = 0;       // ohne Kurs
        public const int BerechnungsmethodeLaufzeitEnde = 1;
        public const int BerechnungsmethodeAktuellerWert = 2;
        public const int BerechnungsmethodeBesterWert = 3;
        public int glGridPortfolSort;
        public static int glGridAnleihenType = BerechnungsmethodeBesterWert;
        double aktKurs = 0;
        double kursVorher = 0;
        string name = "";
        int ak = -1;
        string aKKurz = null;
        string aKName = null;
        double kaufSumme = 0.00;
        double suzahlungen = 0.00;
        long typeid = -1;
        const int GeldKto = 10;
        const int Anleihe = 70;
        DateTime keinDatum = Convert.ToDateTime("01.01.1980");
        public TabWertpapiere() { }
        public void ErstelleDgBankenWertpapiere(View.HauptFenster mw) {
            mw.tabWertpapiere.Visibility = Visibility.Visible;
            mw.dgWertpapiere.EnableRowVirtualization = false;
            DgBanken._wertpapiere = (CollWertpapiere)mw.Resources["wertpapiereXXX"];
            if (mw.dgWertpapiere.Items.Count > 1)
                DgBanken._wertpapiere.Clear();
            ICollectionView cvWertpapiere = CollectionViewSource.GetDefaultView(mw.dgWertpapiere.ItemsSource);
            if (cvWertpapiere != null) {
                cvWertpapiere.GroupDescriptions.Clear();
            }            
            mw.dgWertpapiere.UpdateLayout();
            typeid = -1;
            FelderLöschen();
            //wpVorher = null;       // NOCH holeVortagesDaten();
            DataTable dtt2 = new DataTable();
            DataSetAdmin.dvPortFol.Sort = "WPTypeID";
            DataSetAdmin.dtPortFol.DefaultView.Sort = "WPTypeID ASC";
            dtt2 = DataSetAdmin.dtPortFol.DefaultView.ToTable();
            DataSetAdmin.dtPortFol = dtt2;
            for (int ir = 0; ir < DataSetAdmin.dtPortFol.Rows.Count; ir++) {
                if (typeid == -1)
                    typeid = (int)DataSetAdmin.dtPortFol.Rows[ir]["WPTypeID"];
                ak = DataSetAdmin.dvAnlKat.Find(typeid);
                aKKurz = Convert.ToString(DataSetAdmin.dvAnlKat[ak]["AKKurz"]);
                aKName = Convert.ToString(DataSetAdmin.dvAnlKat[ak]["AKName"]);
                if (typeid != (int)DataSetAdmin.dtPortFol.Rows[ir]["WPTypeID"]) {
                    //Console.WriteLine("typeid1 wechsel typeid:{0} ir:{1} aKName:{2} type:{3}", typeid, ir, aKName, typeid);
                    AnzeigenSummen2(aKName);
                    typeid = (int)DataSetAdmin.dtPortFol.Rows[ir]["WPTypeID"];
                    ak = DataSetAdmin.dvAnlKat.Find(typeid);
                    aKKurz = Convert.ToString(DataSetAdmin.dvAnlKat[ak]["AKKurz"]);
                    aKName = Convert.ToString(DataSetAdmin.dvAnlKat[ak]["AKName"]);
                }
                float anzahl = (float)DataSetAdmin.dtPortFol.Rows[ir]["WPAnzahl"];
                name = (string)DataSetAdmin.dtPortFol.Rows[ir]["WPName"];
                string iSIN = (string)DataSetAdmin.dtPortFol.Rows[ir]["WPISIN"];
                Zahlungen(iSIN, out suZahlungenlfdJ, out suZahlungenAlle);
                suzahlungen = suZahlungenAlle;
                string kontoNr = "";
                kontoNr = (string)DataSetAdmin.dtPortFol.Rows[ir]["WPKontoNr"];
                aktKurs = Convert.ToDouble(DataSetAdmin.dtPortFol.Rows[ir]["WPKurs"]);
                if (typeid == GeldKto) {
                    foreach (var ban in DgBanken.banken) {
                        foreach (BankKonten bako in ban.OCBankKonten) {
                            if (kontoNr == bako.KontoNr8) {
                                aktKurs = Convert.ToDouble(bako.KontoValue8);
                                break;
                            }
                        }
                    }
                }
                kursVorher = Convert.ToDouble(DataSetAdmin.dtPortFol.Rows[ir]["WPKursVorher"]);
                double ertrLZE = 0;
                suAktuWert = anzahl * aktKurs;
                suKaufWert = (double)DataSetAdmin.dtPortFol.Rows[ir]["WPKaufSumme"];
                suErtrag = suAktuWert + suZahlungenAlle - suKaufWert;
                Single heute = 0;
                try {
                    heute = (Single)DataSetAdmin.dtPortFol.Rows[ir]["WPProzentAenderung"];
                } catch (Exception) {
                    heute = 0;
                }
                double rend = 0;
                if (suKaufWert != 0) {
                    if (typeid == Anleihe)
                        rend = (suErtrag * 100 / suKaufWert);
                    else
                        rend = ((suAktuWert + suZahlungenAlle - suKaufWert) * 100 / suKaufWert);
                }
                float rend1j;
                //su0101Wert = (double)(DataSetAdmin.dtPortFol.Rows[ir]["WP0101Summe"]);
                Wertpapier wp1 = Hole0101Wertpapier(iSIN);
                su0101Wert = 0;
                if (wp1 != null)
                    su0101Wert = wp1.Anzahl * wp1.AktKurs;
                //Console.WriteLine("{0} {1,-60} {2,-20} {3,-20} {4,-16} {5,-4} {6,-12}",
                //     wp1.KursZeit, wp1.Name, wp1.ISIN, wp1.KaufDatum, wp1.Kaufsumme, wp1.Type, su0101Wert);
                rend1j = 0;
                if (su0101Wert != 0)
                    rend1j = (float)((suAktuWert + suZahlungenlfdJ - su0101Wert) * 100.00 / su0101Wert);
                DateTime kursZeit = DateTime.Today;
                DateTime kaufDatum = Convert.ToDateTime("01.01.1980");
                int depotID = 0;
                Single zinssatz = 0;
                DateTime abDatum = Convert.ToDateTime("01.01.1980");
                DateTime bisDatum = Convert.ToDateTime("01.01.1980");
                Single sharpe = 0;
                string url = "";
                if (typeid == GeldKto) {
                    kursZeit = (DateTime)DataSetAdmin.dtPortFol.Rows[ir]["WPStand"];
                    rend1j = 0;
                    rend = 0;
                    kaufSumme = 0;
                    suErtrag = 0;
                    depotID = (int)DataSetAdmin.dtPortFol.Rows[ir]["WPDepotID"];
                    //aktKurs = Convert.ToSingle(DataSetAdmin.dtPortFol.Rows[ir]["WPKtoKurs"]);                                        
                } else {
                    try {
                        kaufDatum = (DateTime)DataSetAdmin.dtPortFol.Rows[ir]["WPKaufDatum"];
                        kursZeit = (DateTime)DataSetAdmin.dtPortFol.Rows[ir]["WPStand"];
                        aktKurs = Convert.ToSingle(DataSetAdmin.dtPortFol.Rows[ir]["WPKurs"]);
                        kaufSumme = (double)DataSetAdmin.dtPortFol.Rows[ir]["WPKaufsumme"];
                        depotID = (int)DataSetAdmin.dtPortFol.Rows[ir]["WPDepotID"];
                        zinssatz = (Single)DataSetAdmin.dtPortFol.Rows[ir]["WPZinsSatz"];
                        abDatum = (DateTime)DataSetAdmin.dtPortFol.Rows[ir]["WPAbDatum"];
                        bisDatum = (DateTime)DataSetAdmin.dtPortFol.Rows[ir]["WPBisDatum"];
                        //if (iSIN == "DE0008490962")
                        //    sharpe = (Single)DataSetAdmin.dtPortFol.Rows[ir]["WPSharpe"];
                        sharpe = (Single)DataSetAdmin.dtPortFol.Rows[ir]["WPSharpe"];
                        url = (string)DataSetAdmin.dtPortFol.Rows[ir]["WPUrlText"];
                    } catch (Exception ex) {
                        MessageBox.Show("ErstelleDgBankenWertpapiere() Fehler: " + ex);
                    }
                }
                if (typeid == Anleihe) {
                    double EffZins = EffektivZins(name, anzahl, suKaufWert, anzahl * 100, bisDatum, zinssatz);
                    double stZins = StueckZins(name, anzahl, suKaufWert, anzahl * 100, bisDatum, zinssatz);
                    aktKurs += stZins;
                    //suAktuWert = anzahl * aktKurs;
                    double suErtragAktWert = suAktuWert + suZahlungenAlle - suKaufWert;
                    // 1. Berechnung für:  Bis Laufzeitende halten.
                    ertrLZE = EffZins * suKaufWert / 100;
                    //if (MyPortfolio.glGridAnleihenType == MyPortfolio.BerechnungsmethodeAktuellerWert)
                    suErtrag = suErtragAktWert;           // 2. Berechnung für:  Aktueller Wert.                        
                                                          //if (MyPortfolio.glGridAnleihenType == MyPortfolio.BerechnungsmethodeBesterWert)
                    {
                        if (ertrLZE > suErtrag)     // 3. Berechnung für:  Bester Wert von 1. und 2.
                            suErtrag = ertrLZE;
                        else
                            suErtrag = suErtragAktWert;
                    }
                }
                DgBanken._wertpapiere.Add(new Wertpapier {
                    Anzahl = anzahl,
                    Name = name,
                    Heute = heute,
                    Rend = rend,
                    Rend1J = rend1j,
                    Ertrag = suErtrag,
                    KursZeit = kursZeit,
                    AktKurs = aktKurs,
                    KursVorher = kursVorher,
                    AktWert = suAktuWert,
                    Zahlungen = suzahlungen,  //suZahlungenAlle,
                    Kaufsumme = kaufSumme,
                    KaufDatum = kaufDatum,
                    DepotID = depotID,
                    KontoNr = kontoNr,
                    ISIN = iSIN,
                    Type = (int)typeid,
                    AKName = aKName,
                    AKKurz = aKKurz,
                    Zins = zinssatz,
                    AbDatum = abDatum,
                    BisDatum = bisDatum,
                    Sharpe = sharpe,
                    URL = url,
                    isSumme = false
                });
                suZahlungen2 += suZahlungenAlle;
                if (typeid != GeldKto)
                    suKaufWert2 += suKaufWert;
                su0101Wert2 += su0101Wert;
                //Console.WriteLine("{0,36} su0101Wert: {1,10} su0101Wert2: {2,10}  su0101Wert3: {3,10}",
                //    name, su0101Wert, su0101Wert2, su0101Wert3);
                suAktuWert2 += suAktuWert;
                suErtrag2 += suErtrag;
                /*  if (String.Compare(iSIN, "999") <= 0)
                 {
                     suAktuWertOK += suAktuWert;
                     suZahlungenOK += suZahlungenlfdJ;
                     suKaufWertOK += suKaufWert;
                     su0101WertOK += su0101Wert;
                     suErtragOK += suErtrag;
                 } */
                //Console.WriteLine("{0} {1, -38} {2, -14} {3, 10}", ir, name, iSIN, String.Format("{0:###,##0.00;#0.00-;' '}", suAktuWert));
                suZahlungenlfdJ = 0;
                suAktuWert = 0;
                suKaufWert = 0;
                su0101Wert = 0;
                suErtrag = 0;
            }   // ir ... dtPortFol.Rows.Count   
            // Ende Wertpapiere loop                                                                     
            AnzeigenSummen2(aKName);
            aKName = "EndSummen";
            DgBanken._wertpapiere.Add(new Wertpapier {
                Anzahl = 0,
                Name = "Summe Depot",
                Heute = 0,
                Rend = 0,
                Rend1J = 0,
                Ertrag = suErtragWP,
                KursZeit = keinDatum,
                AktKurs = 0,
                //KtoKurs = 0,
                AktWert = suAktuWertWP,
                Zahlungen = suZahlungenWP,
                Kaufsumme = suKaufWertWP,
                KaufDatum = keinDatum,
                DepotID = 0,
                KontoNr = "",
                ISIN = "",
                Type = 98,
                AKName = aKName,
                AKKurz = aKKurz,
                Zins = 0,
                AbDatum = keinDatum,
                BisDatum = keinDatum,
                Sharpe = 0,
                URL = "",
                isSumme = true
            });

            double re3 = 0.00;
            if (suKaufWert != 0.00)
                re3 = ((suAktuWert3 - suKaufWert3) * 100 / suKaufWert3);
            aKName = "EndSummen";
            Wertpapier wp = new Wertpapier {
                Anzahl = 0,
                Name = "Summe Gesamt",
                Heute = 0,
                Rend = re3,
                Rend1J = 0,
                Ertrag = suErtrag3,
                KursZeit = keinDatum,
                AktKurs = 0,
                //KtoKurs = 0,
                AktWert = suAktuWert3,
                Zahlungen = suZahlungen3,
                Kaufsumme = suKaufWert3,
                KaufDatum = keinDatum,
                DepotID = 0,
                KontoNr = "",
                ISIN = "",
                Type = 98,
                AKName = aKName,
                AKKurz = aKKurz,
                Zins = 0,
                AbDatum = keinDatum,
                BisDatum = keinDatum,
                Sharpe = 0,
                URL = "",
                isSumme = true
            };
            DgBanken._wertpapiere.Add(wp);
            CollectionViewSource.GetDefaultView(mw.dgWertpapiere.ItemsSource).Refresh();
        }
        internal void AnzeigenSummen2(string xname) {
            name = "Summe " + aKName;
            suAktuWert3 += suAktuWert2;
            suZahlungen3 += suZahlungen2;
            suKaufWert3 += suKaufWert2;
            su0101Wert3 += su0101Wert2;
            suErtrag3 += suErtrag2;
            if (typeid >= 20 && typeid <= 79) {
                suAktuWertWP += suAktuWert2;
                suZahlungenWP += suZahlungen2;
                suKaufWertWP += suKaufWert2;
                su0101WertWP += su0101Wert2;
                suErtragWP += suErtrag2;
            }
            //string str1 = string.Format("{0,8:###0.00 ;###0.00-;0.00 }", suAktuWert3);
            //Console.WriteLine("anzeigenSummen2() aKName:{0,-30} suAktuWert2:{1,14} suAktuWert3:{2,14}", name,
            //    string.Format("{0,12:#,##0.00 ;#,##0.00-;0.00 }", suAktuWert2),
            //    string.Format("{0,12:#,##0.00 ;#,##0.00-;0.00 }", suAktuWert3));
            DgBanken._wertpapiere.Add(new Wertpapier {
                Anzahl = 0,
                Name = name,
                Heute = 0,
                Rend = 0,
                Rend1J = 0,
                Ertrag = suErtrag2,
                KursZeit = keinDatum,
                AktKurs = 0,
                //KtoKurs = 0,
                AktWert = suAktuWert2,
                Zahlungen = suZahlungen2,
                Kaufsumme = suKaufWert2,
                KaufDatum = keinDatum,
                DepotID = 0,
                KontoNr = "",
                ISIN = "",
                Type = (int)typeid,
                AKName = aKName,
                AKKurz = aKKurz,
                Zins = 0,
                AbDatum = keinDatum,
                BisDatum = keinDatum,
                Sharpe = 0,
                URL = "",
                isSumme = true
            });
            suErtrag2 = 0;
            suAktuWert2 = 0;
            suKaufWert2 = 0;
            suZahlungen2 = 0;
            su0101Wert2 = 0;
        }
        public void FelderLöschen() {
            suAktuWert = 0;      // Summe pro Zeile
            suKaufWert = 0;
            su0101Wert = 0;
            suErtrag = 0;
            suAktuWert2 = 0;     // Summe pro AnlKat/
            suKaufWert2 = 0;
            suZahlungen2 = 0;
            su0101Wert2 = 0;
            suErtrag2 = 0;
            suAktuWert3 = 0;     // Summe pro Gesamt
            suKaufWert3 = 0;
            suZahlungen3 = 0;
            su0101Wert3 = 0;
            suErtrag3 = 0;
            suAktuWertWP = 0;    // Summe Wertpapiere
            suKaufWertWP = 0;
            suZahlungenWP = 0;
            su0101WertWP = 0;
            suErtragWP = 0;
            //suAktuWertOK = 0;    // Summe pro Gesamt ohne Kurs
            //suKaufWertOK = 0;
            //suZahlungenOK = 0;
            //su0101WertOK = 0;
            //suErtragOK = 0;

        }
        // http://www.stendal.hs-magdeburg.de/project/konjunktur/Fiwi/vorlesung/6.Semester/vorlesungsmaterial/T3%20Effektivzins.pdf
        // Bei Kauf/Verkauf zwischen den Zinszahlungen, bekommt der Verkäufer die entgangenen Zinsen anteilig vom Käufer bezahlt
        // Nominalzins =    8%              3,875
        // Emission         01.06.1998      01.02   
        // Laufzeit         2 Jahre
        // Zinszahlungem    jeweils 01.06   01.02
        // Verkauf          01.12.99(183T)  03.01.12(338T)
        // SZ = inom * (T / 365)
        // SZ = 8% * (183 / 365) = 4,01%
        public static double StueckZins(string strName, double Anzahl, double Kaufsumme, double EndWert, DateTime bisDatum, double Zins) {
            DateTime d1 = new DateTime(DateTime.Now.Year - 1, bisDatum.Month, bisDatum.Day, 0, 0, 0);
            double tageSeit = DateDiff(d1, DateTime.Today);
            if (tageSeit > 365)
                tageSeit -= 365;
            double stZins = Zins * (tageSeit / 365);
            /*Console.WriteLine("StueckZins Name:{0,-32} Zins:{1} bisDatum:{2} EndWert:{3} stZins:{4}",
                strName, Zins.ToString("##0.00"), bisDatum.ToString(), EndWert.ToString("###,##0.00"),
                stZins.ToString("0.00"));              */
            return stZins;
        }
        public static double DateDiff(DateTime t1, DateTime t2) { // in Tagen!!!
            double ret = 0;
            double d1 = t1.ToFileTime();
            double d2 = t2.ToFileTime();
            double d = (d2 - d1) / (double)TimeSpan.TicksPerDay;
            ret = Convert.ToDouble(d);
            return ret;
        }
        public static double EffektivZins(string strName, double Anzahl, double Kaufsumme, double EndWert, DateTime bisDatum, double Zins) {
            double nT = DateDiff(DateTime.Today, bisDatum);
            double nM = nT / 30;
            double RLZ = nM / 12;    //'RestLaufZeit in Jahren
            double Disagio = 100 - (Kaufsumme / Anzahl);
            double EffZins = Math.Round((Zins + (Disagio / RLZ)) / (100 - Disagio) * 10000) / 100;
            double EffZinsen = EffZins * Kaufsumme / 100;
            /*Console.WriteLine("EffektivZins:{0,-32} {1,-7} {2,5} {3,-10} {4,-4} {5,-9} {6,-5} {7,8:c}",
                    strName, Zins.ToString("##0.00"), Disagio.ToString("##0.00"),
                    bisDatum.ToString(), RLZ.ToString("0.00"),
                    EndWert.ToString("###,##0.00"), EffZins.ToString("0.00"),
                    EffZinsen.ToString("#,##0.00"));  */
            return EffZins;
        }
        internal void Zahlungen(string strisin, out double SuZahlungenlfdJ, out double SuZahlungenAlle) {
            SuZahlungenlfdJ = 0;
            SuZahlungenAlle = 0;
            double betr = 0;
            string str = Convert.ToString(DateTime.Now);
            DateTime firstDatelfdJ = Convert.ToDateTime("01.01." + str.Substring(6, 4));
            DateTime firstDateAlle = Convert.ToDateTime("01.01.1900");
            foreach (DataRow cRow in DataSetAdmin.dtPortFolBew.Rows)    // In Zahlungen erstellt. Enthält nur Zahlungen.
            {
                if (System.DBNull.Value.Equals(cRow["isin"]))
                    continue;
                if ((string)cRow["isin"] == strisin) {
                    //if (strisin == "DE000A0YJMG1")
                    //Console.WriteLine("DE000A0YJMG1");
                    string txt = cRow["Feld1"].ToString();
                    //if (txt.Length >= 8)
                    //    if (txt.Substring(0, 8) == "Verkauf:")
                    //        continue;
                    //if ((!txt.Contains("ZINSEN")) && (!txt.Contains("AUSSCHUETTUNG")))
                    //    continue;
                    betr = (double)cRow["Betrag"];
                    if (betr > 0) {

                        SuZahlungenAlle += betr;
                        DateTime secondDate = (DateTime)cRow["Datum"];
                        if (secondDate >= firstDatelfdJ) {
                            SuZahlungenlfdJ += (double)cRow["Betrag"];
                        }
                    }
                }
            }
            return;
        }
        // Wertpapier vom Anfang dieses Jahr holen.  Oder doch 1 Ganzes Jahr???   
        public Wertpapier HoleWertpapier(string fiName, string isin) {
            XmlSerializer xmlserializer = new XmlSerializer(typeof(CollWertpapiere));
            CollWertpapiere wp;
            string pfad = Helpers.GlobalRef.g_Ein.MyDataPfad + @"MyDepot\KursDaten";

            using (Stream reader = new FileStream(pfad + @"\" + fiName, FileMode.Open)) {
                wp = (CollWertpapiere)xmlserializer.Deserialize(reader);
            }
            foreach (Wertpapier wp1 in wp)
                if (wp1.ISIN == isin)
                    return wp1;
            return null;
        }
        public Wertpapier Hole0101Wertpapier(string isin) {
            // wp ab 01.01 des Vorjahres holen.            
            XmlSerializer xmlserializer = new XmlSerializer(typeof(CollWertpapiere));
            Wertpapier wp;
            string pfad = Helpers.GlobalRef.g_Ein.MyDataPfad + @"MyDepot\KursDaten";
            DirectoryInfo ParentDirectory = new DirectoryInfo(pfad);
            FileInfo[] fis = ParentDirectory.GetFiles();
            DateTime dtvj = DateTime.Now.Date.AddYears(-1);
            string strvor1j = "PortFol_" + dtvj.Year.ToString("0000") + dtvj.Month.ToString("00") + dtvj.Day.ToString("00") + ".xml";
            string strvj = "";
            int vgl = 0;
            foreach (FileInfo fi in fis) {
                vgl = string.Compare(strvor1j, fi.Name);
                if (vgl != 1) {
                    strvj = fi.Name;
                    wp = HoleWertpapier(fi.Name, isin);
                    if (wp == null)
                        continue;
                    else
                        return wp;
                }
            }
            return null;
        }
        /* public Wertpapier hole0101Wertpapier_ALT(string isin) {
            XmlSerializer xmlserializer = new XmlSerializer(typeof(CollWertpapiere));
            CollWertpapiere wp;
            string pfad = Helpers.GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten"; 
            DirectoryInfo ParentDirectory = new DirectoryInfo(pfad);
            FileInfo[] fis = ParentDirectory.GetFiles();
            DateTime dtvj = DateTime.Now.Date.AddYears(-1);
            string strvor1j = "PortFol_" + dtvj.Year.ToString("0000") + dtvj.Month.ToString("00") + dtvj.Day.ToString("00") + ".xml";
            string strvj = "";
            int vgl = 0;
            foreach (FileInfo fi in fis) {
                vgl = string.Compare(strvor1j, fi.Name);
                if (vgl != 1) {
                    strvj = fi.Name;
                    break;
                }
            }
            using (Stream reader = new FileStream(pfad + @"\" + strvj, FileMode.Open)) {
                wp = (CollWertpapiere)xmlserializer.Deserialize(reader);
            }
            foreach (Wertpapier wp1 in wp)
                if (wp1.ISIN == isin)
                    return wp1;
            return null;
        } */
        public Wertpapier HoleVortagesDatenXX() {
            // C :\U sers\LuKe\Documents\MeineFinanzen\MyDepot\KursDaten\PortFol_20151019 (2015_10_21 10_17_02 UTC).xml
            Wertpapier wpVorher = null;
            //string s = "";
            string pfad = "";
            DateTime dt = DateTime.Now;
            string[] arrFiles;
            int nFiles = 0;
            pfad = Helpers.GlobalRef.g_Ein.MyDataPfad + @"MyDepot\KursDaten";        //\PortFol_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
            foreach (string fi in Directory.GetFiles(pfad)) {
                FileInfo fsi = new FileInfo(fi);
                if ((fsi.Attributes & FileAttributes.Directory) == 0)
                    if (fsi.Name.Length >= 16)
                        if (fsi.Name.Substring(0, 8) == "PortFol_")
                            if (fsi.Name.Contains(".xml"))
                                nFiles++;                                            // nur Zählen 
            }
            Console.WriteLine("nFiles: {0}", nFiles);
            arrFiles = new string[nFiles];
            int nF = 0;
            foreach (string fi in Directory.GetFiles(pfad)) {
                FileInfo fsi = new FileInfo(fi);
                if ((fsi.Attributes & FileAttributes.Directory) == 0)
                    if (fsi.Name.Length >= 8)
                        if (fsi.Name.Substring(0, 8) == "PortFol_") {
                            Console.WriteLine("{0} {1}", nF, fsi.Name);
                            if (fsi.Name.Contains(".xml")) {
                                //PortFol_20150901
                                //if (!fsi.Name.Contains("_2015"))
                                //    continue;
                                Console.WriteLine("{0} {1}", nF, fsi.Name);
                                arrFiles[nF++] = fsi.Name.Substring(0, 16);           // füllen
                            }
                        }
            }
            IComparer myComparer = new MyReverserClass();
            Array.Sort(arrFiles, myComparer);

            // NOCH  pfad setzen

            XmlSerializer ser = new XmlSerializer(typeof(CollWertpapiere));
            using (Stream rd = new FileStream(pfad, FileMode.Open)) {
                wpVorher = (Wertpapier)ser.Deserialize(rd);
            }
            //Debug.WriteLine(wpVorher[0]);
            return wpVorher;
        }
        public Wertpapier HoleVortagesDatenALT() {
            // Suchen bis Datum kleiner ist als Heute.           
            Wertpapier wpVorher = null;
            string s = "";
            string pfad = "";
            DateTime dt = DateTime.Now;
            for (int i = 1; i < 50; i++) {
                s = (dt - new TimeSpan(i, 0, 0, 0, 0)).ToString();
                pfad = Helpers.GlobalRef.g_Ein.MyDataPfad + @"MyDepot\KursDaten\PortFol_" + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
                bool gef = File.Exists(pfad);
                if (gef)
                    break;
            }
            //while (!gefunden);       //!File.Exists(pfad));
            //XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Wertpapier>));
            XmlSerializer ser = null;
            try {
                ser = new XmlSerializer(typeof(CollWertpapiere));
            } catch (Exception ex) {
                MessageBox.Show("holeVortagesDaten() Fehler: " + ex);
                return wpVorher;
            }
            using (Stream rd = new FileStream(pfad, FileMode.Open)) {
                wpVorher = (Wertpapier)ser.Deserialize(rd);
            }
            //Debug.WriteLine(wpVorher[0]);
            return wpVorher;
        }
        /* public static decimal HoleProzAend(string strLine)
        {
            decimal hProzAend = 0m;
            string str = "";
            int iBeg, iEnd;
            string st = "div class=" + "\"" + "kurs" + "\"" + "";   //'<div class="kurs">  <div class="kurs">
            st = "div class=" + @"""Kurs""";                        //'<div class="kurs">  <div class="kurs">
            //st = "aktueller & nbsp; Kurs";                          //aktueller&nbsp;Kurs
            hProzAend = 0;
            int i1 = strLine.IndexOf(st);               //                        23116
            if (i1 <= 0)
                return 0m;
            iEnd = strLine.IndexOf("%<", i1);           //'"-0,51%   '27716       31561 23209
            if (iEnd == 0)
                return 0m;
            iBeg = strLine.IndexOf("\"", iEnd - 10);    //'27713                 31559 23202
            if ((iEnd - iBeg) < 0)
                return 0m;
            try
            {
                str = strLine.Substring(iBeg + 2, iEnd - iBeg - 2);
                hProzAend = Convert.ToDecimal(str);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Fehler HoleProzAend() hProzAend = Convert.ToDecimal(str);:" + ex.Message + " " + str);
            }
            if (iBeg == 0 || iEnd == 0)
                System.Windows.MessageBox.Show("HoleProzAend-Encoding-Fehler.");
            return hProzAend;
        } */
        public class MyReverserClass : IComparer {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y) {
                return ((new CaseInsensitiveComparer()).Compare(x, y));
            }
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}