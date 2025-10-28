using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
	public class ConexionSIIB
	{
        public static SqlConnection Conectar()
        {
            //SqlConnection conexion = new SqlConnection("Server=localhost; Initial Catalog=BETA_CORP; User ID=sa; Password=12345;");
            SqlConnection conexion = new SqlConnection("Server=192.168.128.2; Initial Catalog=BETA_CORP; User ID=sa; Password=12345;");
            conexion.Open();
            return conexion;
        }

    }
}