// 16.02.2018   -Model-  Kontenaufstellung_HBCI4j.cs 
// 16.02.2018 Pfad
using MeineFinanzen.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollKontenaufstellung : ObservableCollection<CollKontenaufstellung> {
        public CollKontenaufstellung()        //  : base()
       {
            //Add(new CollKontenaufstellung("", "", "", "", "", "", "", "", "", "", "", ""));                      
            }
        //LUTZ KEMPS UND MONIKA KEMPS   9432438765   21352240   GiroOnline  DE     1004902 null  EUR     NOLADE21HOL DE85213522400001004902    Girokonto null
        //Name                          CustomerID   BLZ        Type        Ctry   Number  Subnr Curr    BIC         IBAN                      AccTy    Name2
        XmlSerializer xmlserializer = new XmlSerializer(typeof(Kontenaufstellung));
        public void SerializeWriteKontenaufstellung_HBCI4j(string filename, CollKontenaufstellung ko) {
            // Write
            try {
                using (StreamWriter wr = new StreamWriter(filename, false))     // false == nicht append                                                                           
                {
                    xmlserializer.Serialize(wr, ko);
                    }
                } catch (Exception ex) {
                MessageBox.Show("Fehler: in SerializeWriteKontenaufstellung_HBCI4j() " + ex + Environment.NewLine + filename);
                }
            }
        public void DeserializeReadKontenaufstellung_HBCI4j(string filename, out Kontenaufstellung kohbci) {
            // Read
            kohbci = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    kohbci = (Kontenaufstellung)xmlserializer.Deserialize(_reader);
                    }
                } catch (Exception ex) {
                MessageBox.Show("Fehler: DeserializeReadKontenaufstellung_HBCI4j(): " + ex);
                }
            AktualisiereKontenaufstellung_HBCI4jDaten(filename, kohbci);
            }
        private void AktualisiereKontenaufstellung_HBCI4jDaten(string filename, Kontenaufstellung kohbci) {
            FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));
            DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);
            }
        public void Kontenaufstellung_ReadXml() {
            string datenDir = Helpers.GlobalRef.g_Ein.myDepotPfad + @"\KursDaten\Depot-aus-hbci4j\";
            // laden aus datenDir
            // ---- In List WertpapHBCI4j importieren
            DirectoryInfo ParentDirectory2 = new DirectoryInfo(datenDir);
            FileInfo[] fis2 = ParentDirectory2.GetFiles();
            // s.u. DataSet dsHier = new DataSet();
            DgBanken.ko4js.Clear();
            foreach (FileInfo fi in fis2) {
                string strExt = fi.Extension;
                string strName = fi.Name;
                if ((string.Compare(strExt, ".xml") != 0) || (!strName.StartsWith("Kontenaufstellung_")))
                    continue;
                DgBanken.ko4j = null;
                DeserializeReadKontenaufstellung_HBCI4j(fi.FullName, out DgBanken.ko4j);
                //Console.WriteLine("{0,-28} {1,-16} {2,10} {3}", ko4j.Name, ko4j.BLZ, ko4j.Number, ko4j.Type);
                DgBanken.ko4js.Add(DgBanken.ko4j);
                }   // foreach FileInfo  
            }
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        }
    public class Kontenaufstellung {        //: INotifyPropertyChanged, IEditableObject {
        public string Name { get; set; }
        public string Customerid { get; set; }
        public string BLZ { get; set; }
        public string Type { get; set; }
        public string Ctry { get; set; }
        public string Number { get; set; }
        public string Subnr { get; set; }
        public string Curr { get; set; }
        public string BIC { get; set; }
        public string IBAN { get; set; }
        public string Kontoart { get; set; }
        public string Name2 { get; set; }
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
        public void EndEdit() { }  */
        }
    }