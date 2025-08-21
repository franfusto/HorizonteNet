# Org.OpenAPITools.Api.RedRayosApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaborado**](RedRayosApi.md#mapaconlosrayosregistradosenperiodostandardltimoelaborado) | **GET** /api/red/rayos/mapa | Mapa con los rayos registrados en periodo standard. Último elaborado. |

<a id="mapaconlosrayosregistradosenperiodostandardltimoelaborado"></a>
# **MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaborado**
> Model200 MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaborado ()

Mapa con los rayos registrados en periodo standard. Último elaborado.

Imagen de las descargas caídas en el territorio nacional durante un período de 12 horas.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaboradoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedRayosApi(config);

            try
            {
                // Mapa con los rayos registrados en periodo standard. Último elaborado.
                Model200 result = apiInstance.MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaborado();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedRayosApi.MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaborado: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaboradoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Mapa con los rayos registrados en periodo standard. Último elaborado.
    ApiResponse<Model200> response = apiInstance.MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaboradoWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedRayosApi.MapaConLosRayosRegistradosEnPeriodoStandardLtimoElaboradoWithHttpInfo: " + e.Message);
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

