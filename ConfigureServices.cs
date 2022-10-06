﻿using System.Security.Cryptography.X509Certificates;
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


/*CancellationTokenSource cancellationTokenSource = new();
CancellationToken cancellationToken = cancellationTokenSource.Token;

await host.StartAsync(cancellationToken);

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Iniciando ejemplo de como utilizar los servicios para descargar los CFDIs recibidos del dia de hoy.");

// Parametros de ejemplo
var rutaCertificadoPfx = @"C:\llavero\key.pfx";
byte[] certificadoPfx = await File.ReadAllBytesAsync(rutaCertificadoPfx, cancellationToken);
var certificadoPassword = "23a24c15k";
DateTime fechaInicio= DateTime.Today;
DateTime fechaFin= DateTime.Today;
TipoSolicitud? tipoSolicitud = TipoSolicitud.Cfdi;
var rfcEmisor = "";
var rfcReceptores = new List<string> { "IPL151012RPA" };
var rfcSolicitante = "IPL151012RPA";



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

// Solicitud
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
SolicitudResult solicitudResult = await solicitudService.SendSoapRequestAsync(solicitudRequest, certificadoSat, cancellationToken);

if (string.IsNullOrEmpty(solicitudResult.RequestId))
{
    logger.LogError("La solicitud de solicitud de descarga no fue exitosa. RequestStatusCode:{0} RequestStatusMessage:{1}",
        solicitudResult.RequestStatusCode,
        solicitudResult.RequestStatusMessage);
    throw new Exception();
}

logger.LogInformation("La solicitud de solicitud de descarga fue exitosa. RequestId:{0}", solicitudResult.RequestId);*/