# Org.OpenAPITools.Api.AvisosCapApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**AvisosDeFenmenosMeteorolgicosAdversosArchivo**](AvisosCapApi.md#avisosdefenmenosmeteorolgicosadversosarchivo) | **GET** /api/avisos_cap/archivo/fechaini/{fechaIniStr}/fechafin/{fechaFinStr} | Avisos de Fenómenos Meteorológicos Adversos. Archivo. |
| [**AvisosDeFenmenosMeteorolgicosAdversosLtimo**](AvisosCapApi.md#avisosdefenmenosmeteorolgicosadversosltimo) | **GET** /api/avisos_cap/ultimoelaborado/area/{area} | Avisos de Fenómenos Meteorológicos Adversos. Último. |

<a id="avisosdefenmenosmeteorolgicosadversosarchivo"></a>
# **AvisosDeFenmenosMeteorolgicosAdversosArchivo**
> Model200 AvisosDeFenmenosMeteorolgicosAdversosArchivo (string fechaIniStr, string fechaFinStr)

Avisos de Fenómenos Meteorológicos Adversos. Archivo.

 Avisos de Fenómenos Meteorológicos adversos para el rango de fechas seleccionado (datos desde 18/06/2018).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class AvisosDeFenmenosMeteorolgicosAdversosArchivoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new AvisosCapApi(config);
            var fechaIniStr = "fechaIniStr_example";  // string | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC)
            var fechaFinStr = "fechaFinStr_example";  // string | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC)

            try
            {
                // Avisos de Fenómenos Meteorológicos Adversos. Archivo.
                Model200 result = apiInstance.AvisosDeFenmenosMeteorolgicosAdversosArchivo(fechaIniStr, fechaFinStr);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling AvisosCapApi.AvisosDeFenmenosMeteorolgicosAdversosArchivo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AvisosDeFenmenosMeteorolgicosAdversosArchivoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Avisos de Fenómenos Meteorológicos Adversos. Archivo.
    ApiResponse<Model200> response = apiInstance.AvisosDeFenmenosMeteorolgicosAdversosArchivoWithHttpInfo(fechaIniStr, fechaFinStr);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling AvisosCapApi.AvisosDeFenmenosMeteorolgicosAdversosArchivoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fechaIniStr** | **string** | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **fechaFinStr** | **string** | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC) |  |

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

<a id="avisosdefenmenosmeteorolgicosadversosltimo"></a>
# **AvisosDeFenmenosMeteorolgicosAdversosLtimo**
> Model200 AvisosDeFenmenosMeteorolgicosAdversosLtimo (string area)

Avisos de Fenómenos Meteorológicos Adversos. Último.

 Últimos Avisos de Fenómenos Meteorológicos adversos elaborado para el área seleccionada.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class AvisosDeFenmenosMeteorolgicosAdversosLtimoExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new AvisosCapApi(config);
            var area = "area_example";  // string |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | esp  | España| | 61  | Andalucía   | | 62  | Aragón   | | 63  | Asturias, Principado de  | | 64  | Ballears, Illes   | | 78  | Ceuta   | | 65  | Canarias   | | 66  | Cantabria   | | 67  | Castilla y León   | | 68  | Castilla - La Mancha   | | 69  | Cataluña   | | 77  | Comunitat Valenciana   | | 70  | Extremadura   | | 71  | Galicia   | | 72  | Madrid, Comunidad de    | | 79  | Melilla   | | 73  | Murcia, Región de   | | 74  | Navarra, Comunidad Foral de   | | 75  | País Vasco | | 76  | Rioja, La

            try
            {
                // Avisos de Fenómenos Meteorológicos Adversos. Último.
                Model200 result = apiInstance.AvisosDeFenmenosMeteorolgicosAdversosLtimo(area);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling AvisosCapApi.AvisosDeFenmenosMeteorolgicosAdversosLtimo: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the AvisosDeFenmenosMeteorolgicosAdversosLtimoWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Avisos de Fenómenos Meteorológicos Adversos. Último.
    ApiResponse<Model200> response = apiInstance.AvisosDeFenmenosMeteorolgicosAdversosLtimoWithHttpInfo(area);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling AvisosCapApi.AvisosDeFenmenosMeteorolgicosAdversosLtimoWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **area** | **string** |  | Código | Área | |- -- -- -- -- -|- -- -- -- -- -| | esp  | España| | 61  | Andalucía   | | 62  | Aragón   | | 63  | Asturias, Principado de  | | 64  | Ballears, Illes   | | 78  | Ceuta   | | 65  | Canarias   | | 66  | Cantabria   | | 67  | Castilla y León   | | 68  | Castilla - La Mancha   | | 69  | Cataluña   | | 77  | Comunitat Valenciana   | | 70  | Extremadura   | | 71  | Galicia   | | 72  | Madrid, Comunidad de    | | 79  | Melilla   | | 73  | Murcia, Región de   | | 74  | Navarra, Comunidad Foral de   | | 75  | País Vasco | | 76  | Rioja, La |  |

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

