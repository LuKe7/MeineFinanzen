// 26.11.2018   -Model-  WertpapHBCI4j.cs 
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class WertpapHBCI4j {
        public string BLZ { get; set; }
        public DateTime Zeit { get; set; }
        public string ISIN { get; set; }
        public string Name { get; set; }
        public string wptype { get; set; }
        public DateTime KursZeit { get; set; }
        public double Saldo { get; set; }
        public string SaldoType { get; set; }
        public double Kurs { get; set; }
        public string KursWaehrung { get; set; }
        public double DepotWert { get; set; }
        public string DepotWaehrung { get; set; }
        public string EinstandsPreis { get; set; }
        public string EinstandsPreisWaehrung { get; set; }
        public DateTime KaufDatum { get; set; }
        /*
                public event PropertyChangedEventHandler PropertyChanged;
                internal void NotifyPropertyChanged(string propertyName) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                protected void RaisePropertyChanged(string name) {
                    if (PropertyChanged != null) {
                        PropertyChanged(this, new PropertyChangedEventArgs(name));
                        }
                    }
                public void BeginEdit() { }
                public void CancelEdit() { }
                public void EndEdit() { }
                */
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(WertpapHBCI4j));
        public void SerializeWriteWertpapHBCI4j(string filename, WertpapHBCI4j wp) {
            try {
                using (StreamWriter wr = new StreamWriter(filename, false)) {
                    xmlserializer.Serialize(wr, wp);
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler: in SerializeWriteWertpapHBCI4j() " + ex + Environment.NewLine + filename);
            }
        }
        public void DeserializeReadWertpapHBCI4j(string filename, out WertpapHBCI4j wphbci) {   //List<WertpapHBCI4j> wphbci) {
            wphbci = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    wphbci = (WertpapHBCI4j)xmlserializer.Deserialize(_reader);
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler: DeserializeReadWertpapHBCI4j(): " + ex);
            }
            AktualisiereWertpapHBCI4jDaten(filename, wphbci);
        }
        private void AktualisiereWertpapHBCI4jDaten(string filename, WertpapHBCI4j wphbci) {
            FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));
            DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);
            //bankÜ[0].Bild Pfad7 = Zahlung.B.;
            /* bankÜ.Bank Name7 =
            bankÜ.Bank Value7 =
            bankÜ.LiBankKonten[0].Konto Art8 =
            bankÜ.LiBankKonten[0].Konto Datum8 =
            bankÜ.LiBankKonten[0].Konto Name8 =
            bankÜ.LiBankKonten[0].Konto Nr8 =
            bankÜ.LiBankKonten[0].Konto Value8 = */
        }
    }
}