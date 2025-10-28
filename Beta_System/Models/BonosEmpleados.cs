using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class BonosEmpleados
    {
        public C_evaluaciones_empleados_bonos Bonos { get; set; }
        public C_nomina_empleados Empleados { get; set; }
        public C_evaluaciones_departamentos Departamentos { get; set; }

    }
}