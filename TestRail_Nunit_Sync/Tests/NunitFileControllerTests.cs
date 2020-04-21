using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRail_Nunit_Sync.Controllers;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Tests
{
    [TestFixture]
    public class NunitFileControllerTests
    {
        private static readonly string Nunit3TestCasesFile = AppDomain.CurrentDomain.BaseDirectory + @"Tests\Data\NUnit3TestCases.xml";

        private static readonly string Nunit3TestResultsFile = AppDomain.CurrentDomain.BaseDirectory + @"Tests\Data\NUnit3TestResults.xml";

        private List<TestCaseModel> _nunit3TestCases;

        private List<TestCaseModel> _nunit3TestResults;

        private List<TestCaseModel> _testCases;

        private NunitFileController _nunitFileController;

        [SetUp]
        public void SetUp()
        {
            // Define test case types mimicking TestRail. Should only have one default type.
            var testCaseTypes = new List<TestCaseTypeModel>
            {
                new TestCaseTypeModel {Id = 1, Name = "Regression", IsDefault = false},
                new TestCaseTypeModel {Id = 2, Name = "Other", IsDefault = true},
                new TestCaseTypeModel {Id = 3, Name = "Smoke", IsDefault = false}
            };

            // Read the files
            _nunitFileController = new NunitFileController();
            _nunit3TestCases = _nunitFileController.GetTestsFromNunit3File(Nunit3TestCasesFile, testCaseTypes);
            _nunit3TestResults = _nunitFileController.GetTestsFromNunit3File(Nunit3TestResultsFile, testCaseTypes);

            // Define expected test cases. Results are included by default, but are removed based on this test's params.
            _testCases = new List<TestCaseModel>
            {
                new TestCaseModel
                {
                    FixtureFullName = "Specs.Features.Account",
                    FixtureName = "Login",
                    Title = "Log in with valid credentials",
                    Tags = "browser,smoke,automated",
                    IsAutomated = true,
                    TypeId = 3,
                    TestResult = new TestResultModel
                    {
                        Result = "Passed", Duration = "7.103904", ErrorMessage = "",
                        Attachments = new List<TestResultAttachmentModel>(),
                    },
                },
                new TestCaseModel
                {
                    FixtureFullName = "Specs.Features.Account",
                    FixtureName = "Login",
                    Title = "Log in with invalid credentials (Blank UserName,,password12,The User Name field is required.)",
                    Tags = "browser,regression,automated",
                    IsAutomated = true,
                    TypeId = 1,
                    TestResult = new TestResultModel
                    {
                        Result = "Passed", Duration = "7.567879", ErrorMessage = "",
                        Attachments = new List<TestResultAttachmentModel>(),
                    },
                },
                new TestCaseModel
                {
                    FixtureFullName = "Specs.Features.Account",
                    FixtureName = "Login",
                    Title =
                        "Log in with invalid credentials (Blank Password,test.user,,The Password field is required.)",
                    Tags = "browser,regression,automated",
                    IsAutomated = true,
                    TypeId = 1,
                    TestResult = new TestResultModel
                    {
                        Result = "Passed", Duration = "6.300555", ErrorMessage = "",
                        Attachments = new List<TestResultAttachmentModel>(),
                    },
                },
                new TestCaseModel
                {
                    FixtureFullName = "Specs.Features.Agent",
                    FixtureName = "Agent EFT Info",
                    Title = "Cancel adding new agent EFT info",
                    Tags = "browser,automated",
                    IsAutomated = true,
                    TypeId = 2,
                    TestResult = new TestResultModel
                    {
                        Result = "Passed", Duration = "42.278604", ErrorMessage = "",
                        Attachments = new List<TestResultAttachmentModel>(),
                    },
                },
                new TestCaseModel
                {
                    FixtureFullName = "Specs.Features.Agent",
                    FixtureName = "Agent EFT Info",
                    Title = "Edit agent EFT info",
                    Tags = "browser,regression,automated-skip",
                    IsAutomated = false,
                    TypeId = 1,
                    TestResult = new TestResultModel
                    {
                        Result = "Passed", Duration = "43.204554", ErrorMessage = "",
                        Attachments = new List<TestResultAttachmentModel>(),
                    },
                },
                new TestCaseModel
                {
                    FixtureFullName = "Specs.ServiceTests.Extracts",
                    FixtureName = "EzbConnectorTests",
                    Title = "Generate extracts with various eligibility scenarios",
                    Tags = "app-server,ezb,writes-to-database,shared-test-group-TEST0001,regression,automated",
                    IsAutomated = true,
                    TypeId = 1,
                    TestResult = new TestResultModel
                    {
                        Result = "Failed",
                        Duration = "2217.758296",
                        ErrorMessage = "Expected something but got something else.",
                        Attachments = new List<TestResultAttachmentModel>
                        {
                            new TestResultAttachmentModel
                            {
                                FilePath = @"C:\Windows\Web\Wallpaper\Theme1\img1.jpg", Description = "Screenshot"
                            },
                            new TestResultAttachmentModel
                            {
                                FilePath = @"C:\Windows\Web\Wallpaper\Theme1\img2.jpg"
                            }
                        },
                    },
                },
            };
        }

        [Test]
        public void TestTotalNumberOfTestCasesInNunit3File()
        {
            _nunit3TestCases.Should().HaveCount(6);
        }

        [Test]
        public void TestGetTestCasesFromNunit3File([Range(0, 5, 1)] int idx)
        {
            var nunitTestCase = _nunit3TestCases[idx];
            var expectedTestCase = _testCases[idx];

            // Set the expected Test Result since shared data set already has results
            expectedTestCase.TestResult = new TestResultModel
            {
                Result = null,
                Duration = null,
                ErrorMessage = "",
                Attachments = new List<TestResultAttachmentModel>(),
            };

            nunitTestCase.Should().BeEquivalentTo(expectedTestCase);
        }

        [Test]
        public void TestTotalNumberOfTestResultsInNunit3File()
        {
            _nunit3TestResults.Should().HaveCount(6);
        }

        [Test]
        public void TestGetTestResultsFromNunit3File([Range(0, 5, 1)] int idx)
        {
            var nunitTestCaseResult = _nunit3TestResults[idx];
            var expectedTestCaseResult = _testCases[idx];

            nunitTestCaseResult.TestResult.Should().BeEquivalentTo(expectedTestCaseResult.TestResult);
        }

        [Test]
        public void TestGetTestRunStartTimeFromNunit3File()
        {
            var testRunStartTime = _nunitFileController.GetTestRunStartTimeFromNunit3File(Nunit3TestResultsFile);
            testRunStartTime.Should().Be(new DateTime(2020, 3, 2, 12, 21, 6));
        }
    }
}
