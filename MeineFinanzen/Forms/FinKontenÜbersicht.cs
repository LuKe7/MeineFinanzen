// 30.01.2017 FinKontenÜbersicht 
// Aus FinPadForm erstellt 19.01.2014
// Sub sembly FinTS(Financial Transaction Services) API.    Copyright © 2004-2012 Sub sembly GmbH
// Financial Transaction Services, kurz FinTS, ist ein deutscher Standard für den Betrieb von Online-Banking.
// Änderungen sind gekennzeichnet: 'Geändert!!!!'
// 18.03.2014 Änderungen _Order...
// 22.03.2015 schließt die form.
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Subsembly.FinTS;
using Subsembly.FinTS.Admin;
using Subsembly.FinTS.Forms;
using Subsembly.Sepa;
using Subsembly.Swift;
namespace MeineFinanzen {
    public partial class FinKontenÜbersicht : Form {    // Aus: -public class FinPadForm : Form-      erstellt
        static public FinContact m_aContact;
        static public FinAcct m_aAcct;
        static FinAcctBalBuilder m_aAcctBalBuilder;
        static public FinAcctMvmtsSpecifiedPeriodBuilder m_aAcctMvmtsBuilder;
        //static FinSingRemittBuilder m_aSingRemittBuilder;
        FinSepaSingRemittBuilder m_aSepaRemittBuilder;
        public FinPortfListBuilder m_aPortfListBuilder;
        FinSepaSetupStoBuilder m_aSepaSetupStoBuilder;
        FinSepaAllStoBuilder m_aSepaAllStoBuilder;
        FinSepaSto[] m_vAllSepaStos;
        public string _pin = "xxxxx";              // Geändert!!!!
        public string _blz = "xxxxxxx";
        public string _ktoType = "xxxxx";         // Geändert!!!!
        public FinKontenÜbersicht() {
            InitializeComponent();
        }
        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e) {
        }
        protected override void OnLoad(EventArgs e) {
            //private void FinKontenÜbersicht_Load(object sender, EventArgs e)  
            conWrLi("---- -40- Start FinKontenÜbersicht");
            int lfdnr = -1;             // Geändert!!!!
            int selectNr = 0;           // Geändert!!!!
            // Add all contacts that have been set up with the Sub sembly FinTS Admin to the selection box.
            foreach (FinContact aContact in FinAdmin.DefaultFolder) { //FinContactFolder) {
                ++lfdnr;
                //Debug.WriteLine("FinContact aContact.BankCode {0} ContactName {1}", aContact.BankCode, aContact.ContactName);
                // 50010517 ING-DiBa
                // 21352240 Sparkasse-Holstein
                if (aContact.BankCode == _blz)  // Geändert!!!!
                    selectNr = lfdnr;           // Geändert!!!!
                contactComboBox.Items.Add(aContact);
            }
            if (contactComboBox.Items.Count == 0) {
                MessageBox.Show(
                     "Bitte verwenden Sie den Sub sembly FinTS Admin um zuerst einen " +
                     "Bankkontakt einzurichten");
            }
            else {
                contactComboBox.SelectedIndex = selectNr;  // Geändert!!!!
                pinTextBox.Text = _pin;                    // Geändert!!!!
            }
            //
            versionInfoLabel.Text = "Sub sembly FinTS API " + FinUtil.ApiVersion.ToString();
            //
            base.OnLoad(e);
            conWrLi("---- -48- Ende FinKontenÜbersicht");
        }
        private void contactComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {
            // Get the new contact selection.
            FinContact aContact = (FinContact)contactComboBox.SelectedItem;
            if (aContact == m_aContact) {
                return;
            }
            _SelectContact(aContact);

        }
        private void acctComboBox_TextChanged(object sender, System.EventArgs e) {
            _SelectAcctNo(acctComboBox.Text);
        }
        private void acctComboBox_SelectedIndexChanged(object sender, System.EventArgs e)   // Kontonummer
        {
            _SelectAcctNo(acctComboBox.Text);
        }
        public void _SelectContact(FinContact aContact) {
            m_aContact = aContact;
            m_aAcct = null;

            //

            m_aAcctBalBuilder = null;
            m_aAcctMvmtsBuilder = null;
            //m_aSingRemittBuilder = null;
            m_aSepaRemittBuilder = null;
            m_aPortfListBuilder = null;
            m_aSepaSetupStoBuilder = null;
            m_aSepaAllStoBuilder = null;

            singleBalanceButton.Enabled = false;
            balanceButton.Enabled = false;
            fromDateTimePicker.Enabled = false;
            toDateTimePicker.Enabled = false;
            statementButton.Enabled = false;

            //

            if (m_aContact != null) {
                acctComboBox.Text = null;
                customerIDTextBox.Text = m_aContact.DefaultCustID;
                bankCodeTextBox.Text = m_aContact.BankCode;
                userTextBox.Text = m_aContact.UserID;

                //

                acctComboBox.Items.Clear();
                if (m_aContact.UPD != null) {
                    int lfdnr = -1;         // Geändert!!!!                
                    int selectNr = 0;      // Geändert!!!!                
                    foreach (FinAcctInfo aAcctInfo in m_aContact.UPD) {
                        ++lfdnr;            // Geändert!!!!
                        // Since FinTS 3.0 there may be dummy UPD entries with an absent or empty
                        // account number. We won't add these.
                        //Debug.WriteLine("FinAcctInfo aAcctInfo.AcctTypeClass {0}", aAcctInfo.AcctTypeClass);
                        if ((aAcctInfo.AcctNo != null) && (aAcctInfo.AcctNo != "")) // AcctTypeClass = CreditCard
                        {
                            if (aAcctInfo.AcctTypeClass.ToString() == _ktoType)    // Geändert!!!!
                                selectNr = lfdnr;                                   // Geändert!!!!
                            acctComboBox.Items.Add(aAcctInfo.AcctNo);
                        }
                    }

                    if (acctComboBox.Items.Count > 0) {
                        acctComboBox.SelectedIndex = selectNr;          // Geändert!!!!
                    }
                }

                // Get all the order builder classes for the order types that this application may
                // send. Based on the BPD parameters that are available through the order builder
                // classes individual controls and features are intelligently disabled or enabled.

                if (m_aContact.BPD != null) {
                    m_aAcctBalBuilder = new FinAcctBalBuilder(m_aContact);
                    m_aAcctMvmtsBuilder = new FinAcctMvmtsSpecifiedPeriodBuilder(m_aContact);
                    //m_aSingRemittBuilder = new FinSingRemittBuilder(m_aContact);
                    m_aSepaRemittBuilder = new FinSepaSingRemittBuilder(m_aContact);
                    m_aPortfListBuilder = new FinPortfListBuilder(m_aContact);
                    m_aSepaSetupStoBuilder = new FinSepaSetupStoBuilder(m_aContact);
                    m_aSepaAllStoBuilder = new FinSepaAllStoBuilder(m_aContact);
                }

                //

                singleBalanceButton.Enabled =
                    (m_aAcctBalBuilder != null) &&
                    m_aAcctBalBuilder.IsSupported;

                balanceButton.Enabled =
                    (m_aAcctBalBuilder != null) &&
                    m_aAcctBalBuilder.IsSupported;

                //

                if ((m_aAcctMvmtsBuilder != null) &&
                    m_aAcctMvmtsBuilder.IsSupported) {
                    DateTime tToday = DateTime.Today;
                    DateTime tMinDate = tToday.AddDays(-m_aAcctMvmtsBuilder.AcctMvmtDataCutoff);
                    DateTime tValue = tToday.AddDays(-14);

                    fromDateTimePicker.MinDate = tMinDate;
                    fromDateTimePicker.MaxDate = tToday;
                    fromDateTimePicker.Value = tValue;
                    fromDateTimePicker.Enabled = true;
                    toDateTimePicker.MinDate = tMinDate;
                    toDateTimePicker.MaxDate = tToday;
                    toDateTimePicker.Value = tToday;
                    toDateTimePicker.Enabled = true;
                    statementButton.Enabled = true;
                }
                else {
                    fromDateTimePicker.Enabled = false;
                    toDateTimePicker.Enabled = false;
                    statementButton.Enabled = false;
                }

                //

                depotButton.Enabled =
                    (m_aPortfListBuilder != null) &&
                    m_aPortfListBuilder.IsSupported;
            }

            _AcceptAbleRemit();
            _AcceptAbleSepaRemitt();
        }
        private void _SelectAcctNo(string sAcctNo) {
            //Debug.WriteLine("FinKontenÜbersicht-1 _SelectAcctNo({0})", sAcctNo);
            if ((sAcctNo == null) || (sAcctNo == "")) {
                m_aAcct = null;
                return;
            }

            // Whenever the logon account number is changed we update the customer ID
            // accordingly. If UPD are present and they contain information for the entered
            // account number, then we will pick the customer ID from the matching account
            // information. Otherwise we choose the default customer ID that was entered for
            // the selected contact.

            FinAcctInfo aAcctInfo = null;
            if ((m_aContact != null) && (m_aContact.UPD != null)) {
                aAcctInfo = m_aContact.UPD.FindAccount(sAcctNo);
            }

            if (aAcctInfo == null) {
                // If the selected account exists in the UPD of the contact, then we retrieve the
                // FinAcct information from there. Otherwise we create our own FinAcct information
                // from scratch.
                //
                // WARNING: This procedure does not work with banks that keep multiple differing
                // accounts with the the same account number!

                m_aAcct = new FinAcct(sAcctNo, "280", m_aContact.BankCode);
                m_aAcct.Currency = "EUR";

                //

                if (m_aContact != null) {
                    customerIDTextBox.Text = m_aContact.DefaultCustID;
                }
                else {
                    customerIDTextBox.Text = null;
                }
            }
            else {
                m_aAcct = aAcctInfo.Acct;
                customerIDTextBox.Text = aAcctInfo.CustID;
            }

            // Update the GUI elements that refer to the currently selected account.

            SwiftIBAN aFromIban = new SwiftIBAN(m_aAcct.IBAN);
            sepaDebtorIbanLabel.Text = aFromIban.Format();
        }
        public bool _SendOrder(FinOrder aOrder) {
            // Check if a contact was chosen by the user. If not, then present error message and
            // bail out.

            if (m_aContact == null) {
                MessageBox.Show("Bitte wählen Sie einen Bankkontakt aus der Liste.");
                return false;
            }

            // Get the account that was chosen. If none was entered or chosen, then present
            // error message and bail out.

            if (m_aAcct == null) {
                MessageBox.Show("Bitte wählen Sie ein Konto aus der Liste, oder geben Sie die " +
                    "Kontonummer ein.");
                return false;
            }

            // Get the chosen customer ID. This is optional, hence we allow it to be empty.
            // However, we must translate that choice to null for BeginDialog.

            string sCustID = customerIDTextBox.Text.Trim();
            if (sCustID == "") {
                sCustID = null;
            }

            // Get the optional PIN. It is important to ensure that no empty string is passed
            // to the PIN property because it will be used as a PIN.

            string sPIN = pinTextBox.Text.Trim();
            if (sPIN == "") {
                sPIN = null;
            }

            //

            m_aContact.PIN = sPIN;
            FinScriptSendOrder.AutoCloseDocket = true;      // schließt form!!!!!.
            //Debug.WriteLine(" {0} {1} FinKontenÜbersicht FinScriptSendOrder.Execute -vor-", DateTime.Now, m_aContact.ContactName);
            //Application.DoEvents();
            FinScriptSendOrder.Execute(null, m_aContact, aOrder, sCustID);  // in aOrder : HKWPD Depotaufstellung anfordern.
            //Thread.Sleep(1000);
            //Debug.WriteLine(" {0} {1} FinKontenÜbersicht FinScriptSendOrder.Execute -nach-", DateTime.Now, m_aContact.ContactName);
            return true;
        }
        public string _FormatBalance(SwiftBalance aBal, string sText) {
            StringBuilder sbLine = new StringBuilder();

            string sBalDate = String.Format("{0:D2}.{1:D2}.", aBal.Date.Day, aBal.Date.Month);
            string sAmount = SwiftAmt.Format(aBal.Amount, ',', 2);
            char chSign = aBal.IsDebit ? '-' : '+';

            sbLine.Append(sBalDate);
            sbLine.Append("        ");
            sbLine.Append(sText);
            sbLine.Append(' ', 43 - sText.Length - sAmount.Length);
            sbLine.Append(sAmount);
            sbLine.Append(chSign);
            sbLine.Append("\r\n");

            return sbLine.ToString();
        }
        #region QUERYBALANCE

        private void singleBalanceButton_Click(object sender, System.EventArgs e) {
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                balanceTextBox.Text = "\r\n";

                // Build and send the balance inquiry.

                FinAcctBal aAcctBal = m_aAcctBalBuilder.Build(m_aAcct, false);
                _SendOrder(aAcctBal);

                FinAcctBalResp aAcctBalResp = null;
                if ((aAcctBal.AcctBals != null) && (aAcctBal.AcctBals.Length > 0)) {
                    aAcctBalResp = aAcctBal.AcctBals[0];
                }

                if (aAcctBalResp != null) {
                    balanceTextBox.AppendText(
                        String.Format("{0:N10}    {1}\r\n",
                        aAcctBalResp.Account.AcctNo,
                        aAcctBalResp.AcctName));

                    balanceTextBox.AppendText(
                        _FormatBalance(aAcctBalResp.CurrentBal,
                        "Aktueller Saldo"));

                    if (aAcctBalResp.IncludingPendingTransBal != null) {
                        balanceTextBox.AppendText(
                            _FormatBalance(aAcctBalResp.IncludingPendingTransBal,
                            "Zukünftiger Saldo"));
                    }

                    balanceTextBox.AppendText("\r\n");
                }
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Arrow;
                this.Enabled = true;
            }
        }

        private void balanceButton_Click(object sender, System.EventArgs e) // Alle Salden holen
        {
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                balanceTextBox.Text = "\r\n";

                // Build and send the balance inquiry.

                FinAcctBal aAcctBal = m_aAcctBalBuilder.Build(m_aAcct, true);
                _SendOrder(aAcctBal);

                // Evaluate the returned data.

                FinAcctBalResp[] vaAcctBals = aAcctBal.AcctBals;
                if (vaAcctBals != null) {
                    foreach (FinAcctBalResp aAcctBalResp in vaAcctBals) {
                        balanceTextBox.AppendText(
                            String.Format("{0:N10}    {1}\r\n",
                            aAcctBalResp.Account.AcctNo,
                            aAcctBalResp.AcctName));

                        balanceTextBox.AppendText(
                            _FormatBalance(aAcctBalResp.CurrentBal,
                            "Aktueller Saldo"));

                        if (aAcctBalResp.IncludingPendingTransBal != null) {
                            balanceTextBox.AppendText(
                                _FormatBalance(aAcctBalResp.IncludingPendingTransBal,
                                "Zukünftiger Saldo"));
                        }

                        balanceTextBox.AppendText("\r\n");
                    }
                }
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Arrow;
                this.Enabled = true;
            }
        }

        #endregion
        #region QUERYSTATEMENT
        private void statementButton_Click(object sender, System.EventArgs e)   // Kontoumsätze, Umsätze holen.
        {
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                statementTextBox.Text = null;

                // Build and send the statement inquiry.

                SwiftDate aFromDate = new SwiftDate();
                SwiftDate aToDate = new SwiftDate();
                SwiftStatement aMT940 = null;

                if (fromDateTimePicker.Checked) {
                    aFromDate = new SwiftDate(fromDateTimePicker.Value);
                }
                if (toDateTimePicker.Checked) {
                    aToDate = new SwiftDate(toDateTimePicker.Value);
                }

                FinAcctMvmtsSpecifiedPeriod aAcctMvmts = m_aAcctMvmtsBuilder.Build(
                    m_aAcct, false, aFromDate, aToDate, 0, null);
                _SendOrder(aAcctMvmts);

                //

                SwiftStatementReader aStmtReader = aAcctMvmts.BookedTrans;
                if (aStmtReader != null) {
                    aMT940 = SwiftStatement.ReadMT940(aStmtReader, true);
                }
                if (aMT940 != null) {
                    statementTextBox.Text = _GetStatementHeader();
                    statementTextBox.AppendText("\r\n");

                    statementTextBox.AppendText(
                        _FormatBalance(aMT940.OpeningBalance, "Anfangssaldo"));
                    statementTextBox.AppendText("\r\n");

                    foreach (SwiftStatementLine aStatementLine in aMT940.StatementLines) {
                        statementTextBox.AppendText(
                            _FormatStatementLine(aStatementLine));
                    }

                    statementTextBox.AppendText("\r\n");
                    statementTextBox.AppendText(
                        _FormatBalance(aMT940.ClosingBalance, "Endsaldo"));
                    if (aMT940.ClosingAvailableBalance != null) {
                        statementTextBox.AppendText(
                            _FormatBalance(aMT940.ClosingAvailableBalance, "Valutensaldo"));
                    }

                    foreach (SwiftBalance aForwardBalance in aMT940.ForwardAvailableBalances) {
                        statementTextBox.AppendText(
                            _FormatBalance(aForwardBalance, "zukünftiger Saldo"));
                    }
                }
            }
            catch (SwiftException xSwift) {
                MessageBox.Show(
                    String.Format("Swift Error {0} at Line {1} Field was:\n\n{2}",
                    xSwift.Code, xSwift.Line,
                    (xSwift.Field != null) ? xSwift.Field.ToString() : "n/a"),
                    "Swift Exception");
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Arrow;
                this.Enabled = true;
            }
        }
        private string _GetStatementHeader() {
            return "Bu.Tag  Wert  Informationen                    Betrag EUR\r\n";
            //      00.00.        Gebuchter Saldo             123456789012,45+
            //      00.00. 00.00. 123456789012345678901234567 123456789012,45+
            //                    123456789012345678901234567
            //                    123456789012345678901234567
        }
        private string _FormatStatementLine(SwiftStatementLine aStatementLine) {
            StringBuilder sbLine = new StringBuilder();

            string sEntryDate = String.Format("{0:D2}.{1:D2}.",
                aStatementLine.EntryDate.Day,
                aStatementLine.EntryDate.Month);
            string sValueDate = String.Format("{0:D2}.{1:D2}.",
                aStatementLine.ValueDate.Day,
                aStatementLine.ValueDate.Month);
            string sEntryText = _GetEntryText(aStatementLine);

            string sAmount = SwiftAmt.Format(aStatementLine.Amount.Value, ',', 2);
            char chSign =
                ((aStatementLine.DebitCreditMark == "C") ||
                (aStatementLine.DebitCreditMark == "RD")) ? '+' : '-';

            sbLine.Append(sEntryDate);
            sbLine.Append(' ');
            sbLine.Append(sValueDate);
            sbLine.Append(' ');
            sbLine.Append(sEntryText);
            sbLine.Append(' ', 43 - sEntryText.Length - sAmount.Length);
            sbLine.Append(sAmount);
            sbLine.Append(chSign);
            sbLine.Append("\r\n");

            foreach (string sPurposeLine in aStatementLine.PaymtPurpose) {
                sbLine.Append(' ', 14);
                sbLine.Append(sPurposeLine);
                sbLine.Append("\r\n");
            }

            return sbLine.ToString();
        }
        public string _GetEntryText(SwiftStatementLine aStmtLine) {
            string sEntryText = aStmtLine.EntryText;
            if (sEntryText == null) {
                switch (aStmtLine.ZkaTranCode) {
                    case "004":
                    case "005":
                        sEntryText = "Lastschrift";
                        break;
                    case "008":
                        sEntryText = "Dauerauftrag";
                        break;
                    case "013":
                        sEntryText = "EU-Standardüberweisung";
                        break;
                    case "020":
                        sEntryText = "Überweisung";
                        break;
                    case "051":
                        sEntryText = "Überweisungsgutschrift";
                        break;
                    case "052":
                        sEntryText = "Dauerauftragsgutschrift";
                        break;
                    default:
                        sEntryText = "Sonstige Buchung";
                        break;
                }
            }

            return sEntryText;
        }
        #endregion
        #region REMITT

        private void remittButton_Click(object sender, System.EventArgs e) {
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                FinAcct aPayeeAcct = new FinAcct(payeeAcctTextBox.Text, "280",
                    payeeBankCodeTextBox.Text);
                FinRemitt aRemitt = new FinRemitt(m_aAcct, aPayeeAcct,
                    payeeNameTextBox.Text,
                    new SwiftAmt(amountTextBox.Text, "EUR"), "51");
                string[] vsPaymtPurpose = new string[2];
                vsPaymtPurpose[0] = paymtPurposeTextBox1.Text;
                vsPaymtPurpose[1] = paymtPurposeTextBox2.Text;
                aRemitt.PaymtPurpose = vsPaymtPurpose;

                //FinOrder aSingRemitt = m_aSingRemittBuilder.Build(aRemitt);
                //_SendOrder(aSingRemitt);
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }

        private void payeeAcctTextBox_TextChanged(object sender, System.EventArgs e) {
            _AcceptAbleRemit();
        }

        private void amountTextBox_TextChanged(object sender, System.EventArgs e) {
            _AcceptAbleRemit();
        }

        private void _AcceptAbleRemit() {
            bool fAcceptAble =
                //(m_aSingRemittBuilder != null) &&
                //m_aSingRemittBuilder.IsSupported &&
                (payeeNameTextBox.TextLength > 0) &&
                (payeeAcctTextBox.TextLength > 0) &&
                (payeeBankCodeTextBox.TextLength > 0) &&
                (amountTextBox.TextLength > 0);
            remittButton.Enabled = fAcceptAble;
        }
        #endregion
        #region DEPOT
        private void depotButton_Click(object sender, System.EventArgs e) {
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                depotTextBox.Text = null;

                FinPortfList aPortfListOrder = m_aPortfListBuilder.Build(m_aAcct, null,
                    FinExchRateQuality.Unknown, 0, null);
                _SendOrder(aPortfListOrder);

                SwiftStatementOfHoldings aStatementOfHoldings = aPortfListOrder.PortfSecuritiesList;
                if (aStatementOfHoldings != null) {
                    depotTextBox.AppendText(_MakePageNumberLine(aStatementOfHoldings));
                    depotTextBox.AppendText("\r\n");
                    depotTextBox.AppendText(_MakeSecuritiesAccountLine(aStatementOfHoldings));
                    depotTextBox.AppendText("\r\n");
                    depotTextBox.AppendText(_MakeTotalValueLine(aStatementOfHoldings));
                    depotTextBox.AppendText("\r\n");

                    if (aStatementOfHoldings.NumberOfFinancialInstruments != 0) {
                        depotTextBox.AppendText(_MakeFinancialInstrumentLineHeader());
                    }
                    foreach (SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument in aStatementOfHoldings) {
                        depotTextBox.AppendText(_MakeFinancialInstrumentLine(aFinancialInstrument));
                    }
                }
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }
        private string _MakePageNumberLine(SwiftStatementOfHoldings aStatementOfHoldings) {
            Debug.Assert(aStatementOfHoldings != null);

            StringBuilder aStringBuilder = new StringBuilder();

            aStringBuilder.Append("Seite:");
            aStringBuilder.Append(aStatementOfHoldings.PageNumber.ToString());

            switch (aStatementOfHoldings.ContinuationIndicator) {
                case SwiftStatementOfHoldingsContinuationIndicator.SinglePage:
                    aStringBuilder.Append(" / einzige Seite");
                    break;
                case SwiftStatementOfHoldingsContinuationIndicator.IntermediatePage:
                    aStringBuilder.Append(" / es folgen weitere Seiten");
                    break;
                case SwiftStatementOfHoldingsContinuationIndicator.LastPage:
                    aStringBuilder.Append(" / letzte Seite");
                    break;
            }

            aStringBuilder.Append("\r\n");

            return aStringBuilder.ToString();
        }
        private string _MakeSecuritiesAccountLine(SwiftStatementOfHoldings aStatementOfHoldings) {
            Debug.Assert(aStatementOfHoldings != null);

            StringBuilder aStringBuilder = new StringBuilder();

            aStringBuilder.Append("Depotkonto:");
            aStringBuilder.Append(aStatementOfHoldings.SecuritiesAccountNumber);
            aStringBuilder.Append(" ");
            aStringBuilder.Append("Depotbankcode:");
            aStringBuilder.Append(aStatementOfHoldings.SecuritiesBankCode);

            aStringBuilder.Append("\r\n");

            return aStringBuilder.ToString();
        }
        private string _MakeFinancialInstrumentLine(SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument) {
            Debug.Assert(aFinancialInstrument != null);

            string sSecurityID = String.Format("{0,12}", (aFinancialInstrument.WKN == null) ?
                aFinancialInstrument.ISIN : aFinancialInstrument.WKN);

            string sQuantity = String.Format("{0,16} {1,3}",
                aFinancialInstrument.Quantity,
                (aFinancialInstrument.QuantityCurrency != null) ?
                    aFinancialInstrument.QuantityCurrency : "STK");

            string sPrice = String.Format("{0,16:N2} {1,3}", aFinancialInstrument.Price,
                (aFinancialInstrument.PriceType == SwiftPriceType.PRCT) ? "%" : aFinancialInstrument.PriceCurrency);

            string sHolding = String.Format("{0,16:N2} {1,3}",
                aFinancialInstrument.HoldingValue,
                aFinancialInstrument.HoldingCurrency);

            StringBuilder aStringBuilder = new StringBuilder();

            aStringBuilder.Append(aFinancialInstrument.SecurityName.Pack());
            aStringBuilder.Append("\r\n");

            aStringBuilder.Append(sSecurityID);
            aStringBuilder.Append("  ");
            aStringBuilder.Append(sQuantity);
            aStringBuilder.Append("  ");
            aStringBuilder.Append(sPrice);
            aStringBuilder.Append("\r\n");

            aStringBuilder.Append("    Kurswert");
            aStringBuilder.Append("  ");
            aStringBuilder.Append(' ', 20);
            aStringBuilder.Append("  ");
            aStringBuilder.Append(sHolding);
            aStringBuilder.Append("\r\n");
            aStringBuilder.Append("\r\n");

            return aStringBuilder.ToString();
        }
        private string _MakeFinancialInstrumentLineHeader() {
            StringBuilder aStringBuilder = new StringBuilder();

            aStringBuilder.Append("    WKN/ISIN");
            aStringBuilder.Append("  ");
            aStringBuilder.Append("     Anzahl/Nominale");
            aStringBuilder.Append("  ");
            aStringBuilder.Append("            Kurs    ");

            aStringBuilder.Append("\r\n");

            aStringBuilder.Append('-', 12);
            aStringBuilder.Append("  ");
            aStringBuilder.Append('-', 20);
            aStringBuilder.Append("  ");
            aStringBuilder.Append('-', 20);

            aStringBuilder.Append("\r\n");

            return aStringBuilder.ToString();
        }
        private string _MakeTotalValueLine(SwiftStatementOfHoldings aSwiftStatementOfHoldings) {
            Debug.Assert(aSwiftStatementOfHoldings != null);

            StringBuilder aBuilder = new StringBuilder();

            if (aSwiftStatementOfHoldings.TotalValueCurrency != null) {
                aBuilder.Append("Gesamtwert des Depots:");
                aBuilder.Append(aSwiftStatementOfHoldings.TotalValue);
                aBuilder.Append(" ");
                aBuilder.Append(aSwiftStatementOfHoldings.TotalValueCurrency);
            }

            aBuilder.Append("\r\n");

            return aBuilder.ToString();
        }
        #endregion
        #region SEPAREMITT
        private void sepaRemittButton_Click(object sender, EventArgs e) {
            if (m_aAcct.IBAN == null) {
                MessageBox.Show(
                    "IBAN des Auftraggeberkontos ist nicht vorhanden. Versuchen Sie " +
                    "den Bankkontakt zu synchronisieren um die IBAN zu erhalten. " +
                    "Besteht das Problem weiterhin, dann ist das Konto möglicherweise " +
                    "nicht für SEPA Zahlungen geeignet.");
                return;
            }
            if (m_aAcct.BIC == null) {
                MessageBox.Show(
                    "BIC des Auftraggeberkontos ist nicht vorhanden. Versuchen Sie " +
                    "den Bankkontakt zu synchronisieren um die BIC zu erhalten. " +
                    "Besteht das Problem weiterhin, dann ist das Konto möglicherweise " +
                    "nicht für SEPA Zahlungen geeignet.");
                return;
            }

            // Determine a suitable PAIN format. We just use the first PAIN format that is
            // supported.

            string[] vsSupportedPainFormats = m_aSepaRemittBuilder.SupportedPainFormats;
            if (vsSupportedPainFormats.Length == 0) {
                MessageBox.Show("SEPA Überweisungen werden nicht unterstützt.");
                return;
            }

            SepaWellKnownMessageInfos nZkaFormat =
                SepaMessageInfo.GetZkaWellKnownMessageInfo(vsSupportedPainFormats[0]);
            if (nZkaFormat == SepaWellKnownMessageInfos.Null) {
                MessageBox.Show("Unbekanntes Format für SEPA Überweisungen.");
                return;
            }
            SepaMessageInfo aMessageInfo = SepaMessageInfo.Create(nZkaFormat);
            //
            //
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                string sIBAN = SwiftIBAN.Capture(sepaCreditorIbanTextBox.Text);
                SwiftIBAN aPayeeIBAN = new SwiftIBAN(sIBAN);

                SepaCreditTransferPaymentInitiation aPain =
                    (SepaCreditTransferPaymentInitiation)aMessageInfo.NewMessage();
                SepaCreditTransferPaymentInformation aPmtInf =
                    new SepaCreditTransferPaymentInformation();
                SepaCreditTransferTransactionInformation aCdtTrfTxInf =
                    new SepaCreditTransferTransactionInformation();

                string sDbtrNm = m_aAcct.HolderName.Trim();
                aPain.InitiatingParty.Name = sDbtrNm;
                aPmtInf.Debtor.Name = sDbtrNm;

                aPmtInf.DebtorAccountIBAN = new SepaIBAN(m_aAcct.IBAN);
                aPmtInf.DebtorAgentBIC = new SepaBIC(m_aAcct.BIC);
                aCdtTrfTxInf.Creditor.Name = sepaCreditorNameTextBox.Text.Trim();
                aCdtTrfTxInf.CreditorAccountIBAN = new SepaIBAN(SepaIBAN.Capture(sepaCreditorIbanTextBox.Text));
                aCdtTrfTxInf.CreditorAgentBIC = new SepaBIC(sepaCreditorBicTextBox.Text.Trim());
                aCdtTrfTxInf.Amount = Decimal.Parse(sepaAmountTextBox.Text.Trim());

                string sEndToEndId = sepaPurposeTextBox1.Text.Trim();
                if (sEndToEndId != "") {
                    aCdtTrfTxInf.EndToEndId = sEndToEndId;
                }

                string sRmtInf = sepaPurposeTextBox2.Text.Trim();
                if (sRmtInf != "") {
                    aCdtTrfTxInf.RemittanceInformation = sRmtInf;
                }

                // Build the SEPA document.

                SepaDocument aDoc = new SepaDocument(aMessageInfo, aPain);

                aPain.PaymentInformations.Add(aPmtInf);
                aPmtInf.TransactionInformations.Add(aCdtTrfTxInf);

                FinOrder aSepaOrder = m_aSepaRemittBuilder.Build(m_aAcct, aDoc);
                _SendOrder(aSepaOrder);
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }
        private void sepaCreditorNameTextBox_TextChanged(object sender, EventArgs e) {
            _AcceptAbleSepaRemitt();
        }
        private void sepaCreditorIbanTextBox_TextChanged(object sender, EventArgs e) {
            _AcceptAbleSepaRemitt();
        }
        private void sepaCreditorBicTextBox_TextChanged(object sender, EventArgs e) {
            _AcceptAbleSepaRemitt();
        }
        private void _AcceptAbleSepaRemitt() {
            bool fAcceptAble =
                (m_aSepaRemittBuilder != null) && m_aSepaRemittBuilder.IsSupported &&
                (sepaCreditorNameTextBox.TextLength > 0) &&
                (sepaCreditorIbanTextBox.TextLength > 0) &&
                (sepaCreditorBicTextBox.TextLength > 0) &&
                (sepaAmountTextBox.TextLength > 0);
            sepaRemittButton.Enabled = fAcceptAble;

            //
            sepaSetupStoButton.Enabled = fAcceptAble;
        }
        #endregion
        #region SEPASTO
        private void sepaSetupStoButton_Click(object sender, EventArgs e) {
            // Determine a suitable PAIN format. We just use the first PAIN format that is
            // supported.

            string[] vsSupportedPainFormats = m_aSepaSetupStoBuilder.SupportedPainFormats;
            if (vsSupportedPainFormats.Length == 0) {
                MessageBox.Show("SEPA Daueraufträge werden nicht unterstützt.");
                return;
            }

            SepaWellKnownMessageInfos nZkaFormat =
                SepaMessageInfo.GetZkaWellKnownMessageInfo(vsSupportedPainFormats[0]);
            if (nZkaFormat == SepaWellKnownMessageInfos.Null) {
                MessageBox.Show("Unbekanntes Format für SEPA Dauerauftrag.");
                return;
            }

            SepaMessageInfo aMessageInfo = SepaMessageInfo.Create(nZkaFormat);
            try {
                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                DateTime tToday = DateTime.Today;
                DateTime tExecDay = new DateTime(tToday.Year, tToday.Month, 1).AddMonths(1);

                // Initializes the variables to pass to the MessageBox.Show method.
                string sMessage = String.Format("Wollen Sie wirklich einen SEPA Dauerauftrag mit folgendem Datum einrichten:\n\n {0}", tExecDay.ToLongDateString());

                DialogResult result = MessageBox.Show(sMessage, "Warnung", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) {
                    return;
                }

                SepaCreditTransferPaymentInitiation aPain =
                    (SepaCreditTransferPaymentInitiation)aMessageInfo.NewMessage();
                SepaCreditTransferPaymentInformation aPmtInf =
                    new SepaCreditTransferPaymentInformation();
                SepaCreditTransferTransactionInformation aCdtTrfTxInf =
                    new SepaCreditTransferTransactionInformation();

                string sDbtrNm = m_aAcct.HolderName.Trim();
                aPain.InitiatingParty.Name = sDbtrNm;
                aPmtInf.Debtor.Name = sDbtrNm;

                aPmtInf.DebtorAccountIBAN = new SepaIBAN(m_aAcct.IBAN);
                aPmtInf.DebtorAgentBIC = new SepaBIC(m_aAcct.BIC);
                aPmtInf.RequestedExecutionDate = tExecDay;

                aCdtTrfTxInf.Creditor.Name = sepaCreditorNameTextBox.Text.Trim();
                aCdtTrfTxInf.CreditorAccountIBAN = new SepaIBAN(SepaIBAN.Capture(sepaCreditorIbanTextBox.Text));
                aCdtTrfTxInf.CreditorAgentBIC = new SepaBIC(sepaCreditorBicTextBox.Text.Trim());
                aCdtTrfTxInf.Amount = Decimal.Parse(sepaAmountTextBox.Text.Trim());

                string sEndToEndId = sepaPurposeTextBox1.Text.Trim();
                if (sEndToEndId != "") {
                    aCdtTrfTxInf.EndToEndId = sEndToEndId;
                }

                string sRmtInf = sepaPurposeTextBox2.Text.Trim();
                if (sRmtInf != "") {
                    aCdtTrfTxInf.RemittanceInformation = sRmtInf;
                }

                aPain.PaymentInformations.Add(aPmtInf);
                aPmtInf.TransactionInformations.Add(aCdtTrfTxInf);

                FinStoDetails aStoDetails = new FinStoDetails();
                aStoDetails.ExecuteFirstTimeOn = new SwiftDate(tExecDay);
                aStoDetails.TimeUnit = FinStoTimeUnit.Month;
                aStoDetails.PeriodLen = 1;
                aStoDetails.ExecDay = tExecDay.Day;
                aStoDetails.LastScheduledDate = SwiftDate.NullDate;

                // Build the SEPA document.
                SepaDocument aDoc = new SepaDocument(aMessageInfo, aPain);

                FinSepaSetupSto aSepaStoOrder = (FinSepaSetupSto)m_aSepaSetupStoBuilder.Build(m_aAcct, aDoc, aStoDetails);
                _SendOrder(aSepaStoOrder);
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }
        private void sepaAllStoButton_Click(object sender, EventArgs e) {
            try {
                sepaDepotTextBox.Clear();
                FinSepaAllSto aSepaListOrder = (FinSepaAllSto)m_aSepaAllStoBuilder.Build(m_aAcct, null, null, 0, null);
                _SendOrder(aSepaListOrder);
                foreach (FinSepaSto aSto in aSepaListOrder.AllSepaStos) {
                    SepaDocument aDoc = (SepaDocument)aSto.SepaDocument;
                    SepaCreditTransferPaymentInitiation aPain = (SepaCreditTransferPaymentInitiation)aDoc.Message;
                    SepaCreditTransferPaymentInformation aPmtInf = (SepaCreditTransferPaymentInformation)aPain.PaymentInformations[0];
                    SepaCreditTransferTransactionInformation aTxn = (SepaCreditTransferTransactionInformation)aPmtInf.TransactionInformations[0];
                    sepaDepotTextBox.AppendText("Name:\t" + aTxn.Creditor.Name);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("BIC:\t" + aTxn.CreditorAgentBIC);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("IBAN:\t" + aTxn.CreditorAccountIBAN);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("Amount:\t" + aTxn.Amount);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("EndToEnd:\t" + aTxn.EndToEndId);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("RemittanceInformation:\t" + aTxn.RemittanceInformation);
                    sepaDepotTextBox.AppendText("\r\n");
                    sepaDepotTextBox.AppendText("\r\n");
                }
                m_vAllSepaStos = aSepaListOrder.AllSepaStos;
            }
            catch (Exception x) {
                MessageBox.Show(x.ToString(), "Exception");
            }
            finally {
                this.Cursor = Cursors.Default;
                this.Enabled = true;
            }
        }
        #endregion
        private void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
}