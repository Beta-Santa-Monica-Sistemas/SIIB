using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReporteInversiones
    {
        private string _fecha;
        private string _justificacion;
        private string _concepto;
        private string _folio;
        private int _requisicion;
        private string _centro;
        private string _usuario_registra;
        private string _usuario_autoriza;
        private decimal _monto_inversion;
        private decimal _monto_orden;
        private decimal _diferencia;
        private int _orden_compra;
        public ReporteInversiones()
        {

        }
        public ReporteInversiones(string Fecha, string Justificacion, string Concepto, string Folio, int Requisicion, string Centro,
            string Usuario_registra, string Usuario_autoriza, decimal Monto_autorizacion, decimal Monto_inversion, decimal Diferencia, int OrdenCompra)
        {
            this._fecha = Fecha;
            this._justificacion = Justificacion;
            this._concepto = Concepto;
            this._folio = Folio;
            this._requisicion = Requisicion;
            this._centro = Centro;
            this._usuario_registra = Usuario_registra;
            this._usuario_autoriza = Usuario_autoriza;
            this._monto_inversion = Monto_autorizacion;
            this._monto_orden = Monto_inversion;
            this._diferencia = Diferencia;
            this._orden_compra = OrdenCompra;
        }
        public string Fecha { get => _fecha; set => _fecha = value; }
        public string Justificacion { get => _justificacion; set => _justificacion = value; }
        public string Concepto { get => _concepto; set => _concepto = value; }
        public string Folio { get => _folio; set => _folio = value; }
        public int Requisicion { get => _requisicion; set => _requisicion = value; }
        public string Centro { get => _centro; set => _centro = value; }
        public string Usuario_registra { get => _usuario_registra; set => _usuario_registra = value; }
        public string Usuario_autoriza { get => _usuario_autoriza; set => _usuario_autoriza = value; }
        public decimal Monto_inversion { get => _monto_inversion; set => _monto_inversion = value; }
        public decimal Monto_orden { get => _monto_orden; set => _monto_orden = value; }
        public decimal Diferencia { get => _diferencia; set => _diferencia = value; }
        public int OrdenCompra { get => _orden_compra; set => _orden_compra = value; }
    }
}