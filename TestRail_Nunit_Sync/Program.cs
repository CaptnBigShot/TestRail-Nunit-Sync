using System;
using System.IO;
using TestRail_Nunit_Sync.Controllers;

namespace TestRail_Nunit_Sync
{
    public class Program
    {
        public static string RootSectionName = "Synced";

        private static int Main(string[] args)
        {
            // Parse command line arguments
            var commandLineArgumentParser = new CommandLineArgumentParser();
            var options = new Options();
            var areCommandLineArgumentsValid = commandLineArgumentParser.Parse(args, options);
            var areOptionsValid = AreOptionsValid(options);

            // Proceed with application if options are valid
            if (areCommandLineArgumentsValid && areOptionsValid)
            {
                // Output run options
                options.PrintOptions();

                try
                {
                    // Initialize TestRail controller
                    var testRailApiController = new TestRailApiController(
                        options.TestRailUrl, options.TestRailUserEmail,
                        options.TestRailPassword, options.TestRailProjectId);

                    // Test the TestRail API connection
                    testRailApiController.TestConnection();

                    // Initialize TestSync controller
                    var nunitController = new NunitFileController();
                    var testSyncController = new TestSyncController(nunitController, testRailApiController, RootSectionName);

                    // Ensure Root section exists for TestRail project
                    testSyncController.InitializeRootSectionInTestRail();

                    // Sync Test Cases
                    if (options.ShouldSyncTestCases)
                    {
                        testSyncController.SyncTestCases(options.NunitTestCasesFile);
                    }

                    // Sync Test Results
                    if (options.ShouldSyncTestResults)
                    {
                        testSyncController.SyncTestResults(options.NunitTestResultsFile, options.TestRailRunName);
                    }

                    Console.WriteLine();
                    Console.WriteLine("TestRail-NUnit-Sync completed successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    Console.WriteLine("TestRail-NUnit-Sync completed unsuccessfully.");
                    return 1;
                }
            }

            return 0;
        }

        private static bool AreOptionsValid(Options options)
        {
            // Verify at least one NUnit test cases or results file is passed
            if (string.IsNullOrEmpty(options.NunitTestCasesFile) && string.IsNullOrEmpty(options.NunitTestResultsFile))
            {
                Console.WriteLine("No NUnit Test Cases file or Test Results file supplied.");
                return false;
            }

            // Verify NUnit test cases file exists
            if (!string.IsNullOrEmpty(options.NunitTestCasesFile))
            {
                if (!File.Exists(options.NunitTestCasesFile))
                {
                    Console.WriteLine("NUnit Test Cases file not found.");
                    return false;
                }
            }

            // Verify NUnit test results file exists
            if (!string.IsNullOrEmpty(options.NunitTestResultsFile))
            {
                if (!File.Exists(options.NunitTestResultsFile))
                {
                    Console.WriteLine("NUnit Test Results file not found.");
                    return false;
                }
            }

            return true;
        }
    }
}
