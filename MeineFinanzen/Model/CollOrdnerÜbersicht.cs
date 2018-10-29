// 18.10.2018   -Model-  CollOrdnerÜbersicht.cs 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollOrdnerÜbersicht : ObservableCollection<OrdnerÜbersicht> { }
    public class OrdnerÜbersicht {       
        public string Header { get; set; }
        public string Tag { get; set; }
        public string IsExpanded { get; set; }
        public double FontWeight { get; set; }
        public string BildPfad9 { get; set; }
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(List<OrdnerÜbersicht>));
        public void DeserializeReadOrdnerÜbersicht(string filename, out List<OrdnerÜbersicht> ordÜ) {
            // Read   
            ordÜ = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    ordÜ = (List<OrdnerÜbersicht>)xmlserializer.Deserialize(_reader);
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler: DeserializeReadBankÜbersicht -Read- " + ex);
            }
            AktualisiereOrdnerÜbersichtsDaten(filename, ordÜ);
            Console.WriteLine("===>DeserializeReadOrdnerÜbersicht = " + ordÜ[0].Header + "\n--->" + ordÜ[0].Tag +
               " " + ordÜ[0].BildPfad9);
            //ConWrLi("---- -x- DeserializeReadOrdnerÜbersicht()");
        }
        private void AktualisiereOrdnerÜbersichtsDaten(string filename, List<OrdnerÜbersicht> ordÜ) {
            throw new NotImplementedException();
        }
        public void SerializeWriteOrdnerÜbersicht(string filename, IList<OrdnerÜbersicht> ordÜ) {
            // Write             
            try {
                using (Stream writer = new FileStream(filename, FileMode.Create)) {
                    xmlserializer.Serialize(writer, ordÜ);
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler: SerializeWriteOrdnerÜbersicht -Write-  " + ex);
            }
            Console.WriteLine("===>SerializeWriteOrdnerÜbersicht =   " + ordÜ[0].Header + "\n--->" + ordÜ[0].Tag +
               " " + ordÜ[0].BildPfad9);
            //ConWrLi("---- -x- SerializeWriteOrdnerÜbersicht()");
        }
        private void AktualisiereOrdnerÜbersichtsDaten(string filename, IList<OrdnerÜbersicht> ordÜ) {
            FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));
            DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);            
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
    public class Ordner {
        public string KontoName8 { get; set; }
        public string KontoArt8 { get; set; }
        public string KontoNr8 { get; set; }       
    }
}