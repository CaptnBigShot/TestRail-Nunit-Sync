using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Controllers
{
    public class NunitFileController
    {
        public List<TestCaseModel> GetTestsFromNunit3File(string xmlFilePath, List<TestCaseTypeModel> testCaseTypes)
        {
            var testCases = new List<TestCaseModel>();

            // Load the XML file
            var doc = new XmlDocument();
            doc.Load(xmlFilePath);

            // Get test fixtures
            var testFixtureNodes = doc.SelectNodes("//test-suite[@type='TestFixture']");
            foreach (XmlNode testFixtureNode in testFixtureNodes)
            {
                // Get test fixture name
                var fixtureDesc = testFixtureNode.SelectSingleNode("./properties/property[@name='Description']")?.Attributes["value"]?.InnerText;
                var fixtureName = testFixtureNode.Attributes["name"]?.InnerText;
                var testFixtureName = fixtureDesc ?? fixtureName;
                var fixtureFullName = testFixtureNode.Attributes["fullname"]?.InnerText.Replace($".{fixtureName}", "");

                // Define test categories
                var fixtureCategories = new List<string>();
                var fixtureCategoryNodes = testFixtureNode.SelectNodes("./properties/property[@name='Category']");
                foreach (XmlNode n in fixtureCategoryNodes)
                    fixtureCategories.Add(n?.Attributes["value"]?.InnerText);

                // Get test cases (non-parameterized)
                var testCaseNodes = testFixtureNode.SelectNodes("./test-case");
                foreach (XmlNode testCaseNode in testCaseNodes)
                {
                    // Get test case name
                    var tcName = testCaseNode.Attributes["name"]?.InnerText;
                    var testCaseName = tcName;

                    // Get categories
                    var tcCategories = new List<string>();
                    tcCategories.AddRange(fixtureCategories);
                    var tcCategoryNodes = testCaseNode.SelectNodes("./properties/property[@name='Category']");
                    foreach (XmlNode n in tcCategoryNodes)
                        tcCategories.Add(n?.Attributes["value"]?.InnerText);

                    // Get result info
                    var testCaseResult = testCaseNode.Attributes["result"]?.InnerText;
                    var testCaseDuration = testCaseNode.Attributes["duration"]?.InnerText;
                    var testCaseFailureMessage = "";

                    // Get failure info if exists
                    var testCaseFailureNode = testCaseNode.SelectSingleNode("./failure");
                    if (testCaseFailureNode != null)
                    {
                        testCaseFailureMessage = testCaseFailureNode.SelectSingleNode("./message")?.InnerText;
                    }

                    // Get attachments
                    var testCaseResultAttachments = new List<TestResultAttachmentModel>();
                    var tcAttachmentsNode = testCaseNode.SelectSingleNode("./attachments");
                    if (tcAttachmentsNode != null)
                    {
                        foreach (XmlNode n in tcAttachmentsNode.SelectNodes("./attachment"))
                        {
                            testCaseResultAttachments.Add(new TestResultAttachmentModel
                            {
                                FilePath = n.SelectSingleNode("./filePath").InnerText,
                                Description = n.SelectSingleNode("./description")?.InnerText,
                            });
                        }
                    }

                    // Create model and add to list
                    var testCase = new TestCaseModel
                    {
                        FixtureName = testFixtureName,
                        Title = testCaseName,
                        IsAutomated = tcCategories.Contains("automated"),
                        Tags = string.Join(",", tcCategories),
                        FixtureFullName = fixtureFullName,
                        TestResult = new TestResultModel
                        {
                            Result = testCaseResult,
                            Duration = testCaseDuration,
                            ErrorMessage = testCaseFailureMessage,
                            Attachments = testCaseResultAttachments,
                        }
                    };
                    testCases.Add(testCase);
                }

                // Get test cases (parameterized)
                var testSuiteParameterizedNodes = testFixtureNode.SelectNodes("./test-suite[@type='ParameterizedMethod']");
                foreach (XmlNode testSuiteParameterizedNode in testSuiteParameterizedNodes)
                {
                    // Get categories
                    var tcpCategories = new List<string>();
                    tcpCategories.AddRange(fixtureCategories);
                    var tcpCategoryNodes = testSuiteParameterizedNode.SelectNodes("./properties/property[@name='Category']");
                    foreach (XmlNode tcPropertyNode in tcpCategoryNodes)
                        tcpCategories.Add(tcPropertyNode?.Attributes["value"]?.InnerText);

                    // Iterate through parameterized test cases
                    var testSuiteParameterizedTestCases = testSuiteParameterizedNode.SelectNodes("./test-case");
                    foreach (XmlNode testSuiteParameterizedTestCase in testSuiteParameterizedTestCases)
                    {
                        // Get parameterized test case name
                        var testCaseParameterizedName = testSuiteParameterizedTestCase.Attributes["name"]?.InnerText;

                        // Get Result and Duration
                        var testCaseParameterizedResult = testSuiteParameterizedTestCase.Attributes["result"]?.InnerText;
                        var testCaseParameterizedDuration = testSuiteParameterizedTestCase.Attributes["duration"]?.InnerText;

                        // Get failure info if exists
                        var testSuiteParameterizedTestCasesFailureMessage = "";
                        var testSuiteParameterizedTestCaseFailureNode = testSuiteParameterizedTestCase.SelectSingleNode("./failure");
                        if (testSuiteParameterizedTestCaseFailureNode != null)
                        {
                            testSuiteParameterizedTestCasesFailureMessage = testSuiteParameterizedTestCaseFailureNode.SelectSingleNode("./message")?.InnerText;
                        }

                        // Get attachments
                        var testCaseParameterizedResultAttachments = new List<TestResultAttachmentModel>();
                        var testCaseParameterizedAttachmentsNode = testSuiteParameterizedTestCase.SelectSingleNode("./attachments");
                        if (testCaseParameterizedAttachmentsNode != null)
                        {
                            foreach (XmlNode n in testCaseParameterizedAttachmentsNode.SelectNodes("./attachment"))
                            {
                                testCaseParameterizedResultAttachments.Add(new TestResultAttachmentModel
                                {
                                    FilePath = n.SelectSingleNode("./filePath").InnerText,
                                    Description = n.SelectSingleNode("./description")?.InnerText,
                                });
                            }
                        }

                        // Create model and add to list
                        var testCaseParameterized = new TestCaseModel
                        {
                            FixtureName = testFixtureName,
                            Title = testCaseParameterizedName,
                            IsAutomated = tcpCategories.Contains("automated"),
                            Tags = string.Join(",", tcpCategories),
                            FixtureFullName = fixtureFullName,
                            TestResult = new TestResultModel
                            {
                                Result = testCaseParameterizedResult,
                                Duration = testCaseParameterizedDuration,
                                ErrorMessage = testSuiteParameterizedTestCasesFailureMessage,
                                Attachments = testCaseParameterizedResultAttachments,
                            }
                        };
                        testCases.Add(testCaseParameterized);
                    }
                }
            }

            // Set the Test Case Type
            foreach (var t in testCases)
            {
                // Case Type
                TestCaseTypeModel associatedCaseType = null;
                foreach (var tag in t.Tags.Split(','))
                {
                    associatedCaseType = testCaseTypes.Find(x => x.Name.ToLower().Equals(tag));
                    if (associatedCaseType != null)
                        break;
                }

                // Set the case type to default if none was found
                associatedCaseType = associatedCaseType ?? testCaseTypes.First(x => x.IsDefault);
                t.TypeId = associatedCaseType.Id;
            }

            return testCases;
        }

        public DateTime GetTestRunStartTimeFromNunit3File(string xmlFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(xmlFilePath);

            var testRunNode = doc.SelectSingleNode("/test-run");
            var startTimeText = testRunNode?.Attributes?["start-time"]?.InnerText;

            return DateTime.Parse(startTimeText);
        }
    }
}
