// 27.10.2017 UmsätzeHolen.cs
// Lade .dtKontoumsätze aus g_Ein.myDepotPfad + \L og\Umsätze-" + sKtoNr + .csv Daten
// 27.10.2017 Orner angepasst, Notebook.
using System.Data;
using DataSetAdminNS;
namespace MeineFinanzen.Helpers {
    public class UmsätzeHolen {
        public Model.CollKontoumsätze _kontoumsätze = null;
        View.HauptFenster _mw;
        public UmsätzeHolen() { }
        public UmsätzeHolen(View.HauptFenster mw) {
            _mw = mw;
            _kontoumsätze = (Model.CollKontoumsätze)_mw.Resources["kontoumsätze"];
            //ErstelledtKontoumsätze();
            foreach (DataRow dr in DataSetAdmin.dtKontoumsätze.Rows) {
                //Console.WriteLine("{0} ---- ValueDate: {1} {2}", ++nnnn, dr["ValueDate"].ToString(), dr["Value"].ToString());
                //try {
                    _kontoumsätze.Add(new Model.Kontoumsatz {
                        Kontonummer = dr["Kontonummer"].ToString(),
                        EntryDate = dr["EntryDate"].ToString(),
                        ValueDate = dr["ValueDate"].ToString(),
                        Value = dr["Value"].ToString(),
                        AcctNo = dr["AcctNo"].ToString(),
                        BankCode = dr["BankCode"].ToString(),
                        Name1 = dr["Name1"].ToString(),
                        Name2 = dr["Name2"].ToString(),
                        PaymtPurpose = dr["PaymtPurpose"].ToString(),
                        EntryText = dr["EntryText"].ToString(),
                        PrimaNotaNo = dr["PrimaNotaNo"].ToString(),
                        TranTypeIdCode = dr["TranTypeIdCode"].ToString(),
                        ZkaTranCode = dr["ZkaTranCode"].ToString(),
                        TextKeyExt = dr["TextKeyExt"].ToString(),
                        BankRef = dr["BankRef"].ToString(),
                        OwnerRef = dr["OwnerRef"].ToString(),
                        SupplementaryDetails = dr["SupplementaryDetails"].ToString()
                    });
               // }
               // catch (Exception ex) {
               //     MessageBox.Show("aaaa" + ex);
               // }
            }
        }
    }
}