using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class TransitoArticulo
    {
        private string _clave;
        private string _articulo;
        private int _requisiciones;
        private decimal _cotizacion;
        private decimal _confirmacion;
        private decimal _parcialidad;
        private decimal _precepcion;

        private string _registrado;
        private DateTime _fecha;
        private string _cargo;
        private string _cuenta;
        private string _centro;
        private string _familia;
        public TransitoArticulo()
        {

        }

        public TransitoArticulo(string clave, string articulo, int requisicion, decimal cotizacion, decimal confirmacion, decimal parcialidad, decimal precepcion,
            string registrado, DateTime fecha, string cargo, string cuenta, string centro, string familia)
        {
            _clave = clave;
            _articulo = articulo;
            _requisiciones = requisicion;
            _cotizacion = cotizacion;
            _confirmacion = confirmacion;
            _parcialidad = parcialidad;
            _precepcion = precepcion;


            _registrado = registrado;
            _fecha = fecha;
            _cargo = cargo;
            _cuenta = cuenta;
            _centro = centro;
            _familia = familia;
        }


        public string clave { get => _clave; set => _clave = value; }
        public string articulo { get => _articulo; set => _articulo = value; }
        public int requisicion { get => _requisiciones; set => _requisiciones = value; }
        public decimal cotizacion { get => _cotizacion; set => _cotizacion = value; }
        public decimal confirmacion { get => _confirmacion; set => _confirmacion = value; }
        public decimal parcialidad { get => _parcialidad; set => _parcialidad = value; }
        public decimal precepcion { get => _precepcion; set => _precepcion = value; }

        public string registrado { get => _registrado; set => _registrado = value; }
        public DateTime fecha { get => _fecha; set => _fecha = value; }
        public string cargo { get => _cargo; set => _cargo = value; }
        public string cuenta { get => _cuenta; set => _cuenta = value; }
        public string centro { get => _centro; set => _centro = value; }
        public string familia { get => _familia; set => _familia = value; }
    }
}