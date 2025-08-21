# Org.OpenAPITools.Api.PrediccionMaritimaApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**PrediccinMartimaCostera**](PrediccionMaritimaApi.md#prediccinmartimacostera) | **GET** /api/prediccion/maritima/costera/costa/{costa} | Predicción marítima costera. |
| [**PrediccinMartimaDeAltaMar**](PrediccionMaritimaApi.md#prediccinmartimadealtamar) | **GET** /api/prediccion/maritima/altamar/area/{area} | Predicción marítima de alta mar. |

<a id="prediccinmartimacostera"></a>
# **PrediccinMartimaCostera**
> Model200 PrediccinMartimaCostera (string costa)

Predicción marítima costera.

Predicción para un periodo de 24 horas de las condiciones meteorológicas para la zona costera pasada por parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinMartimaCosteraExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionMaritimaApi(config);
            var costa = "costa_example";  // string |  | Código | Área Costera | |- -- -- -- -- -|- -- -- -- -- -| | 42 | Costa de Andalucía Occidental y Ceuta   | | 47  | Costa de Andalucía Oriental y Melilla   | | 41  | Costa de Asturias, Cantabria y País Vasco  | | 45  | Costa de Cataluña   | | 40  | Costa de Galicia   | | 44  | Costa de Illes Balears   | | 43  | Costa de las Islas Canarias  | | 46  | Costa de Valencia y Murcia

            try
            {
                // Predicción marítima costera.
                Model200 result = apiInstance.PrediccinMartimaCostera(costa);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionMaritimaApi.PrediccinMartimaCostera: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinMartimaCosteraWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción marítima costera.
    ApiResponse<Model200> response = apiInstance.PrediccinMartimaCosteraWithHttpInfo(costa);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionMaritimaApi.PrediccinMartimaCosteraWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **costa** | **string** |  | Código | Área Costera | |- -- -- -- -- -|- -- -- -- -- -| | 42 | Costa de Andalucía Occidental y Ceuta   | | 47  | Costa de Andalucía Oriental y Melilla   | | 41  | Costa de Asturias, Cantabria y País Vasco  | | 45  | Costa de Cataluña   | | 40  | Costa de Galicia   | | 44  | Costa de Illes Balears   | | 43  | Costa de las Islas Canarias  | | 46  | Costa de Valencia y Murcia |  |

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

<a id="prediccinmartimadealtamar"></a>
# **PrediccinMartimaDeAltaMar**
> Model200 PrediccinMartimaDeAltaMar (string area)

Predicción marítima de alta mar.

Predicción para un periodo de 24 horas de las condiciones meteorológicas para el área marítima pasada por parámetro.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class PrediccinMartimaDeAltaMarExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new PrediccionMaritimaApi(config);
            var area = "area_example";  // string |  | Código | Área de Alta Mar | |- -- -- -- -- -|- -- -- -- -- -| | 0 | Océano Atlántico al sur de 35º N   | | 1  | Océano Atlántico al norte de 30º N   | | 2  | Mar Mediterráneo

            try
            {
                // Predicción marítima de alta mar.
                Model200 result = apiInstance.PrediccinMartimaDeAltaMar(area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling PrediccionMaritimaApi.PrediccinMartimaDeAltaMar: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the PrediccinMartimaDeAltaMarWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Predicción marítima de alta mar.
    ApiResponse<Model200> response = apiInstance.PrediccinMartimaDeAltaMarWithHttpInfo(area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling PrediccionMaritimaApi.PrediccinMartimaDeAltaMarWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código | Área de Alta Mar | |- -- -- -- -- -|- -- -- -- -- -| | 0 | Océano Atlántico al sur de 35º N   | | 1  | Océano Atlántico al norte de 30º N   | | 2  | Mar Mediterráneo |  |

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

