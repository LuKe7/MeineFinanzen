// 27.10.2018 KontensynchronisierenSubsembly 
// Aus FinPadForm erstellt 19.01.2014
// Sub sembly FinTS(Financial Transaction Services) API.    Copyright © 2004-2012 Sub sembly GmbH
// Financial Transaction Services, kurz FinTS, ist ein deutscher Standard für den Betrieb von Online-Banking.
// Änderungen sind gekennzeichnet: 'Geändert!!!!'
// 18.03.2014 Änderungen _Order...
// 22.03.2015 schließt die form.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DataSetAdminNS;
using MeineFinanzen.Helpers;
using MeineFinanzen.Model;
using MeineFinanzen.ViewModel;
using Subsembly.FinTS;
using Subsembly.FinTS.Admin;
using Subsembly.FinTS.Forms;
using Subsembly.Sepa;
using Subsembly.Swift;
namespace MeineFinanzen {
    public partial class KontensynchronisierenSubsembly : Form {    // Aus: -public class FinPadForm : Form-      erstellt
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
        public KontensynchronisierenSubsembly() {
            InitializeComponent();
        }
        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e) {
        }
        protected override void OnLoad(EventArgs e) {
            //private void KontensynchronisierenSubsembly_Load(object sender, EventArgs e)  
            ConWrLi("---- -40- Start KontensynchronisierenSubsembly");
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
            ConWrLi("---- -48- Ende KontensynchronisierenSubsembly");
        }
        private void contactComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {
            // Get the new contact selection.
            FinContact aContact = (FinContact)contactComboBox.SelectedItem;
            if (aContact == m_aContact) {
                return;
            }
            _SelectContact(aContact);

        }
        private void acctComboBox_TextChanged(object sender, EventArgs e) {
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
            //Debug.WriteLine("KontensynchronisierenSubsembly-1 _SelectAcctNo({0})", sAcctNo);
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
            //Debug.WriteLine(" {0} {1} KontensynchronisierenSubsembly FinScriptSendOrder.Execute -vor-", DateTime.Now, m_aContact.ContactName);
            //Application.DoEvents();
            FinScriptSendOrder.Execute(null, m_aContact, aOrder, sCustID);  // in aOrder : HKWPD Depotaufstellung anfordern.
            //Thread.Sleep(1000);
            //Debug.WriteLine(" {0} {1} KontensynchronisierenSubsembly FinScriptSendOrder.Execute -nach-", DateTime.Now, m_aContact.ContactName);
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
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
    }
    // 11.07.2018 VMKontenSynchronisierenSubsembly.cs
    // Get Passw verändert. NOCH verbessern.
    // SortFeld7 < 888 sind Banken.
    public class VMKontenSynchronisierenSubsembly {
        internal DgBanken _b;
        public static DepotHolen _depHolen;
        string pathMeineBilder;
        string filenameBÜD = GlobalRef.g_Ein.myDepotPfad + @"\Daten\BankÜbersichtsDaten.xml";
        public VMKontenSynchronisierenSubsembly() {
            _b = GlobalRef.g_dgBanken;
            pathMeineBilder = GlobalRef.g_Ein.strBilderPfad + @"\";
            _depHolen = new DepotHolen();    // In VMKontenSynchronisierenSubsembly Grundwert
        }
        public void Ausführen(View.HauptFenster mw, bool laden) {
            /* Verzeichnis: D :\MeineFinanzen\MyDepot\Log                                                                       
            * --funktion--            -was-           -wohin-                                                                                
            * KontoStändeFinCmd       Kontostände     \Kontenstände - sKontoNr-DateTime.csv balance   -contactname...               
            * KontoUmsätzeFinCmdStmt  Kontoumsätze    \Umsätze-KontoNr-DateTime.csv         statement -contactname... 
            *                                         \logKontoUmsätzeHolen.txt                                                                     
            * DepotHolen_ausführen    WertpapierDepot dtWertpapSubsembly. (Mit angepasstem FinPadForm geholt)   */
            if (!laden)
                return;
            ConWrLi("---- -20- Start VMKontenSynchronisierenSubsembly");
            string[] strResult;
            double BankBetrag = 0.00;
            double GesamtBetrag = 0.00;
            DgBanken._wertpapiere.Clear();
            DgBanken.umsätze.Clear();
            DgBanken.konten.Clear();
            DgBanken.banken.Clear();                                                         // -ArrayOfBankÜbersicht                     
            mw.OpenLogFile();
            int anzBanken = 0;
            DataSetAdmin.dtKontoumsätze.Clear();
            DataSetAdmin.dtKontoumsätze.Columns.Clear();

            DataSetAdmin.dtWertpapSubsembly = new DataTable("tblWertpapSubsembly");
            DataSetAdmin.dtWertpapSubsembly.Columns.Clear();
            DataSetAdmin.dtWertpapSubsembly.Rows.Clear();
            DataSetAdmin.dtWertpapSubsembly.PrimaryKey = new DataColumn[] { DataSetAdmin.dtWertpapSubsembly.Columns["ISIN"] };

            foreach (FinContact aContact in mw.liContacte) {                                 // Banken
                Console.WriteLine("Bank: {0}", aContact.ContactName);
                DgBanken.bank = new BankÜbersicht();                                         // -BankÜbersicht
                DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
                DgBanken.banken.Add(DgBanken.bank);
                DgBanken.bank.SortFeld7 = (anzBanken++ + 300).ToString();                    // aContact.BankCode;   50010517
                DgBanken.bank.BankName7 = aContact.ContactName;                              // +BankName7         
                string strImagePath = "";
                if (aContact.ContactName.Contains("Spark"))
                    strImagePath += "Spk";
                else if (aContact.ContactName.Contains("DiBa")) // NOCH
                    strImagePath += "DiBa";
                else
                    strImagePath += "nichts";
                strImagePath += ".png";
                DgBanken.bank.BildPfad7 = pathMeineBilder + strImagePath;                    // +BildPfad7 
                // D :\Visual Studio 2015\Projects\SubsemblyFinTS\DiBa.png
                DgBanken.bank.BLZ7 = aContact.BankCode;
                DgBanken.bank.UserID7 = aContact.UserID;
                DgBanken.bank.Datum7 = File.GetLastWriteTime(filenameBÜD);
                DgBanken.bank.Bearbeitungsart7 = "bearb...";
                DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";
                DgBanken.bank.Status7 = "sta";
                _depHolen._bank = aContact.ContactName;
                _depHolen._blz = aContact.BankCode;
                _depHolen._pin = GetPasswort(_depHolen._blz);
                foreach (FinAcctInfo aAcctInfo in aContact.UPD) {                   // Konten der Bank                          
                    Console.WriteLine("   Konto: {0,-30} {1,-16} {2,-20}", aAcctInfo.AcctName, aAcctInfo.AcctTypeClass, aAcctInfo.AcctNo);
                    if (aAcctInfo.AcctTypeClass.ToString().Contains("Portfolio")) {
                        _b.Betrag = Convert.ToDouble(_depHolen.DepotHolen_ausführen()); // ===> Wertpapiere   Bank ---> dtWertpapSubsembly  <<Subsembly.FinTS>>                               
                        WertpapSubsemblyToPortFol();                    // dtWertpapSubsembly ---> dtPortFol.
                        mw._tabwertpapiere.ErstelleDgBankenWertpapiere(mw);     // dtPortFol          ---> (CollWertpapiere)_wertpapiere     // aus dtWertpapSubsembly                   
                        if (aAcctInfo.AcctName == "Wertpapierdepot") {
                            if (DgBanken._wertpapiere.Count > 0) {
                                DgBanken.konto.OCWertpap = new ObservableCollection<Wertpapier>(DgBanken._wertpapiere);
                                mw.dgFinanzübersicht.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Visible;
                            }
                        }
                    } else if (aAcctInfo.AcctTypeClass.ToString().Contains("Giro")) {
                        _depHolen._ktoNr = aAcctInfo.AcctNo;
                        string sCmd = "";
                        string strBank = _depHolen._bank;
                        strBank.Replace(" ", "-");
                        sCmd = "balance" + " -contactname " + strBank + " -pin " + _depHolen._pin + " -bankcode " + _depHolen._blz + " -acctno " + _depHolen._ktoNr;
                        strResult = _b.KontoStändeFinCmd(sCmd, _depHolen._ktoNr);       // ===> Konto-Stände  Bank ---> @"Log\Kontenstände -" + sKontoNr
                        if (strResult == null) {
                            Console.WriteLine("strResult == null!!!!");
                            _b.Betrag = 0;
                        } else {
                            _b.Betrag = Convert.ToDouble(strResult[5]);
                            sCmd = "statement" + " -contactname " + _depHolen._bank + " -pin " + _depHolen._pin + " -acctno " + _depHolen._ktoNr;
                            _b.KontoUmsätzeFinCmdStmt(sCmd, _depHolen._ktoNr);          // ===> Konto-Umsätze Bank ---> @"Log\Umsätze -" + sKontoNr 
                        }
                    } else {
                        //Console.WriteLine("nix von Beiden!!!!" + aAcctInfo.AcctTypeClass.ToString());
                        _b.Betrag = 0;
                    }
                    DgBanken.konto = new BankKonten();
                    DgBanken.konto.KontoName8 = aAcctInfo.AcctName;                          // +KontoName 8                      
                    DgBanken.konto.KontoArt8 = aAcctInfo.AcctTypeClass.ToString();           // +KontoArt8                    
                    DgBanken.konto.KontoNr8 = aAcctInfo.AcctNo;                              // +KontoNr8                     
                    DgBanken.konto.KontoValue8 = _b.Betrag;                                  // +KontoValue8                                  

                    DateTime dt = File.GetLastWriteTime(filenameBÜD);// +KontoDatum8
                    DgBanken.konto.KontoDatum8 = dt;

                    List<Kontoumsatz> liku = _b.KontoumsatzFüllen(aAcctInfo.AcctNo);  // aus dtKontoumsätze
                    if (liku.Count > 0) {
                        DgBanken.konto.OCUmsätze = new ObservableCollection<Kontoumsatz>(liku);
                        mw.dgFinanzübersicht.RowDetailsVisibilityMode = System.Windows.Controls.DataGridRowDetailsVisibilityMode.Visible;
                    }

                    DgBanken.konten.Add(DgBanken.konto);
                    DgBanken.bank.OCBankKonten.Add(DgBanken.konto);
                    BankBetrag += _b.Betrag;
                    DgBanken.bank.BankValue7 = BankBetrag;                                   // +BankValue7
                    GesamtBetrag += _b.Betrag;
                    Console.WriteLine("GesamtBetrag: {0} BankBetrag: {1} Betrag: {2}", GesamtBetrag, BankBetrag, _b.Betrag);
                }   // loop AcctInfo = Konten der Bank
            }   //  loop Contact = Bank
            ConWrLi("---- -26- In KontenSynchronisierenSubsembly");

            DgBanken.bank = new BankÜbersicht();                                             // -BankÜbersicht
            DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
            DgBanken.banken.Add(DgBanken.bank);
            DgBanken.bank.SortFeld7 = "888";
            DgBanken.bank.Bearbeitungsart7 = "bearb...";
            DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";
            DgBanken.bank.Datum7 = File.GetLastWriteTime(filenameBÜD);
            DgBanken.bank.BankName7 = "GeschlFonds";                                         // +BankName7     
            DgBanken.bank.BildPfad7 = pathMeineBilder + "Aktualisieren1.png";                // +BildPfad7 
            // @"C :\U sers\Public\Pictures\index.png";                 
            DgBanken.bank.BankValue7 = _b.SummeGeschlFonds();                                // +BankValue7

            DgBanken.bank = new BankÜbersicht();                                             // -BankÜbersicht
            DgBanken.bank.OCBankKonten = new ObservableCollection<BankKonten>();
            DgBanken.banken.Add(DgBanken.bank);
            DgBanken.bank.SortFeld7 = "889";
            DgBanken.bank.Bearbeitungsart7 = "bearb...";
            DgBanken.bank.FunktionenPfad7 = pathMeineBilder + "Aktualisieren1.png";                  
            DgBanken.bank.Datum7 = File.GetLastWriteTime(filenameBÜD);
            DgBanken.bank.BankName7 = "Alle Konten";                                         // +BankName7     
            DgBanken.bank.BildPfad7 = pathMeineBilder + "Aktualisieren1.png";                // +BildPfad7 
            // @"C :\U sers\Public\Pictures\index.png";                
            DgBanken.bank.BankValue7 = GesamtBetrag + _b.SummeGeschlFonds();                 // +BankValue7  NOCH
            mw.swLog.Close();
            string str = DataSetAdmin.DatasetSichernInXml(GlobalRef.g_Ein.myDataPfad);
            if (str != null) {
                MessageBox.Show("Fehler DatasetSichernInXml(): " + str);
            } else {
                GlobalRef.g_Büb.SerializeWriteBankÜbersicht(GlobalRef.g_Ein.myDepotPfad
                    + @"\Daten\BankÜbersichtsDaten.xml", DgBanken.banken);
                ConWrLi("---- -29- In KontenSynchronisierenSubsembly");
            }

            mw.dgBankenÜbersicht.UpdateLayout();
            ConWrLi("---- -29b- In VMKontenSynchronisierenSubsembly vor neuStart");
            mw.NeuStarten();
            //testBankAnzeige();
            ConWrLi("---- -29c- Fertig VMKontenSynchronisierenSubsembly nach neuStart");
        }
        public bool WertpapSubsemblyToPortFol() {  // Update von dtWertpapSubsembly ---> dtPortFol.          
            if (DataSetAdmin.dtWertpapSubsembly.Columns.Count == 0)
                return true;
            DataTable dtt1 = new DataTable();
            DataSetAdmin.dvWertpapSubsembly.Sort = "ISIN";
            DataSetAdmin.dtWertpapSubsembly.DefaultView.Sort = "ISIN ASC";
            dtt1 = DataSetAdmin.dtWertpapSubsembly.DefaultView.ToTable();
            DataSetAdmin.dtWertpapSubsembly = dtt1;
            DataView dvWertpapiereGesamt = new DataView(DataSetAdmin.dtWertpapSubsembly, "", "ISIN", DataViewRowState.CurrentRows);

            DataTable dtt2 = new DataTable();
            DataSetAdmin.dvPortFol.Sort = "WPISIN";
            DataSetAdmin.dtPortFol.DefaultView.Sort = "WPISIN ASC";
            dtt2 = DataSetAdmin.dtPortFol.DefaultView.ToTable();
            DataSetAdmin.dtPortFol = dtt2;

            string securityName = "";
            string ISIN = "";
            Single Price = -1;          // Kurs
            string PriceCurrency = "";  // EUR oder USD oder so         
            Single quantity = -1;       // Anzahl
            double CostPriceRate = -1;  // Kaufsumme
            string isinGes;
            Single wpanzahl = -1;
            string wpname = "";
            string wpisin = "";
            DateTime wpkaufdatum;
            double wpkurs = 0;
            double wpkaufsumme = 0;
            double wpaktwert = 0;
            Single HoldingValue = 0;
            string HoldingCurrency = ""; // EUR
            // Prüfen ob WP in dtWertpapSubsembly auch in dtPortFol sind.
            PortFolDatensatz portfolNeu = new PortFolDatensatz();
            foreach (DataRow rowGesamt in DataSetAdmin.dtWertpapSubsembly.Rows) {
                isinGes = (string)rowGesamt["ISIN"];
                int nPortfol = DataSetAdmin.dvPortFol.Find(isinGes);
                //Debug.WriteLine("isinGes:{0} nPortfol:{1} SecurityName: {2}", isinGes, nPortfol, rowGesamt["SecurityName"]);
                if (nPortfol < 0)           // Nicht, also in dtPortFol einfügen.
                {
                    DataRow newRow = DataSetAdmin.dtPortFol.NewRow();
                    newRow = portfolNeu.dtPortFolAusdtGesamt(newRow, rowGesamt);

                    newRow["WPName"] = rowGesamt["SecurityName"];
                    newRow["WPIsin"] = rowGesamt["ISIN"];

                    DataSetAdmin.dtPortFol.Rows.Add(newRow);
                }
            }
            // Prüfen ob WP in dtPortFol auch in dvWertpapiereGesamt sind.
            foreach (DataRow rowPortFol in DataSetAdmin.dtPortFol.Rows) {
                wpname = (string)rowPortFol["WPName"];
                wpisin = (string)rowPortFol["WPIsin"];
                string wpid = rowPortFol["WPTypeID"].ToString();
                if ((rowPortFol["WPTypeID"].ToString() == "80") ||
                    (rowPortFol["WPTypeID"].ToString() == "10"))
                    continue;
                if (wpisin.Length < 9)
                    continue;
                int wpges = dvWertpapiereGesamt.Find(wpisin);
                if (wpges == -1) {
                    // Nicht, also aus aus dtPortFol löschen.
                    MessageBox.Show("WertpapSubsemblyToPortFol() Löschen: " + wpname + " " + wpisin);
                    try {
                        DataSetAdmin.dtPortFol.Rows.Remove(rowPortFol);
                    } catch (Exception ex) {
                        MessageBox.Show("" + ex);
                    }
                    return false;
                }
                wpkaufdatum = (DateTime)rowPortFol["WPBisDatum"];
                wpanzahl = Convert.ToSingle(rowPortFol["WPAnzahl"]);
                quantity = Convert.ToSingle(dvWertpapiereGesamt[wpges]["quantity"]);

                HoldingValue = Convert.ToSingle(dvWertpapiereGesamt[wpges]["HoldingValue"]);
                wpaktwert = HoldingValue;
                rowPortFol["WPAktWert"] = wpaktwert;
                wpkaufsumme = Convert.ToDouble(rowPortFol["WPKaufsumme"]);
                wpkurs = Convert.ToDouble(rowPortFol["WPKurs"]);
                CostPriceRate = Convert.ToDouble(dvWertpapiereGesamt[wpges]["CostPriceRate"]);
                //Debug.WriteLine("wpkaufsumme: {0} CostPriceRate: {1} Kaufsumme: {2}", wpkaufsumme, CostPriceRate, CostPriceRate * wpanzahl);               
                if (rowPortFol["WPTypeID"].ToString() == "70")
                    quantity /= 100;
                securityName = (string)dvWertpapiereGesamt[wpges]["securityName"];
                ISIN = (string)dvWertpapiereGesamt[wpges]["ISIN"];
                Price = Convert.ToSingle(dvWertpapiereGesamt[wpges]["Price"].ToString());
                PriceCurrency = dvWertpapiereGesamt[wpges]["PriceCurrency"].ToString();
                HoldingCurrency = dvWertpapiereGesamt[wpges]["HoldingCurrency"].ToString();
                if (wpanzahl != quantity) {
                    //Debug.WriteLine("w:{0, -27} {1, -12} {2, -5} {3, -4}", wpname, wpisin, wpanzahl, wpges);
                    //Debug.WriteLine("g:{0, -27} {1, -12} {2, -5} {3, -4}", securityName, ISIN, quantity, wpges);
                    MessageBox.Show("WPAnzahl (" + wpname + ") wird übernommen! Alt: " + wpanzahl + " Neu: " + quantity);
                    rowPortFol["WPAnzahl"] = quantity;
                }
                /* if (wpname != securityName)
                {
                    MessageBox.Show("WPName wird übernommen! Alt: " + wpname + " Neu: " + securityName);
                    rowPortFol["WPName"] = securityName;
                } */
                if (CostPriceRate != 0) {
                    if (wpkaufsumme != CostPriceRate * wpanzahl) {
                        MessageBox.Show("WPKaufsumme (" + wpname + ") wird übernommen! Alt: " + wpkaufsumme + " Neu: " + CostPriceRate * wpanzahl);
                        wpkaufsumme = CostPriceRate * wpanzahl;
                    }
                }
                if (PriceCurrency == "")
                    PriceCurrency = "EUR";
                if (PriceCurrency == "USD") {
                    //Price = _mw.USDtoEuro(Price);
                    Price = Convert.ToSingle(HoldingValue) / quantity;      // NOCH
                } else if (PriceCurrency == "EUR") { } else {
                    MessageBox.Show("Update_Kaufdatum_KtoKurs Fehler: PriceCurrency: " + PriceCurrency);
                    continue;
                }
                if (wpkurs != Price) {
                    //MessageBox.Show("WPKurs (" + wpname + ") wird übernommen! Alt: " + wpkurs + " Neu: " + Price);
                    rowPortFol["WPKurs"] = Price;
                }
                DateTime standBank = Convert.ToDateTime(dvWertpapiereGesamt[wpges]["QuotationDateOfPrice"].ToString());
                DateTime dt = Convert.ToDateTime(rowPortFol["WPStand"]);
                if (dt < standBank) {
                    rowPortFol["WPStand"] = standBank;
                    rowPortFol["WPKursVorher"] = Price;
                    rowPortFol["WPStandVorher"] = standBank;
                }
                rowPortFol["WPKaufsumme"] = wpkaufsumme;
                DateTime kaufdatum2 = (DateTime)dvWertpapiereGesamt[wpges]["MaturityDate"];         // Fälligkeitstag
                if (kaufdatum2 == new DateTime(1980, 1, 1))
                    continue;
                //Debug.WriteLine("WPBisDatum        : {0} {1} {2} {3}", name1, isin1, kaufdatum1, kaufdatum2);
                if (wpkaufdatum == kaufdatum2)
                    continue;
                //Debug.WriteLine("WPBisDatum updaten: {0} {1} {2}", name1, kaufdatum1, kaufdatum2);
                rowPortFol["WPBisDatum"] = kaufdatum2;
            }
            return true;
        }
        public void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        public static string[] HoleFiles(string path, string searchPattern, SearchOption searchOption) {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        internal string GetPasswort(string blz) {
            string path = GlobalRef.g_Ein.myDepotPfad + @"\daten";
            string searchPattern = "dwp_*|*_";
            string[] files = HoleFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            string dwp = null;
            string xmlblz = null;
            foreach (string file in files) {
                FileInfo fsi = new FileInfo(file);
                if (!fsi.Name.Contains(".xml"))
                    continue;
                XmlTextReader reader = new XmlTextReader(path + "/" + fsi.Name);
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("Passworte"))
                                continue;
                            xmlblz = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            if (xmlblz.Equals("BLZ" + blz))
                                dwp = reader.Value;
                            break;
                        case XmlNodeType.EndElement:
                            //Console.Write("</" + reader.Name + ">");
                            break;
                    }
                }
            }
            return dwp;
        }
    }
    public class DepotHolen {
        public DataSet dsWertpapiere;
        static KontensynchronisierenSubsembly fkü;
        public string _bank = "", _pin = "", _blz = "", _ktoNr = "", _ktoType = "";
        Random rand = new Random();
        public decimal total;
        public DepotHolen() {
            fkü = new KontensynchronisierenSubsembly {
                _pin = "xxxx",
                _blz = "xxxx",
                _ktoType = "xxxx"
            };
            fkü.Show();             // führt dort OnLoad aus
            fkü.Dispose();
        }
        public decimal DepotHolen_ausführen() {
            ConWrLi("---- -30- Start DepotHolen_ausführen");
            //Application.DoEvents();
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( _ktoType:{1} BLZ:{2}", DateTime.Now, _ktoType, _blz);
            fkü = new KontensynchronisierenSubsembly {
                _pin = _pin,
                _blz = _blz,
                _ktoType = _ktoType
            };
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
            } else {
                fkü.contactComboBox.SelectedIndex = selectNr;
                fkü.pinTextBox.Text = fkü._pin;
            }
            fkü.versionInfoLabel.Text = "Sub sembly FinTS API " + FinUtil.ApiVersion.ToString();
            fkü.tabControl.SelectedTab = fkü.depotTabPage;
            //ro = -1;
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( vor  fkü.m_aPortfListBuilder.Build", DateTime.Now);
            FinPortfList aPortfListOrder = fkü.m_aPortfListBuilder.Build(KontensynchronisierenSubsembly.m_aAcct, null, FinExchRateQuality.Unknown, 0, null); // Build and send the balance inquiry.            
            fkü._SendOrder(aPortfListOrder);
            //Debug.WriteLine("{0} mkDepot DepotHolen_ausführen( nach _SendOrder", DateTime.Now);
            SwiftStatementOfHoldings aStatementOfHoldings = aPortfListOrder.PortfSecuritiesList; // Provides access to the returned portfolio list (MT-535 or MT-571), if any.                       
            if (aStatementOfHoldings.NumberOfFinancialInstruments > 0) {
                total = FinToDtGesamt(aPortfListOrder);
            }
            fkü.Dispose();
            fkü = null;
            ConWrLi("---- -39- Ende DepotHolen_ausführen");
            return total;
        }
        public decimal FinToDtGesamt(FinPortfList aPortfListOrder) {
            SwiftStatementOfHoldings aStatementOfHoldings = aPortfListOrder.PortfSecuritiesList;    // Evaluate the returned data.                          
            SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument1 = aStatementOfHoldings[0];
            Object[] properties1 = aFinancialInstrument1.GetType().GetProperties();
            DataColumn column;
            Type typeInt32 = Type.GetType("System.Int32");
            // ------ Spalten erstellen ------ 
            Console.WriteLine("---- FinToDtGesamt() in dtWertpapSubsembly ------ Spalten erstellen ------ ");
            foreach (Object prop in properties1) {
                System.Reflection.PropertyInfo pif = (System.Reflection.PropertyInfo)prop;
                Object val1 = pif.GetValue(aFinancialInstrument1, null);
                Object val3 = pif.PropertyType;
                Console.WriteLine("    Spalte: {0,-34} Value: {1,-24} Type: {2}", pif.Name, val1, val3);                
                column = new DataColumn(pif.Name);
                if (pif.PropertyType.Name == "String")
                    column.DataType = Type.GetType("System.String");
                else if (pif.PropertyType.Name == "Int32")
                    column.DataType = Type.GetType("System.Int32");
                else if (pif.PropertyType.Name == "Decimal")
                    column.DataType = Type.GetType("System.Decimal");
                else if (pif.PropertyType.Name == "SwiftDate")                  // 20140711 00000000               
                    column.DataType = Type.GetType("System.DateTime");
                else if (pif.PropertyType.Name == "SwiftTime")                  // 000000                
                    column.DataType = Type.GetType("System.DateTime");
                else if (pif.PropertyType.Name == "SwiftQuantityType")               
                    column.DataType = Type.GetType("System.String");
                else if (pif.PropertyType.Name == "SwiftTextLines")
                    column.DataType = Type.GetType("System.String");
                else if (pif.PropertyType.Name == "SwiftPriceType")
                    column.DataType = Type.GetType("System.String");                
                else {
                    Debug.WriteLine("kein type!! pif.Name:" + pif.Name + " PropertyType:" + pif.PropertyType.Name);
                    column.DataType = Type.GetType("System.String");
                }
                DataSetAdmin.dtWertpapSubsembly.Columns.Add(column);
            }
            string sKto = aStatementOfHoldings.SecuritiesAccountNumber;
            DataRow newRow;
            decimal aktwert = 0;
            int co = -1;
            // ------ Zeilen erstellen ------
            Console.WriteLine("---- FinToDtGesamt() in dtWertpapSubsembly ------ Zeilen(Daten) erstellen ------ ");
            foreach (SwiftStatementOfHoldingsFinancialInstrument aFinancialInstrument in aStatementOfHoldings) {
                Console.WriteLine("    Zeile: {0, -50} {1,-16}", aFinancialInstrument.SecurityName.Pack(), aFinancialInstrument.ISIN);
                newRow = DataSetAdmin.dtWertpapSubsembly.NewRow();
                co = -1;
                Object[] properties2 = aFinancialInstrument.GetType().GetProperties();
                string str = null;
                string str2 = null;
                //Console.Write("---- Eigenschaften ");
                foreach (Object prop in properties2) {
                    System.Reflection.PropertyInfo pif = (System.Reflection.PropertyInfo)prop;
                    Object val1 = pif.GetValue(aFinancialInstrument, null);
                    Object val3 = pif.PropertyType;
                    //Console.WriteLine("    {0,-34} {1,-24} {2}", pif.Name, val1, val3);
                    ++co;
                    try {
                        if (pif.PropertyType.Name == "SwiftDate") {
                            str = val1.ToString();
                            if (str == "00000000")
                                newRow[co] = Convert.ToDateTime("01/01/1980");
                            else
                                newRow[co] = Convert.ToDateTime(str.Substring(6, 2) + "/" + str.Substring(4, 2) + "/" + str.Substring(0, 4) + " 12:00:00");
                        } else if (pif.PropertyType.Name == "SwiftTime") {
                            str = val1.ToString();
                            str2 = Convert.ToDateTime("01/01/1980 " + str.Substring(0, 2) + ":" + str.Substring(2, 2)).ToString();
                            newRow[co] = str2;
                        } else if (pif.PropertyType.Name == "SwiftQuantityType") {
                            newRow[co] = val1;
                        } else if (pif.PropertyType.Name == "SwiftTextLines") {
                            newRow[co] = val1;
                        } else if (pif.PropertyType.Name == "SwiftPriceType") {
                            newRow[co] = val1;
                        } else {
                            newRow[co] = val1;
                        }
                    } catch (Exception ex) {
                        MessageBox.Show("Fehler in finToDtGesamt() : val1: " + val1.ToString() + " ex: " + ex);
                        Console.WriteLine("Fehler in finToDtGesamt() : val1: " + val1.ToString() + " ex: " + ex);
                        newRow[co] = Convert.ToDateTime("01/01/1980");  // Ist wohl Date???
                    }
                }
                Console.WriteLine("    SecurityName: {0,-50} ISIN: {1} Price: {2}", newRow["SecurityName"], newRow["ISIN"], newRow["Price"]);
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