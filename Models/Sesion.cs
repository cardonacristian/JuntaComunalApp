using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuntaComunalApp.Models
{
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }

        public static bool EsAdmin => UsuarioActual?.Rol == "ADMINISTRADOR";

        public static void CerrarSesion() => UsuarioActual = null;
    }
}
