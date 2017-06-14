using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace naithar {

    public partial class HTTP {

        public enum Method
        {
            GET,
            POST,
            PUT,
            OPTIONS,
            HEAD,
            DELETE,
        }

        public string BaseURL;

        public static readonly HTTP Instance = new HTTP();

        public HTTP(string BaseURL = null) {
            this.BaseURL = BaseURL;
        }

        private Uri ApplyURL(string url) {
			UriBuilder uriBuilder;

            if ((this.BaseURL?.Length ?? 0) != 0)
			{
                uriBuilder = new UriBuilder(this.BaseURL);
				uriBuilder.Path = url;
			}
			else
			{
				uriBuilder = new UriBuilder(url);
			}


            return uriBuilder.Uri;
        }

        private void ApplyEncode(System.Net.Http.HttpRequestMessage request,
                            HTTP.Method method,
                            HTTP.Parameters parameters, 
                            HTTP.Encoder encoder) {

            HTTP.Encoder requestEncoder = encoder ?? URLEncoder.Instance;

			if ((parameters?.Count ?? 0) != 0
			  && method == Method.GET)
			{
				requestEncoder = URLEncoder.Instance;
			}

            if (requestEncoder != null
                && (parameters?.Count ?? 0) != 0) {
                requestEncoder.Encode(request, parameters);
            }
        }

        private void ApplyHeaders(System.Net.Http.HttpRequestMessage request, 
                             HTTP.Headers headers) {
			if ((headers?.Count ?? 0) != 0)
			{
				foreach (var header in headers)
				{
					request.Headers.Add(header.Key, header.Value);
				}
			}
        }
        private System.Net.Http.HttpRequestMessage Request(string URL, 
                                                           HTTP.Method method, 
                                                           HTTP.Parameters parameters, 
                                                           HTTP.Headers headers,
														   HTTP.Encoder encoder) {
			var request = new HttpRequestMessage();
            request.Method = method.Method();
            request.RequestUri = this.ApplyURL(URL);

            this.ApplyEncode(request, method, parameters, encoder);
            this.ApplyHeaders(request, headers);

			return request;
        }

        public Task<HTTP.Response> Perform(
            string URL,
            HTTP.Method method = Method.GET,
            HTTP.Parameters parameters = null,
            HTTP.Headers headers = null,
            HTTP.Encoder encoder = null) {
            return Task<HTTP.Response>.Run(async () =>
            {
                var client = new System.Net.Http.HttpClient();
                var request = this.Request(URL, method, parameters, headers, encoder);

                return new HTTP.Response(await client.SendAsync(request));
            });
        }
    }
}