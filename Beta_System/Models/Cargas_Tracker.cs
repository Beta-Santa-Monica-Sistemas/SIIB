using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class Cargas_Tracker
    {
        public string cve_mov { get; set; }
        public int id_carga { get; set; }
        public int id_barch { get; set; }
        public int racion_id { get; set; }
        public string racion_cve { get; set; }
        public string racion { get; set; }
        public string line_status { get; set; }
        public string corral { get; set; }
        public string ing_cve { get; set; }
        public int ing_id { get; set; }
        public string ing_nombre { get; set; }
        public string ing_tipo { get; set; }
        public int animales { get; set; }
        public decimal peso_cargado { get; set; }
        public decimal peso_real { get; set; }
        public decimal deiferencia { get; set; }
        public int id_mezclador { get; set; }
        public string nombre_mezclador { get; set; }
        public int id_operador { get; set; }
        public string nombre_operador { get; set; }
        public decimal PMS_P { get; set; }
        public decimal PMS_R { get; set; }
        public decimal precio { get; set; }
        public DateTime fecha_registro { get; set; }
    }
}