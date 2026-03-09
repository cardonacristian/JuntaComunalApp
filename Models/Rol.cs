using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuntaComunalApp.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string NombreRol {  get; set; }

        public override string ToString() => NombreRol;

    }
}
