using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class User
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int? Status { get; set; }

        public Guid UserId { get; set; }

        public ICollection<UserVerification> UserVerifications { get; set; } = new List<UserVerification>();

        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
