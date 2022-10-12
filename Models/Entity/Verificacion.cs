
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SATCFDITEST.Models.Entity
{
    public class Verificacion
    {
        [Key]
        public string IdSolicitudSat { get; set; }
        public string rfcSolicitante { get; set; }
        public string Idpaquetes { get; set; }
        public int Estadosolicitud { get; set; }
        public string Codigoestadosolicitud { get; set; }  
        public int Numerocfdi { get; set; }
        public string Codestatus { get; set; }
        public string Mensaje { get; set; }
    }
  
}