// 07.12.2017 JahreswechselView.cs
// Setzt 
using System;
using System.Windows;
using System.Data;
using DataSetAdminNS;
using System.Xml.Serialization;
using System.IO;
namespace MeineFinanzen.View {
    public partial class JahreswechselView : Window {
        //List<ISIN> isins = new List<ISIN>();
        public Model.CollWertpapiere _wertpap = null;
        HauptFenster _mw;
        public JahreswechselView() {
            conWrLi("---- -xx- JahreswechselView()");
        }
        public JahreswechselView(HauptFenster mw) {
            conWrLi("---- -xx- JahreswechselView (HauptFenster mw)");
            _mw = mw;
            _wertpap = (Model.CollWertpapiere)Resources["wertpapiere"];          
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            conWrLi("---- -xx- JahreswechselView Window_Loaded");                       
            double aktWert = 0;
            double su0101Wert;           
            foreach (DataRow dr in DataSetAdmin.dtPortFol.Rows) {
                if (DBNull.Value.Equals(dr["WPISIN"]))
                    if (dr["WPISIN"].ToString().Length != 12)
                        continue;
                Model.Wertpapier wp = hole0101Wertpapier(dr["WPISIN"].ToString());                
                if (wp != null)
                    su0101Wert = wp.Anzahl * wp.AktKurs;
                else
                    su0101Wert = 0;                                     
                aktWert = wp.AktKurs * wp.Anzahl;
                //if (su0101Wert != 0)
                    //rend1j = (float)((suAktuWert + suZahlungenlfdJ - su0101Wert) * 100.00 / su0101Wert);
                Console.WriteLine("{0,-36} {1,-16} KursZeit:{2,12} AktWert: {3,12:c} ",
                    wp.Name, wp.ISIN, wp.KursZeit, wp.AktWert);
            }
        }
        public Model.Wertpapier holeWertpapier(string fiName, string isin) {
            XmlSerializer xmlserializer = new XmlSerializer(typeof(Model.CollWertpapiere));
            Model.CollWertpapiere wp;
            string pfad = Helpers.GlobalRef.g_Ein.myDataPfad + @"MyDepot\KursDaten";
           
            using (Stream reader = new FileStream(pfad + @"\" + fiName, FileMode.Open)) {
                wp = (Model.CollWertpapiere)xmlserializer.Deserialize(reader);
            }
            foreach (Model.Wertpapier wp1 in wp)
                if (wp1.ISIN == isin)
                    return wp1;
            return null;
        }
        public Model.Wertpapier hole0101Wertpapier(string isin) {
            // wp ab 01.01 des Vorjahres holen.            
            XmlSerializer xmlserializer = new XmlSerializer(typeof(Model.CollWertpapiere));
            Model.Wertpapier wp;
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
                    wp = holeWertpapier(fi.Name, isin);
                    if (wp == null)
                        continue;
                    else
                        return wp;
                }
            }
            return null;
        }
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
 /*   public class ISIN {
        public string Isin { get; set; }
        public string Name { get; set; }
        public bool eingefügt { get; set; }
        public string Datum { get; set; }
        public float Anzahl { get; set; }
        public double Wert { get; set; }
        public string EntryText { get; set; }
        public string PaymtPurpose { get; set; }
    }  */
}
