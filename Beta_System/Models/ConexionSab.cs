using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Beta_System.Models
{
    public class ConexionSab
    {
        public static SqlConnection Conectar()
        {
            //SqlConnection conexion = new SqlConnection("Server=localhost; Initial Catalog=base_forrajes; User ID=sab; Password=12345;");
            SqlConnection conexion = new SqlConnection("Server=192.168.128.1; Initial Catalog=base_forrajes; User ID=beta; Password=12345;");
            //SqlConnection conexion = new SqlConnection("Server=localhost; Initial Catalog=base_forrajes; Integrated Security=True;");
            conexion.Open();
            return conexion;
        }

        public static SqlConnection ConectarMasterBeta()
        {
            //SqlConnection conexion = new SqlConnection("Server=localhost; Initial Catalog=base_forrajes; User ID=sab; Password=12345;");
            SqlConnection conexion = new SqlConnection("Server=192.168.128.1; Initial Catalog=master_beta; User ID=beta; Password=12345;");
            conexion.Open();
            return conexion;
        }

        public static SqlConnection ConectarPrueba()
        {
            SqlConnection conexion = new SqlConnection("Server=127.0.0.1; Initial Catalog=base_forrajes; User ID=sab; Password=12345;");
            conexion.Open();
            return conexion;
        }

        public static SqlConnection ConectarPtto()
        {
            SqlConnection conexion = new SqlConnection("Server=192.168.128.1; Initial Catalog=Master_SIIB; User ID=beta; Password=12345;");
            conexion.Open();
            return conexion;
        }



        public class No_Pozos
        {
            public List<int> ID { get; set; }
            public List<string> No_pozo { get; set; }

            public No_Pozos()
            {
                ID = new List<int>();
                No_pozo = new List<string>();
            }
        }
        public class Lineas
        {
            public List<int> ID_Linea { get; set; }
            public List<string> No_linea { get; set; }

            public Lineas()
            {
                ID_Linea = new List<int>();
                No_linea = new List<string>();
            }
        }
        public class Proveedores
        {
            public List<string> ID_Proveedor { get; set; }
            public List<string> No_proveedor { get; set; }

            public Proveedores()
            {
                ID_Proveedor = new List<string>();
                No_proveedor = new List<string>();
            }
        }
        public class Articulos_Sab
        {
            public List<string> ID_Articulo { get; set; }
            public List<string> No_articulo { get; set; }

            public Articulos_Sab()
            {
                ID_Articulo = new List<string>();
                No_articulo = new List<string>();
            }
        }
    }
}