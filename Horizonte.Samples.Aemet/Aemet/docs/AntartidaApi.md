# Org.OpenAPITools.Api.AntartidaApi

All URIs are relative to *https://opendata.aemet.es/opendata*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**DatosAntrtida**](AntartidaApi.md#datosantrtida) | **GET** /api/antartida/datos/fechaini/{fechaIniStr}/fechafin/{fechaFinStr}/estacion/{identificacion} | Datos Antártida. |

<a id="datosantrtida"></a>
# **DatosAntrtida**
> Model200 DatosAntrtida (string fechaIniStr, string fechaFinStr, string identificacion)

Datos Antártida.

Datos de observación de las campañas Antárticas en las que participa AEMET. Contiene observaciones diezminutales históricas de las estaciones meteorológicas y radiométricas de las bases de Juan Carlos I y Gabriel de Castilla. Frecuencia de actualización: Anual.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Client;
using Org.OpenAPITools.Model;

namespace Example
{
    public class DatosAntrtidaExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "https://opendata.aemet.es/opendata";
            // Configure API key authorization: api_key
            config.AddApiKey("api_key", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // config.AddApiKeyPrefix("api_key", "Bearer");

            var apiInstance = new AntartidaApi(config);
            var fechaIniStr = "fechaIniStr_example";  // string | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC)
            var fechaFinStr = "fechaFinStr_example";  // string | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC)
            var identificacion = "identificacion_example";  // string |  | Identificacion | Estación | |- -- -- -- -- -|- -- -- -- -- -| | 89064      | Estación Meteorológica Juan Carlos I   | | 89064R      | Estación Radiométrica Juan Carlos I| | 89064RA  | Estación Radiométrica Juan Carlos I (hasta 08/03/2007)) | | 89070  | Estación Meteorológica Gabriel de Castilla   

            try
            {
                // Datos Antártida.
                Model200 result = apiInstance.DatosAntrtida(fechaIniStr, fechaFinStr, identificacion);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling AntartidaApi.DatosAntrtida: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the DatosAntrtidaWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Datos Antártida.
    ApiResponse<Model200> response = apiInstance.DatosAntrtidaWithHttpInfo(fechaIniStr, fechaFinStr, identificacion);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling AntartidaApi.DatosAntrtidaWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **fechaIniStr** | **string** | Fecha Inicial (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **fechaFinStr** | **string** | Fecha Final (AAAA-MM-DDTHH:MM:SSUTC) |  |
| **identificacion** | **string** |  | Identificacion | Estación | |- -- -- -- -- -|- -- -- -- -- -| | 89064      | Estación Meteorológica Juan Carlos I   | | 89064R      | Estación Radiométrica Juan Carlos I| | 89064RA  | Estación Radiométrica Juan Carlos I (hasta 08/03/2007)) | | 89070  | Estación Meteorológica Gabriel de Castilla    |  |

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

