// 03.11.2018   -Model-  CollBanken.cs 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollBankÜbersicht : ObservableCollection<BankÜbersicht> {   }
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
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
    public class BankKonten : INotifyPropertyChanged, IEditableObject {
        public string KontoName8 { get; set; }
        public string KontoArt8 { get; set; }
        public string KontoNr8 { get; set; }
        public double KontoValue8 { get; set; }
        public DateTime KontoDatum8 { get; set; }
        public ObservableCollection<Kontoumsatz> OCUmsätze { get; set; }
        public ObservableCollection<Wertpapier> OCWertpap { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
}