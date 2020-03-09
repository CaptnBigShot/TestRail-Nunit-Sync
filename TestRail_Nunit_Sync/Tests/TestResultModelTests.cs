using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Tests
{
    [TestFixture]
    public class TestResultModelTests
    {
        [TestCase("Passed", "0.01", "", "1", "1s", "Passed via automated test.")]
        [TestCase("Failed", "10.01", "Error Message.", "5", "11s", "Error Message.")]
        [TestCase("Passed", "1.219082", "", "1", "2s", "Passed via automated test.")]
        [TestCase("Passed", "0", "", "1", "1s", "Passed via automated test.")]
        [TestCase("Passed", "129.949936", "", "1", "130s", "Passed via automated test.")]
        public void TestToDict(string result, string duration, string errorMessage, string statusId, string elapsed, string comment)
        {
            var testResult = new TestResultModel
            {
                Result = result,
                Duration = duration,
                ErrorMessage = errorMessage
            };

            const int caseId = 17;

            var dict = new Dictionary<string, object>
            {
                ["case_id"] = caseId,
                ["status_id"] = statusId,
                ["elapsed"] = elapsed,
                ["comment"] = comment,
            };

            testResult.ToDict(caseId).Should().BeEquivalentTo(dict);
        }
    }
}
