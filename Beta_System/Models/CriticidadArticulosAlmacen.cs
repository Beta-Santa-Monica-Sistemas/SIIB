using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class CriticidadArticulosAlmacen
    {
        public C_articulos_catalogo Articulos { get; set; }
        public C_almacen_criticidad_articulos Criticidad { get; set; }

    }
}