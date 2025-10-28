using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReportePolizasAlmacenCuentasContables
    {
        private string _cuenta;
        private string _cuenta_contable;
        private string _cargo_contable;
        private decimal _importe_entrada;
        private decimal _importe_salida;
        private decimal _importe_total;
        private decimal _presupuesto;
        private decimal _disponible;
        private int _id_cargo_contable;
        private int _id_cuenta_contable;
        public ReportePolizasAlmacenCuentasContables()
        {

        }
        public ReportePolizasAlmacenCuentasContables(string cuenta, string cuenta_contable, string cargo_contable, decimal importe_entrada, decimal importe_salida, 
                                                    decimal importe_total, decimal presupuesto, decimal disponible, int id_cargo_contable, int id_cuenta_contable)
        {
            this._cuenta = cuenta;
            this._cuenta_contable = cuenta_contable;
            this._cargo_contable = cargo_contable;
            this._importe_entrada = importe_entrada;
            this._importe_salida = importe_salida;
            this._importe_total = importe_total;
            this._presupuesto = presupuesto;
            this._disponible = disponible;
            this._id_cargo_contable = id_cargo_contable;
            this._id_cuenta_contable = id_cuenta_contable;
        }
        public string Cuenta { get => _cuenta; set => _cuenta = value; }
        public string CuentaContable { get => _cuenta_contable; set => _cuenta_contable = value; }
        public string CargoContable { get => _cargo_contable; set => _cargo_contable = value; }
        public Decimal Importe_salida { get => _importe_salida; set => _importe_salida = value; }
        public Decimal Importe_entrada { get => _importe_entrada; set => _importe_entrada = value; }
        public Decimal Importe_total { get => _importe_total; set => _importe_total = value; }
        public Decimal Presupuesto { get => _presupuesto; set => _presupuesto= value; }
        public Decimal Disponible { get => _disponible; set => _disponible = value; }
        public int Id_cargo_contable { get => _id_cargo_contable; set => _id_cargo_contable = value; }
        public int Id_cuenta_contable { get => _id_cuenta_contable; set => _id_cuenta_contable = value; }
    }
}