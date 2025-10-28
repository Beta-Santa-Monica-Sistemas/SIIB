using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReportePolizasAlmacenCuentasContablesDetalle
    {
        private int _id_cargo_contable;
        private string _cargo_contable;
        private string _mov;
        private string _folio;
        private string _fecha;
        private string _clave;
        private string _articulo;
        private string _medida;
        private decimal _cantidad;
        private decimal _precio;
        private decimal _importe;
        private string _cuenta;
        private string _cuenta_contable;
        private string _movimiento;
        private string _requisicion;
        private string _remision;
        private string _proveedor;
        private string _trabajador;
        private string _equipo;
        private string _departamento;
        private string _centro;
        private string _almacen;

        public ReportePolizasAlmacenCuentasContablesDetalle()
        {

        }
        public ReportePolizasAlmacenCuentasContablesDetalle(int id_cargo_contable, string cargo_contable, string mov, string folio,
            string fecha, string clave, string articulo, string medida, decimal cantidad, decimal precio,
            decimal importe, string cuenta, string cuenta_contable, string movimiento, string requisicion,
            string remision, string proveedor, string trabajador, string equipo, string departamento, string centro, string almacen)
        {
            this._id_cargo_contable = id_cargo_contable;
            this._cargo_contable = cargo_contable;
            this._mov = mov;
            this._folio = folio;
            this._fecha = fecha;
            this._clave = clave;
            this._articulo = articulo;
            this._medida = medida;
            this._cantidad = cantidad;
            this._precio = precio;
            this._importe = importe;
            this._cuenta = cuenta;
            this._cuenta_contable = cuenta_contable;
            this._movimiento = movimiento;
            this._requisicion = requisicion;
            this._remision = remision;
            this._proveedor = proveedor;
            this._trabajador = trabajador;
            this._equipo = equipo;
            this._departamento = departamento;
            this._centro = centro;
            _almacen = almacen;
        }
        public int Id_cargo_contable { get => _id_cargo_contable; set => _id_cargo_contable = value; }
        public string Cargo_Contable { get => _cargo_contable; set => _cargo_contable = value; }
        public string Mov { get => _mov; set => _mov = value; }
        public string Folio { get => _folio; set => _folio = value; }
        public string Fecha { get => _fecha; set => _fecha = value; }
        public string Clave { get => _clave; set => _clave = value; }
        public string Articulo { get => _articulo; set => _articulo = value; }
        public string Medida { get => _medida; set => _medida = value; }
        public Decimal Cantidad { get => _cantidad; set => _cantidad = value; }
        public Decimal Precio { get => _precio; set => _precio = value; }
        public Decimal Importe { get => _importe; set => _importe = value; }
        public string Cuenta { get => _cuenta; set => _cuenta = value; }
        public string Cuenta_Contable { get => _cuenta_contable; set => _cuenta_contable = value; }
        public string Movimiento { get => _movimiento; set => _movimiento = value; }
        public string Requisicion { get => _requisicion; set => _requisicion = value; }
        public string Remision { get => _remision; set => _remision = value; }
        public string Proveedor { get => _proveedor; set => _proveedor = value; }
        public string Trabajador { get => _trabajador; set => _trabajador = value; }
        public string Equipo { get => _equipo; set => _equipo = value; }
        public string Departamento { get => _departamento; set => _departamento = value; }
        public string Centro { get => _centro; set => _centro = value; }
        public string Almacen { get => _almacen; set => _almacen = value; }

    }
}