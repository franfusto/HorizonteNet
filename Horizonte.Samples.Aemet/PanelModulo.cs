using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Horizonte.Samples.Aemet.Widgets;
using Horizonte;
using Horizonte.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Horizonte.Samples.Aemet;

/// <summary>
/// La clase PanelModulo pertenece al módulo Horizonte.Samples.Aemet.
/// Se encarga de gestionar funcionalidades relacionadas con las predicciones meteorológicas
/// de distintas comunidades autónomas de España a través de comandos específicos.
/// </summary>
[HorizonteModule("Horizonte.Samples.Aemet")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IHCredManager _credManager;
    private AemetConfig? _config;
    private PrediccionesEspecificasApi? _prediccionesEspecificasApi = new PrediccionesEspecificasApi();
    
    private PrediccionesNormalizadasTextoApi?
        _prediccionesNormalizadasTextoApi = new PrediccionesNormalizadasTextoApi();


    /// <summary>
    /// La clase <c>PanelModulo</c> representa un módulo dentro del espacio de muestras de Horizonte, englobado en el contexto de "Aemet".
    /// Se encarga de inicializar y configurar widgets específicos, así como de ofrecer predicciones meteorológicas para comunidades autónomas.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IHCredManager credManager)
    {
        _logger = logger;
        _context = context;
        _credManager = credManager;
    }

    // module

    [HorizonteRole("init")]
    [HorizonteCommand("Aemet_Init")]
    public bool Init()
    {
        _config = _context.Get<AemetConfig>() ?? new AemetConfig();
        ConfigureApi();
        _logger.LogInformation("Módulo Aemet Inciciado");
        return true;
    }

    /// <summary>
    /// El método <c>ConfigPage</c> devuelve una definición de widget (<c>WidgetDef</c>) para el módulo Aemet,
    /// especificando el tipo de widget de configuración utilizado para personalizar las opciones del módulo Aemet.
    /// Esta definición no contiene parámetros adicionales.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que representa el widget de configuración del módulo Aemet.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("Aemet_ConfigPage", "Widget de configuración del módulo Aemet")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AemetConfigWidget), Parameters = null };


    //aemet service

    //predicción por comunidad autonoma

    /// <summary>
    /// El método <c>PrediccinCCAAHoyTiempoActual</c> obtiene la predicción meteorológica actual para una comunidad autónoma determinada, válida para el mismo día de la fecha de la petición.
    /// Si la predicción para el día de la solicitud aún no se ha elaborado, se devolverá la última elaborada. La información se actualiza continuamente.
    /// </summary>
    /// <param name="ccaa">El código de la comunidad autónoma para la cual se desea obtener la predicción. Ejemplos de códigos incluyen: "and" para Andalucía, "arn" para Aragón, "ast" para Asturias, entre otros.</param>
    /// <returns>Un <c>string</c> que representa la predicción meteorológica actual para la comunidad autónoma especificada. En caso de error, devuelve un mensaje de error indicando que no fue posible realizar la consulta.</returns>
    [HorizonteCommand("Aemet_PrediccinCCAAHoyTiempoActual",
        @"Predicción CCAA hoy. Tiempo actual. Predicción para la CCAA que se pasa como parámetro con validez para mismo día que la fecha de petición. En el caso de que en la fecha de petición este producto todavía no se hubiera elaborado, se retornará el último elaborado. Actualización continuamente.
    <param name=""ccaa""> | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La")]
    public async Task<string>? PrediccinCCAAHoyTiempoActual(string ccaa)
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = _prediccionesNormalizadasTextoApi?.PrediccinCCAAHoyTiempoActual(ccaa);
            return  GetModel200Data(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


    /// <summary>
    /// El método <c>PrediccinCCAAMaanaTiempoActual</c> realiza una predicción meteorológica para una comunidad autónoma específica,
    /// para el día siguiente a la fecha de la petición. Si el producto no ha sido elaborado al momento de la petición,
    /// se devolverá el último producto disponible. La actualización de esta predicción es continua.
    /// </summary>
    /// <param name="ccaa">Código de la comunidad autónoma para la cual se solicita la predicción. Ejemplos de códigos: and (Andalucía), cat (Cataluña), mad (Madrid), etc.</param>
    /// <returns>Una tarea que representa la operación asincrónica y contiene la predicción meteorológica como una cadena de texto. Si ocurre un error, se devuelve un mensaje indicando que no fue posible realizar la consulta.</returns>
    [HorizonteCommand("Aemet_PrediccinCCAAMaanaTiempoActual",
        @"Predicción CCAA mañana. Tiempo actual.Predicción para la comunidad autónoma que se pasa como parámetro para el día siguiente a la fecha de la petición. En el caso de el producto no se hubiera elaborado todavía en la fecha de petición se retornará el último producto elaborado. Periodicidad de actualización: continuamente.
    <param name=""ccaa""> | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La")]
    public async Task<string>? PrediccinCCAAMaanaTiempoActual(string ccaa)
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = _prediccionesNormalizadasTextoApi?.PrediccinCCAAMaanaTiempoActual(ccaa);
            return  GetModel200Data(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


    /// <summary>
    /// El método <c>PrediccinCCAAPasadoMaanaTiempoActual</c> proporciona la predicción meteorológica para una comunidad autónoma específica para el día siguiente a la fecha de la solicitud.
    /// Si la predicción no está disponible en el momento de la consulta, retorna la última predicción disponible.
    /// La actualización de esta información se realiza de manera continua.
    /// </summary>
    /// <param name="ccaa">Código de la Comunidad Autónoma (CCAA) para la que se solicita la predicción. Ejemplos de códigos incluyen: and (Andalucía), arn (Aragón), ast (Asturias), entre otros.</param>
    /// <returns>Devuelve un <c>string</c> con la información de la predicción meteorológica o un mensaje de error si no se puede completar la consulta.</returns>
    [HorizonteCommand("Aemet_PrediccinCCAAPasadoMaanaTiempoActual",
        @"Predicción CCAA mañana. Tiempo actual.Predicción para la comunidad autónoma que se pasa como parámetro para el día siguiente a la fecha de la petición. En el caso de el producto no se hubiera elaborado todavía en la fecha de petición se retornará el último producto elaborado. Periodicidad de actualización: continuamente.
    <param name=""ccaa""> | Código de CCAA | CCAA | |- -- -- -- -- -|- -- -- -- -- -| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La")]
    public async Task<string>? PrediccinCCAAPasadoMaanaTiempoActual(string ccaa)
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = _prediccionesNormalizadasTextoApi?.PrediccinCCAAPasadoMaanaTiempoActual(ccaa);
            return  GetModel200Data(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }

    // predicción nivel nacional

    /// <summary>
    /// El método <c>PrediccinNacionalHoyTiempoActual</c> facilita la predicción del tiempo actual a nivel nacional.
    /// Realiza una consulta a la API de AEMET para obtener datos meteorológicos del día en curso.
    /// En caso de que el producto no esté disponible en la fecha de la solicitud, se devuelve el último producto elaborado.
    /// Si ocurre un error durante el proceso, se registra en el log y se devuelve un mensaje de error apropiado.
    /// </summary>
    /// <returns>Devuelve una cadena de texto con la predicción del tiempo o un mensaje de error si la consulta no es exitosa.</returns>
    [HorizonteCommand("Aemet_PrediccinNacionalHoyTiempoActual",
        @"Predicción CCAA mañana. Tiempo actual.Predicción para la comunidad autónoma que se pasa como parámetro para el día siguiente a la fecha de la petición. En el caso de el producto no se hubiera elaborado todavía en la fecha de petición se retornará el último producto elaborado. Periodicidad de actualización: continuamente.")]
    public async Task<string>? PrediccinNacionalHoyTiempoActual()
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = await _prediccionesNormalizadasTextoApi.PrediccinNacionalHoyTiempoActualAsync();
            return  GetModel200Data(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


    /// <summary>
    /// El método <c>PrediccinNacionalMaanaTiempoActual</c> proporciona la predicción nacional para el día siguiente a la fecha de elaboración, es decir, el tiempo actual y la predicción nacional para mañana.
    /// La predicción se actualiza diariamente y, en caso de que no se haya elaborado en el día actual, se devuelve el último producto de predicción disponible.
    /// </summary>
    /// <returns>Una cadena de texto con los datos de la predicción o un mensaje de error si no es posible realizar la consulta.</returns>
    [HorizonteCommand("Aemet_PrediccinNacionalMaanaTiempoActual",
        @"Predicción nacional mañana. Tiempo actual. Predicción nacional para el día siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual todavía no se haya elaborado se devolverá el último producto de predicción nacional para mañana elaborado.")]
    public async Task<string>? PrediccinNacionalMaanaTiempoActual()
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = _prediccionesNormalizadasTextoApi?.PrediccinNacionalMaanaTiempoActual();
            return  GetModel200Data(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


    /// <summary>
    /// El método <c>PrediccinNacionalPasadoMaanaTiempoActual</c> se encarga de obtener la predicción nacional para pasado mañana, basada en la fecha actual de elaboración.
    /// Realiza una consulta a la API de AEMET para recuperar el pronóstico correspondiente y lo devuelve como una cadena de texto.
    /// Si aún no se ha elaborado la predicción para el día actual, se devuelve el último producto disponible.
    /// </summary>
    /// <returns>
    /// Una cadena de texto que contiene la predicción del tiempo para pasado mañana, o un mensaje de error si la consulta no ha sido exitosa.
    /// </returns>
    [HorizonteCommand("Aemet_PrediccinNacionalPasadoMaanaTiempoActual",
        @"Predicción nacional pasado mañana. Tiempo actual.Predicción nacional para pasado mañana siguiente a la fecha de elaboración. En este caso la fecha de elaboración es el día actual. Actualización diaria. En el caso de que en el día actual todavía no se haya elaborado se devolverá el último producto de predicción nacional para pasado mañana elaborado.")]
    public async Task<string>? PrediccinNacionalPasadoMaanaTiempoActual()
    {
        try
        {
            if (_prediccionesNormalizadasTextoApi == null) throw new Exception("API AEMET no configurada");
            var result = _prediccionesNormalizadasTextoApi?.PrediccinNacionalPasadoMaanaTiempoActual();
            var ret =  GetModel200Data(result);
            return ret;
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


    // Auxiliares

    private string GetModel200Data(Model200? model)
    {
        const string error = "No se ha podido obtener el contenido del archivo.";
        try
        {
            if (model == null!) return error;
            if (model.Datos == null!) return error;
            if (model.Datos.Length == 0) return error;
            if (!model.Datos.StartsWith("http")) return error;
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(model.Datos).Result;
            response.EnsureSuccessStatusCode();
            byte[] contenidoBytes = response.Content.ReadAsByteArrayAsync().Result;
            string contenido = System.Text.Encoding.Latin1.GetString(contenidoBytes);
            return contenido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return error;
        }
    }


    private void ConfigureApi()
    {
        try
        {
            if (_config == null) return;
            var apiconfig = new Configuration();
            apiconfig.BasePath = _credManager.GetCredential(_config.BaseUrl) ?? string.Empty;
            apiconfig.AddApiKey("api_key", _credManager.GetCredential(_config.ApiKey) ?? string.Empty );
            _prediccionesEspecificasApi = new PrediccionesEspecificasApi(apiconfig);
            _prediccionesNormalizadasTextoApi = new PrediccionesNormalizadasTextoApi(apiconfig);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}