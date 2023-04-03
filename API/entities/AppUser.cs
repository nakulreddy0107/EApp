using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public byte[] passwordHash { get; set; }

        public byte[] passwordSalt { get; set; }
    }
}