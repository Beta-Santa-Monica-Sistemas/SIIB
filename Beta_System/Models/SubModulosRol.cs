using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class SubModulosRol
    {

        public virtual C_modulos c_modulos { get; set; }
        public virtual C_modulos_sub c_modulos_sub { get; set; }
        public virtual C_modulos_sub_permisos c_modulos_sub_perm { get; set; }
    }
}