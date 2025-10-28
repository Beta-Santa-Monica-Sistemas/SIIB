using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class Incidencias_MS
    {
        public int INCIDENCIA_ID { get; set; }
        public int EMPELADO_ID { get; set; }
        public int REG_PATRONAL_ID { get; set; }
        public int NUEVO_REG_PATRONAL_ID { get; set; }
        public int TIPO { get; set; }
        public DateTime FECHA { get; set; }
        public decimal SALARIO_DIARIO { get; set; }
        public decimal SALARIO_HORA { get; set; }
        public decimal SALARIO_INTEG { get; set; }
        public decimal PCTJE_INTEG { get; set; }
        public int CASUSA_BAJA { get; set; }
        public string OBSERVACIONES { get; set; }
        public int ID_PUESTO_ACTUAL { get; set; }
        public int ID_PUESTO_NUEVO { get; set; }
        public int ID_DEPARTAMENTO_ACTUAL { get; set; }
        public int ID_DEPARTAMENTO_NUEVO { get; set; }
        public int ID_USUARIO_REGISTRA { get; set; }
        public decimal SALARIO_DIARIO_NUEVO { get; set; }
        public decimal SALARIO_HORA_NUEVO { get; set; }
        public decimal SALARIO_INTEG_NUEVO { get; set; }
        public bool LISTA_NEGRA { get; set; }
    }
}