# Org.OpenAPITools.Api.RedRadaresApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ImagenComposicinNacionalRadaresTiempoActualEstndar**](RedRadaresApi.md#imagencomposicinnacionalradarestiempoactualestndar) | **GET** /api/red/radar/nacional | Imagen composición nacional radares. Tiempo actual estándar. |
| [**RadarRegional**](RedRadaresApi.md#radarregional) | **GET** /api/red/radar/regional/{radar} | Imagen gráfica radar regional. Tiempo actual estándar. |

<a id="imagencomposicinnacionalradarestiempoactualestndar"></a>
# **ImagenComposicinNacionalRadaresTiempoActualEstndar**
> Model200 ImagenComposicinNacionalRadaresTiempoActualEstndar ()

Imagen composición nacional radares. Tiempo actual estándar.

Imagen composición nacional radares. Tiempo actual estándar. Periodicidad: 30 minutos.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class ImagenComposicinNacionalRadaresTiempoActualEstndarExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedRadaresApi(config);

            try
            {
                // Imagen composición nacional radares. Tiempo actual estándar.
                Model200 result = apiInstance.ImagenComposicinNacionalRadaresTiempoActualEstndar();
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedRadaresApi.ImagenComposicinNacionalRadaresTiempoActualEstndar: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ImagenComposicinNacionalRadaresTiempoActualEstndarWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Imagen composición nacional radares. Tiempo actual estándar.
    ApiResponse<Model200> response = apiInstance.ImagenComposicinNacionalRadaresTiempoActualEstndarWithHttpInfo();
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedRadaresApi.ImagenComposicinNacionalRadaresTiempoActualEstndarWithHttpInfo: " + e.Message);
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

<a id="radarregional"></a>
# **RadarRegional**
> Model200 RadarRegional (string radar)

Imagen gráfica radar regional. Tiempo actual estándar.

Imagen del radar regional de la región pasada por parámetro. Tiempo actual estándar. Periodicidad: 10 minutos.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class RadarRegionalExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new RedRadaresApi(config);
            var radar = "radar_example";  // string |  | Código | Radar | |- -- -- -- -- -|- -- -- -- -- -| | am  | Almería   | | sa  | Asturias   | | pm  | Illes Balears   | | ba  | Barcelona  | | cc  | Cáceres   | | co  | A Coruña   | | ma  | Madrid   | | ml  | Málaga   | | mu  | Murcia   | | vd  | Palencia   | | ca  | Las Palmas   | | se  | Sevilla   | | va  | Valencia   | | ss  | Vizcaya   | | za  | Zaragoza   

            try
            {
                // Imagen gráfica radar regional. Tiempo actual estándar.
                Model200 result = apiInstance.RadarRegional(radar);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RedRadaresApi.RadarRegional: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RadarRegionalWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Imagen gráfica radar regional. Tiempo actual estándar.
    ApiResponse<Model200> response = apiInstance.RadarRegionalWithHttpInfo(radar);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RedRadaresApi.RadarRegionalWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **radar** | **string** |  | Código | Radar | |- -- -- -- -- -|- -- -- -- -- -| | am  | Almería   | | sa  | Asturias   | | pm  | Illes Balears   | | ba  | Barcelona  | | cc  | Cáceres   | | co  | A Coruña   | | ma  | Madrid   | | ml  | Málaga   | | mu  | Murcia   | | vd  | Palencia   | | ca  | Las Palmas   | | se  | Sevilla   | | va  | Valencia   | | ss  | Vizcaya   | | za  | Zaragoza    |  |

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

