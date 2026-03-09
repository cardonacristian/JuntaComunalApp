using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuntaComunalApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        private string _username;
        public string Username
        {
            get => _username; 
            set => _username = value?.Trim();
        }
        public string Password { get; set; }
        public int IdRol {  get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }

    }
}
