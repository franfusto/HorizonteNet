using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Horizonte.Samples.Aemet.Widgets;
using Horizonte;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Horizonte.Samples.Aemet;

[HorizonteModule("Horizonte.Samples.Aemet")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    private AemetConfig? _config;
    private PrediccionesEspecificasApi? _prediccionesEspecificasApi = new PrediccionesEspecificasApi();

    private PrediccionesNormalizadasTextoApi?
        _prediccionesNormalizadasTextoApi = new PrediccionesNormalizadasTextoApi();


    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }

    // module

    [HorizonteRole("init")]
    [HorizonteCommand("Aemet_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        _config = context?.Get<AemetConfig>() ?? new AemetConfig();
        ConfigureApi();
        _logger?.LogInformation("Módulo Aemet Inciciado");
        return true;
    }

    [HorizonteRole("configpage")]
    [HorizonteCommand("Aemet_ConfigPage", "Widget de configuración del módulo Aemet")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(AemetConfigWidget), Parameters = null };


    //aemet service

    //predicción por comunidad autonoma

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
            _logger?.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


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
            _logger?.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


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
            _logger?.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }

    // predicción nivel nacional

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
            _logger?.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


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
            _logger?.LogError(e.ToString());
            return "Se ha producido un error, no ha sido posible realizar la consulta.";
        }
    }


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
            _logger?.LogError(e.ToString());
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
            _logger?.LogError(ex.ToString());
            return error;
        }
    }


    private void ConfigureApi()
    {
        try
        {
            if (_config == null) return;
            var apiconfig = new Configuration();
            apiconfig.BasePath = _config.BaseUrl;
            apiconfig.AddApiKey("api_key", _config.ApiKey);
            _prediccionesEspecificasApi = new PrediccionesEspecificasApi(apiconfig);
            _prediccionesNormalizadasTextoApi = new PrediccionesNormalizadasTextoApi(apiconfig);
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }
    }
}