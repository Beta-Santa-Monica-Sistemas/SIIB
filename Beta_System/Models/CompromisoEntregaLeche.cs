using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class CompromisoEntregaLeche
    {        
        private int _idCumplimientoSemanal;
        private string _Cliente;
        private decimal _fullCompromisoSemanal;
        private decimal _fullProgramaSemanal;
        private string[][] _observaciones;
        private string _comentarioGeneral;

        public CompromisoEntregaLeche()
        {

        }

        public CompromisoEntregaLeche(int idCumplimientoSemanal, string Cliente, decimal fullCompromisoSemanal, decimal fullProgramaSemanal, string[][] observaciones, string comentarioGeneral)
        {
            this._idCumplimientoSemanal = idCumplimientoSemanal;
            this._Cliente = Cliente;
            this._fullCompromisoSemanal = fullCompromisoSemanal;
            this._fullProgramaSemanal = fullProgramaSemanal;
            this._observaciones = observaciones;
            this._comentarioGeneral = comentarioGeneral;
        }

        public int idCumplimientoSemanal { get => _idCumplimientoSemanal; set => _idCumplimientoSemanal = value; }
        public string Cliente { get => _Cliente; set => _Cliente = value; }
        public decimal fullCompromisoSemanal { get => _fullCompromisoSemanal; set => _fullCompromisoSemanal = value; }
        public decimal fullProgramaSemanal { get => _fullProgramaSemanal; set => _fullProgramaSemanal = value; }
        public string[][] observaciones { get => _observaciones; set => _observaciones = value; }
        public string comentarioGeneral { get => _comentarioGeneral; set => _comentarioGeneral = value; }
    }
}