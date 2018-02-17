// 16.02.2018 KontenSynHBCI4j.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DataSetAdminNS;
using MeineFinanzen.Model;
using MeineFinanzen.Helpers;
namespace MeineFinanzen.View {
    public partial class KontenSynHBCI4j : Window {
        public WertpapHBCI4j wp4j = new WertpapHBCI4j();
        public List<WertpapHBCI4j> wp4js = new List<WertpapHBCI4j>();
        //internal DgBanken _b;
        public static DepotHolen _depHolen;
        public KontenSynHBCI4j() {
            InitializeComponent();
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
            double quantity = -1;       // Anzahl
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
                quantity = HBCI4j.Saldo;
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
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }

        private void Window_Loaded(Object sender, RoutedEventArgs e) {

        }

        private void CloseWindow(Object sender, System.ComponentModel.CancelEventArgs e) {

        }
    }
}