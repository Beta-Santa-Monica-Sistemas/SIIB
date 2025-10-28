using Beta_System.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReporteSeguimientoRequisiciones
    {
        public int id_requisicion { get; set; }
        public int id_centro { get; set; }
        public string centro { get; set; }

        public string concepto { get; set; }
        public int id_tipo_requisicion { get; set; }
        public string tipo_requisicion { get; set; }
        public string color_tipo_requisicion { get; set; }
        public DateTime fecha_registro { get; set; }

        public string clave { get; set; }
        public string articulo { get; set; }
        public string observaciones { get; set; }
        public string unidad { get; set; }
        public decimal cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal total { get; set; }
        public string cuenta { get; set; }
        public int id_cuenta { get; set; }


        public int id_usuario_registra { get; set; }
        public string usuario_registra { get; set; }


        public int id_usuario_revisa { get; set; }
        public string usuario_revisa { get; set; }
        public DateTime fecha_revisa { get; set; }
        public TimeSpan tiempo_revisa { get; set; }


        public int id_usuario_autoriza { get; set; }
        public string usuario_autoriza { get; set; }
        public DateTime fecha_autoriza { get; set; }
        public TimeSpan tiempo_autoriza { get; set; }

        public string usuario_recepcion { get; set; }
        public DateTime fecha_recepcion { get; set; }
        public DateTime fecha_cotiza { get; set; }

        public int id_usuario_cotiza { get; set; }
        public string usuario_cotiza { get; set; }

        public string estatus_requisicion { get; set; }

        public int no_cotizaciones { get; set; }
        public int no_parcialidades { get; set; }
        public int no_ordenes { get; set; }
        public int no_facturas { get; set; }
        public DateTime fecha_orden { get; set; }



        public TimeSpan tiempo_cotizacion { get; set; }

        public bool solicita_autorizacion { get; set; }
        public bool orden_generada { get; set; }

        public bool recepcionada { get; set; }
        public bool importada { get; set; }

        public string ordenes_string { get; set; }
        public string facturas_string { get;set; }

        public string nombre_proveedor { get; set; }
        public decimal importe { get; set; }

        public bool anomalia { get; set; }
    }
}