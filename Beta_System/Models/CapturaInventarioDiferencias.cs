using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class CapturaInventarioDiferencias
    {
        private int _id_articulo;
        private string _clave;
        private string _nombre_articulo;

        private string _almacen;
        private string _tipo_articulo;
        private string _clasificacion_articulo;

        private string _ubicacion;
        private string _unidad_medida;
        private int _acepta_decimales;

        private decimal _entradas;
        private decimal _salidas;

        private decimal _entradas_trasp;
        private decimal _salidas_trasp;

        private decimal _entradas_devolucion;
        private decimal _entradas_ajust;
        private decimal _salidas_ajust;

        private decimal _mermas;
        private decimal _minimo;
        private decimal _maximo;
        private decimal _promedio;

        private decimal _cantidad_sistema;

        private string _fecha_ultimo_inv;
        private decimal _ultima_cap;
        private string _color;

        private decimal _precio;
        private decimal _total;

        private List<string> _meses_consumo_nombre;
        private List<decimal> _meses_consumo_valor;


        public CapturaInventarioDiferencias()
        {

        }

        public CapturaInventarioDiferencias(int id_articulo, string nombre_articulo, decimal cantidad_sistema, decimal entradas, decimal salidas, decimal mermas, decimal entradas_trasp, decimal salidas_trasp, 
            string fecha_ultimo_inv, decimal ultima_cap, decimal minimo, decimal maximo, decimal promedio, string color, string clave, string unidad_medida, string ubicacion, decimal precio, decimal total, string almacen, 
            string tipo_articulo, string clasificacion_articulo, decimal entradas_devolucion, decimal entradas_ajuste, decimal salidas_ajuste, List<string> meses_consumo_nombre, List<decimal> meses_consumo_valor,
            int acepta_decimal)
        {
            this._id_articulo = id_articulo;
            this._nombre_articulo = nombre_articulo;
            this._unidad_medida = unidad_medida;
            this._cantidad_sistema = cantidad_sistema;
            this._entradas = entradas;
            this._salidas = salidas;
            this._mermas = mermas;
            this._entradas_trasp = entradas_trasp;
            this._salidas_trasp = salidas_trasp;
            this._fecha_ultimo_inv = fecha_ultimo_inv;
            this._ultima_cap = ultima_cap;
            this._minimo = minimo;
            this._maximo = maximo;
            this._promedio = promedio;
            this._color = color;
            this._clave = clave;
            this._ubicacion = ubicacion;
            this._precio = precio;
            this._total = total;
            this._almacen = almacen;
            this._tipo_articulo = tipo_articulo;
            this._clasificacion_articulo = clasificacion_articulo;
            this._entradas_devolucion = entradas_devolucion;
            this._entradas_ajust = entradas_ajuste;
            this._salidas_ajust = salidas_ajuste;
            this._meses_consumo_nombre = meses_consumo_nombre;
            this._meses_consumo_valor = meses_consumo_valor;
            this._acepta_decimales = acepta_decimal;
        }

        public int Id_articulo { get => _id_articulo; set => _id_articulo = value; }
        public string Nombre_articulo { get => _nombre_articulo; set => _nombre_articulo = value; }
        public decimal Cantidad_sistema { get => _cantidad_sistema; set => _cantidad_sistema = value; }
        public decimal Entradas { get => _entradas; set => _entradas = value; }
        public decimal Salidas { get => _salidas; set => _salidas = value; }
        public decimal Mermas { get => _mermas; set => _mermas = value; }
        public string Fecha_ultimo_inv { get => _fecha_ultimo_inv; set => _fecha_ultimo_inv = value; }
        public decimal Ultima_Cap { get => _ultima_cap; set => _ultima_cap = value; }
        public decimal Entradas_trasp { get => _entradas_trasp; set => _entradas_trasp = value; }
        public decimal Salidas_trasp { get => _salidas_trasp; set => _salidas_trasp = value; }

        public decimal Entradas_ajuste { get => _entradas_ajust; set => _entradas_ajust = value; }
        public decimal Entradas_devolucion { get => _entradas_devolucion; set => _entradas_devolucion = value; }
        public decimal Salidas_ajuste { get => _salidas_ajust; set => _salidas_ajust = value; }


        public decimal Minimo { get => _minimo; set => _minimo = value; }
        public decimal Maximo { get => _maximo; set => _maximo = value; }
        public decimal Promedio { get => _promedio; set => _promedio = value; }
        public string Color { get => _color; set => _color = value; }
        public string Clave { get => _clave; set => _clave = value; }
        public string Unidad_medida { get => _unidad_medida; set => _unidad_medida = value; }
        public string Ubicacion { get => _ubicacion; set => _ubicacion= value; }

        public decimal Precio { get => _precio; set => _precio = value; }
        public decimal Total { get => _total; set => _total = value; }
        public string Almacen { get => _almacen; set => _almacen = value; }
        public string Tipo_articulo { get => _tipo_articulo; set => _tipo_articulo = value; }
        public string Clasificacion_articulo { get => _clasificacion_articulo; set => _clasificacion_articulo = value; }

        public List<string> Meses_consumo_nombre { get => _meses_consumo_nombre; set => _meses_consumo_nombre = value; }
        public List<decimal> Meses_consumo_valor { get => _meses_consumo_valor; set => _meses_consumo_valor = value; }
        public int Acepta_decimales { get => _acepta_decimales; set => _acepta_decimales = value; }
    
    
    }
}