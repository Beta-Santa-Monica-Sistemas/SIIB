using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class QuickViewArticulo
    {
        public C_articulos_catalogo articulo_info { get; set; }
        public List<C_compras_ordenes_d> Ultimas_compras  { get; set; }
        public string Existencia { get; set; }
        public string Ubicacion { get; set; }
        public decimal PrecioActual { get; set; }
        public string Fecha_ult_inventario { get; set; }
        public C_inventario_almacen_establo_maximo_minimo Maximos_Minimos { get; set; }
        public List<C_compras_requi_d> Articulos_Pendientes { get; set; }


    }
}