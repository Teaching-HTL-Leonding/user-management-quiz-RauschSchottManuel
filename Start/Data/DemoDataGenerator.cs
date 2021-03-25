using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserManagement.Data
{
    /// <summary>
    /// Provides methods for filling the database with demo data
    /// </summary>
    public class DemoDataGenerator
    {
        private readonly UserManagementDataContext dc;

        public DemoDataGenerator(UserManagementDataContext dc)
        {
            this.dc = dc;
        }

        /// <summary>
        /// Delete all data in the database
        /// </summary>
        /// <returns></returns>
        public async Task ClearAll()
        {
            dc.Users.RemoveRange(await dc.Users.ToArrayAsync());
            dc.Groups.RemoveRange(await dc.Groups.ToArrayAsync());
            await dc.SaveChangesAsync();
        }

        /// <summary>
        /// Fill database with demo data
        /// </summary>
        public async Task Fill()
        {
            #region Add some users
            User foo, john, jane;
            dc.Users.Add(foo = new User
            {
                NameIdentifier = "foo.bar",
                FirstName = "Foo",
                LastName = "Bar",
                Email = "foo.bar@acme.corp"
            });

            dc.Users.Add(john = new User
            {
                NameIdentifier = "john.doe",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@acme.corp"
            });

            dc.Users.Add(jane = new User
            {
                NameIdentifier = "jane.doe",
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@acme.corp"
            });
            #endregion

            #region Add some groups
            Group johnAndFamily, lonely, emptyGroup, child;
            dc.Groups.Add(johnAndFamily = new Group
            {
                Name = "John and Family",
                Members = new List<User>() { john,jane }
            });

            dc.Groups.Add(lonely = new Group
            {
                Name = "Lonely Group",
                Members = new List<User> { jane }
            });

            dc.Groups.Add(emptyGroup = new Group
            {
                Name = "Empty Group"
            });

            dc.Groups.Add(child = new Group
            {
                Name = "ChildGroup",
                Members = new List<User> { john, jane},
                ParentGroup = emptyGroup
            });

            dc.Groups.Add(new Group
            {
                Name = "Nested 2",
                Members = new List<User> { foo },
                ParentGroup = child
            });

            dc.Groups.Add(new Group
            {
                Name = "childGroup1",
                ParentGroup = lonely
            });

            dc.Groups.Add(new Group
            {
                Name = "Nested 4",
                ParentGroup = child
            });
            #endregion

            await dc.SaveChangesAsync();
        }
    }
}
