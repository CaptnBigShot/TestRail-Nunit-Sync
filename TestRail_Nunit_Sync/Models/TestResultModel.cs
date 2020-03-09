using System;
using System.Collections.Generic;

namespace TestRail_Nunit_Sync.Models
{
    public class TestResultModel
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public string Duration { get; set; }
        public string ErrorMessage { get; set; }
        public List<TestResultAttachmentModel> Attachments { get; set; }

        public override string ToString()
        {
            return $"TestResult => Result: '{Result}' | Duration: '{Duration}' | ErrorMessage: '{ErrorMessage}'";
        }

        public Dictionary<string, object> ToDict(int caseId)
        {
            return new Dictionary<string, object>
            {
                ["case_id"] = caseId,
                ["status_id"] = Result == "Passed" ? "1" : "5",
                ["comment"] = Result == "Passed" ? "Passed via automated test." : ErrorMessage,
                ["elapsed"] = Math.Ceiling(decimal.Parse(Duration) + (decimal)0.01) + "s"
            };
        }
    }
}
