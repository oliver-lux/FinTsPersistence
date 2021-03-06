﻿using System.Linq;
using FinTsPersistence;
using FinTsPersistence.Actions;
using FinTsPersistence.Actions.Result;
using FinTsPersistence.Model;
using FinTsPersistenceIntegrationTests.Helper;
using FluentAssertions;
using Machine.Specifications;
using Status = FinTsPersistence.Actions.Result.Status;

namespace FinTsPersistenceIntegrationTests
{
    /// <summary>
    /// Receiving data to persist from FinTS/HBCI
    /// This test really works against the real database!
    /// </summary>
    [Subject("FinTsPersistence")]
    public class when_receiving_data_to_persist_full_ef_test
    {
        static string contactfileLocation;
        static CmdArguments cmdArguments;
        static IActionResult result;

        Establish context = () =>
        {
            contactfileLocation = IntegrationTestData.GetContacfileLocation();
            cmdArguments = IntegrationTestData.GetCmdArguments();

            ITransactionContext ctx = ContainerConfig.Resolve<ITransactionContext>();
            ctx.Transactions.ToList().ForEach(t => ctx.Transactions.Remove(t));
            ctx.SaveChanges();
        };

        Because of = () => result = Start.DoAction(new[]
            {
                ActionPersist.ActionName, 
                Arguments.ContactFile, contactfileLocation,
                Arguments.Pin, cmdArguments.Pin,
                Arguments.AcctNo, cmdArguments.Acctno,
                Arguments.AcctBankCode, cmdArguments.Acctbankcode
            });

        It should_execute_successfully = () => result.Status.Should().Be(Status.Success);
        It should_return_a_list_of_transactions = () => result.Response.Transactions.Should().NotBeEmpty();
        It should_not_return_formatted_output = () => result.Response.Formatted.Should().BeNull();

        // for my bank there are transactions with no name, payment purpose and zero value (e.g. 'payments' with only informational text)
        //It should_return_transactions_with_a_name = () => result.Response.Transactions.ForEach(x => x.Name.Should().NotBeNullOrWhiteSpace());
        //It should_return_transactions_with_a_payment_purpose = () => result.Response.Transactions.ForEach(x => x.PaymentPurpose.Should().NotBeNullOrWhiteSpace());
        //It should_return_transactions_with_an_amount = () => result.Response.Transactions.ForEach(x => x.Value.Should().NotBe(0));
    }
}