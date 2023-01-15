using CSharp.Net.Util;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


public static class IHttpClientFactoryExtension
{
    /// <summary>
    /// IHttpClientFactoryExtension
    /// 1.services.AddHttpClient();
    /// </summary>
    /// <param name="clientFactory"></param>
    /// <param name="url"></param>
    /// <param name="httpMethod"></param>
    /// <param name="httpContent">null,FormUrlEncodedContent,StringContent,MultipartFormDataContent</param>
    /// <param name="timeOut">default 3s.</param>
    /// <param name="connectionClose">default true.</param>
    /// <returns></returns>
    public static async Task<HttpResponseDto> SendAsync(this IHttpClientFactory clientFactory, string url, HttpMethod httpMethod, HttpContent httpContent = null, uint timeOut = 3, bool connectionClose = true)
    {
        HttpResponseDto ret = new HttpResponseDto();
        var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.ConnectionClose = connectionClose;
        client.Timeout = TimeSpan.FromSeconds(timeOut);

        var request = new HttpRequestMessage(httpMethod, url);
        request.Content = httpContent;

        var response = await client.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            ret.Data = res;
            ret.Success = true;
        }
        else
        {
            ret.Success = false;
        }
        ret.StatusCode = response.StatusCode.ToString();
        return ret;
    }
}

