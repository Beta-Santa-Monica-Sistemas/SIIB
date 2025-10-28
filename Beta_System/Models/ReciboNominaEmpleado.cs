using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReciboNominaEmpleado
    {
        public C_nomina_empleados data_empleado { get; set; }
        public int id_nomina { get; set; }
        public int id_nomina_det { get; set; }

        public string nombre { get; set; }
        public string puesto { get; set; }
        public string departamento { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public string Domicilio_fisc { get; set; }
        public string Regimn_fisc { get; set; }

        public string XML { get; set; }
        public string forma_pago { get; set; }
        public string frec_pago { get; set; }
        public string fecha_pago { get; set; }
        public string fecha_inicio { get; set; }
        public string fecha_fin { get; set; }

        public string registro_nss { get; set; }
        public string tipo_salario { get; set; }
        public decimal salario_integrado { get; set; }
        public string jornada { get; set; }
        public string fecha_ingreso { get; set; }

        public string naturaleza_concepto { get; set; }
        public string concepto { get; set; }
        public decimal monto { get; set; }
        public decimal unidades { get; set; }
        public decimal saldo_prestamo { get; set; }


        public decimal total_percepciones { get; set; }
        public decimal total_retenciones { get; set; }
        public decimal pago { get; set; }


        public int id_cfdi { get; set; }
        public string UUID { get; set; }
        public string regimen_fiscal_sat { get; set; }
        public string folio_sat { get; set; }
        public string lugar_sat { get; set; }
        public string certificado_emisor_sat { get; set; }
        public string certificado_cfdi { get; set; }
        public string sello_cfdi { get; set; }
        public string noCertificado_cfdi { get; set; }
        public string folio_cfdi { get; set; }
        public string codigo_qr { get; set; }
        public string cadena_cfdi { get; set; }
        public string sello_digital_cfdi { get; set; }
        public string sello_digital_sat { get; set; }

        public string fecha_periodo{ get; set; }
        public string fecha_emision_sat { get; set; }
        public string fecha_certificacion { get; set; }



    }
}