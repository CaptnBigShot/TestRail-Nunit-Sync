using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gurock.TestRail;
using Newtonsoft.Json.Linq;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Controllers
{
    public class TestRailApiController
    {
        public TestRailApiController(string url, string userEmail, string password, string projectId)
        {
            _testRailApiClient = new APIClient(url) { User = userEmail, Password = password };
            _testRailProjectId = projectId;
        }

        private static APIClient _testRailApiClient;

        private static string _testRailProjectId;


        #region Shared Methods

        public object SendRequest(Func<string, object, object> requestMethod, string uri, object data)
        {
            const int maxNumOfAttempts = 10;

            // Send the request to TestRail and re-try if it returns a handled API Exception
            for (var i = 0; i < maxNumOfAttempts; i++)
            {
                try
                {
                    return requestMethod(uri, data);
                }
                catch (APIException e)
                {
                    var httpCode = int.Parse(e.Message.Substring(e.Message.IndexOf("HTTP ", StringComparison.Ordinal) + 5, 3));
                    switch (httpCode)
                    {
                        case 409:
                        {
                            const int retryAfterSeconds = 120;
                            Console.WriteLine("TestRail account undergoing maintenance. Retrying after " + retryAfterSeconds + " seconds.");
                            Thread.Sleep(TimeSpan.FromSeconds(retryAfterSeconds));
                            break;
                        }
                        case 429:
                        {
                            const string substringStart = "Retry after ";
                            const string substringEnd = " seconds.";
                            var startIdx = e.Message.IndexOf(substringStart, StringComparison.Ordinal) + substringStart.Length;
                            var endIdx = e.Message.IndexOf(substringEnd, StringComparison.Ordinal);
                            var length = endIdx - startIdx;
                            var retryAfterSecondsSubstring = e.Message.Substring(startIdx, length);
                            var retryAfterSeconds = int.Parse(retryAfterSecondsSubstring);

                            Console.WriteLine("TestRail API rate limit reached. Retrying after " + retryAfterSeconds + " seconds.");

                            // Wait for X number of seconds per the API response
                            Thread.Sleep(TimeSpan.FromSeconds(retryAfterSeconds));
                            break;
                        }
                        default:
                        {
                            throw;
                        }
                    }
                }
            }

            Console.WriteLine("Failed to send request after " + maxNumOfAttempts + " attempts.");
            return null;
        }

        public void TestConnection()
        {
            try
            {
                GetUserIdByEmail(_testRailApiClient.User);
            }
            catch (Exception e)
            {
                Console.WriteLine("TestRail connection failed.");
                Console.WriteLine();
                Console.WriteLine(e.Message);
                throw;
            }
        }

        #endregion


        #region Sections

        public List<SectionModel> GetSections()
        {
            // Get sections from TestRail
            var sectionsJson = (JArray)SendRequest(_testRailApiClient.SendGet, "get_sections/" + _testRailProjectId, null);

            // Format sections list
            var sections = new List<SectionModel>();
            foreach (var sectionJson in sectionsJson)
            {
                var section = new SectionModel
                {
                    Name = (string)sectionJson["name"],
                    Description = (string)sectionJson["description"],
                    Depth = (int)sectionJson["depth"],
                    Id = (int)sectionJson["id"],
                    ParentId = (int?)sectionJson["parent_id"]
                };

                sections.Add(section);
            }

            // Update each section with the root section
            sections.ForEach(s => s.RootSectionId = s.GetRootSection(sections).Id);

            return sections;
        }

        public SectionModel CreateSection(SectionModel section)
        {
            Console.WriteLine($"Creating section: {section.Name}");

            // Send the request
            var response = (JObject)SendRequest(_testRailApiClient.SendPost, "add_section/" + _testRailProjectId, section.ToDict());

            var createdSection = new SectionModel
            {
                Name = (string)response["name"],
                Description = (string)response["description"],
                Depth = (int)response["depth"],
                Id = (int)response["id"],
                ParentId = (int?)response["parent_id"]
            };

            return createdSection;
        }

        #endregion


        #region Test Cases

        public List<TestCaseModel> GetTestCases()
        {
            // Get sections from TestRail
            var sections = GetSections().ToList();

            // Get test cases from TestRail
            var testCasesJson = (JArray)SendRequest(_testRailApiClient.SendGet, "get_cases/" + _testRailProjectId, null);

            // Convert JArray to concrete list
            var testCases = new List<TestCaseModel>();
            foreach (var testCaseJson in testCasesJson)
            {
                var sectionId = (int)testCaseJson["section_id"];
                var section = sections.First(s => s.Id == sectionId);
                var testCase = new TestCaseModel
                {
                    Id = (int)testCaseJson["id"],
                    Title = (string)testCaseJson["title"],
                    FixtureName = (string)testCaseJson["custom_feature_name"],
                    IsAutomated = (bool)testCaseJson["custom_is_automated"],
                    TypeId = (int)testCaseJson["type_id"],
                    SectionId = sectionId,
                    Section = section,
                    RootSectionId = section.RootSectionId
                };

                testCases.Add(testCase);
            }

            return testCases;
        }

        public void ProcessTestCases(List<TestCaseModel> testCasesToCreate, List<TestCaseModel> testCasesToUpdate, List<TestCaseModel> testCasesToDelete)
        {
            Console.WriteLine("\n\nProcessing test cases in TestRail..");
            Console.WriteLine($"Create: {testCasesToCreate.Count}");
            Console.WriteLine($"Update: {testCasesToUpdate.Count}");
            Console.WriteLine($"Delete: {testCasesToDelete.Count}");

            // Create
            foreach (var testCase in testCasesToCreate)
            {
                SendRequest(_testRailApiClient.SendPost, "add_case/" + testCase.SectionId, testCase.ToDict());
            }

            // Update
            foreach (var testCase in testCasesToUpdate)
            {
                SendRequest(_testRailApiClient.SendPost, "update_case/" + testCase.Id, testCase.ToDict());
            }

            // Delete
            foreach (var testCase in testCasesToDelete)
            {
                SendRequest(_testRailApiClient.SendPost, "delete_case/" + testCase.Id, null);
            }
        }

        #endregion


        #region Test Runs

        public int CreateTestRun(string name, List<TestCaseModel> testCases)
        {
            var payload = new Dictionary<string, object>
            {
                ["name"] = name,
                ["assignedto_id"] = GetUserIdByEmail(_testRailApiClient.User),
                ["case_ids"] = testCases.Select(x => x.Id).Distinct().ToArray(),
                ["include_all"] = false,
            };

            var response = (JObject)SendRequest(_testRailApiClient.SendPost, "add_run/" + _testRailProjectId, payload);
            var runId = (int)response["id"];
            return runId;
        }

        public void AddTestRunResults(int runId, List<TestCaseModel> testCases)
        {
            Console.WriteLine("\n\nSubmitting test results to TestRail..");

            // Create list of test results
            var testResults = new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["results"] = new List<Dictionary<string, object>>()
            };

            // Iterate through each result and create the result body
            foreach (var testCase in testCases)
            {
                testResults["results"].Add(testCase.TestResult.ToDict(testCase.Id));
            }

            // Send test results to TestRail
            try
            {
                // POST test results to TestRail
                var response = (JArray)SendRequest(_testRailApiClient.SendPost, "add_results_for_cases/" + runId, testResults);

                // Add attachments to TestRail results
                for (var i = 0; i < testCases.Count; i++)
                {
                    var testCase = testCases[i];

                    // Set the created TestRail Result ID to each test result that was sent
                    testCase.TestResult.Id = (int)response[i]["id"];

                    // POST attachments to TestRail test result (if any)
                    foreach (var attachment in testCase.TestResult.Attachments)
                    {
                        SendRequest(_testRailApiClient.SendPost, "add_attachment_to_result/" + testCase.TestResult.Id, attachment.FilePath);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(testResults));
                throw;
            }
        }

        #endregion


        #region Case Types

        public List<TestCaseTypeModel> GetCaseTypes()
        {
            // Get case types from TestRail
            var caseTypesJson = (JArray)SendRequest(_testRailApiClient.SendGet, "get_case_types", null);

            // Format case types list
            var caseTypes = new List<TestCaseTypeModel>();
            foreach (var caseTypeJson in caseTypesJson)
            {
                var caseType = new TestCaseTypeModel
                {
                    Id = (int)caseTypeJson["id"],
                    Name = (string)caseTypeJson["name"],
                    IsDefault = (bool)caseTypeJson["is_default"],
                };

                caseTypes.Add(caseType);
            }

            return caseTypes;
        }

        #endregion


        #region Users

        private int GetUserIdByEmail(string email)
        {
            var response = (JObject)SendRequest(_testRailApiClient.SendGet, "get_user_by_email&email=" + email, null);
            var userId = (int)response["id"];
            return userId;
        }

        #endregion

    }
}
