using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using FinTsPersistence.Actions.Result;
using Subsembly.FinTS;
using Subsembly.Swift;

namespace FinTsPersistence.Actions
{
    /// <summary>
    /// Important step for the persisting - raw data is returned
    /// </summary>
    public class ActionPersist : ActionBase
    {
        public const string ActionName = "persist";

        SwiftDate fromDate = SwiftDate.NullDate;

        protected override bool OnParse(string action, StringDictionary arguments)
        {
            string sFromDate = arguments["-fromdate"];
            if (sFromDate != null)
            {
                fromDate = SwiftDate.Parse(sFromDate, SwiftDateFormat.StandardDate);
            }

            return true;
        }

        protected override FinOrder OnCreateOrder(FinService aService)
        {
            return aService.CreateDownloadStatement(fromDate);
        }

        protected override ResponseData OnGetResponseData(FinService service, FinOrder order)
        {
            FinAcctMvmtsSpecifiedPeriod aAcctMvmts = order as FinAcctMvmtsSpecifiedPeriod;
            if (aAcctMvmts == null)
            {
                return null;
            }

            return _GetResponseData(aAcctMvmts);
        }

        private ResponseData _GetResponseData(FinAcctMvmtsSpecifiedPeriod aAcctMvmts)
        {
            SwiftStatementReader aMT940 = aAcctMvmts.BookedTrans;
            if (aMT940 == null)
            {
                return null;
            }

            SwiftStatement aStmt = SwiftStatement.ReadMT940(aMT940, true);
            if (aStmt == null)
            {
                return null;
            }

            return new ResponseData
                       {
                           Transactions = GetTransactions(aStmt)
                       };
        }
   
        private List<FintTsTransaction> GetTransactions(Swift9xxBase aStmt)
        {
            List<FintTsTransaction> result = new List<FintTsTransaction>();

            foreach (SwiftStatementLine aStmtLine in aStmt.StatementLines)
            {
                string paymentPurpose = null;
                if (aStmtLine.PaymtPurpose != null)
                {
                    paymentPurpose = String.Join(Environment.NewLine, aStmtLine.PaymtPurpose);
                }

                result.Add(new FintTsTransaction(
                    aStmtLine.EntryDate.ToDateTime(),
                    aStmtLine.ValueDate.ToDateTime(),
                    aStmtLine.DecValue,
                    aStmtLine.PayeePayerAcctNo,
                    aStmtLine.PayeePayerBankCode,
                    aStmtLine.PayeePayerName,
                    paymentPurpose,
                    aStmtLine.EntryText,
                    aStmtLine.PrimaNotaNo,
                    aStmtLine.TranTypeIdCode,
                    aStmtLine.ZkaTranCode,
                    aStmtLine.TextKeyExt,
                    aStmtLine.BankRef,
                    aStmtLine.OwnerRef,
                    aStmtLine.SupplementaryDetails));
            }

            return result;
        }
    }
}