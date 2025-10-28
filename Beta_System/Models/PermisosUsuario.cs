using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class PermisosUsuario
    {
        private int _id_modulo;
        private string _nombre_modulo;
        private int _id_submodulo;

        public PermisosUsuario()
        {

        }

        public PermisosUsuario(int id_modulo, string nombre_modulo, int id_submodulo)
        {
            this._id_modulo = id_modulo;
            this._nombre_modulo = nombre_modulo;
            this._id_submodulo = id_submodulo;

        }
        public int Id_modulo { get => _id_modulo; set => _id_modulo = value; }
        public string Nombre_modulo { get => _nombre_modulo; set => _nombre_modulo = value; }
        public int Id_submodulo { get => _id_submodulo; set => _id_submodulo = value; }
    }
}