# Org.OpenAPITools.Api.MapasYGraficosApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**MapasDeAnlisisLtimaPasada**](MapasYGraficosApi.md#mapasdeanlisisltimapasada) | **GET** /api/mapasygraficos/analisis | Mapas de análisis. Última pasada. |
| [**MapasSignificativosTiempoActual**](MapasYGraficosApi.md#mapassignificativostiempoactual) | **GET** /api/mapasygraficos/mapassignificativos/fecha/{fecha}/{ambito}/{dia} | Mapas significativos. Tiempo actual. |

<a id="mapasdeanlisisltimapasada"></a>
# **MapasDeAnlisisLtimaPasada**
> Model200 MapasDeAnlisisLtimaPasada ()

Mapas de análisis. Última pasada.

Estos mapas muestran la configuración de la presión en superficie usando isobaras (lineas de igual presión), áreas de alta (A, a) y baja (B, b) presión y los frentes en Europa y el Atlántico Norte.El mapa de análisis presenta el estado de la atmósfera a la hora correspondiente y los fenómenos más relevantes observados en España. Periodicidad de actualización: cada 12 horas (00, 12).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MapasDeAnlisisLtimaPasadaExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new MapasYGraficosApi(config);

            try
            {
                // Mapas de análisis. Última pasada.
                Model200 result = apiInstance.MapasDeAnlisisLtimaPasada();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling MapasYGraficosApi.MapasDeAnlisisLtimaPasada: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MapasDeAnlisisLtimaPasadaWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mapas de análisis. Última pasada.
    ApiResponse<Model200> response = apiInstance.MapasDeAnlisisLtimaPasadaWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling MapasYGraficosApi.MapasDeAnlisisLtimaPasadaWithHttpInfo: " + e.Message);
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

<a id="mapassignificativostiempoactual"></a>
# **MapasSignificativosTiempoActual**
> Model200 MapasSignificativosTiempoActual (string fecha, string ambito, string dia)

Mapas significativos. Tiempo actual.

Mapas significativos de ámbito nacional o CCAA, para una fecha dada y ese mismo día (D+0),  al día siguiente (D+1) o a los dos días (D+2), en el periodo horario de (00_12) ó (12-24). Hasta el 22/01/2020.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MapasSignificativosTiempoActualExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new MapasYGraficosApi(config);
            var fecha = "fecha_example";  // string | Fecha de elaboración (AAAA-MM-DD)
            var ambito = "ambito_example";  // string |  | Código | Ámbito | |- -- -- -- -- -|- -- -- -- -- -| | esp  | España| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La
            var dia = "dia_example";  // string |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | a | D+0 (00-12)  | | b  | D+0 (12-24)   | |  c | D+1 (00-12)  | | d  | D+1 (12-24) | | e  | D+2 (00-12) | | f  | D+2 (12-24)

            try
            {
                // Mapas significativos. Tiempo actual.
                Model200 result = apiInstance.MapasSignificativosTiempoActual(fecha, ambito, dia);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling MapasYGraficosApi.MapasSignificativosTiempoActual: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MapasSignificativosTiempoActualWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mapas significativos. Tiempo actual.
    ApiResponse<Model200> response = apiInstance.MapasSignificativosTiempoActualWithHttpInfo(fecha, ambito, dia);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling MapasYGraficosApi.MapasSignificativosTiempoActualWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fecha** | **string** | Fecha de elaboración (AAAA-MM-DD) |  |
| **ambito** | **string** |  | Código | Ámbito | |- -- -- -- -- -|- -- -- -- -- -| | esp  | España| | and  | Andalucía   | | arn  | Aragón   | | ast  | Asturias  | | bal  | Ballears, Illes   | | coo  | Canarias   | | can  | Cantabria   | | cle  | Castilla y León   | | clm  | Castilla - La Mancha   | | cat  | Cataluña   | | val  | Comunitat Valenciana   | | ext  | Extremadura   | | gal  | Galicia   | | mad  | Madrid, Comunidad de    | | mur  | Murcia, Región de   | | nav  | Navarra, Comunidad Foral de   | | pva  | País Vasco | | rio  | Rioja, La |  |
| **dia** | **string** |  | Código de día | Día | |- -- -- -- -- -|- -- -- -- -- -| | a | D+0 (00-12)  | | b  | D+0 (12-24)   | |  c | D+1 (00-12)  | | d  | D+1 (12-24) | | e  | D+2 (00-12) | | f  | D+2 (12-24) |  |

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

