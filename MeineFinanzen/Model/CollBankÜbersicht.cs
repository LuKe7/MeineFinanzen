// 23.12.2017   -Model-  CollBanken.cs 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollBankÜbersicht : ObservableCollection<BankÜbersicht> {     
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            Wertpapier wp = obj as Wertpapier;
            if (wp != null)
                return wp.DepotID.CompareTo(((Wertpapier)obj).DepotID);
            throw new NotImplementedException();
            }
        }
    public class BankÜbersicht {
        public string SortFeld7 { get; set; }
        public string BildPfad7 { get; set; }
        public string BankName7 { get; set; }
        public double BankValue7 { get; set; }
        public string BLZ7 { get; set; }
        public string UserID7 { get; set; }
        public DateTime Datum7 { get; set; }
        public string Bearbeitungsart7 { get; set; }
        public string FunktionenPfad7 { get; set; }
        public string Status7 { get; set; }
        public ObservableCollection<BankKonten> OCBankKonten { get; set; }
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(List<BankÜbersicht>));
        public void DeserializeReadBankÜbersicht(string filename, out List<BankÜbersicht> bankÜ) {
            // Read   
            bankÜ = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    bankÜ = (List<BankÜbersicht>)xmlserializer.Deserialize(_reader);
                    }
                } catch (Exception ex) {
                MessageBox.Show("Fehler: DeserializeReadBankÜbersicht -Read- " + ex);
                }
            AktualisiereBankÜbersichtsDaten(filename, bankÜ);
            Console.WriteLine("===>DeserializeReadBankÜbersicht = " + bankÜ[0].BildPfad7 + "\n--->" + bankÜ[0].BankName7 +
               " " + bankÜ[0].BankValue7 + " " + bankÜ[0].OCBankKonten.Count);
            //ConWrLi("---- -x- DeserializeReadBankÜbersicht()");
            }
        public void SerializeWriteBankÜbersicht(string filename, IList<BankÜbersicht> bankÜ) {
            // Write             
            try {
                using (Stream writer = new FileStream(filename, FileMode.Create)) {
                    xmlserializer.Serialize(writer, bankÜ);
                    }
                } catch (Exception ex) {
                MessageBox.Show("Fehler: SerializeWriteBankÜbersicht -Write-  " + ex);
                }
            Console.WriteLine("===>SerializeWriteBankÜbersicht =   " + bankÜ[0].BildPfad7 + "\n--->" + bankÜ[0].BankName7 +
               " " + bankÜ[0].BankValue7 + " " + bankÜ[0].OCBankKonten.Count);
            //ConWrLi("---- -x- SerializeWriteBankÜbersicht()");
            }
        private void AktualisiereBankÜbersichtsDaten(string filename, IList<BankÜbersicht> bankÜ) {
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
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        }
    public class BankKonten {
        public string KontoName8 { get; set; }
        public string KontoArt8 { get; set; }
        public string KontoNr8 { get; set; }
        public double KontoValue8 { get; set; }
        public DateTime KontoDatum8 { get; set; }
        public ObservableCollection<Kontoumsatz> OCUmsätze { get; set; }
        public ObservableCollection<Wertpapier> OCWertpap { get; set; }
        }
    }