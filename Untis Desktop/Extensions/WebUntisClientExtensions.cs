using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WebUntisAPI.Client;
using System.Threading;

namespace UntisDesktop.Extensions;

internal static class WebUntisClientExtensions
{
    private static FieldInfo _clientField = typeof(WebUntisClient).GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static FieldInfo _bearerTokenField = typeof(WebUntisClient).GetField("_bearerToken", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static FieldInfo _schoolNameField = typeof(WebUntisClient).GetField("_schoolName", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static FieldInfo _sessionIdField = typeof(WebUntisClient).GetField("_sessionId", BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// A fixed <see cref="WebUntisClient.ReloadSessionAsync(CancellationToken)"/> method
    /// </summary>
    /// <param name="client"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public static async Task ReloadSessionFixAsync(this WebUntisClient client, CancellationToken ct = default)
    {
        HttpRequestMessage httpRequestMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(client.ServerUrl + "/WebUntis/api/token/new")
        };
        httpRequestMessage.Headers.Add("Cookie", new[]
        {
            $"schoolname=\"{_schoolNameField.GetValue(client)!}\"",
            $"JSESSIONID=\"{_sessionIdField.GetValue(client)!}\""
        });

        HttpResponseMessage httpResponseMessage = await ((HttpClient)_clientField.GetValue(client)!).SendAsync(httpRequestMessage, ct);
        if (!ct.IsCancellationRequested)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"There was an error while the http request (Code: {httpResponseMessage.StatusCode}).");
            }

            _bearerTokenField.SetValue(client, await httpResponseMessage.Content.ReadAsStringAsync(ct));
        }
    }
}
