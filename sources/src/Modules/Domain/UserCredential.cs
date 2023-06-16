using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class UserCredential
    {
        public long Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}
