using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        public int PersonId { get; set; }
        public string Username { get; set; }
        public DateTime RegistrationDate { get; set; }
        public Person Person { get; set; }
        public string NombrePersona { get; set; }

    }
}
