using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class CapturaInventarioDiferenciasUtileria
    {
        private int _id_inventario_captura;
        private int _id_articulo;
        private bool _existe_registro;
        private int _id_tipo_inventario;
        private string _clave;
        private string _nombre_articulo;

        private string _ubicacion;

        private string _unidad_medida;
        private decimal _cantidad_sistema;

        private decimal _entradas;
        private decimal _salidas;
        private decimal _mermas;
        private decimal _minimo;
        private decimal _maximo;
        private decimal _promedio;


        private decimal _entradas_trasp;
        private decimal _salidas_trasp;

        private decimal _entradas_ajuste;
        private decimal _salidas_ajuste;
        private decimal _entradas_devolucion;


        private decimal _ultima_cap;
        private string _color;


        public CapturaInventarioDiferenciasUtileria()
        {

        }

        public CapturaInventarioDiferenciasUtileria(int id_inventario_captura, int id_articulo, bool existe_registro, int id_tipo_inventario, string nombre_articulo, decimal cantidad_sistema, decimal entradas, decimal salidas, 
            decimal mermas, decimal entradas_trasp, decimal salidas_trasp, decimal ultima_cap, decimal minimo, decimal maximo, decimal promedio, string color, string clave, 
            string unidad_medida, string ubicacion, decimal entrada_ajuste, decimal salida_ajuste, decimal entrada_devolucion)
        {
            this._id_inventario_captura = id_inventario_captura;
            this._id_articulo = id_articulo;
            this._existe_registro = existe_registro;
            this._id_tipo_inventario = id_tipo_inventario;
            this._nombre_articulo = nombre_articulo;
            this._unidad_medida = unidad_medida;
            this._cantidad_sistema = cantidad_sistema;
            this._entradas = entradas;
            this._salidas = salidas;
            this._mermas = mermas;
            this._entradas_trasp = entradas_trasp;
            this._salidas_trasp = salidas_trasp;
            this._ultima_cap = ultima_cap;
            this._minimo = minimo;
            this._maximo = maximo;
            this._promedio = promedio;
            this._color = color;
            this._clave = clave;
            this._ubicacion = ubicacion;
            this._entradas_ajuste = entrada_ajuste;
            this._salidas_ajuste = salida_ajuste;
            this._entradas_devolucion = entrada_devolucion;
        }
        public int Id_inventario_captura { get => _id_inventario_captura; set => _id_inventario_captura = value; }
        public int Id_articulo { get => _id_articulo; set => _id_articulo = value; }
        public bool Existe_registro { get => _existe_registro; set => _existe_registro = value; }
        public int Id_tipo_inventario { get => _id_tipo_inventario; set => _id_tipo_inventario = value; }
        public string Nombre_articulo { get => _nombre_articulo; set => _nombre_articulo = value; }
        public decimal Cantidad_sistema { get => _cantidad_sistema; set => _cantidad_sistema = value; }
        public decimal Entradas { get => _entradas; set => _entradas = value; }
        public decimal Salidas { get => _salidas; set => _salidas = value; }
        public decimal Mermas { get => _mermas; set => _mermas = value; }
        public decimal Ultima_Cap { get => _ultima_cap; set => _ultima_cap = value; }
        public decimal Entradas_trasp { get => _entradas_trasp; set => _entradas_trasp = value; }
        public decimal Salidas_trasp { get => _salidas_trasp; set => _salidas_trasp = value; }
        public decimal Minimo { get => _minimo; set => _minimo = value; }
        public decimal Maximo { get => _maximo; set => _maximo = value; }
        public decimal Promedio { get => _promedio; set => _promedio = value; }
        public string Color { get => _color; set => _color = value; }
        public string Clave { get => _clave; set => _clave = value; }
        public string Unidad_medida { get => _unidad_medida; set => _unidad_medida = value; }
        public string Ubicacion { get => _ubicacion; set => _ubicacion = value; }
        public decimal Entrada_ajuste { get => _entradas_ajuste; set => _entradas_ajuste = value; }
        public decimal Salida_ajuste { get => _salidas_ajuste; set => _salidas_ajuste = value; }
        public decimal Entrada_devolucion { get => _entradas_devolucion; set => _entradas_devolucion = value; }

    }
}