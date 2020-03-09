using System.Collections.Generic;

namespace TestRail_Nunit_Sync.Models
{
    public class TestCaseModel
    {
        public string FixtureName { get; set; }
        public string Title { get; set; }
        public int Id { get; set; }
        public int? SectionId { get; set; }
        public int? RootSectionId { get; set; }
        public int? TypeId { get; set; }
        public bool IsAutomated { get; set; }
        public string Tags { get; set; }
        public string FixtureFullName { get; set; }
        public SectionModel Section { get; set; }
        public TestResultModel TestResult { get; set; }

        public override string ToString()
        {
            return $"TestCase => Title: '{Title}' | FixtureName: '{FixtureName}' | IsAutomated: '{IsAutomated}' | ID: '{Id}' ";
        }

        public bool MatchesOther(TestCaseModel other)
        {
            return IsAutomated == other.IsAutomated &&
                   TypeId == other.TypeId;
        }

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["title"] = Title,
                ["type_id"] = TypeId,
                ["custom_is_automated"] = IsAutomated
            };
        }
    }
}
