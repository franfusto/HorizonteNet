# Org.OpenAPITools.Api.ObservacionConvencionalApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**DatosDeObservacinTiempoActual**](ObservacionConvencionalApi.md#datosdeobservacintiempoactual) | **GET** /api/observacion/convencional/todas | Datos de observación. Tiempo actual. |
| [**DatosDeObservacinTiempoActual1**](ObservacionConvencionalApi.md#datosdeobservacintiempoactual1) | **GET** /api/observacion/convencional/datos/estacion/{idema} | Datos de observación. Tiempo actual. |
| [**MensajesDeObservacinLtimoElaborado**](ObservacionConvencionalApi.md#mensajesdeobservacinltimoelaborado) | **GET** /api/observacion/convencional/mensajes/tipomensaje/{tipomensaje} | Mensajes de observación. Último elaborado. |

<a id="datosdeobservacintiempoactual"></a>
# **DatosDeObservacinTiempoActual**
> Model200 DatosDeObservacinTiempoActual ()

Datos de observación. Tiempo actual.

Datos de observación horarios de las últimas 12 horas todas las estaciones meteorológicas de las que se han recibido datos en ese período. Frecuencia de actualización: continuamente. <br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Observación convencional horaria</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class DatosDeObservacinTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ObservacionConvencionalApi(config);

            try
            {
                // Datos de observación. Tiempo actual.
                Model200 result = apiInstance.DatosDeObservacinTiempoActual();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ObservacionConvencionalApi.DatosDeObservacinTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DatosDeObservacinTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Datos de observación. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.DatosDeObservacinTiempoActualWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ObservacionConvencionalApi.DatosDeObservacinTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters
This endpoint does not need any parameter.
### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="datosdeobservacintiempoactual1"></a>
# **DatosDeObservacinTiempoActual1**
> Model200 DatosDeObservacinTiempoActual1 (string idema)

Datos de observación. Tiempo actual.

Datos de observación horarios de las últimas 12 horas de la estación meterológica que se pasa como parámetro (idema). Frecuencia de actualización: continuamente. <br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Observación convencional horaria</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class DatosDeObservacinTiempoActual1Example
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ObservacionConvencionalApi(config);
            var idema = "idema_example";  // string | Índicativo climatológico de la EMA

            try
            {
                // Datos de observación. Tiempo actual.
                Model200 result = apiInstance.DatosDeObservacinTiempoActual1(idema);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ObservacionConvencionalApi.DatosDeObservacinTiempoActual1: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DatosDeObservacinTiempoActual1WithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Datos de observación. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.DatosDeObservacinTiempoActual1WithHttpInfo(idema);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ObservacionConvencionalApi.DatosDeObservacinTiempoActual1WithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **idema** | **string** | Índicativo climatológico de la EMA |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="mensajesdeobservacinltimoelaborado"></a>
# **MensajesDeObservacinLtimoElaborado**
> Model200 MensajesDeObservacinLtimoElaborado (string tipomensaje)

Mensajes de observación. Último elaborado.

Últimos mensajes de observación. Para los SYNOP y TEMP devuelve los mensajes de las últimas 24 horas y para los CLIMAT de los 40 últimos dias. Se pasa como parámetro el tipo de mensaje que se desea (tipomensaje). El resultado de la petición es un fichero en formato tar.gz, que contiene los boletines en formato json y bufr.<br><br> <a href='https://opendata.aemet.es/centrodedescargas/rssatom'     target='_blank'>Canales RSS</a> disponibles:         <ul>         <li>Observación convencional mensajes: climat</li>         <li>Observación convencional mensajes: synop</li>         <li>Observación convencional mensajes: temp</li>         <li>Observación convencional mensajes: todos</li>         </ul>

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MensajesDeObservacinLtimoElaboradoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new ObservacionConvencionalApi(config);
            var tipomensaje = "tipomensaje_example";  // string |  | Código | Tipo de Mensaje | |- -- -- -- -- -|- -- -- -- -- -| | climat  | climat   | | synop  | synop   | | temp  | temp  

            try
            {
                // Mensajes de observación. Último elaborado.
                Model200 result = apiInstance.MensajesDeObservacinLtimoElaborado(tipomensaje);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ObservacionConvencionalApi.MensajesDeObservacinLtimoElaborado: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MensajesDeObservacinLtimoElaboradoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mensajes de observación. Último elaborado.
    ApiResponse<Model200> response = apiInstance.MensajesDeObservacinLtimoElaboradoWithHttpInfo(tipomensaje);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ObservacionConvencionalApi.MensajesDeObservacinLtimoElaboradoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **tipomensaje** | **string** |  | Código | Tipo de Mensaje | |- -- -- -- -- -|- -- -- -- -- -| | climat  | climat   | | synop  | synop   | | temp  | temp   |  |

### Return type

[**Model200**](Model200.md)

### Authorization

[api_key](../README.md#api_key)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | respuesta con éxito |  -  |
| **401** | petición no autorizada |  -  |
| **404** | petición sin datos |  -  |
| **429** | petición que sobrepasa los límites del servicio |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

