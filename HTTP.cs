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
//namespace mapp.core {

//	using HttpMethod = WHttpRequest.HttpMethod;

//	public delegate void HttpFileConstrictionAction(HttpFileConstructor constructor);
	

	

	

//	public class HttpResponse: IWebResponse<HttpResponseMessage> {

//		public static HttpResponse Empty {
//			get {
//				return new HttpResponse();
//			}
//		}

//		public static HttpResponse EmptyWithStatus(HttpStatusCode statusCode) {
//			var result = new HttpResponse();
//			result._statusCode = statusCode;
//			return result;
//		}

//		#region IWebReponse

//		public WebResponseStatus Status {
//			get {
//				return this.Response == null ? WebResponseStatus.Failed : WebResponseStatus.Success;
//			}
//		}

//		public Task<String> String {
//			get {
//				if (this.Response == null) {
//					return Task.Run(() => string.Empty);
//				}

//				var resultString = this.Response.Content.ReadAsStringAsync();
//				return resultString;
//			}
//		}

//		public Task<Stream> Stream {
//			get {
//				if (this.Response == null) {
//					return Task.Run<Stream>(() => (Stream)null);
//				}

//				var resultStream = this.Response.Content.ReadAsStreamAsync();
//				return resultStream;
//			}
//		}

//		public HttpResponseMessage Response {
//			private set;
//			get;
//		}

//		private HttpStatusCode? _statusCode;

//		public HttpStatusCode StatusCode {
//			get {
//				return this.Response?.StatusCode ?? this._statusCode ?? HttpStatusCode.BadRequest;
//			}
//		}

//		#endregion

//		public async Task<T> Data<T>() {
//			if (this.Response == null) {
//				return default(T);
//			}

//			try {
//				var jsonString = await this.String;
//				var result = JsonConvert.DeserializeObject<T>(jsonString);
//				return result;
//			} catch (Exception ex) {
//				Debug.WriteLine(ex);
//				return default(T);
//			}
//		}

//		public HttpResponse() : this(null) {
//		}

//		public HttpResponse(HttpResponseMessage response) {
//			this.Response = response;
//		}
//	}

//	public class HttpFileConstructor {

//		private MultipartFormDataContent content;

//		public HttpFileConstructor(MultipartFormDataContent content) {
//			this.content = content;
//		}

//		public void AddFile(string parameterName, string fileName, byte[] bytes, string mimeType = null) {
//			var byteContent = new ByteArrayContent(bytes);
//			this.AddContent(parameterName, fileName, byteContent, mimeType);
//		}

//		public void AddFile(string parameterName, string fileName, Stream stream, string mimeType = null) {
//			var streamContent = new StreamContent(stream);
//			this.AddContent(parameterName, fileName, streamContent, mimeType);
//		}

//		private void AddContent(string parameterName, string fileName, HttpContent content, string mimeType) {
			//if (mimeType != null) {
			//	MediaTypeHeaderValue mediaType;
			//	if (MediaTypeHeaderValue.TryParse(mimeType, out mediaType)) {
			//		content.Headers.ContentType = mediaType;
			//	}
			//}

			//this.content.Add(content, parameterName, fileName);
//		}
//	}

//	public class WHttpRequest: IWebRequest<HttpMethod> {

//		public enum HttpMethod {
//			GET,
//			POST,
//			PUT,
//			DELETE,
//			HEAD,
//			OPTIONS
//		}

//		#region IWebRequest

//		public string Path { get; set; }

//		public HttpMethod Method { get; set; }

//		public TimeSpan Timeout { get; set; }

//		public bool UseBaseUrl { get; set; }

//		public Uri BuildRequestUri(string baseUrl = null) {
//			try {
				//Uri resultUri;
				//UriBuilder uriBuilder;

				//if ((baseUrl?.Length ?? 0) != 0
				//	&& this.UseBaseUrl) {
				//	uriBuilder = new UriBuilder(baseUrl);
				//	uriBuilder.Path = this.Path;
				//} else {
				//	uriBuilder = new UriBuilder(this.Path);
				//}

				//if (this.Method == HttpMethod.GET
				//    && (this.Parameters?.Count ?? 0) != 0) {
				//	var queryString = this.parameterParser.ParseQuery(this.Parameters);
				//	uriBuilder.Query = queryString;
				//}

				//resultUri = uriBuilder.Uri;

//				return resultUri;
//			} catch (Exception ex) {
//				Debug.WriteLine(ex);
//				return null;
//			}
//		}

//		#endregion

//		public HttpHeaders Headers;
//		public HttpParameters Parameters;
//		public HttpFileConstrictionAction ConstructionAction;

//		protected HttpParameterParser parameterParser;

//		public WHttpRequest() : this(HttpParameterParser.Instance) {
//		}

//		public WHttpRequest(HttpParameterParser parser) {
//			this.Path = null;
//			this.Method = HttpMethod.GET;
//			this.Headers = null;
//			this.Parameters = null;
//			this.ConstructionAction = null;
//			this.Timeout = TimeSpan.FromSeconds(100);

//			this.parameterParser = parser;

//			this.UseBaseUrl = true;
//		}

		//public HttpRequestMessage HttpRequest {
		//	get {
		//		var resultRequest = new HttpRequestMessage();
		//		resultRequest.Method = this.Method.HttpMethod();

		//		if ((this.Headers?.Count ?? 0) != 0) {
		//			foreach (var header in this.Headers) {
		//				resultRequest.Headers.Add(header.Key, header.Value);
		//			}
		//		}

		//		if (this.Method != HttpMethod.GET
		//		    && (this.Parameters?.Count ?? 0) != 0) {

		//			if (this.ConstructionAction != null) {
		//				var multipartContent = new MultipartFormDataContent();

						

		//				var fileConstructor = new HttpFileConstructor(multipartContent);
		//				this.ConstructionAction(fileConstructor);

		//				resultRequest.Content = multipartContent;

		//			} else {
		//				var parsedParameters = this.parameterParser.Parse(this.Parameters);
		//				var formContent = new FormUrlEncodedContent(parsedParameters);
		//				resultRequest.Content = formContent;
		//			}
		//		}

		//		return resultRequest;
		//	}
		//}
//	}

//	public class HttpPerformer: IWebPerformer<HttpResponse, WHttpRequest> {

//		//private NativeCookieHandler cookieHandler;

//		public HttpClient HttpClient {
//			get {
//				//var httpMessageHandler = new NativeMessageHandler(false, false, this.cookieHandler);
//				//httpMessageHandler.AllowAutoRedirect = true;
//				//var client = new HttpClient(httpMessageHandler);

//				//if ((this.Headers?.Count ?? 0) != 0) {
//				//	foreach (var header in this.Headers) {
//				//		client.DefaultRequestHeaders.Add(header.Key, header.Value);
//				//	}
//				//}

//				//return client;
//			}
//		}

//		#region IWebPerformer

//		public string BaseUrl { get; set; }

//		public async Task<HttpResponse> Perform(WHttpRequest request) {
//		//	if (request == null) {
//				return HttpResponse.Empty;
//		//	}

//		//	var requestUri = request.BuildRequestUri(this.BaseUrl);

//		//	if (requestUri == null) {
//		//		return HttpResponse.Empty;
//		//	}

//		//	try {
//		//		var client = this.HttpClient;
//		//		var clientRequest = request.HttpRequest;
//		//		clientRequest.RequestUri = requestUri;
//		//		client.Timeout = request.Timeout;

//		//		var clientResponse = client.SendAsync(clientRequest);
//		//		var resultResponse = new HttpResponse(await clientResponse);

//		//		return resultResponse;
//		//	} catch (WebException serverException) {
//		//		Debug.WriteLine(serverException);
//		//		var statusCode = serverException.HttpStatusCode();
//		//		return HttpResponse.EmptyWithStatus(statusCode);
//		//	} catch (TaskCanceledException timeout) {
//		//		Debug.WriteLine(timeout);
//		//		return HttpResponse.EmptyWithStatus(HttpStatusCode.RequestTimeout);
//		//	} catch (Exception ex) {
//		//		Debug.WriteLine(ex);
//		//		return HttpResponse.Empty;
//		//	}
//		}

//		#endregion

//		public HttpHeaders Headers;

//		public HttpPerformer() {
//			this.BaseUrl = null;
//			this.Headers = null;
//		}

//		//public HttpPerformer(NativeCookieHandler cookieHandler) {
			
//		//	this.cookieHandler = cookieHandler;
//		//}
//	}

//	//[DependsOn(typeof(IOService))]
//	//public abstract class HttpService : Service {

//	//	protected NativeCookieHandler cookieHandler;

//	//	public HttpPerformer RequestPerformer {
//	//		private set;
//	//		get;
//	//	}

//	//	#region Service

//	//	public override async Task Initialize() {
// //           await Task.Run(() => {
// //               this.cookieHandler = new NativeCookieHandler();
//	//		    this.RequestPerformer = new HttpPerformer(this.cookieHandler);
// //           });
//	//	}

//	//	#endregion

//	//	#region Cookies methods

//	//	public virtual void SetCookie(Cookie cookie) {
//	//		this.SetCookies(new []{ cookie });
//	//	}

//	//	public virtual void SetCookies(IEnumerable<Cookie> cookies) {
//	//		this.cookieHandler.SetCookies(cookies);
//	//	}

//	//	public virtual void DeleteCookies(string name = null, string domain = null) {
//	//		var cookieList = this.GetCookies(name, domain);
//	//		this.DeleteCookies(cookieList);
//	//	}

//	//	public virtual void DeleteCookie(Cookie cookie) {
//	//		this.DeleteCookies(new [] { cookie });
//	//	}

//	//	public abstract void DeleteCookies(IEnumerable<Cookie> cookieList);

//	//	public virtual IEnumerable<Cookie> GetCookies(string name = null, string domain = null) {
//	//		var comparison = StringComparison.CurrentCultureIgnoreCase;
//	//		var cookieList = this.cookieHandler.Cookies
//	//			.Where(item => (name == null || string.Equals(item.Name, name, comparison))
//	//		                 && (domain == null || string.Equals(item.Domain, domain, comparison)));

//	//		return cookieList;
//	//	}

//	//	#endregion

//	//	#region Request methods

//	//	public Task<HttpResponse> Perform(HttpRequest request) {
//	//		return this.RequestPerformer?.Perform(request);
//	//	}

//	//	#endregion
//	//}
//}
