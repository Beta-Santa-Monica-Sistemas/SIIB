using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReporteJornadaLaboral
    {
        private int _punches_id;
        private int _empleado_id;
        private String _empleado;
        private String _puesto;

        private DateTime _lunes_entrada;
        private DateTime _lunes_salida;
        private String _lunes_dia;

        private DateTime _martes_entrada;
        private DateTime _martes_salida;
        private String _martes_dia;

        private DateTime _miercoles_entrada;
        private DateTime _miercoles_salida;
        private String _miercoles_dia;

        private DateTime _jueves_entrada;
        private DateTime _jueves_salida;
        private String _jueves_dia;

        private DateTime _viernes_entrada;
        private DateTime _viernes_salida;
        private String _viernes_dia;

        private DateTime _sabado_entrada;
        private DateTime _sabado_salida;
        private String _sabado_dia;

        private DateTime _domingo_entrada;
        private DateTime _domingo_salida;
        private String _domingo_dia;

        private Decimal _total_horas;
        private String _area;
        private int _id_area;

        public ReporteJornadaLaboral()
        {

        }

        public ReporteJornadaLaboral
            (
                int _empleado_id,
                DateTime _lunes_entrada, DateTime _lunes_salida, string _lunes_dia,
                DateTime _martes_entrada, DateTime _martes_salida, string _martes_dia,
                DateTime _miercoles_entrada, DateTime _miercoles_salida, string _miercoles_dia,
                DateTime _jueves_entrada, DateTime _jueves_salida, string _jueves_dia,
                DateTime _viernes_entrada, DateTime _viernes_salida, string _viernes_dia,
                DateTime _sabado_entrada, DateTime _sabado_salida, string _sabado_dia,
                DateTime _domingo_entrada, DateTime _domingo_salida, string _domingo_dia,
                decimal _total_horas,
                string _area,
                int _id_area,
                string _empleado,
                string _puesto,
                int _punches_id
            )

        {
            this._empleado_id = _empleado_id;
            this._lunes_entrada = _lunes_entrada;
            this._lunes_salida = _lunes_salida;
            this._lunes_dia = _lunes_dia;
            this._martes_entrada = _martes_entrada;
            this._martes_salida = _martes_salida;
            this._martes_dia = _martes_dia;
            this._miercoles_entrada = _miercoles_entrada;
            this._miercoles_salida = _miercoles_salida;
            this._miercoles_dia = _miercoles_dia;
            this._jueves_entrada = _jueves_entrada;
            this._jueves_salida = _jueves_salida;
            this._jueves_dia = _jueves_dia;
            this._viernes_entrada = _viernes_entrada;
            this._viernes_salida = _viernes_salida;
            this._viernes_dia = _viernes_dia;
            this._sabado_entrada = _sabado_entrada;
            this._sabado_salida = _sabado_salida;
            this._sabado_dia = _sabado_dia;
            this._domingo_entrada = _domingo_entrada;
            this._domingo_salida = _domingo_salida;
            this._domingo_dia = _domingo_dia;
            this._total_horas = _total_horas;
            this._area = _area;
            this._id_area = _id_area;
            this._empleado = _empleado;
            this._puesto = _puesto;
            this._punches_id = _punches_id;
        }

        public int EmpleadoId { get => _empleado_id; set => _empleado_id = value; }
        public String Empleado { get => _empleado; set => _empleado = value; }
        public String Puesto { get => _puesto; set => _puesto = value; }
        public DateTime LunesE { get => _lunes_entrada; set => _lunes_entrada = value; }
        public DateTime LunesS { get => _lunes_salida; set => _lunes_salida = value; }
        public String LunesDia { get => _lunes_dia; set => _lunes_dia = value; }
        public DateTime MartesE { get => _martes_entrada; set => _martes_entrada = value; }
        public DateTime MartesS { get => _martes_salida; set => _martes_salida = value; }
        public String MartesDia { get => _martes_dia; set => _martes_dia = value; }
        public DateTime MiercolesE { get => _miercoles_entrada; set => _miercoles_entrada = value; }
        public DateTime MiercolesS { get => _miercoles_salida; set => _miercoles_salida = value; }
        public String MiercolesDia { get => _miercoles_dia; set => _miercoles_dia = value; }
        public DateTime JuevesE { get => _jueves_entrada; set => _jueves_entrada = value; }
        public DateTime JuevesS { get => _jueves_salida; set => _jueves_salida = value; }
        public String JuevesDia { get => _jueves_dia; set => _jueves_dia = value; }
        public DateTime ViernesE { get => _viernes_entrada; set => _viernes_entrada = value; }
        public DateTime ViernesS { get => _viernes_salida; set => _viernes_salida = value; }
        public String ViernesDia { get => _viernes_dia; set => _viernes_dia = value; }
        public DateTime SabadoE { get => _sabado_entrada; set => _sabado_entrada = value; }
        public DateTime SabadoS { get => _sabado_salida; set => _sabado_salida = value; }
        public String SabadoDia { get => _sabado_dia; set => _sabado_dia = value; }
        public DateTime DomingoE { get => _domingo_entrada; set => _domingo_entrada = value; }
        public DateTime DomingoS { get => _domingo_salida; set => _domingo_salida = value; }
        public String DomingoDia { get => _domingo_dia; set => _domingo_dia = value; }
        public Decimal Total_Horas { get => _total_horas; set => _total_horas = value; }
        public String Area { get => _area; set => _area = value; }
        public int IdArea { get => _id_area; set => _id_area = value; }
        public int PunchesId { get => _punches_id; set => _punches_id = value; }
    }
}