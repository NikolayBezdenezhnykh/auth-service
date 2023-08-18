using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    internal class UserRegisterMessage
    {
        public string Email { get; set; }

        public Guid UserId { get; set; }

        public string EmailTo { get; set; }

        public string Template { get; set; }

        public Dictionary<string, string> DynamicParams { get; set; }
    }
}
