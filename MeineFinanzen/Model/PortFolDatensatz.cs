// 21.06.2018 Erstellt ein dtPortFol aus tblWertpapSubsembly oder Leersatz.
// 07.12.2017 Quantity/Anzahl ist Single
// 2017-09-21 l max 8
// WPAktWert neu aus HoldingValue
// 31.12.2017 neu dtPortFolAusdtNull() Leersatz.
using System;
using System.Collections.Specialized;
using System.Data;
using System.Net;
using System.Windows.Controls;
namespace MeineFinanzen.Model {
    public class PortFolDatensatz {
        Random rand = new Random();
        public PortFolDatensatz() { }
        public DataRow dtPortFolAusdtNull(DataRow newRow) {
            newRow["WPID"] = rand.Next();
            newRow["WPPortFolNr"] = 1;
            newRow["WPName"] = "";
            newRow["WPTypeID"] = "51";
            newRow["WPAnzahl"] = 0.00;
            newRow["WPDepotID"] = 1;
            newRow["WPISIN"] = "";
            newRow["WPKontoNr"] = 0;
            newRow["WPKurs"] = 0.00;
            newRow["WPStand"] = new DateTime(1980, 1, 1);
            newRow["WPKaufDatum"] = new DateTime(1980, 1, 1);
            newRow["WPKaufsumme"] = 0.00;
            newRow["WP0101Summe"] = 0.00;
            newRow["WPWaehrung"] = "";
            newRow["WPKurz"] = "";
            newRow["WPZinsSatz"] = 0;
            newRow["WPAbDatum"] = new DateTime(1980, 1, 1);
            newRow["WPBisDatum"] = new DateTime(1980, 1, 1);
            newRow["WPKursVorher"] = 0;
            newRow["WPStandVorher"] = new DateTime(1980, 1, 1);
            newRow["WPProzentAenderung"] = 0;
            newRow["WPSharpe"] = 0;
            newRow["WPVolatil"] = 0;
            newRow["WPPerfHeute"] = 0;
            newRow["WPUrlText"] = "";
            newRow["WPTextVorKurs"] = "";
            newRow["WPURLSharpe"] = "";
            newRow["WPTextVorSharpe"] = "";
            newRow["WPDezimaltrennzeichen"] = "";
            newRow["WPTextVorZeit"] = "";
            newRow["WPKtoKurs"] = 0;
            newRow["WPXPathKurs"] = "";
            newRow["WPXPathZeit"] = "";
            newRow["WPXPathAend"] = "";
            newRow["WPXPathSharp"] = "";
            newRow["WPAktWert"] = 0.00;
            return newRow;
        }
        public DataRow dtPortFolAusdtGesamt(DataRow newRow, DataRow rowGesamt) {
            newRow["WPID"] = rand.Next();
            newRow["WPPortFolNr"] = 1;
            newRow["WPName"] = (string)rowGesamt["securityName"];
            newRow["WPTypeID"] = "51";
            newRow["WPAnzahl"] = Convert.ToSingle(rowGesamt["quantity"]);
            newRow["WPDepotID"] = 1;
            newRow["WPISIN"] = (string)rowGesamt["ISIN"];
            newRow["WPKontoNr"] = 0;
            newRow["WPKurs"] = Convert.ToSingle(rowGesamt["Price"].ToString());
            newRow["WPStand"] = new DateTime(1980, 1, 1);
            newRow["WPKaufDatum"] = new DateTime(1980, 1, 1);
            newRow["WPKaufsumme"] = 0.00;
            newRow["WP0101Summe"] = 0.00;
            newRow["WPWaehrung"] = "";
            int l = rowGesamt["securityName"].ToString().Length;
            if (l > 8)
                l = 8;
            newRow["WPKurz"] = rowGesamt["securityName"].ToString().Substring(0, l);
            newRow["WPZinsSatz"] = 0;
            newRow["WPAbDatum"] = new DateTime(1980, 1, 1);
            newRow["WPBisDatum"] = new DateTime(1980, 1, 1);
            newRow["WPKursVorher"] = 0;
            newRow["WPStandVorher"] = new DateTime(1980, 1, 1);
            newRow["WPProzentAenderung"] = 0;
            newRow["WPSharpe"] = 0;
            newRow["WPVolatil"] = 0;
            newRow["WPPerfHeute"] = 0;
            newRow["WPUrlText"] = URLSuchenISIN((string)rowGesamt["ISIN"]);    // Die URL bei neuen WPs ermitteln. NOCH
            newRow["WPTextVorKurs"] = "";
            newRow["WPURLSharpe"] = "";
            newRow["WPTextVorSharpe"] = "";
            newRow["WPDezimaltrennzeichen"] = "";
            newRow["WPTextVorZeit"] = "";
            newRow["WPKtoKurs"] = 0;
            newRow["WPXPathKurs"] = "";
            newRow["WPXPathZeit"] = "";
            newRow["WPXPathAend"] = "";
            newRow["WPXPathSharp"] = "";
            newRow["WPAktWert"] = Convert.ToSingle(rowGesamt["HoldingValue"].ToString());
            return newRow;
        }
        public string URLSuchenISIN(string isin) {
            WebBrowser wbGoogleSearch = new WebBrowser();
            // NOCH wbGoogleSearch.Navigate("https://www.google.de/imghp?q=" + "https://www.finanzen.net", true);
            return @"//https://www.finanzen.net/";
        }
        private void Suchen() {
            string uriString = "http://www.google.com/search";
            string keywordString = "Test Keyword";

            WebClient webClient = new WebClient();

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("q", keywordString);

            webClient.QueryString.Add(nameValueCollection);
            string text = webClient.DownloadString(uriString);
        }
    }
}