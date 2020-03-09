using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRail_Nunit_Sync.Controllers;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Tests
{
    [TestFixture]
    public class TestSyncControllerTests
    {
        private List<TestCaseModel> _testRailCases;

        private TestSyncController _testSyncController;

        [SetUp]
        public void SetUp()
        {
            // Section shared by multiple TestRail cases
            var section = new SectionModel
            {
                Id = 3,
                ParentId = 2,
                RootSectionId = 1,
                Name = "Login",
                Description = "",
                Depth = 2,
            };

            // Define TestRail cases
            _testRailCases = new List<TestCaseModel>
            {
                new TestCaseModel
                {
                    Id = 1,
                    Title = "Invalid user login",
                    Section = section,
                    SectionId = section.Id,
                    IsAutomated = true,
                    TypeId = 1,
                },
                new TestCaseModel
                {
                    Id = 2,
                    Title = "Valid user login",
                    Section = section,
                    SectionId = section.Id,
                    IsAutomated = true,
                    TypeId = 1,
                },
            };

            // Initialize Controllers
            var nunitFileController = new NunitFileController();
            var testRailApiController = new TestRailApiController("", "", "", "");
            const string rootSectionName = "SyncTest";
            _testSyncController = new TestSyncController(nunitFileController, testRailApiController, rootSectionName);
        }

        [TestCase("Login", "Valid user login", true)]
        [TestCase("User Login", "Valid user login", false)]
        [TestCase("Login", "Non-Valid user login", false)]
        public void TestMatchTestRailCaseToNunitCase(string fixtureName, string title, bool shouldMatch)
        {
            var nunitTestCase = new TestCaseModel
            {
                FixtureFullName = "Specs.Features.Account",
                FixtureName = fixtureName,
                Title = title,
                Tags = "regression,automated",
                IsAutomated = true,
                TypeId = 1,
            };

            var matchingCase = _testSyncController.MatchTestRailCaseToNunitCase(_testRailCases, nunitTestCase);

            if (shouldMatch)
                matchingCase.Should().Be(_testRailCases[1]);
            else
                matchingCase.Should().BeNull();
        }
    }
}
