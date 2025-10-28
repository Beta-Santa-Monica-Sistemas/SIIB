using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class Empleados_MS
    {
        public int EMPLEADO_ID { get; set; }
        public int NUMERO { get; set; }
        public string NOMBRE_COMPLETO { get; set; }
        public string APELLIDO_PATERNO { get; set; }
        public string APELLIDO_MATERNO { get; set; }
        public string NOMBRES { get; set; }
        public string REGIMEN { get; set; }
        public int PUESTO_NO_ID { get; set; }
        public int DEPTO_NO_ID { get; set; }
        public int FREPAG_ID { get; set; }
        public int REG_PATRONAL_ID { get; set; }
        public bool ES_JEFE { get; set; }
        public char FORMA_PAGO { get; set; }
        public char CONTRATO { get; set; }
        public string DIAS_HRS_JSR { get; set; }
        public char TURNO { get; set; } 
        public decimal JORNADA { get; set; }
        public int REGIMEN_FISCAL { get; set; }
        public string CONTRATO_SAT { get; set; }
        public string JORNADA_SAT { get; set; }
        public char ES_SINDICALIZADO { get; set; }
        public DateTime FECHA_INGRESO { get; set; }
        public string ESTATUS { get; set; }
        public char ZONA_SALMIN { get; set; }
        public int TABLA_ANTIG_ID { get; set; }
        public int TIPO_SALARIO { get; set; }
        public decimal PCTJE_INTEG { get; set; }
        public decimal SALARIO_DIARIO { get; set; }
        public decimal SALARIO_HORA { get; set; }
        public decimal SALARIO_INTEG { get; set; }
        public bool ES_DIR_ADMR_GTE_GRAL { get; set; }
        public bool PTU { get; set; }
        public bool IMSS { get; set; }
        public bool CAS { get; set; }
        public bool PENSIONADO { get; set; }
        public bool DESHAB_IMPTOS { get; set; }
        public bool CALC_ISR_ANUAL { get; set; }
        public string CALLE { get; set; }
        public string NOMBRE_CALLE { get; set; }
        public string NUM_EXTERIOR { get; set; }
        public string COLONIA { get; set; }
        public string POBLACION { get; set; }
        public string REFERENCIA { get; set; }
        public int CIUDAD_ID { get; set; }
        public string CODIGO_POSTAL { get; set; }
        public string TELEFONO1 { get; set; }
        public string EMAIL { get; set; }
        public char SEXO { get; set; }
        public DateTime FECHA_NACIMIENTO { get; set; }
        public int CIUDAD_NACIMIENTO_ID { get; set; }
        public string ESTADO_CIVIL { get; set; }
        public int NUM_HIJOS { get; set; }
        public string NOMBRE_PADRE { get; set; }
        public string NOMBRE_MADRE { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public string REG_IMSS { get; set; }
        public int GRUPO_PAGO_ELECT_ID { get; set; }
        public char TIPO_CTABAN_PAGO_ELECT { get; set; }
        public string NUM_CTABAN_PAGO_ELECT { get; set; }
        public string USUARIO_CREADOR { get; set; }
        public DateTime FECHA_HORA_CREACION { get; set; }
        public decimal SALARIO_DIARIO_ESTABLO { get; set; }
        public decimal SALARIO_HORA_ESTABLO { get; set; }
        public string PATH_CONSTANCIA_FISCAL { get; set; }
    }
}