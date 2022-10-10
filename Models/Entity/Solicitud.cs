using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SATCFDITEST.Models.Entity
{
    [Table("solicitud")]
    public class SolicitudArhivo
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdSolicitudIntern { get; set; }
        public string UUID { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFin { get; set; }
        public string rfcEmisor { get; set; }
        public string rfcReceptores { get; set;}
        public string rfcSolicitante { get; set;}
        public int TipoSolicitud { get; set; }
        public int TipoComprobante { get; set; }
        public int EstadoComprobante { get; set;}
        public int  Complemento { get; set; }
        public byte[] signature { get; set; }
        public string CodEstatus { get;  set; }
        public string Mensaje { get; set; }
        public string IdSolicitudSat { get; set; }

    }
}
