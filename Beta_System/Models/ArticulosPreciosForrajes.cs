using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ArticulosPreciosForrajes
    {
        public C_articulos_catalogo Articulos { get; set; }
        public C_alimentacion_articulos_precios Precios { get; set; }
    }
}