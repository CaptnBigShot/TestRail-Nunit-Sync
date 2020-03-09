using System;
using System.Reflection;
using CommandLine;

namespace TestRail_Nunit_Sync
{
    public class Options
    {
        [Option("nunit-test-cases-file", Required = false, HelpText = "Export file of NUnit3 tests to sync into TestRail (output from nunit3-console --explore).")]
        public string NunitTestCasesFile { get; set; }

        [Option("nunit-test-results-file", Required = false, HelpText = "Export file of NUnit3 test results to sync into TestRail.")]
        public string NunitTestResultsFile { get; set; }

        [Option("testrail-url", Required = true, HelpText = "URL of TestRail instance.")]
        public string TestRailUrl { get; set; }

        [Option("testrail-user-email", Required = true, HelpText = "TestRail user's email for authentication.")]
        public string TestRailUserEmail { get; set; }

        [Option("testrail-user-password", Required = true, HelpText = "TestRail user's password for authentication.")]
        public string TestRailPassword { get; set; }

        [Option("testrail-project-id", Required = true, HelpText = "TestRail project ID to sync cases/results.")]
        public string TestRailProjectId { get; set; }

        [Option("testrail-run-name", Required = false, HelpText = "Test Run name to use for reporting test results.")]
        public string TestRailRunName { get; set; }

        public bool ShouldSyncTestCases => !string.IsNullOrEmpty(NunitTestCasesFile);

        public bool ShouldSyncTestResults => !string.IsNullOrEmpty(NunitTestResultsFile);

        public void PrintOptions()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine();
            Console.WriteLine("TestRail-NUnit-Sync v." + version);
            Console.WriteLine();
            Console.WriteLine("Syncing NUnit3 Tests into TestRail based on the following parameters");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("NUnit Test Cases File    : " + NunitTestCasesFile);
            Console.WriteLine("NUnit Test Results File  : " + NunitTestResultsFile);
            Console.WriteLine("Should Sync Test Cases   : " + ShouldSyncTestCases);
            Console.WriteLine("Should Sync Test Results : " + ShouldSyncTestResults);
            Console.WriteLine("TestRail URL             : " + TestRailUrl);
            Console.WriteLine("TestRail User Email      : " + TestRailUserEmail);
            Console.WriteLine("TestRail Project ID      : " + TestRailProjectId);
            Console.WriteLine("TestRail Run Name        : " + TestRailRunName);
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine();
        }
    }
}