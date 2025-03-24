using Jint;
using Jint.Native;
using Jint.Native.Json;

namespace YATsDB.Server.Services.Implementation.JsEngine;

internal class HttpFunctions
{
    private readonly Engine engine;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly bool isEnabled;

    public HttpFunctions(Engine engine, IHttpClientFactory httpClientFactory, bool isEnabled)
    {
        this.engine = engine;
        this.httpClientFactory = httpClientFactory;
        this.isEnabled = isEnabled;
    }

    public JsValue GetJson(string url)
    {
        return GetJsonAsync(url, null).GetAwaiter().GetResult();
    }

    public JsValue GetJson(string url, IDictionary<string, object>? headers)
    {
        return GetJsonAsync(url, headers).GetAwaiter().GetResult();
    }

    public Task<JsValue> GetJsonAsync(string url)
    {
        return GetJsonAsync(url, null);
    }

    public async Task<JsValue> GetJsonAsync(string url, IDictionary<string, object>? headers)
    {
        CheckEnabled();

        var httpClient = httpClientFactory.CreateClient();
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
            url);

        if (headers != null)
        {
            foreach ((var headerName, var headerValue) in headers)
            {
                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }
        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        using var httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        return new JsonParser(engine).Parse(content);
    }

    public async Task<JsValue> PostJsonAsync(string url, IDictionary<string, object> json,
        IDictionary<string, object>? headers)
    {
        CheckEnabled();

        var httpClient = httpClientFactory.CreateClient();
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
            url);

        if (headers != null)
        {
            foreach ((var headerName, var headerValue) in headers)
            {
                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }
        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        httpRequestMessage.Content = JsonContent.Create(json);

        using var httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            return JsValue.Null;
        }

        return new JsonParser(engine).Parse(content);
    }

    public Task<JsValue> PostStringAsync(string url, string contentValue)
    {
        return PostStringAsync(url, contentValue, null);
    }

    public async Task<JsValue> PostStringAsync(string url, string contentValue, IDictionary<string, object>? headers)
    {
        CheckEnabled();

        var httpClient = httpClientFactory.CreateClient();
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
            url);

        if (headers != null)
        {
            foreach ((var headerName, var headerValue) in headers)
            {
                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }
        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        httpRequestMessage.Content = new StringContent(contentValue);

        using var httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        var content = await httpResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            return JsValue.Null;
        }

        return new JsonParser(engine).Parse(content);
    }

    public Task<JsValue> PostJsonAsync(string url, IDictionary<string, object> json)
    {
        return PostJsonAsync(url, json, null);
    }

    private void CheckEnabled()
    {
        if (!isEnabled)
        {
            throw new JsApiException("HTTP API is not enabled.");
        }
    }
}