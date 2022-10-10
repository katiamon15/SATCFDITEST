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

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((context,services)=> {
        var cns = "Data Source= .; Initial Catalog = master; Integrated security= True"; 
        services.AddDbContext<MyAppDbContext>(options => {
            options.UseSqlServer(cns);
        });
        services.AddHttpClient<IHttpSoapClient, HttpSoapClient>();
        services.AddTransient<IAutenticacionService,AutenticacionService>();
        services.AddTransient<ISolicitudService, SolicitudService>();
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


//private void creaSolicitudAlmacen(){}




// Metodo solicitud Solicitud
// Primero llamar al metodo para crear y almacenar en base de datos
//Datos desde el front
DateTime fechaInicio = DateTime.Today;
DateTime fechaFin = DateTime.Today;
TipoSolicitud? tipoSolicitud = TipoSolicitud.Cfdi;
var rfcEmisor = "";
var rfcReceptores = new List<string> { "IPL151012RPA" };
var rfcSolicitante = "IPL151012RPA";

SolicitudArhivo solicitudEntity = new SolicitudArhivo();
solicitudEntity.FechaInicial = fechaInicio;
solicitudEntity.FechaFin = fechaFin;
solicitudEntity.rfcEmisor = rfcEmisor;
solicitudEntity.rfcReceptores = rfcReceptores[0];
solicitudEntity.rfcSolicitante = rfcSolicitante;
solicitudEntity.TipoSolicitud = tipoSolicitud.Value;
solicitudEntity.Complemento=1;

//solicitudEntity.uuid = "111AAA1A-1AA1-1A11-11A1-11A1AA111A11";

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

SolicitudResult solicitudResult = SolicitudResult.CreateInstance("490f9e0d-2fa2-4a82-aa48-7c729e118eaf",
    "5000", "Solicitud aceptada", HttpStatusCode.Accepted, "Mensaje respuesta SAT");


solicitudEntity.IdSolicitudSat = solicitudResult.RequestId;
solicitudEntity.CodEstatus = solicitudResult.RequestStatusCode;
solicitudEntity.Mensaje = solicitudResult.RequestStatusMessage;

try
{
    context.SolicitudArhivo.Add(solicitudEntity);
    //context.SaveChanges();
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