// 22.11.2017   -Model-   ChartDatenHolen.cs
// Aus  C :\Users\LuKe\Documents\MeineFinanzen\MyDepot\KursDaten\PortFol_20061122.xml einlesen.
// Auch  PortFol_20151124 (2015_11_24 12_45_03 UTC).xml
// max 2.000.000,-
using System;
using System.IO;
using System.Data;
using System.Xml.Serialization;
using System.Globalization;
using System.Windows;
using System.Collections.Generic;
namespace MeineFinanzen.Helpers {
    public class ChartDatenHolen : IDisposable {
        string strKursdaten = GlobalRef.g_Ein.myDepotPfad + @"\KursDaten\";
        static private string[] arrFiles;
        DataTable dtWoche = new DataTable("tblWoche");
        int dj, vj;
        public ChartDatenHolen() { }
        public DataTable ChartDatenHolenX() {
            dtWoche.Columns.Add("Woche", typeof(int));
            dtWoche.Columns.Add("Datum", typeof(string));
            dtWoche.Columns.Add("Tageswert", typeof(double));
            DataColumn[] keys6 = new DataColumn[1];
            keys6[0] = dtWoche.Columns["Woche"];
            dtWoche.PrimaryKey = keys6;
            DateTime dtheute = DateTime.Now;
            DateTime dtNeuJahr = new DateTime(dtheute.Year, 1, 1, 0, 0, 0);
            int dy1 = (int)dtNeuJahr.DayOfYear;
            int dw1 = (int)dtNeuJahr.DayOfWeek;
            string str1 = dtNeuJahr.ToString("F", new CultureInfo("de-DE"));
            arrFiles = new string[999];
            //int nF = 0;
            string diesesJahr = dtheute.Year.ToString();
            dj = Convert.ToInt32(diesesJahr);
            string vorJahr = (dtheute.Year - 1).ToString();
            vj = Convert.ToInt32(vorJahr);
            string searchPattern = "*_" + vorJahr + "*|*_" + diesesJahr + "*";    // *_2015*|*_2016*
            string[] files = HoleFiles(strKursdaten, searchPattern, SearchOption.TopDirectoryOnly);
            foreach (string file in files) {
                FileInfo fsi = new FileInfo(file);
                if (fsi.Name.Substring(0, 8) == "PortFol_")
                    if (fsi.Name.Contains(".xml"))
                        AddToDatatable(file, fsi.Name);
            }
            conWrLi("---- -73- Vor return ChartDatenHolenX()");
            return dtWoche;
        }
        public static string[] HoleFiles(string path, string searchPattern, SearchOption searchOption) {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        private void AddToDatatable(string pfad, string fname) {
            bool gefunden;
            string strDat;
            if (fname == null)
                return;            
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            DateTime tToday = DateTime.Today;
            string str = tToday.ToString("F", new CultureInfo("de-DE"));
            int wo = cal.GetWeekOfYear(tToday, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            int woHoch = 0;
            if (wo > woHoch)
                woHoch = wo;
            Model.CollWertpapiere wp;
            if (File.Exists(pfad)) {
                XmlSerializer ser = new XmlSerializer(typeof(Model.CollWertpapiere));
                try {
                    using (Stream rd = new FileStream(pfad, FileMode.Open)) {
                        wp = (Model.CollWertpapiere)ser.Deserialize(rd);
                    }
                    if (wp.Count > 0) {
                        gefunden = false;
                        foreach (Model.Wertpapier f1 in wp) {
                            if (f1.Name == "Summe Gesamt") {
                                gefunden = true;
                                string sAktWert = String.Format("{0:###,##0.00 ;#0.00-;' '}", f1.AktWert);
                                if (f1.AktWert < 500000 || f1.AktWert > 2000000)
                                    Console.WriteLine("ChartDatenHolen() Fehler. AKName: {0} AktWert:{1} ofad: {2}", f1.AKName, f1.AktWert, pfad);
                                int tt = Convert.ToInt32(fname.Substring(14, 2));
                                int mm = Convert.ToInt32(fname.Substring(12, 2));
                                int jj = Convert.ToInt32(fname.Substring(8, 4));
                                DateTime dt = new DateTime(jj, mm, tt, 0, 0, 0);
                                wo = cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                                if (jj == dj && wo >= 53) {
                                    //Debug.WriteLine("----> tt:{0} mm:{1} jj:{2} wo:{3} AktWert:{4} ist noch altes Jahr!!!!", tt, mm, jj, wo, sAktWert);
                                }
                                else {
                                    if (jj == dj)
                                        wo += 53;
                                }
                                double dbl = Convert.ToDouble(sAktWert);
                                strDat = fname.Substring(14, 2) + "." + fname.Substring(12, 2) + "." + fname.Substring(8, 4);
                                DataRow foundRow = dtWoche.Rows.Find(wo);
                                if (foundRow == null)
                                    dtWoche.Rows.Add(wo, strDat, dbl);
                                break;
                            }
                        }
                        if (!gefunden)
                            MessageBox.Show("ChartDatenHolenX() Summe Gesamt nicht gefunden: " + fname);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("ChartDatenHolenX Exception pfad: " + pfad + Environment.NewLine + ex);
                }
            }
        }
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }
                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.
                disposedValue = true;
            }
        }
        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~ChartDatenHolen() {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}