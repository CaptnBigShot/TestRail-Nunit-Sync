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
                var fixtureCategories = GetTestCaseCategories(testFixtureNode);

                // Get test cases (non-parameterized)
                var testCaseNodes = testFixtureNode.SelectNodes("./test-case");
                foreach (XmlNode testCaseNode in testCaseNodes)
                {
                    // Get categories
                    var tcCategories = GetTestCaseCategories(testCaseNode);
                    tcCategories.AddRange(fixtureCategories);

                    // Create model and add to list
                    var testCase = new TestCaseModel
                    {
                        FixtureName = testFixtureName,
                        Title = GetTestCaseName(testCaseNode),
                        Tags = string.Join(",", tcCategories),
                        FixtureFullName = fixtureFullName,
                        TestResult = GetTestResult(testCaseNode)
                    };
                    testCases.Add(testCase);
                }

                // Get test cases (parameterized)
                var testSuiteParameterizedNodes = testFixtureNode.SelectNodes("./test-suite[@type='ParameterizedMethod']");
                foreach (XmlNode testSuiteParameterizedNode in testSuiteParameterizedNodes)
                {
                    // Get categories
                    var tcpCategories = GetTestCaseCategories(testSuiteParameterizedNode);
                    tcpCategories.AddRange(fixtureCategories);

                    // Iterate through parameterized test cases
                    var testSuiteParameterizedTestCases = testSuiteParameterizedNode.SelectNodes("./test-case");
                    foreach (XmlNode testSuiteParameterizedTestCase in testSuiteParameterizedTestCases)
                    {
                        var testCaseParameterized = new TestCaseModel
                        {
                            FixtureName = testFixtureName,
                            Title = GetTestCaseName(testSuiteParameterizedTestCase),
                            Tags = string.Join(",", tcpCategories),
                            FixtureFullName = fixtureFullName,
                            TestResult = GetTestResult(testSuiteParameterizedTestCase)
                        };
                        testCases.Add(testCaseParameterized);
                    }
                }
            }

            // Final formatting for test cases
            foreach (var t in testCases)
            {
                // Sort the tags
                var tags = t.Tags.Split(',').OrderBy(tag => tag).ToList();
                t.Tags = string.Join(",", tags);

                // Make sure Title doesn't exceed max length in TestRail
                if (t.Title.Length > 250)
                    t.Title = t.Title.Substring(0, 250);

                // Case Type
                TestCaseTypeModel associatedCaseType = null;
                foreach (var tag in tags)
                {
                    associatedCaseType = testCaseTypes.Find(x => x.Name.ToLower().Equals(tag));
                    if (associatedCaseType != null)
                        break;
                }
                associatedCaseType = associatedCaseType ?? testCaseTypes.First(x => x.IsDefault);
                t.TypeId = associatedCaseType.Id;

                // Is Automated
                t.IsAutomated = tags.Contains("automated");
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

        private static string GetTestCaseName(XmlNode testCaseNode)
        {
            return testCaseNode?.Attributes?["name"]?.InnerText;
        }

        private static List<string> GetTestCaseCategories(XmlNode node)
        {
            var categoryNodes = node.SelectNodes("./properties/property[@name='Category']");
            return (from XmlNode categoryNode in categoryNodes select categoryNode?.Attributes?["value"]?.InnerText).ToList();
        }

        private static TestResultModel GetTestResult(XmlNode testCaseNode)
        {
            // Get Result and Duration
            var result = testCaseNode.Attributes?["result"]?.InnerText;
            var duration = testCaseNode.Attributes?["duration"]?.InnerText;

            // Get failure info if exists
            var failureMessage = "";
            var failureNode = testCaseNode.SelectSingleNode("./failure");
            if (failureNode != null)
            {
                var messageText = failureNode.SelectSingleNode("./message")?.InnerText;
                var stackTraceText = failureNode.SelectSingleNode("./stack-trace")?.InnerText;

                if (!string.IsNullOrWhiteSpace(messageText)) failureMessage += messageText;
                if (!string.IsNullOrWhiteSpace(stackTraceText)) failureMessage += "\n\n" + stackTraceText;
            }

            // Get attachments
            var attachments = new List<TestResultAttachmentModel>();
            var attachmentsNode = testCaseNode.SelectSingleNode("./attachments");
            if (attachmentsNode != null)
            {
                foreach (XmlNode attachmentNode in attachmentsNode.SelectNodes("./attachment"))
                {
                    attachments.Add(new TestResultAttachmentModel
                    {
                        FilePath = attachmentNode.SelectSingleNode("./filePath")?.InnerText,
                        Description = attachmentNode.SelectSingleNode("./description")?.InnerText,
                    });
                }
            }

            return new TestResultModel
            {
                Result = result,
                Duration = duration,
                ErrorMessage = failureMessage,
                Attachments = attachments,
            };
        }
    }
}
