using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class EmpleadoNChecador
    {
        // id de C_nomina_empleados_checador
        private int _id_empleado_NChecador;

        // esto es hr.employee
        private string _nombre_empleado;
        private string _apellido_empleado;
        private int _empleado_id;

        // esto es c_nomina_empleados
        private string _nombre_empleado_nomina;
        private string _apellido_empleado_nomina;
        private int _id_empleado_checador;

        public EmpleadoNChecador()
        {

        }

        public EmpleadoNChecador(int id_empleadoNChecador, string nombre_empleado, string apellido_empleado,
            int empleado_id, int id_empleado_checador, string nombre_empleado_nomina, string apellido_empleado_nomina)
        {
            this._id_empleado_NChecador = id_empleadoNChecador;
            this._nombre_empleado = nombre_empleado;
            this._apellido_empleado = apellido_empleado;
            this._empleado_id = empleado_id;
            this._id_empleado_checador = id_empleado_checador;
            this._nombre_empleado_nomina = nombre_empleado_nomina;
            this._apellido_empleado_nomina = apellido_empleado_nomina;
        }

        //public int id_empleadoNC { get => _id_empleado_NChecador; set => _id_empleado_NChecador = value; }

        public int IdEmpleadoNChecador
        {
            get => _id_empleado_NChecador;
            set => _id_empleado_NChecador = value;
        }

        // nomina
        public string NombreEmpleadoNomina
        {
            get => _nombre_empleado;
            set => _nombre_empleado = value;
        }

        public string ApellidoEmpleadoNomina
        {
            get => _apellido_empleado;
            set => _apellido_empleado = value;
        }

        public int EmpleadoIdNomina
        {
            get => _empleado_id;
            set => _empleado_id = value;
        }

        /// checa 
        public string NombreCheca
        {
            get => _nombre_empleado_nomina;
            set => _nombre_empleado_nomina = value;
        }

        public string ApellidoCheca
        {
            get => _apellido_empleado_nomina;
            set => _apellido_empleado_nomina = value;
        }

        public int IdEmpleadoChecador
        {
            get => _id_empleado_checador;
            set => _id_empleado_checador = value;
        }
    }
}