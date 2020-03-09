using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRail_Nunit_Sync.Models;

namespace TestRail_Nunit_Sync.Tests
{
    [TestFixture]
    public class SectionModelTests
    {
        [Test]
        public void TestGetRootSection()
        {
            var id = 0;

            // Depth 0
            var synced = new SectionModel { Id = id += 1, Name = "Synced", Depth = 0, ParentId = null };
            var archived = new SectionModel { Id = id += 1, Name = "Archived", Depth = 0, ParentId = null };

            // Depth 1
            var specs = new SectionModel { Id = id += 1, Name = "Specs", Depth = 1, ParentId = synced.Id };
            var archivedTests = new SectionModel { Id = id += 1, Name = "Archived Tests", Depth = 1, ParentId = archived.Id };

            // Depth 2
            var features = new SectionModel { Id = id += 1, Name = "Features", Depth = 2, ParentId = specs.Id };
            var serviceTests = new SectionModel { Id = id += 1, Name = "ServiceTests", Depth = 2, ParentId = specs.Id };

            // Depth 3
            var account = new SectionModel { Id = id += 1, Name = "Account", Depth = 3, ParentId = features.Id };
            var webApi = new SectionModel { Id = id += 1, Name = "Web API", Depth = 3, ParentId = serviceTests.Id };

            // Depth 4
            var login = new SectionModel { Id = id += 1, Name = "Login", Depth = 4, ParentId = account.Id };

            var sections = new List<SectionModel>
            {
                synced,
                archived,
                specs,
                archivedTests,
                features,
                serviceTests,
                account,
                webApi,
                login
            };

            // Depth 0
            synced.GetRootSection(sections).Should().Be(synced);
            archived.GetRootSection(sections).Should().Be(archived);

            // Depth 1
            specs.GetRootSection(sections).Should().Be(synced);
            archivedTests.GetRootSection(sections).Should().Be(archived);

            // Depth 2
            features.GetRootSection(sections).Should().Be(synced);
            serviceTests.GetRootSection(sections).Should().Be(synced);

            // Depth 3
            account.GetRootSection(sections).Should().Be(synced);
            webApi.GetRootSection(sections).Should().Be(synced);

            // Depth 4
            login.GetRootSection(sections).Should().Be(synced);
        }

        [Test]
        public void TestToDict()
        {
            var section = new SectionModel
            {
                Name = "Login",
                Description = "Description Content",
                ParentId = 1
            };

            var dict = new Dictionary<string, object>
            {
                ["name"] = "Login",
                ["description"] = "Description Content",
                ["parent_id"] = 1
            };

            section.ToDict().Should().BeEquivalentTo(dict);
        }
    }
}
