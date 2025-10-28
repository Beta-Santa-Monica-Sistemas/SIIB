using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models.GastosMensualesMS
{
    public class DOCTOS_CP_IMPORTES_PROVEEDORES
    {
        public int Docto_Cp_Id { get; set; }       // DOCTO_CP_ID
        public DateTime Fecha { get; set; }      // FECHA
        public string Clave_Prov { get; set; }    // CLAVE_PROV
        public int Proveedor_Id { get; set; }     // PROVEEDOR_ID
        public string Descripcion { get; set; }  // DESCRIPCION
        public string Cuenta_Concepto { get; set; } // CUENTA_CONCEPTO
        public string Folio { get; set; }        // FOLIO
        public string Cuenta { get; set; }        // NO_CUENTA
        public string Nombre_Proveedor { get; set; }
        public decimal Importe { get; set; }
        public decimal Tipo_Cambio { get; set; }

        public string folio_requisicion { get; set; }
        public string observaciones { get; set; }
        public DateTime fecha_requisicion { get; set; }
    }
}