using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace Beta_System.Models
{
    public class ReporteChecadorNomina
    {
        public int id_punches_e { get; set; }
        public int id_punches_s { get; set; }
        public int id_empleado { get; set; }
        public string nombre_empleado { get; set; }
        public int id_departamento { get; set; }
        public string nombre_departamento { get; set; }
        public string nombre_dia { get; set; }
        public int no_dia { get; set; }
        public DateTime fecha_punch { get; set; }
        public DateTime fecha_punch_e { get; set; }
        public DateTime fecha_punch_s { get; set; }
        public bool falta { get; set; }
        public int id_checador_concepto { get; set; }
        public string nombre_concepto { get; set; }


        public decimal valor_hra { get; set; }
        public TimeSpan hrs_totales { get; set; }


        public int hrs_extra { get; set; }
        public decimal hrs_extra_valor { get; set; }


        public int hrs_estimulo { get; set; }
        public decimal hrs_estimulo_valor { get; set; }

        public bool pagar_hrs_extra { get; set; }
        public bool pagar_hrs_estimulo { get; set; }

        public bool concepto_modificado { get; set; }

    }
}