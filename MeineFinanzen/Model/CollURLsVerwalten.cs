using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
namespace MeineFinanzen.Model {
    public class CollURLsVerwalten : ObservableCollection<UrlVerwalten>, IComparable {
        public int CompareTo(object obj) {           
                return 0;         
        }
    }
    public class UrlVerwalten : INotifyPropertyChanged, IEditableObject  {       
        private string _boxanfang;
        private string _ausschluss1;
        private string _wert1;
        private string _wert2;
        private string _wert3;
        private string _wert4;
        private string _vg2Color;       
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Boxanfang {
            get { return _boxanfang; }
            set {
                _boxanfang = value;
                RaisePropertyChanged("Boxanfang");
            }
        }
        public string Ausschluss1 {
            get { return _ausschluss1; }
            set {
                _ausschluss1 = value;
                RaisePropertyChanged("Ausschluss1");
            }
        }
        public string Wert1 {
            get { return _wert1; }
            set {
                _wert1 = value;
                RaisePropertyChanged("Wert1");
            }
        }
        public string Wert2 {
            get { return _wert2; }
            set {
                _wert2 = value;
                RaisePropertyChanged("Wert2");
            }
        }
        public string Wert3 {
            get { return _wert3; }
            set {
                _wert3 = value;
                RaisePropertyChanged("Wert3");
            }
        }
        public string Wert4 {
            get { return _wert4; }
            set {
                _wert4 = value;
                RaisePropertyChanged("Wert4");
            }
        }
        public string Vg2Color {
            get { return _vg2Color; }
            set {
                _vg2Color = value;
                RaisePropertyChanged("Vg2Color");
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
    }
    public static class MyExtensionClassInt2 {
        public static List<T> ToCollectionInt2<T>(this DataTable dt) {
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
