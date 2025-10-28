using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ExistenciaAlmacenArticulo
    {
        private string _almacen;
        private decimal _existencia;
        private decimal _transito;
        public ExistenciaAlmacenArticulo()
        {

        }

        public ExistenciaAlmacenArticulo(string almacen, decimal existencia, decimal transito)
        {
            _almacen = almacen;
            _existencia = existencia;
            _transito = transito;
        }


        public string almacen { get => _almacen; set => _almacen = value; }
        public decimal existencia { get => _existencia; set => _existencia = value; }
        public decimal transito { get => _transito; set => _transito = value; }
    }
}