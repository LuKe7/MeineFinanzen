// 03.10.2016 TabKontoumsätze.cs
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using MeineFinanzen.Model;
using DataSetAdminNS;
namespace MeineFinanzen.ViewModel {
    public class TabKontoumsätze {
        public List<Kontoumsatz> liKontoUms = new List<Kontoumsatz>();
        public View.HauptFenster _mw;
        public TabKontoumsätze() { }
        public TabKontoumsätze(View.HauptFenster mw) {
            _mw = mw;
            string strKto = _mw.cbKonten.SelectedValue.ToString();
            List<Kontoumsatz> lku = Kontoumsatz_Füllen(strKto);
            mw.gridKontoumsätze.ItemsSource = null;
            mw.gridKontoumsätze.ItemsSource = lku;
        }
        private List<Kontoumsatz> Kontoumsatz_Füllen(string strKtoNr) {
            liKontoUms.Clear();
            DataTable dtKonto = new DataTable();
            dtKonto = DataSetAdmin.dtKontoumsätze.DefaultView.ToTable();
            try {
                string KtoNummer = "";
                if (strKtoNr == "Alle Konten")
                    KtoNummer = "Kontonummer <> '" + strKtoNr + "'";
                else
                    KtoNummer = "Kontonummer = '" + strKtoNr + "'";  //string expression = "Date > '1/1/00'";
                string sortOrder = "ValueDate DESC";
                DataRow[] foundRows = dtKonto.Select(KtoNummer, sortOrder);
                string strPaym;
                foreach (DataRow row in foundRows) {
                    strPaym = row["PaymtPurpose"].ToString();
                    if (strPaym.Length > 80)
                        strPaym = strPaym.Substring(0, 80);
                    liKontoUms.Add(new Kontoumsatz {
                        Kontonummer = row["Kontonummer"].ToString(),
                        EntryDate = String.Format("{0:yyyy/MM/dd }", row["EntryDate"]),
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
            }
            catch (Exception ex) {
                MessageBox.Show("Kontoumsatz_Füllen Fehler: " + ex);
            }
            return liKontoUms;
        }
    }
}