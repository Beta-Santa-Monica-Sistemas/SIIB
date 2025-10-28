using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ConexionMS
    {
        private string Servidor;
        private string Puerto;
        private string Usuario;
        private string Clave;
        private string BaseDatos;
        private static ConexionMS Con = null;

        public ConexionMS()
        {
            this.Servidor = "Beta2";
            this.Puerto = "3050";
            this.Usuario = "SYSDBA";
            this.Clave = "masterkey";
            //this.BaseDatos = "C:\\Microsip datos\\BETA SANTA MONICA PRUEBA.fdb";
            this.BaseDatos = "C:\\Microsip datos\\BETA SANTA MONICA.fdb";
        }

        public FbConnection CrearConexion()
        {
            FbConnection Cadena = new FbConnection();
            try
            {
                Cadena.ConnectionString = "User =" + this.Usuario +
                                            ";Password=" + this.Clave +
                                            ";Database=" + this.BaseDatos +
                                            ";DataSource=" + this.Servidor +
                                            ";Port=" + this.Puerto +
                                            ";Dialect=3" +
                                            ";Charset=NONE;";
            }
            catch (Exception ex)
            {
                Cadena = null;
                string msj = ex.ToString();
            }
            return Cadena;
        }
        public static ConexionMS GetConexionMS()
        {
            if (Con == null)
            {
                Con = new ConexionMS();
            }
            return Con;
        }

    }
}