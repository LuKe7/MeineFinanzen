// 14.04.2018   -Model-  CollWertpapSynchroNeu.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace MeineFinanzen.Model {
    public class CollWertpapSynchroNeu : ObservableCollection<WertpapSynchroNeu>, IComparable {
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            WertpapSynchroNeu wp = obj as WertpapSynchroNeu;
            if (wp != null)
                return wp.WPVISIN.CompareTo(((WertpapSynchroNeu)obj).WPVISIN);
            throw new NotImplementedException();
        }
    }
    public class WertpapSynchroNeu : INotifyPropertyChanged, IEditableObject {
        // ---- private
        private float _WPVAnzahl;
        //private string _WPVName;
        //private string _WPVURL;
        //private DateTime _WPVKursZeit;
        //private string _WPVISIN;
        private double _WPVKurs;
        private double _WPVKursNeu;
        private double _WPVProzentAenderung;
        private double _WPVProzentAenderungNeu;
        private DateTime _WPVKursZeit;
        private DateTime _WPVKursZeitNeu;
        //private int _WPVType; 
        private Single _WPVSharpe;
        private Single _WPVSharpeNeu;
        //private string _WPVURLSharp;
        private string _WPVColor;
        // ---- public
        public float WPVAnzahl {
            get { return _WPVAnzahl; }
            set {
                _WPVAnzahl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVAnzahl"));
            }    }        
        public string WPVName { get; set; }
        public string WPVURL { get; set; }
        public string WPVISIN { get; set; }                    
        public double WPVKurs {
            get { return _WPVKurs; }
            set {
                _WPVKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKurs"));
            }
        }
        public double WPVKursNeu {
            get { return _WPVKursNeu; }
            set {
                _WPVKursNeu = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKursNeu"));
            }
        }
        public double WPVProzentAenderung {
            get { return _WPVProzentAenderung; }
            set {
                _WPVProzentAenderung = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVProzentAenderung"));
            }
        }
        public double WPVProzentAenderungNeu {
            get { return _WPVProzentAenderungNeu; }
            set {
                _WPVProzentAenderungNeu = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVProzentAenderungNeu"));
            }
        }
        public DateTime WPVKursZeit {
            get { return _WPVKursZeit; }
            set {
                _WPVKursZeit = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKursZeit"));
            }
        }
        public DateTime WPVKursZeitNeu {
            get { return _WPVKursZeitNeu; }
            set {
                _WPVKursZeitNeu = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKursZeitNeu"));
            }
        } 
        public Single WPVSharpe {
            get { return _WPVSharpe; }
            set {
                _WPVSharpe = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVSharpe"));
            }
        }
        public Single WPVSharpeNeu {
            get { return _WPVSharpeNeu; }
            set {
                _WPVSharpeNeu = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVSharpeNeu"));
            }
        }
        public string WPVColor {
            get { return _WPVColor; }
            set {
                _WPVColor = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVColor"));
            }
        }
        public int WPVType { get; set; }              
        public string WPVURLSharp { get; set; }             
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
}