// 23.11.2018   -Model-  CollWertpapiere.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
namespace MeineFinanzen.Model {
    public class CollWertpapiere : ObservableCollection<Wertpapier>, IComparable {
        static XmlSerializer ser = new XmlSerializer(typeof(Collection<Wertpapier>));
        public void SerializeWertpapiere(string filename, CollWertpapiere wp) {
            string s = Convert.ToString(DateTime.Now).Trim();
            // Write
            try {
                using (StreamWriter wr = new StreamWriter(filename, false))     // false == nicht append                                                                           
                {
                    ser.Serialize(wr, wp);
                }
            } catch (Exception ex) {
                MessageBox.Show("Fehler: in SerializeWertpapiere() " + ex + Environment.NewLine + filename);
            }
        }       
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            if (obj is Wertpapier wp)
                return wp.DepotID.CompareTo(((Wertpapier)obj).DepotID);
            throw new NotImplementedException();
        }
    }
    public class Wertpapier : INotifyPropertyChanged, IEditableObject {
        private string _name;
        private float _anzahl;
        private float _heute;
        private double _rend;
        private float _rend1J;
        private double _ertrag;
        private string _URL;
        private DateTime _kursZeit;
        private double _aktKurs;
        private double _aktWert;
        private Single _sharpe;
        private double _kaufsumme;
        private DateTime _kaufDatum;
        private double _zahlungen;
        public float Anzahl {
            get { return _anzahl; }
            set {
                _anzahl = value;
                RaisePropertyChanged("Anzahl");
            }
        }
        public string Name {
            get { return _name; }
            set {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public float Heute {
            get { return _heute; }
            set {
                _heute = value;
                RaisePropertyChanged("Heute");
            }
        }
        public double Rend {
            get { return _rend; }
            set {
                _rend = value;
                RaisePropertyChanged("Rend");
            }
        }
        public float Rend1J {
            get { return _rend1J; }
            set {
                _rend1J = value;
                RaisePropertyChanged("Rend1J");
            }
        }
        public double Ertrag {
            get { return _ertrag; }
            set {
                _ertrag = value;
                RaisePropertyChanged("Ertrag");
            }
        }
        public string URL {
            get { return _URL; }
            set {
                _URL = value;
                RaisePropertyChanged("URL");
            }
        }
        public DateTime KursZeit {
            get { return _kursZeit; }
            set {
                _kursZeit = value;
                RaisePropertyChanged("KursZeit");
            }
        }       
        public string ISIN { get; set; }
        public double AktKurs {
            get { return _aktKurs; }
            set {
                _aktKurs = value;
                RaisePropertyChanged("AktKurs");
            }
        }     // 10
        public double KursVorher { get; set; }
        public double AktWert {
            get { return _aktWert; }
            set {
                _aktWert = value;
                RaisePropertyChanged("AktWert");
            }
        }
        public double Zahlungen {
            get { return _zahlungen; }
            set {
                _zahlungen = value;
                RaisePropertyChanged("Zahlungen");
            }
        }
        public double Kaufsumme {
            get { return _kaufsumme; }
            set {
                _kaufsumme = value;
                RaisePropertyChanged("Kaufsumme");
            }
        }
        public DateTime KaufDatum {
            get { return _kaufDatum; }
            set {
                _kaufDatum = value;
                RaisePropertyChanged("KaufDatum");
            }
        }
        public int DepotID { get; set; }
        public string KontoNr { get; set; }
        public int Type { get; set; }
        public string AKKurz { get; set; }
        public string AKName { get; set; }      // 20
        public float Zins { get; set; }
        public DateTime AbDatum { get; set; }
        public DateTime BisDatum { get; set; }
        public float Sharpe {
            get { return _sharpe; }
            set {
                _sharpe = value;
                RaisePropertyChanged("Sharpe");
            }
        }
        public bool IsSumme { get; set; }      // 25          
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