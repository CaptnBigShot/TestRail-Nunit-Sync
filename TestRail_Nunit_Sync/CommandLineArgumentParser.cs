using CommandLine;

namespace TestRail_Nunit_Sync
{
    public class CommandLineArgumentParser
    {
        public bool Parse(string[] args, Options options)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                if (!string.IsNullOrEmpty(o.NunitTestCasesFile))
                {
                    options.NunitTestCasesFile = o.NunitTestCasesFile;
                }

                if (!string.IsNullOrEmpty(o.NunitTestResultsFile))
                {
                    options.NunitTestResultsFile = o.NunitTestResultsFile;
                }

                if (!string.IsNullOrEmpty(o.TestRailUrl))
                {
                    options.TestRailUrl = o.TestRailUrl;
                }

                if (!string.IsNullOrEmpty(o.TestRailUserEmail))
                {
                    options.TestRailUserEmail = o.TestRailUserEmail;
                }

                if (!string.IsNullOrEmpty(o.TestRailPassword))
                {
                    options.TestRailPassword = o.TestRailPassword;
                }

                if (!string.IsNullOrEmpty(o.TestRailProjectId))
                {
                    options.TestRailProjectId = o.TestRailProjectId;
                }

                if (!string.IsNullOrEmpty(o.TestRailRunName))
                {
                    options.TestRailRunName = o.TestRailRunName;
                }
            });

            return true;
        }
    }
}
