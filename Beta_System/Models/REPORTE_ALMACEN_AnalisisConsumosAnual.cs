using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
	public class REPORTE_ALMACEN_AnalisisConsumosAnual
    {
        public string Clave { get; set; }
        public int id_articulo { get; set; }
        public string Nombre_articulo { get; set; }
        public string Unidad_medida { get; set; }
        public string Tipo { get; set; }
        public int No_mes { get; set; }
        public string Mes { get; set; }
        public decimal Cantidad { get; set; }
        public string Almacen { get; set; }


        public string Proveedor { get; set; }
        public string Ubicacion { get; set; }
        public string ExistenciaActual { get; set; }
        public string Precio { get; set; }
        public string CostoInventario { get; set; }
        public decimal PromedioMensual { get; set; }
        public decimal DiasInventario { get; set; }

    }
}