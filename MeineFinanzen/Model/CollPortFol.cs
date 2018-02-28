// 05.01.2018   -Model-  CollPortFol.cs 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollPortFol : ObservableCollection<PortFol>, IComparable {
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            PortFol wp = obj as PortFol;
            if (wp != null)
                return wp.WPDepotID.CompareTo(((PortFol)obj).WPDepotID);
            throw new NotImplementedException();
            }
        }
    public class PortFol : INotifyPropertyChanged, IEditableObject {
        public PortFol() { }
        /* public string _name;
public float Anzahl {
get { return _anzahl; } 
set {
_anzahl = value;
RaisePropertyChanged("Anzahl");
} */
        public int WPID { get; set; }
        public int WPPortFolNr { get; set; }
        public string WPName { get; set; }
        public int WPTypeID { get; set; }
        public float WPAnzahl { get; set; }
        public int WPDepotID { get; set; }
        public string WPISIN { get; set; }
        public string WPKontoNr { get; set; }
        public float WPKurs { get; set; }
        public DateTime WPStand { get; set; }
        public DateTime WPKaufDatum { get; set; }
        public double WPKaufsumme { get; set; }
        public double WP0101Summe { get; set; }
        public string WPWaehrung { get; set; }
        public string WPKurz { get; set; }
        public float WPZinsSatz { get; set; }
        public DateTime WPAbDatum { get; set; }
        public DateTime WPBisDatum { get; set; }
        public double WPKursVorher { get; set; }
        public DateTime WPStandVorher { get; set; }
        public float WPProzentAenderung { get; set; }
        public float WPSharpe { get; set; }
        public float WPVolatil { get; set; }
        public float WPPerfHeute { get; set; }
        public string WPUrlText { get; set; }
        public string WPTextVorKurs { get; set; }
        public string WPUrlSharpe { get; set; }
        public string WPTextVorSharpe { get; set; }
        public string WPDezimaltrennzeichen { get; set; }
        public string WPTextVorZeit { get; set; }
        public float WPKtoKurs { get; set; }
        public string WPXPathKurs { get; set; }
        public string WPXPathZeit { get; set; }
        public string WPXPathAend { get; set; }
        public string WPXPathSharp { get; set; }
        public int WPXKursX { get; set; }
        public int WPXKursY { get; set; }
        public int WPXZeitX { get; set; }
        public string WPAktWert { get; set; }
        public int WPXZeitY { get; set; }
        public class ClassName {
            public string ItemCode { get; set; }
            public string Cost { get; set; }

            public override string ToString() {
                return "ItemCode : " + ItemCode + ", Cost : " + Cost;
                }
            }
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
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(List<PortFol>));
        public void DeserializeReadPortFol(string filename, out List<PortFol> portfol) {
            portfol = null;
            try {
                using (StreamReader _reader = new StreamReader(filename)) {
                    portfol = (List<PortFol>)xmlserializer.Deserialize(_reader);
                    }
                } catch (Exception ex) {
                MessageBox.Show("Fehler: DeserializeReadPortFol -Read- " + ex);
                }
            }
        public void SortList(List<PortFol> objListPortFol, int mode) {
            /*  objListPortFol.Add(new PortFol(6, Convert.ToDateTime("06/06/2016"), "Aspirin"));
              objListPortFol.Add(new PortFol(2, Convert.ToDateTime("02/05/2016"), "xxxxxxx"));
              objListPortFol.Add(new PortFol(1, Convert.ToDateTime("03/04/2016"), "qqqqqqqq"));
              objListPortFol.Add(new PortFol(4, Convert.ToDateTime("04/03/2016"), "uuuuuuuu"));
              objListPortFol.Add(new PortFol(5, Convert.ToDateTime("05/02/2016"), "cccccccc"));
              objListPortFol.Add(new PortFol(3, Convert.ToDateTime("01/01/2016"), "hhhhhhhh"));  */
            // mode 1
            if (mode == 1) {
                Console.WriteLine("Sort the list by WPName ascending:");
                objListPortFol.Sort((x, y) => x.WPName.CompareTo(y.WPName));
                foreach (PortFol o in objListPortFol)
                    Console.WriteLine("WPTypeID = " + o.WPTypeID + " WPStand = " + o.WPStand.ToString() + " WPName = " + o.WPName);
                }
            // mode 2
            if (mode == 2) {
                Console.WriteLine("Sort the list by WPStand descending:");
                objListPortFol.Sort((x, y) => y.WPStand.CompareTo(x.WPStand));
                foreach (PortFol o in objListPortFol)
                    Console.WriteLine("WPTypeID = " + o.WPTypeID + " WPStand = " + o.WPStand.ToString() + " WPName = " + o.WPName);
                }
            // mode 3
            if (mode == 3) {
                Console.WriteLine("Sort the list by WPTypeID ascending:");
                objListPortFol.Sort((x, y) => x.WPTypeID.CompareTo(y.WPTypeID));
                foreach (PortFol o in objListPortFol)
                    Console.WriteLine("WPTypeID = " + o.WPTypeID + " WPStand = " + o.WPStand.ToString() + " WPName = " + o.WPName);
                }
            }
        }
    public static class MyExtensionClass {
        public static List<T> ToCollection<T>(this DataTable dt) {
            List<T> lst = new List<T>();
            Type tClass = typeof(T);
            PropertyInfo[] pClass = tClass.GetProperties();
            List<DataColumn> dc = dt.Columns.Cast<DataColumn>().ToList();
            T cn;
            string daten = null;
            foreach (DataRow item in dt.Rows) {
                cn = (T)Activator.CreateInstance(tClass);
                foreach (PropertyInfo pc in pClass) {
                    DataColumn d = dc.Find(c => c.ColumnName == pc.Name);
                    daten = item[pc.Name].ToString();
                    if (d != null) {
                        string typ = pc.PropertyType.FullName;
                        //Console.WriteLine("Fehler PropertyInfo Name : {0,-20} Daten: {1,-20} hex: {2:X,-20} {3}", pc.Name, daten, daten, typ);
                        if (DBNull.Value.Equals(item[pc.Name]))
                            daten = "";
                        if (daten.Length == 0)
                            pc.SetValue(cn, null, null);
                        if (daten.Length != 0) {
                            try {
                                pc.SetValue(cn, item[pc.Name], null);
                                } catch (ArgumentException) {
                                Console.WriteLine("Fehler PropertyInfo Name : {0,-20} Daten: {1,-20} hex: {2:X,-20} {3}", pc.Name, daten, daten, typ);
                                //MessageBox.Show("Fehler in MyExtensionClass ToCollection() pc.Name: " + pc.Name + " !!!!" + ex);
                                pc.SetValue(cn, 0, null);
                                }
                            }
                        }
                    }
                lst.Add(cn);
                }
            return lst;
            }
        }
    }