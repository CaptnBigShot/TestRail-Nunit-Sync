using System;
using System.Collections.Generic;
using System.Linq;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Controllers
{
    public class TestSyncController
    {
        public TestSyncController(NunitFileController nunitFileController, TestRailApiController testRailApiController, string rootSectionName)
        {
            _nunitFileController = nunitFileController;
            _testRailApiController = testRailApiController;
            _rootSectionName = rootSectionName;
        }

        private static NunitFileController _nunitFileController;

        private static TestRailApiController _testRailApiController;

        private readonly string _rootSectionName;

        private static int? _rootSectionId;

        public static int NthOccurence(string s, char t, int n) => s.TakeWhile(c => (n -= (c == t ? 1 : 0)) > 0).Count();

        public void InitializeRootSectionInTestRail()
        {
            // Get TestRail sections
            var testRailSections = _testRailApiController.GetSections();

            // Find the target root section
            var rootSection = testRailSections.Find(s => s.Name == _rootSectionName);

            // If the root section wasn't found, create it
            if (rootSection == null)
            {
                Console.WriteLine($"Root section named '{_rootSectionName}' not found in TestRail.");
                rootSection = new SectionModel { Name = _rootSectionName };
                rootSection = _testRailApiController.CreateSection(rootSection);
            }

            // Assign the shared root section ID
            _rootSectionId = rootSection.Id;
        }

        public TestCaseModel MatchTestRailCaseToNunitCase(List<TestCaseModel> testRailCases, TestCaseModel nunitTestCase)
        {
            return testRailCases.Find(trCase => trCase.Section.Name + trCase.Title == nunitTestCase.FixtureName + nunitTestCase.Title);
        }

        public void SyncSectionsWithTestRail(List<TestCaseModel> nunitTestCases)
        {
            // Process sections into TestRail
            Console.WriteLine("Processing sections...");

            var fixtures = nunitTestCases.Select(x => x.FixtureFullName + "." + x.FixtureName).Distinct().ToList();
            var fixtureLongestDepth = fixtures.OrderByDescending(x => x.Length - x.Replace(".", "").Length).First();
            var maxSectionDepth = fixtureLongestDepth.Length - fixtureLongestDepth.Replace(".", "").Length + 1;
            var listOfFixtureLists = new List<List<string>>();

            // Get distinct fixtures for each depth level
            for (var i = 0; i < maxSectionDepth; i++)
            {
                listOfFixtureLists.Add(new List<string>());

                foreach (var fixture in fixtures)
                {
                    if (fixture.Length - fixture.Replace(".", "").Length >= i)
                    {
                        var title = fixture.Substring(0, NthOccurence(fixture, '.', i + 1));
                        if (!listOfFixtureLists[i].Contains(title))
                        {
                            listOfFixtureLists[i].Add(title);
                        }
                    }
                }
            }

            // Here's how the section processing works
            // Specs.Features.Group

            // listOfFixtureLists[0]
            // From testRailSections, get section where title = "Specs" and ParentId = _rootSectionId
            // If doesn't exist, create section
            // To listOfSectionLists[0], add new Section instance with Title = "Specs", Id = Specs.Id, and ParentId = _rootSectionId

            // listOfFixtureLists[1]
            // From listOfSectionLists[0], get section with title = "Specs" and ParentId = _rootSectionId
            // From testRailSections, get section with title = "Features" and ParentId = Specs.Id
            // If doesn't exist, create section
            // To listOfSectionLists[1], add new Section instance with Title = "Features", Id = Features.Id, and ParentId = Specs.Id

            // listOfFixtureLists[2]
            // From listOfSectionLists[0], get section with title = "Specs" and ParentId = _rootSectionId
            // From listOfSectionLists[1], get section with title = "Features" and ParentId = Specs.Id
            // From testRailSections, get section with title = "Group" and ParentId = Features.Id
            // If doesn't exist, create section
            // To listOfSectionLists[2], add new Section instance with Title = "Group", Id = Group.Id, and ParentId = Features.Id

            var listOfSectionLists = new List<List<SectionModel>>();

            for (int i = 0; i < listOfFixtureLists.Count; i++)
            {
                Console.WriteLine("Sections level " + i);

                listOfSectionLists.Add(new List<SectionModel>());

                // Get sections from TestRail
                var testRailSectionsLatest = _testRailApiController.GetSections()
                    .Where(x => x.RootSectionId == _rootSectionId).ToList();

                // Iterate through each fixture to create the sections
                foreach (var fixtureFull in listOfFixtureLists[i])
                {
                    var parentId = _rootSectionId;

                    var fixtureFullSplit = fixtureFull.Split('.').ToList();
                    for (var j = 0; j < fixtureFullSplit.Count - 1; j++)
                    {
                        // Get existing parent section from list
                        var parentSection = listOfSectionLists[j]
                            .Find(x => x.Name == fixtureFullSplit[j] && x.ParentId == parentId);

                        // Update parent id for use in section next level down
                        parentId = parentSection.Id;
                    }

                    // Check if section exists and if not, create it
                    var section = new SectionModel { Name = fixtureFullSplit.Last(), ParentId = parentId };
                    SectionModel testRailSection;
                    try
                    {
                        // Check if the section exists
                        testRailSection = testRailSectionsLatest.First(x =>
                            x.Name == section.Name && x.ParentId == parentId);

                        Console.WriteLine($"Section {section.Name} exists.");
                    }
                    catch
                    {
                        // If section doesn't exist, create it
                        testRailSection = _testRailApiController.CreateSection(section);
                    }

                    listOfSectionLists[i].Add(testRailSection);
                }
            }

            Console.WriteLine("Processing sections complete.\n");
        }

        public void SyncTestCasesWithTestRail(List<TestCaseModel> nunitTestCases)
        {
            // Get test cases from TestRail
            var syncedTestRailCases = _testRailApiController.GetTestCases().Where(tc => tc.RootSectionId == _rootSectionId).ToList();

            // Get sections from TestRail
            var syncedTestRailSections = _testRailApiController.GetSections().Where(s => s.RootSectionId == _rootSectionId).ToList();

            // Iterate through each NUnit test case and categorize those that need to be Created & Updated
            var casesToCreate = new List<TestCaseModel>();
            var casesToUpdate = new List<TestCaseModel>();
            foreach (var nunitTestCase in nunitTestCases)
            {
                // Find the matching test case in TestRail
                var testRailTestCase = MatchTestRailCaseToNunitCase(syncedTestRailCases, nunitTestCase);

                // If the TestRail test case was not found, it needs to be created. Otherwise, update it.
                if (testRailTestCase == null)
                {
                    nunitTestCase.SectionId = syncedTestRailSections.First(s => s.Name == nunitTestCase.FixtureName).Id; // Set the Section ID to add the TestCase under
                    casesToCreate.Add(nunitTestCase);
                }
                else
                {
                    if (!nunitTestCase.MatchesOther(testRailTestCase))
                    {
                        nunitTestCase.Id = testRailTestCase.Id; // Set TestRail Case ID
                        casesToUpdate.Add(nunitTestCase);
                    }
                }
            }

            // Get TestRail test cases that aren't found in the list of NUnit cases that need to be Deleted
            var casesToDelete = syncedTestRailCases
                .Where(trCase => nunitTestCases
                    .All(nuCase => nuCase.FixtureName + nuCase.Title != trCase.Section.Name + trCase.Title)).ToList();

            // Process the cases into TestRail
            _testRailApiController.ProcessTestCases(casesToCreate, casesToUpdate, casesToDelete);
        }

        public void SyncTestCases(string nunitTestCasesFile)
        {
            // Get test case types from TestRail
            var testRailCaseTypes = _testRailApiController.GetCaseTypes();

            // Get Tests from Nunit export file
            var nunitTestCases = _nunitFileController.GetTestsFromNunit3File(nunitTestCasesFile, testRailCaseTypes);

            // Sync sections with TestRail
            SyncSectionsWithTestRail(nunitTestCases);

            // Sync test cases with TestRail
            SyncTestCasesWithTestRail(nunitTestCases);
        }

        public void SyncTestResults(string nunitTestResultsFile, string testRailRunName)
        {
            // Get test case types from TestRail
            var testRailCaseTypes = _testRailApiController.GetCaseTypes();

            // Get Tests from Nunit export file
            var nunitTestCases = _nunitFileController.GetTestsFromNunit3File(nunitTestResultsFile, testRailCaseTypes);
            var nunitTestRunStartTime = _nunitFileController.GetTestRunStartTimeFromNunit3File(nunitTestResultsFile);

            // Get test cases from TestRail
            var syncedTestRailCases = _testRailApiController.GetTestCases().Where(tc => tc.RootSectionId == _rootSectionId).ToList();

            // Match cases with TestRail Case ID
            foreach (var nunitTestCase in nunitTestCases)
            {
                // Find the matching TestRail test case
                var testRailTestCase = MatchTestRailCaseToNunitCase(syncedTestRailCases, nunitTestCase);
                if (testRailTestCase == null)
                {
                    throw new Exception("No matching test case found for test result. \n" +
                                        "Sync this test case into TestRail before syncing results. \n" + nunitTestCase);
                }
                nunitTestCase.Id = testRailTestCase.Id;
            }

            // Create new Test Run in TestRail
            testRailRunName = testRailRunName + " " + nunitTestRunStartTime.ToString("MM/dd/yyyy hh:mm tt");
            var runId = _testRailApiController.CreateTestRun(testRailRunName, nunitTestCases);
            _testRailApiController.AddTestRunResults(runId, nunitTestCases);
            _testRailApiController.CloseTestRun(runId);
        }
    }
}
