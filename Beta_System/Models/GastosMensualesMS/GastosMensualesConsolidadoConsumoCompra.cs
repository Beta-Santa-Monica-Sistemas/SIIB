using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models.GastosMensualesMS
{
    public class GastosMensualesConsolidadoConsumoCompra
    {
        //-------- DETALLE COMPRA DIRECTA
        public int Docto_Cp_Id { get; set; }       // DOCTO_CP_ID
        public DateTime Fecha { get; set; }      // FECHA
        public string Clave_Prov { get; set; }    // CLAVE_PROV
        public int Proveedor_Id { get; set; }     // PROVEEDOR_ID
        public string Descripcion { get; set; }  // DESCRIPCION
        public string Cuenta_Concepto { get; set; } // CUENTA_CONCEPTO
        public string Folio { get; set; }        // FOLIO
        public string Cuenta { get; set; }        // Cuenta
        public string Nombre_Proveedor { get; set; }
        public decimal Importe { get; set; }
        public decimal Tipo_Cambio { get; set; }

        public string Folio_requisicion { get; set; }
        public string Observaciones { get; set; }
        public DateTime Fecha_requisicion { get; set; }



        //-------------DETALLE CONSUMOS
        public string Concepto { get; set; }
        public string CargoContable { get; set; }
        public string Clave_Art { get; set; }
        public string Articulo { get; set; }
        public Decimal Cantidad { get; set; }
        public Decimal Costo { get; set; }
        public Decimal Importe_articulo { get; set; }
        public int Id_cuenta_contable { get; set; }
        public string Almacen { get; set; }
        public string Usuario_registra { get; set; }


    }
}