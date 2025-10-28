using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class UsuariosAlmacenes
    {
        public virtual C_almacen_almacenes_g c_almacenes { get; set; }
        public virtual C_almacen_almacenes_usuarios c_almacen_almacenes_usuarios { get; set; }
    }
}