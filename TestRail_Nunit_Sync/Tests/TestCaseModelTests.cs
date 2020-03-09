using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Tests
{
    [TestFixture]
    public class TestCaseModelTests
    {
        [TestCase(1, true, true)]
        [TestCase(2, true, false)]
        [TestCase(1, false, false)]
        public void TestMatchesOther(int typeId, bool isAutomated, bool shouldMatch)
        {
            var test = new TestCaseModel { TypeId = 1, IsAutomated = true };
            var testOther = new TestCaseModel { TypeId = typeId, IsAutomated = isAutomated };
            test.MatchesOther(testOther).Should().Be(shouldMatch);
        }

        [Test]
        public void TestToDict()
        {
            var test = new TestCaseModel
            {
                Title = "Test Case 1",
                TypeId = 1,
                IsAutomated = true
            };

            var dict = new Dictionary<string, object>
            {
                ["title"] = "Test Case 1",
                ["type_id"] = 1,
                ["custom_is_automated"] = true
            };

            test.ToDict().Should().BeEquivalentTo(dict);
        }
    }
}
