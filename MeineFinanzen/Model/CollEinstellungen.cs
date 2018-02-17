// 17.02.2018   --Model--   CollEinstellungen.cs
// 2017.10.27 Ordner Subsembly... wg Notebook angepasst.
// 16.02.2018 strHBCI4j   C :/Users/username/hbci4j-core/hbci4j-core-3.0.10
// 17.02.2018 BilderPfad.
using System.IO;
using System;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollEinstellungen : ObservableCollection<Einstellungen> { }
    public class Einstellungen : INotifyPropertyChanged, IEditableObject {
        public string[] strAusGr = new string[30];
        public string strPortfolioGridFontName { get; set; }
        public float sPortfolioGridFontSize { get; set; }
        public string strPortfolioGridFontStyle { get; set; }
        public string strPortfolioGridFontColor { get; set; }
        public int iPortfolioGridRowTemplateHeight { get; set; }
        public float sTabControl1FontSize { get; set; }
        public string strTabControl1FontName { get; set; }
        public string strTabControl1FontColor { get; set; }
        public string strTabControl1FontStyle { get; set; }
        public int iTabControl1SizeWidth { get; set; }
        public int iTabControl1SizeHeight { get; set; }
        public int iTabControl1ItemWidth { get; set; }
        public int iTabControl1ItemHeight { get; set; }
        public float bsHeight { get; set; }
        public float bsWidth { get; set; }
        public float bsHkorr { get; set; }
        public float bsWkorr { get; set; }
        public string myDocPfad { get; set; }       
        public string myDataPfad { get; set; }
        public string myDepotPfad { get; set; }
        public string strKompileTime { get; set; }
        public string strUrlIndizes { get; set; }
        // z.Zt."http://kurse.boerse.ard.de/ard/indizes_einzelkurs_uebersicht.htn?bigIndex=0&i=159096&sektion=einzelwerte&sortierung=descriptionShort&ascdesc=ASC"                                    
        public string strEinstellungen { get; set; }
        public string strVersion { get; set; }
        public string strSubsemblyAPI { get; set; }
        public string strHBCI4j { get; set; }
        public string strEclipseHbci4jClasses { get; set; }
        public string strBilderPfad { get; set; }
        public string strStartBild { get; set; }        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
        public Einstellungen() { }
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(Model.Einstellungen));
        public void DeSerializeReadEinstellungen(string filename, out Model.Einstellungen einD) {            
            einD = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    einD = (Model.Einstellungen)xmlserializer.Deserialize(_reader);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler: DeSerializeReadEinstellungen -Read- " + ex);
            }
            AktualisiereEinstellungsDaten(einD);
            //Console.WriteLine("===>DeSerializeReadEinstellungen Read = " + einD.strEinstellungen + "\n--->" + einD.myAppName +
            //   " " + einD.strVersion + " " + einD.strKompileTime +
            //    "\n--->" + einD.myDataPfad + "\n--->" + einD.myDepotPfad);
            //conWrLi("---- -x- DeSerializeReadEinstellungen()");
        }
        public void SerializeWriteEinstellungen(string filename, Einstellungen einD) { 
            // Write             
            try {
                using (Stream writer = new FileStream(filename, FileMode.Create)) {
                    xmlserializer.Serialize(writer, einD);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler: SerializeWriteEinstellungen -Write-  " + ex);
            }
            //Console.WriteLine("===>SerializeWriteEinstellungen Write  = " + einD.strEinstellungen + "\n--->" + einD.myAppName +
            //    " " + einD.strVersion + " " + einD.strKompileTime +
            //    "\n--->" + einD.myDataPfad + "\n--->" + einD.myDepotPfad);
            //conWrLi("---- -x- SerializeWriteEinstellungen()");
        }
        private void AktualisiereEinstellungsDaten(Einstellungen einD) {
            conWrLi("---- -x- AktualisiereEinstellungsDaten");
            FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));
            DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);
            // fiExe.FullName: D:\Visual Studio 2015\Projects\MeineFinanzenProjekte\MeineFinanzen\MeineFinanzen\bin\Debug\MeineFinanzen.exe
            einD.strKompileTime = dtLeUmw.ToString();
            // 21.06.2016 10:22:33           
            einD.myDocPfad = fiExe.FullName.Substring(0, 2);
            einD.myDataPfad = einD.myDocPfad + @"\" + Assembly.GetExecutingAssembly().GetName().Name + @"\";
            // D :\\MeineFinanzen\\                     
            einD.myDepotPfad = einD.myDataPfad + @"MyDepot"; // NOCH MyDepot aus Datei holen: DepotNr=1.
            einD.strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            // 1.0.0.0            
            einD.strEinstellungen = einD.myDepotPfad + @"\Einstellungen\EinstellungsDaten.xml";
            // D :\MeineFinanzen\MyDepot\Einstellungen\EinstellungsDaten.xml            
            einD.strSubsemblyAPI = einD.myDocPfad + @"\Visual Studio 2015\Projects\SubsemblyFinTS";
            // D :\Visual Studio 2015\Projects\SubsemblyFinTS
            // C :\Users\username\Documents\Subsembly FinTs API
            einD.strEclipseHbci4jClasses = @"C:\Users\LuKe\eclipse-workspace\hbci4java-master.zip_expanded\hbci4java-master\target\classes";
            einD.strHBCI4j = @"C:/Users/LuKe/hbci4j-core/hbci4j-core-3.0.10/";    // xxxxxxx.properties";
            einD.strBilderPfad = einD.myDocPfad + @"\Visual Studio 2015\Projects\MeineFinanzenProjekte\MeineFinanzen\MeineFinanzen\MeineBilder";
            einD.bsHeight = (int)SystemParameters.PrimaryScreenWidth;
            einD.bsWidth = (int)SystemParameters.PrimaryScreenHeight;
            einD.bsHkorr = einD.bsHeight / 1080;
            einD.bsWkorr = einD.bsWidth / 1920;                    
        }
        /*  [Einstellungen]
                [Allgemein]
                Height=309
                Width=192
                [TabControl1]
                FontSize=10
                FontName=Arial
                FontColor=Black
                FontStyle=Regular
                SizeWidth=1900
                SizeHeight=1000
                ItemWidth=200
                ItemHeight=24
                [Portfolio]
                GridFontSize=10
                GridFontName=Arial
                GridFontColor=Black
                GridFontStyle=Regular
                GridRowTemplateHeight=20
                [Anleihen]
                [Attribution]
                [Historie]
                [Aktualisieren]
                [PortfolioHistorie]
                [Ende]*/
        //public Einstellungen HoleEinstellungsDatenALT() {            
        /*
        string st3 = Environment.CurrentDirectory;            
        // C :\Users\LuKe\Documents\Visual Studio 2015\Projects\MeineFinanzenProjekte\MeineFinanzen\MeineFinanzen\bin\Debug
        string st4 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // C :\Users\LuKe\AppData\Roaming
        string st5 = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        // C :\ProgramData
        string st6 = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        // C :\Users\LuKe\AppData\Local
       Console.WriteLine("Environment.CurrentDirectory                                              :{0}", st3);
       Console.WriteLine("Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)      :{0}", st4);
       Console.WriteLine("Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData):{0}", st5);
       Console.WriteLine("Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) :{0}", st6);
        */
        /*
        FileInfo fiExe = (new FileInfo(Assembly.GetEntryAssembly().Location));            
        DateTime dtLeUmw = File.GetLastWriteTime(fiExe.FullName);
        // fiExe.FullName: C :\Users\LuKe\Documents\Visual Studio 2015\Projects\MeineFinanzenProjekte\MeineFinanzen\MeineFinanzen\bin\Debug\MeineFinanzen.exe
        strKompileTime = dtLeUmw.ToString();
        // 21.06.2016 10:22:33           
        myDocPfad = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // C :\Users\LuKe\Documents                        
        myAppName = Assembly.GetExecutingAssembly().GetName().Name;
        // MeineFinanzen
        myDataPfad = myDocPfad + @"\" + myAppName + @"\";
        // C :\Users\LuKe\Documents\MeineFinanzen\                                              
        string sVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        // 1.0.0.0
        str EinstellungenPfad = einD.myDocPfad + @"\Einstellungs--Daten.xml";
        bsHeight = (int)SystemParameters.PrimaryScreenWidth;
        bsWidth = (int)SystemParameters.PrimaryScreenHeight;
        bsHkorr = bsHeight / 1080;
        bsWkorr = bsWidth / 1920;
        string strrr = Environment.CurrentDirectory;            
        // C :\Users\LuKe\Documents\Visual Studio 2010\Projects\PortfolioManager\PortfolioManager\bin\DebugXXXXXXZZZ
        string[] Zeilen = null;            
        Zeilen = File.ReadAllLines(myDataPfad + @"MyDepot\Settings\Settings-Portfolio.txt");           
        strPortfolioGridFontName = AusTextDatenHolen(Zeilen, "Portfolio", "GridFontName");            
        sPortfolioGridFontSize = Convert.ToSingle(AusTextDatenHolen(Zeilen, "Portfolio", "GridFontSize"));
        strPortfolioGridFontStyle = AusTextDatenHolen(Zeilen, "Portfolio", "GridFontStyle");
        strPortfolioGridFontColor = AusTextDatenHolen(Zeilen, "Portfolio", "GridFontColor");
        iPortfolioGridRowTemplateHeight = (int)(Convert.ToInt32(AusTextDatenHolen(Zeilen, "Portfolio", "GridRowTemplateHeight")) * bsHkorr);
        sTabControl1FontSize = Convert.ToSingle(AusTextDatenHolen(Zeilen, "TabControl1", "FontSize"));
        strTabControl1FontName = AusTextDatenHolen(Zeilen, "TabControl1", "FontName");
        strTabControl1FontColor = AusTextDatenHolen(Zeilen, "TabControl1", "FontColor");
        strTabControl1FontStyle = AusTextDatenHolen(Zeilen, "TabControl1", "FontStyle");
        iTabControl1SizeWidth = (int)(Convert.ToSingle(AusTextDatenHolen(Zeilen, "TabControl1", "SizeWidth")) * bsWkorr);
        iTabControl1SizeHeight = (int)(Convert.ToInt32(AusTextDatenHolen(Zeilen, "TabControl1", "SizeHeight")) * bsHkorr);
        iTabControl1ItemWidth = (int)(Convert.ToInt32(AusTextDatenHolen(Zeilen, "TabControl1", "ItemWidth")) * bsWkorr);
        iTabControl1ItemHeight = (int)(Convert.ToInt32(AusTextDatenHolen(Zeilen, "TabControl1", "ItemHeight")) * bsHkorr);
        string FileNameXml = myDataPfad + @"MyDepot\Settings\Settings-AusschlText.xml";
        strUrlIndizes = "http://kurse.boerse.ard.de/ard/indizes_einzelkurs_uebersicht.htn?bigIndex=0&i=159096&sektion=einzelwerte&sortierung=descriptionShort&ascdesc=ASC";
        //PortfolioManager.myDatenPfad + @ "\PortfolioManagerDaten\1.0.0.0\Settings-AusschlText.xml";
        XmlTextReader reader = new XmlTextReader(FileNameXml);
        //Debug.WriteLine("Einstellungs Daten HoleEinstellungsDaten() FileNameXml: {0}", FileNameXml);
        reader.WhitespaceHandling = WhitespaceHandling.None;
        reader.Read();
        int ir = -1;
        while (reader.Read()) {
            switch (reader.NodeType) {
                // case XmlNodeType.Element:
                //   Console.WriteLine("Element:<{0}>", reader.Name);
                //    break; 
                case XmlNodeType.Text:
                    //Debug.WriteLine("Text:{0}", reader.Value);
                    strAusGr[++ir] = reader.Value;                      // NOCH ist jetzt hier !!!
                    break;
                    /* case XmlNodeType.CDATA:
                       Console.WriteLine("<![CDATA[{0}]]>", reader.Value);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                       Console.WriteLine("<?{0} {1}?>", reader.Name, reader.Value);
                        break;
                    case XmlNodeType.Comment:
                       Console.WriteLine("<!--{0}-->", reader.Value);
                        break;
                    case XmlNodeType.XmlDeclaration:
                       Console.WriteLine("<?xml version='1.0'?>");
                        break;
                    case XmlNodeType.Document:
                        break;
                    case XmlNodeType.DocumentType:
                       Console.WriteLine("<!DOCTYPE {0} [{1}]", reader.Name, reader.Value);
                        break;
                    case XmlNodeType.EntityReference:
                       Console.WriteLine(reader.Name);
                        break;
                    case XmlNodeType.EndElement:
                       Console.WriteLine("</{0}>", reader.Name);
                        break;
                    default:
                       Console.WriteLine("default {0}", reader.NodeType);
                        break;
            }
        }
        return this;
    }
    private string AusTextDatenHolen(string[] Zeilen, string gruppe, string name) {
        string Zeile = "", strWert = "";
        int nz = -1, i0;
        Zeile = Zeilen[++nz];                               // 1.Zeile
        while (nz < Zeilen.Length) {
            if (Zeile.Substring(0, 1) == "[") {
                i0 = Zeile.IndexOf(gruppe);
                if (i0 >= 0) {
                    while (nz < Zeilen.Length) {
                        Zeile = Zeilen[++nz];
                        if (Zeile.Substring(0, 1) == "[") {
                            return strWert;
                        }
                        else {
                            i0 = Zeile.IndexOf(name);
                            if (i0 >= 0) {
                                i0 = Zeile.IndexOf("=");
                                strWert = Zeile.Substring(i0 + 1);
                                return strWert;
                            }
                        }
                    }
                }
            }
            Zeile = Zeilen[++nz];
        }
        return "";
    } */
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}