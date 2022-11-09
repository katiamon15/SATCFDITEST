using System.Security.Cryptography.X509Certificates;
using ARSoftware.Cfdi. DescargaMasiva.Enumerations;
using ARSoftware.Cfdi.DescargaMasiva.Helpers;
using ARSoftware.Cfdi.DescargaMasiva.Interfaces;
using ARSoftware.Cfdi.DescargaMasiva.Models;
using ARSoftware.Cfdi.DescargaMasiva.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SATCFDITEST;
using SATCFDITEST.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Formats.Asn1.AsnWriter;
using SATCFDITEST.Models.Entity;
using System.Net;
using System.Reflection.Metadata;
using SATCFDITEST.Service;
using System.Xml.Serialization;
using Microsoft.IdentityModel.Tokens;
using RazorEngineCore;
using System.Diagnostics;
using SATCFDITEST.CFDI4;
using System.Xml;


/*IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((context,services)=> {
        var cns = "Data Source= .; Initial Catalog = master; Integrated security= True"; 
        services.AddDbContext<MyAppDbContext>(options => {
            options.UseSqlServer(cns);
        });
        services.AddHttpClient<IHttpSoapClient, HttpSoapClient>();
        services.AddTransient<IAutenticacionService,AutenticacionService>();
        services.AddTransient<ISolicitudService, SolicitudService>();
        services.AddTransient<IVerificacionService, VerificacionService>();
        services.AddTransient<IDescargaService, DescargaService>();
    }).Build();

var context = host.Services.GetRequiredService<MyAppDbContext>();


var resultados = context.Complemento.ToArray();

foreach (Complemento resultado in resultados)
{
    Console.WriteLine(resultado.tipocomplemento);
}


CancellationTokenSource cancellationTokenSource = new();
CancellationToken cancellationToken = cancellationTokenSource.Token;

await host.StartAsync(cancellationToken);

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Iniciando ejemplo de como utilizar los servicios para descargar los CFDIs recibidos del dia de hoy.");

// Parametros de ejemplo
var rutaCertificadoPfx = @"C:\llavero\key.pfx";
byte[] certificadoPfx = await File.ReadAllBytesAsync(rutaCertificadoPfx, cancellationToken);
var certificadoPassword = "23a24c15k";

logger.LogInformation("Creando el certificado SAT con el certificado PFX y contrasena.");
X509Certificate2 certificadoSat = X509Certificate2Helper.GetCertificate(certificadoPfx, certificadoPassword);

// Autenticacion
logger.LogInformation("Buscando el servicio de autenticacion en el contenedor de servicios (Dependency Injection).");
var autenticacionService = host.Services.GetRequiredService<IAutenticacionService>();

logger.LogInformation("Creando solicitud de autenticacion.");
var autenticacionRequest = AutenticacionRequest.CreateInstance();

logger.LogInformation("Enviando solicitud de autenticacion.");
AutenticacionResult autenticacionResult =
    await autenticacionService.SendSoapRequestAsync(autenticacionRequest, certificadoSat, cancellationToken);
 
if (!autenticacionResult.AccessToken.IsValid)
{
    logger.LogError("La solicitud de autenticacion no fue exitosa. FaultCode:{0} FaultString:{1}",
        autenticacionResult.FaultCode,
        autenticacionResult.FaultString);
    throw new Exception();
}

logger.LogInformation("La solicitud de autenticacion fue exitosa. AccessToken:{0}", autenticacionResult.AccessToken.DecodedValue);

//Solicitud


//private void creaSolicitudAlmacen(){}

// Metodo solicitud Solicitud
// Primero llamar al metodo para crear y almacenar en base de datos
//Datos desde el front
DateTime fechaInicio = DateTime.Today;
DateTime fechaFin = DateTime.Today;
TipoSolicitud? tipoSolicitud = TipoSolicitud.Metadata;
var rfcEmisor = "";
var rfcReceptores = new List<string> { "IPL151012RPA" };
var rfcSolicitante = "IPL151012RPA";



//solicitudEntity.uuid = "";

logger.LogInformation("Buscando el servicio de solicitud de descarga en el contenedor de servicios (Dependency Injection).");
var solicitudService = host.Services.GetRequiredService<ISolicitudService>();

logger.LogInformation("Creando solicitud de solicitud de descarga.");
var solicitudRequest = SolicitudRequest.CreateInstance(fechaInicio,
    fechaFin,
    tipoSolicitud,
    rfcEmisor,
    rfcReceptores,
    rfcSolicitante,
    autenticacionResult.AccessToken);

logger.LogInformation("Enviando solicitud de solicitud de descarga.");
//SolicitudResult solicitudResult = await solicitudService.SendSoapRequestAsync(solicitudRequest, certificadoSat, cancellationToken);

SolicitudResult solicitudResult = SolicitudResult.CreateInstance("C223B7CC-0576-4411-8694-D1DA74D06007",
    "5000", "Solicitud aceptada", HttpStatusCode.Accepted, "Mensaje respuesta SAT");

SolicitudArhivo solicitudEntity = new SolicitudArhivo();
solicitudEntity.FechaInicial = fechaInicio;
solicitudEntity.FechaFin = fechaFin;
solicitudEntity.rfcEmisor = rfcEmisor;
solicitudEntity.rfcReceptores = rfcReceptores[0];
solicitudEntity.rfcSolicitante = rfcSolicitante;
solicitudEntity.TipoSolicitud = tipoSolicitud.Value;
solicitudEntity.Complemento = 1;
solicitudEntity.IdSolicitudSat = solicitudResult.RequestId;
solicitudEntity.CodEstatus = solicitudResult.RequestStatusCode;
solicitudEntity.Mensaje = solicitudResult.RequestStatusMessage;

try
 {
    context.SolicitudArhivo.Add(solicitudEntity);
    context.SaveChanges();
}catch (Exception e)
{
    Console.WriteLine(e.InnerException.Message);
    throw new Exception("Problemas al insertar en la base de datos");
}

if (string.IsNullOrEmpty(solicitudResult.RequestId))
{
    logger.LogError("La solicitud de solicitud de descarga no fue exitosa. RequestStatusCode:{0} RequestStatusMessage:{1}",
        solicitudResult.RequestStatusCode,
        solicitudResult.RequestStatusMessage);
    throw new Exception();
}

logger.LogInformation("La solicitud de solicitud de descarga fue exitosa. RequestId:{0}", solicitudResult.RequestId);


//Verificacion


logger.LogInformation("Buscando el servicio de verificacion en el contenedor de servicios (Dependency Injection).");
var verificaSolicitudService = host.Services.GetRequiredService<IVerificacionService>();

logger.LogInformation("Creando solicitud de verificacion.");
var verificacionRequest = VerificacionRequest.CreateInstance(solicitudResult.RequestId, rfcSolicitante, autenticacionResult.AccessToken);



logger.LogInformation("Enviando solicitud de verificacion.");
VerificacionResult verificacionResult = await verificaSolicitudService.SendSoapRequestAsync(verificacionRequest,
    certificadoSat,
    cancellationToken);

if (verificacionResult.DownloadRequestStatusNumber != EstadoSolicitud.Terminada.Value.ToString())
{ 
    logger.LogError(
        "La solicitud de verificacion no fue exitosa. DownloadRequestStatusNumber:{0} RequestStatusCode:{1} RequestStatusMessage:{2}",
        verificacionResult.DownloadRequestStatusNumber,
        verificacionResult.RequestStatusCode,
        verificacionResult.RequestStatusMessage);

    throw new Exception();
}


else if (verificacionResult.DownloadRequestStatusNumber == EstadoSolicitud.EnProceso.Value.ToString())
{
    logger.LogInformation(
        "El estado de la solicitud esta en Proceso. Mandar otra solicitud de verificación mas tarde para que el servicio web pueda procesar la solicitud.");
    throw new Exception();
}

else if (verificacionResult.DownloadRequestStatusNumber == EstadoSolicitud.Aceptada.Value.ToString())
{
    logger.LogInformation(
        "El estado de la solicitud es Aceptada.");

}


logger.LogInformation("La solicitud de verificacion fue exitosa.");

foreach (string idsPaquete in verificacionResult.PackageIds)
{
    Verificacion Entityverificacion = new Verificacion();
    Entityverificacion.IdSolicitudSat = solicitudEntity.IdSolicitudSat;
    Entityverificacion.rfcSolicitante = solicitudEntity.rfcSolicitante;
    Entityverificacion.Idpaquetes = idsPaquete;
    Entityverificacion.Estadosolicitud = Int32.Parse(verificacionResult.DownloadRequestStatusNumber);
    Entityverificacion.Codigoestadosolicitud = verificacionResult.DownloadRequestStatusCode;
    Entityverificacion.Numerocfdi = Int32.Parse(verificacionResult.NumberOfCfdis);
    Entityverificacion.Mensaje = verificacionResult.RequestStatusMessage;
    Entityverificacion.Codestatus = verificacionResult.RequestStatusCode;

    context.Verificacion.Add(Entityverificacion);
    context.SaveChanges();

    logger.LogInformation("PackageId:{0}", idsPaquete);
}

// Descarga
var rutaDescarga = @"C:\DescargaMasiva\Cfdi";
logger.LogInformation("Buscando el servicio de verificacion en el contenedor de servicios (Dependency Injection).");
var descargarSolicitudService = host.Services.GetRequiredService<IDescargaService>();

foreach (string? idsPaquete in verificacionResult.PackageIds)
{
    logger.LogInformation("Creando solicitud de descarga.");
    var descargaRequest = DescargaRequest.CreateInstace(idsPaquete, rfcSolicitante, autenticacionResult.AccessToken);

    logger.LogInformation("Enviando solicitud de descarga.");
    DescargaResult? descargaResult = await descargarSolicitudService.SendSoapRequestAsync(descargaRequest,
        certificadoSat,
        cancellationToken);

    string fileName = Path.Combine(rutaDescarga, $"{idsPaquete}.zip");
    byte[] paqueteContenido = Convert.FromBase64String(descargaResult.Package);

    logger.LogInformation("Guardando paquete descargado en un archivo .zip en la ruta  de descarga.");
    using FileStream fileStream = File.Create(fileName, paqueteContenido.Length);
    await fileStream.WriteAsync(paqueteContenido, 0, paqueteContenido.Length, cancellationToken);
}

await host.StopAsync(cancellationToken);

logger.LogInformation("Proceso terminado.");*/



//transformacion PDF 3.3

// 1 paso leer el xml pasar timbrado a una clase


string pathxml = @"C:\Users\nousfera\Documents\Descarga CFDI\4.0\f2bc39fc-2a47-4593-a7cd-88e4dedd50bc.xml";

XmlDocument doc = new XmlDocument();
doc.Load(pathxml);

XmlNodeList elemList = doc.GetElementsByTagName("cfdi:Comprobante");
XmlAttributeCollection atributos = elemList[0].Attributes;
string version = "";
for (int j = 0; j < atributos.Count; j++)
{
    if (atributos[j].Name == "Version")
    {
        version = atributos[j].Value;
    }
    
}

Console.WriteLine(version);

if (version == "4.0")
{
    SATCFDITEST.CFDI4.Comprobante ocomprobantes = new SATCFDITEST.CFDI4.Comprobante();
    XmlSerializer oSerializer = new XmlSerializer(typeof(SATCFDITEST.CFDI4.Comprobante));

    using (StreamReader reader = new StreamReader(pathxml))
    {
        //aqui desearilizamos
        ocomprobantes = (SATCFDITEST.CFDI4.Comprobante)oSerializer.Deserialize(reader);  //fallo aqui 

        //complementos
        var oComplemento = ocomprobantes.Complemento;
        
            foreach (var complementointerior in oComplemento.Any)
            {
                if (complementointerior.Name.Contains("TimbreFiscalDigital"))

                //iftipo comprobante == "I"
                //else{new Exception("El documento XML no es un ingreso")}

                {
                    XmlSerializer oSerializerComplemento = new XmlSerializer(typeof(TimbreFiscalDigital));

                    using (var readerComplemento = new StringReader(complementointerior.OuterXml))
                    {
                        ocomprobantes.TimbreFiscalDigital =
                            (TimbreFiscalDigital)oSerializerComplemento.Deserialize(readerComplemento);
                    }
                }

            }
        
    }//using


    //paso 2 proceso de lectura apñicandolo con razor y haciendo pdf  

    string path = AppDomain.CurrentDomain.BaseDirectory;
    string pathHTMLTemp = path + "mihtml.html"; //temporal 
    string pathplantilla = path + "Plantilla.html";
    string shtml = Razor.GetStringOffile(pathplantilla);
    string resulthtml = "";
    //resulthtml = RazorEngineCore.Razor.Parse(shtml, ocomprobante);

    IRazorEngine razorEngine = new RazorEngine();
    IRazorEngineCompiledTemplate template = razorEngine.Compile(shtml);

    string result = template.Run(ocomprobantes);


    Console.WriteLine(result);
    Console.Read();

    //creamos el temporal
    File.WriteAllText(pathHTMLTemp, result);

    string pathhtmlpdf = @"C:\SATCFDITEST\wkhtmltopdf\wkhtmltopdf.exe";

    ProcessStartInfo oprocessStartInfo = new ProcessStartInfo();
    oprocessStartInfo.UseShellExecute = false;
    oprocessStartInfo.FileName = pathhtmlpdf;
    oprocessStartInfo.Arguments = "mihtml.html mipdf.pdf";

    using (Process oProcess = Process.Start(oprocessStartInfo))
    {
        oProcess.WaitForExit();
    }


    //elimminar el temporral
    File.Delete(pathHTMLTemp);
}
else if (version == "3.3")
{
    Comprobante ocomprobante = new Comprobante();
    XmlSerializer oSerializer = new XmlSerializer(typeof(Comprobante));

    using (StreamReader reader = new StreamReader(pathxml))
    {
        //aqui desearilizamos
        ocomprobante = (Comprobante)oSerializer.Deserialize(reader);

        //complementos
        foreach (var oComplemento in ocomprobante.Complemento)
        {
            foreach (var ocomplementointerior in oComplemento.Any)
            {
                if (ocomplementointerior.Name.Contains("TimbreFiscalDigital"))

                //iftipo comprobante == "I"
                //else{new Exception("El documento XML no es un ingreso")}

                {
                    XmlSerializer oSerializerComplemento = new XmlSerializer(typeof(TimbreFiscalDigital));

                    using (var readerComplemento = new StringReader(ocomplementointerior.OuterXml))
                    {
                        ocomprobante.TimbreFiscalDigital =
                            (TimbreFiscalDigital)oSerializerComplemento.Deserialize(readerComplemento);
                    }
                }

            }
        }
    }//using


    //paso 2 proceso de lectura apñicandolo con razor y haciendo pdf  

    string path = AppDomain.CurrentDomain.BaseDirectory;
    string pathHTMLTemp = path + "mihtml.html"; //temporal 
    string pathplantilla = path + "Plantilla.html";
    string shtml = Razor.GetStringOffile(pathplantilla);
    string resulthtml = "";
    //resulthtml = RazorEngineCore.Razor.Parse(shtml, ocomprobante);

    IRazorEngine razorEngine = new RazorEngine();
    IRazorEngineCompiledTemplate template = razorEngine.Compile(shtml);

    string result = template.Run(ocomprobante);


    Console.WriteLine(result);
    Console.Read();

    //creamos el temporal
    File.WriteAllText(pathHTMLTemp, result);

    string pathhtmlpdf = @"C:\SATCFDITEST\wkhtmltopdf\wkhtmltopdf.exe";

    ProcessStartInfo oprocessStartInfo = new ProcessStartInfo();
    oprocessStartInfo.UseShellExecute = false;
    oprocessStartInfo.FileName = pathhtmlpdf;
    oprocessStartInfo.Arguments = "mihtml.html mipdf.pdf";

    using (Process oProcess = Process.Start(oprocessStartInfo))
    {
        oProcess.WaitForExit();
    }


    //elimminar el temporral
    File.Delete(pathHTMLTemp);  
}
else {
    Console.WriteLine("Error en la version");
}


public class Razor
{
    public static string GetStringOffile(string pathFile)
    {
        string contenido = File.ReadAllText(pathFile);
        return contenido;
    }

}