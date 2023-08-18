using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class UserVerification
    {
        public long Id { get; set; }

        public User User { get; set; }

        public string Code { get; set; }

        public DateTime? ExpiresDateAt { get; set; }

        public bool? IsCompleted { get; set; }
    }
}
