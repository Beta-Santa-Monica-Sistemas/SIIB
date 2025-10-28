using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ConexionDinamicaMS
    {

        private string Servidor;
        private string Puerto;
        private string Usuario;
        private string Clave;
        private string BaseDatos;
        private static ConexionDinamicaMS Con = null;

        public ConexionDinamicaMS(string Usuario, string Clave, string BaseDatos, string Servidor, string Puerto)
        {
            this.Servidor = Servidor;
            this.Puerto = Puerto;
            this.Usuario = Usuario;
            this.Clave = Clave;
            this.BaseDatos = BaseDatos;
        }

        public FbConnection CrearConexion(string Usuario, string Clave, string BaseDatos, string Servidor, string Puerto)
        {
            FbConnection Cadena = new FbConnection();
            try
            {
                Cadena.ConnectionString = "User =" + Usuario +
                                            ";Password=" + Clave +
                                            ";Database=" + BaseDatos +
                                            ";DataSource=" + Servidor +
                                            ";Port=" + Puerto +
                                            ";Dialect=3;" +
                                            ";Charset=NONE;";

            }
            catch (Exception ex)
            {
                Cadena = null;
                string msj = ex.ToString();
            }
            return Cadena;
        }

        public static ConexionDinamicaMS GetConexionMS(string Usuario, string Clave, string BaseDatos, string Servidor, string Puerto)
        {
            if (Con == null)
            {
                Con = new ConexionDinamicaMS(Usuario, Clave, BaseDatos, Servidor, Puerto);
            }
            return Con;
        }
    }
}