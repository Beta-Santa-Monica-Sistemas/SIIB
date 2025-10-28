using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Beta_System.Models
{
    public class ResumenPresupuestoCuentaRequi
    {
        private string _cargo_contable;
        private string _cuenta;
        private string _cargo;
        private string _nombre_cuenta;
        private decimal _presupuesto;
        private decimal _ejercido;
        private decimal _proceso;
        private decimal _disponible;
        private decimal _requisicion;
        private decimal _resultado;
        private decimal _pendiente;
        private decimal _diferencia;
        private int _id_tipo_requi;
        private int _id_requi_g;

        private decimal _porcentaje_tolerancia;
        private decimal _monto_tolerancia;

        private decimal _valor_dolar;
        private string _clave_fiscal;

        private bool _cta_auth;
        private int _id_cuenta;

        public ResumenPresupuestoCuentaRequi()
        {

        }

        public ResumenPresupuestoCuentaRequi(string cargo_contable, string cuenta, string nombrecuenta, decimal presupuesto, decimal ejercido, decimal proceso, decimal disponible,
            decimal requisicion, decimal resultado, decimal pendiente, decimal diferencia, string cargo, int id_tipo_requi, int id_requi_g, decimal porcentaje_tolerancia, decimal monto_tolerancia,
            decimal valor_dolar, string clave_fiscal, bool cta_ath, int id_cuenta)
        {
            this._cargo_contable = cargo_contable;
            this._cuenta = cuenta;
            this._nombre_cuenta = nombrecuenta;
            this._presupuesto = presupuesto;
            this._ejercido = ejercido;
            this._proceso = proceso;
            this._disponible = disponible;
            this._requisicion = requisicion;
            this._resultado = resultado;
            this._pendiente = pendiente;
            this._diferencia = diferencia;
            this._cargo = cargo;
            this._id_tipo_requi = id_tipo_requi;
            this._id_requi_g = id_requi_g;
            this._porcentaje_tolerancia = porcentaje_tolerancia;
            this._monto_tolerancia = monto_tolerancia;
            this._valor_dolar = valor_dolar;
            this._clave_fiscal = clave_fiscal;
            this._cta_auth = cta_ath;
            this._id_cuenta = id_cuenta;
        }   
        public string Cargo_contable { get => _cargo_contable; set => _cargo_contable = value; }
        public string Cuenta { get => _cuenta; set => _cuenta = value; }
        public string Cargo { get => _cargo; set => _cargo = value; }
        public string Nombre_cuenta { get => _nombre_cuenta; set => _nombre_cuenta = value; }
        public decimal Presupuesto { get => _presupuesto; set => _presupuesto = value; }
        public decimal Ejercido { get => _ejercido; set => _ejercido = value; }
        public decimal Proceso { get => _proceso; set => _proceso = value; }
        public decimal Disponible { get => _disponible; set => _disponible = value; }
        public decimal Requisicion { get => _requisicion; set => _requisicion = value; }
        public decimal Resultado { get => _resultado; set => _resultado = value; }
        public decimal Pendiente { get => _pendiente; set => _pendiente = value; }
        public decimal Diferencia { get => _diferencia; set => _diferencia = value; }
        public int Id_tipo_requisicion { get => _id_tipo_requi; set => _id_tipo_requi = value; }
        public int Id_requi_g { get => _id_requi_g; set => _id_requi_g = value; }

        public decimal Porcentaje_tolerancia { get => _porcentaje_tolerancia; set => _porcentaje_tolerancia = value; }
        public decimal Monto_tolerancia { get => _monto_tolerancia; set => _monto_tolerancia = value; }
        public decimal Valor_dolar { get => _valor_dolar; set => _valor_dolar = value; }
        public string Clave_fiscal { get => _clave_fiscal; set => _clave_fiscal = value; }
        public bool Cta_auth { get => _cta_auth; set => _cta_auth = value; }
        public int Id_cuenta { get => _id_cuenta; set => _id_cuenta = value; }

    }
}