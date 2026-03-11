using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuntaComunalApp.Models
{
    public static class Conexion
    {
        public static string ObtenerCadena()
        {
            
            string carpetaSistema = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            
            string rutaCarpetaApp = Path.Combine(carpetaSistema, "JuntaComunalApp");

            
            if (!Directory.Exists(rutaCarpetaApp))
            {
                Directory.CreateDirectory(rutaCarpetaApp);
            }

            
            string rutaFinalDB = Path.Combine(rutaCarpetaApp, "JuntaComunal.db");

            if(!File.Exists(rutaFinalDB))
            {
                string rutaLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JuntaComunal.db");
                if(File.Exists(rutaLocal))
                {
                    File.Copy(rutaLocal, rutaFinalDB);
                }
            }

            
            return $"Data Source={rutaFinalDB};Version=3;";
        }
    }
}
