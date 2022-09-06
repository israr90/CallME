using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace foneMeService.Identity
{
    public class IdentityRole : IRole<Guid>
    {
        public IdentityRole()
        {
            Id = new Guid();
        }

        public IdentityRole(string name)
            : this()
        {
            this.Name = name;
        }

        public IdentityRole(string name, Guid id)
        {
            this.Name = name;
            this.Id = id;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}