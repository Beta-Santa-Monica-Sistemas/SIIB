using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ModulosUsuario
    {
        private int _id_modulo;
        private string _nombre_modulo;
        private string _icono;

        public ModulosUsuario()
        {

        }

        public ModulosUsuario(int id_modulo, string nombre_modulo, string icono)
        {
            this._id_modulo = id_modulo;
            this._nombre_modulo = nombre_modulo;
            this._icono = icono;

        }
        public int Id_modulo { get => _id_modulo; set => _id_modulo = value; }
        public string Nombre_modulo { get => _nombre_modulo; set => _nombre_modulo = value; }
        public string Icono { get => _icono; set => _icono = value; }
    }
}