using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ReporteCumplimientoEnviosLecheDetalleData
    {
        private int _id_cliente;
        private string _cliente;
        private string _destino;
        private string _establo;
        private decimal[] _ppto_envio;
        private decimal[] _real_envio;
        private decimal _cumplimiento_envio;

        public ReporteCumplimientoEnviosLecheDetalleData() { }
        public ReporteCumplimientoEnviosLecheDetalleData(int id_cliente, string cliente, string destino, string establo,
            decimal[] ppto_envio, decimal[] real_envio, decimal cumplimiento_envio)
        {
            this._id_cliente = id_cliente;
            this._cliente = cliente;
            this._destino = destino;
            this._establo = establo;
            this._ppto_envio = ppto_envio;
            this._real_envio = real_envio;
            this._cumplimiento_envio = cumplimiento_envio;
        }
        public int Id_cliente { get => _id_cliente; set => _id_cliente = value; }
        public string Cliente { get => _cliente; set => _cliente = value; }
        public string Destino { get => _destino; set => _destino = value; }
        public string Establo { get => _establo; set => _establo = value; }
        public decimal[] Ppto_envio { get => _ppto_envio; set => _ppto_envio = value; }
        public decimal[] Real_envio { get => _real_envio; set => _real_envio = value; }
        public decimal Cumplimiento_envio { get => _cumplimiento_envio; set => _cumplimiento_envio = value; }
    }
}