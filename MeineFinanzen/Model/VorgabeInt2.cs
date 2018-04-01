using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class VorgabeInt2 {
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Boxanfang { get; set; }
        public string Ausschluss1 { get; set; }
        public string Wert1 { get; set; }
        public string Wert2 { get; set; }
        public string Wert3 { get; set; }
        public string Wert4 { get; set; }
        public XmlSerializer xmlserializer = new XmlSerializer(typeof(List<VorgabeInt2>));
        public void DeserializeVorgabeInt2(string filename, out VorgabeInt2 vg2) {
            vg2 = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    vg2 = (VorgabeInt2)xmlserializer.Deserialize(_reader);
                }
            } catch (Exception ex) {
                System.Windows.MessageBox.Show("Fehler: DeSerializeVorgabeInt2 -Read- " + ex);
            }
        }
    }
    class MyXmlData {
        public int elf = 23;
        public string hello = "Hello World";
    }
}
