using System.Collections.Generic;
using System.Linq;

namespace TestRail_Nunit_Sync.Models
{
    public class SectionModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Depth { get; set; }
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public int? RootSectionId { get; set; }

        public override string ToString()
        {
            return $"Section => Name: '{Name}' | Depth: '{Depth}' | ID: '{Id}' | ParentId: '{ParentId}' | RootSectionId:'{RootSectionId}'";
        }

        public SectionModel GetRootSection(List<SectionModel> sections)
        {
            var maxDepth = sections.Select(s => s.Depth).Max() + 1;
            var iterations = 0;
            var rootSection = this;

            while (rootSection.Depth > 0 && iterations < maxDepth)
            {
                rootSection = sections.First(a => a.Id == rootSection.ParentId);
                iterations++;
            }

            return rootSection;
        }

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["name"] = Name,
                ["description"] = Description,
                ["parent_id"] = ParentId
            };
        }
    }
}
