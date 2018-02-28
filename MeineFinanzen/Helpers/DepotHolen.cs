// 28.12.2017 DepotHolen.cs
// Depot-Daten von Bank holen und nach DataSetAdmin.dtWertpapSubsembly.
// 18.03.2014 Delete DataRecord in dvPortfol wenn nicht mehr im Bank-Depot. Über 'Verkaufen'.
// 11.07.2014 Columns in DataTable über PropertyType erstellen.
// 15.11.2015 Return wenn NumberOfFinancialInstruments == 0.
// 28.12.2017 if NumberOfFinancialInstruments > 0 vorverlegt wg löschen.
using System;
using System.Data;
using System.Windows.Forms;
using Subsembly.FinTS;
using Subsembly.Swift;
using DataSetAdminNS;
using Subsembly.FinTS.Admin;
namespace MeineFinanzen.Helpers {
    public class DepotHolen {
        public DataSet dsWertpapiere;
        static FinKontenÜbersicht fkü;
        public string _bank = "", _pin = "", _blz = "", _ktoNr = "", _ktoType = "";
        Random rand = new Random();
        public decimal total;
        public DepotHolen() {
            fkü = new FinKontenÜbersicht();
            fkü._pin = "xxxx";
            fkü._blz = "xxxx";
            fkü._ktoType = "xxxx";
            fkü.Show();             // führt dort OnLoad aus
            fkü.Dispose();
        }
        public decimal DepotHolen_ausführen() {
            ConWrLi("---- -30- Start DepotHolen_ausführen");
            Application.DoEvents();
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( _ktoType:{1} BLZ:{2}", DateTime.Now, _ktoType, _blz);
            fkü = new FinKontenÜbersicht();
            fkü._pin = _pin;
            fkü._blz = _blz;
            fkü._ktoType = _ktoType;
            fkü.tabControl.SelectedTab = fkü.depotTabPage;
            fkü.Show();             // führt dort OnLoad aus
                                    // Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( nach .Show", DateTime.Now);
            FinContact xContact = (FinContact)fkü.contactComboBox.SelectedItem;
            fkü._SelectContact(xContact);
            int lfdnr = -1;
            int selectNr = -1;            
            foreach (FinContact aContact in FinAdmin.DefaultFolder) {     // FinContactFolder.Default) {
                ++lfdnr;
                //Debug.WriteLine("{0} DepotHolen_ausführen( FinContact aContact.BankCode {1} ContactName {2}",
                //   DateTime.Now, aContact.BankCode, aContact.ContactName);
                // 50010517 ING-DiBa
                // 21352240 Sparkasse-Holstein
                if (aContact.BankCode == fkü._blz)
                    selectNr = lfdnr;
                fkü.contactComboBox.Items.Add(aContact);
            }
            if (fkü.contactComboBox.Items.Count == 0) {
                System.Windows.MessageBox.Show("Bitte verwenden Sie den Sub sembly FinTS Admin um zuerst einen Bankkontakt einzurichten");
            }
            else {
                fkü.contactComboBox.SelectedIndex = selectNr;
                fkü.pinTextBox.Text = fkü._pin;
            }
            fkü.versionInfoLabel.Text = "Sub sembly FinTS API " + FinUtil.ApiVersion.ToString();
            fkü.tabControl.SelectedTab = fkü.depotTabPage;
            //ro = -1;
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( vor  fkü.m_aPortfListBuilder.Build", DateTime.Now);
            FinPortfList aPortfListOrder = fkü.m_aPortfListBuilder.Build(FinKontenÜbersicht.m_aAcct, null, FinExchRateQuality.Unknown, 0, null); // Build and send the balance inquiry.            
            fkü._SendOrder(aPortfListOrder);
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( nach _SendOrder", DateTime.Now);
            SwiftStatementOfHoldings aStatementOfHoldings = aPortfListOrder.PortfSecuritiesList; // Provides access to the returned portfolio list (MT-535 or MT-571), if any.                       
            if (aStatementOfHoldings.NumberOfFinancialInstruments > 0) {
                total = finToDtGesamt(aPortfListOrder);
            }
            fkü.Dispose();
            fkü = null;
            ConWrLi("---- -39- Ende DepotHolen_ausführen");
            return total;
        }
        public decimal finToDtGesamt(FinPortfList aPortfListOrder) {
            SwiftStatementOfHoldings aStatementOfHoldings = aPortfListOrder.PortfSecuritiesList;    // Evaluate the returned data.                          
            SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument1 = aStatementOfHoldings[0];
            Object[] properties1 = aFinancialInstrument1.GetType().GetProperties();
            DataColumn column;
            Type typeInt32 = Type.GetType("System.Int32");
            // ------ Spalten erstellen ------ 
            foreach (Object prop in properties1) {
                System.Reflection.PropertyInfo pif = (System.Reflection.PropertyInfo)prop;
                Object val1 = pif.GetValue(aFinancialInstrument1, null);
                Object val3 = pif.PropertyType;
                //Console.WriteLine("dtWertpapSubsembly Spalten pif.Name:{0,-34} GetValue:{1,-24} PropertyType:{2}",
                //    pif.Name, val1, val3);                
                column = new DataColumn(pif.Name);
                if (pif.PropertyType.Name == "String")
                    column.DataType = System.Type.GetType("System.String");
                else if (pif.PropertyType.Name == "Int32")
                    column.DataType = System.Type.GetType("System.Int32");
                else if (pif.PropertyType.Name == "Decimal")
                    column.DataType = System.Type.GetType("System.Decimal");
                else if (pif.PropertyType.Name == "SwiftDate")                  // 20140711 00000000               
                    column.DataType = System.Type.GetType("System.DateTime");
                else if (pif.PropertyType.Name == "SwiftTime")                  // 000000                
                    column.DataType = System.Type.GetType("System.DateTime");
                else {
                    //Debug.WriteLine("kein type!! pif.Name:" + pif.Name + " PropertyType:" + pif.PropertyType.Name);
                    column.DataType = System.Type.GetType("System.String");
                }
                DataSetAdmin.dtWertpapSubsembly.Columns.Add(column);                                                                                        
            }
            string sKto = aStatementOfHoldings.SecuritiesAccountNumber;
            DataRow newRow;
            decimal aktwert = 0;
            int co = -1;
            // ------ Zeilen erstellen ------
            foreach (SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument in aStatementOfHoldings) {
                newRow = DataSetAdmin.dtWertpapSubsembly.NewRow();
                co = -1;
                Object[] properties2 = aFinancialInstrument.GetType().GetProperties();
                foreach (Object prop in properties2) {
                    System.Reflection.PropertyInfo pif = (System.Reflection.PropertyInfo)prop;
                    Object val1 = pif.GetValue(aFinancialInstrument, null);
                    Object val3 = pif.PropertyType;
                    //Console.WriteLine("dtWPGes pif.Name:{0,-34} GetValue:{1,-24} PropertyType:{2}", pif.Name, val1, val3); 
                    ++co;
                    if (pif.PropertyType.Name == "SwiftDate") {
                        string str = val1.ToString();
                        if (str == "00000000")
                            newRow[co] = Convert.ToDateTime("01.01.1980");
                        else
                            newRow[co] = Convert.ToDateTime(str.Substring(6, 2) + "." + str.Substring(4, 2) + "." + str.Substring(0, 4) + " 12:00:00");
                    }
                    else if (pif.PropertyType.Name == "SwiftTime") {
                        string str = val1.ToString();
                        newRow[co] = Convert.ToDateTime("01.01.1980 " + str.Substring(4, 2) + ":" + str.Substring(0, 2));
                    }
                    else {
                        newRow[co] = val1;
                    }
                }
                DataSetAdmin.dtWertpapSubsembly.Rows.Add(newRow);
                aktwert += (decimal)newRow["HoldingValue"]; // Value of the total holding.
            }
            return aktwert; // FALSCH: ab 02.2017 aStatementOfHoldings.TotalValue;
        }
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        /*The interest amount that has been accrued in between coupon payment periods. Verbindlichkeiten aus aufgelaufenen Zinsen auf Einlagen
              public decimal AccruedInterestAmount { get; }
           The currency of the Sub sembly.Swift.SwiftStatementOfHoldingsFinancialInstrument.AccruedInterestAmount.
              public string AccruedInterestCurrency { get; }
           Buying date
              public SwiftDate BuyingDate { get; }                    Kaufdatum
           Cost Price Rate
              public decimal CostPriceRate { get; }                   Rate
           Cost price rate currency                                   Währung
              public string CostPriceRateCurrency { get; }
           The exchange rate that converts the currency of the price or quantity to
           the currency of the portfolio.
              public decimal ExchangeRate { get; }
           The first currency of the Sub sembly.Swift.SwiftStatementOfHoldingsFinancialInstrument.ExchangeRate
              public string ExchangeRateFirstCurrency { get; }
           The second currency of the Sub sembly.Swift.SwiftStatementOfHoldingsFinancialInstrument.ExchangeRate
              public string ExchangeRateSecondCurrency { get; }
           The amount of the basic price of the future contract.
              public decimal FutureContractBasicPrice { get; }
           The currency of the basic price of the future contract.
              public string FutureContractBasicPriceCurrency { get; }                         
           Future Contract Expiry Date
              public SwiftDate FutureContractExpiryDate { get; }
           Future Contract Key
              public string FutureContractKey { get; }
           Unit/Contract size of the futures contract.
              public string FutureContractSize { get; }
           Future Contract Symbol
              public string FutureContractSymbol { get; }            
            Future contract underlying ISIN
              public string FutureContractUnderlyingISIN { get; }
           Future contract underlying WKN
              public string FutureContractUnderlyingWKN { get; }
           Future Contract Version: {"null", "0", "1", ... "9"}
              public string FutureContractVersion { get; }
           The currency of the Sub sembly.Swift.SwiftStatementOfHoldingsFinancialInstrument.HoldingValue.
              public string HoldingCurrency { get; }
           Value of the total holding.
              public decimal HoldingValue { get; }                                                                                                 
           Interest Rate (as a percentage in case of a interest-bearing securities).
              public decimal InterestRate { get; }
           The securities identification via an ISIN.
              public string ISIN { get; }
           Issuer Country
              public string IssuerCountry { get; }
           Maturity date
              public SwiftDate MaturityDate { get; }                                          
           Number of days used for calculating the accrued interest amount.
              public int NumberOfDaysAccrued { get; }
           Returns the number of sub balances.
              public int NumberOfSubBalances { get; }
           The origin of a price: ("LMAR", "THEO", "VEND" or null)
              public string OriginOfPrice { get; }
           The name of the stock exchange or null.
              public string OriginOfPriceText { get; }
           Price (percentage or amount). Zero if no price is available.
              public decimal Price { get; }
           Price currency. Null, if no price is available or if price is a percentage.
              public string PriceCurrency { get; }
           The type of the given Sub sembly.Swift.SwiftStatementOfHoldingsFinancialInstrument.Price.
              public SwiftPriceType PriceType { get; }
           Quantity (signed)
              public decimal Quantity { get; }                                                                         
           Quantity currency.
              public string QuantityCurrency { get; }
           Quantity type.
              public SwiftQuantityType QuantityType { get; }
           Quote Date of price.
              public SwiftDate QuotationDateOfPrice { get; }
           Quote Time of price.
              public SwiftTime QuotationTimeOfPrice { get; }                          
           Sector code in accordance with WM GD 200.
              public string SectorCode { get; }                                                   
           The securities name as a Sub sembly.Swift.SwiftTextLines instance.
              public SwiftTextLines SecurityName { get; }
           Type of security in accordance with WM GD 195
              public string TypeOfSecurity { get; }
           The securities identification via an WKN.
              public string WKN { get; }     */
    }
}