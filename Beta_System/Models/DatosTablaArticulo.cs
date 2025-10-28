using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class DatosTablaArticulo
    {
        public int identificador { get; set; }
        public int id_articulo { get; set; }
        public string nombre_articulo { get; set; }
        public string descripcion_articulo { get; set; }
        public string unidad_medida { get; set; }
        public string nombre_tipo { get; set; }
        public string nombre_clasificacion { get; set; }
        public string nombre_linea { get; set; }
        public bool almacenable { get; set; }
        public double precio { get; set; }
        public bool activo { get; set; }
    }
}