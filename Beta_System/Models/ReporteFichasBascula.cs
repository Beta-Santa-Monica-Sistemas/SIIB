using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReporteFichasBascula
    {


        private string _ficha;
        private string _folio;
        private string _articulo;
        private string _cliente;
        private DateTime _fecha1;
        private DateTime _fecha2;
        private string _linea;
        private string _chofer;
        private string _placas;
        private decimal _p1;
        private decimal _p2;
        private decimal _pt;
        private decimal _materia;
        private string _maquilador;
        private string _pozo;
        private string _observacion;
        private string _movimiento;
        private string _tmovimiento;
        private decimal _origen;

        private string _tabla;
        private string _variedad;
        private decimal _corte;
        private decimal _pacas;
        private string _ensilador;


        private string _establo;
        private int _idestablo;
        public ReporteFichasBascula()
        {

        }
        public ReporteFichasBascula(string ficha, string folio, string articulo, string cliente,
            DateTime fecha1, DateTime fecha2, string linea, string chofer, string placas, decimal p1, decimal p2,
            decimal pt, decimal materia, string maquilador, string pozo, string observacion, string movimiento, string tmovimiento,
            decimal origen, string tabla, string variedad, decimal corte, decimal pacas, string ensilador, string establo, int idestablo)
        {
            this._ficha = ficha;
            this._folio = folio;
            this._articulo = articulo;
            this._cliente = cliente;
            this._fecha1 = fecha1;
            this._fecha2 = fecha2;
            this._linea = linea;
            this._chofer = chofer;
            this._placas = placas;
            this._p1 = p1;
            this._p2 = p2;
            this._pt = pt;
            this._materia = materia;
            this._maquilador = maquilador;
            this._pozo = pozo;
            this._observacion = observacion;
            this._movimiento = movimiento;
            this._tmovimiento = tmovimiento;
            this._origen = origen;
            this._tabla = tabla;
            this._variedad = variedad;
            this._corte = corte;
            this._pacas = pacas;
            this._ensilador = ensilador;
            this._establo = establo;
            this._idestablo = idestablo;
        }
        public String Ficha { get => _ficha; set => _ficha = value; }
        public String Folio { get => _folio; set => _folio = value; }
        public String Articulo { get => _articulo; set => _articulo = value; }
        public String Cliente { get => _cliente; set => _cliente = value; }
        public DateTime Fecha1 { get => _fecha1; set => _fecha1 = value; }
        public DateTime Fecha2 { get => _fecha2; set => _fecha2 = value; }
        public String Linea { get => _linea; set => _linea = value; }
        public String Chofer { get => _chofer; set => _chofer = value; }
        public String Placas { get => _placas; set => _placas = value; }
        public Decimal P1 { get => _p1; set => _p1 = value; }
        public Decimal P2 { get => _p2; set => _p2 = value; }
        public Decimal PT { get => _pt; set => _pt = value; }
        public Decimal Materia { get => _materia; set => _materia = value; }
        public String Maquilador { get => _maquilador; set => _maquilador = value; }
        public String Pozo { get => _pozo; set => _pozo = value; }
        public String Observacion { get => _observacion; set => _observacion = value; }
        public String Movimiento { get => _movimiento; set => _movimiento = value; }
        public String TMovimiento { get => _tmovimiento; set => _tmovimiento = value; }
        public Decimal Origen { get => _origen; set => _origen = value; }
        public String Tabla { get => _tabla; set => _tabla = value; }

        public String Variedad { get => _variedad; set => _variedad = value; }
        public Decimal Corte { get => _corte; set => _corte = value; }
        public Decimal Pacas { get => _pacas; set => _pacas = value; }
        public String Ensilador { get => _ensilador; set => _ensilador = value; }
        public String Establo { get => _establo; set => _establo = value; }
        public int IDEstablo { get => _idestablo; set => _idestablo = value; }
    }
}