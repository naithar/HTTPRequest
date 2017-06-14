using System;
using mapp.core;
using mapp.IoC;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using ModernHttpClient;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

//TODO: summary, comments.

namespace mapp.core {

	using HttpMethod = HttpRequest.HttpMethod;

	public delegate void HttpFileConstrictionAction(HttpFileConstructor constructor);
	public class HttpParameters : Dictionary<string, object> {

	}

	public class HttpHeaders : Dictionary<string, IEnumerable<string>> {

	}

	public class HttpArray : List<object> {

		public HttpArray() : base() {
		}

		public HttpArray(IEnumerable<object> list) : base() {
			this.AddRange(list);
		}
	}

	public class HttpDictionary : Dictionary<object, object> {

		public HttpDictionary() : base() {
		}

		public HttpDictionary(IDictionary<object, object> dictionary) : base() {
			this.AddRange(dictionary);
		}
	}

	public class HttpResponse: IWebResponse<HttpResponseMessage> {

		public static HttpResponse Empty {
			get {
				return new HttpResponse();
			}
		}

		public static HttpResponse EmptyWithStatus(HttpStatusCode statusCode) {
			var result = new HttpResponse();
			result._statusCode = statusCode;
			return result;
		}

		#region IWebReponse

		public WebResponseStatus Status {
			get {
				return this.Response == null ? WebResponseStatus.Failed : WebResponseStatus.Success;
			}
		}

		public Task<String> String {
			get {
				if (this.Response == null) {
					return Task.Run(() => string.Empty);
				}

				var resultString = this.Response.Content.ReadAsStringAsync();
				return resultString;
			}
		}

		public Task<Stream> Stream {
			get {
				if (this.Response == null) {
					return Task.Run<Stream>(() => (Stream)null);
				}

				var resultStream = this.Response.Content.ReadAsStreamAsync();
				return resultStream;
			}
		}

		public HttpResponseMessage Response {
			private set;
			get;
		}

		private HttpStatusCode? _statusCode;

		public HttpStatusCode StatusCode {
			get {
				return this.Response?.StatusCode ?? this._statusCode ?? HttpStatusCode.BadRequest;
			}
		}

		#endregion

		public async Task<T> Data<T>() {
			if (this.Response == null) {
				return default(T);
			}

			try {
				var jsonString = await this.String;
				var result = JsonConvert.DeserializeObject<T>(jsonString);
				return result;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return default(T);
			}
		}

		public HttpResponse() : this(null) {
		}

		public HttpResponse(HttpResponseMessage response) {
			this.Response = response;
		}
	}

	public class HttpFileConstructor {

		private MultipartFormDataContent content;

		public HttpFileConstructor(MultipartFormDataContent content) {
			this.content = content;
		}

		public void AddFile(string parameterName, string fileName, byte[] bytes, string mimeType = null) {
			var byteContent = new ByteArrayContent(bytes);
			this.AddContent(parameterName, fileName, byteContent, mimeType);
		}

		public void AddFile(string parameterName, string fileName, Stream stream, string mimeType = null) {
			var streamContent = new StreamContent(stream);
			this.AddContent(parameterName, fileName, streamContent, mimeType);
		}

		private void AddContent(string parameterName, string fileName, HttpContent content, string mimeType) {
			if (mimeType != null) {
				MediaTypeHeaderValue mediaType;
				if (MediaTypeHeaderValue.TryParse(mimeType, out mediaType)) {
					content.Headers.ContentType = mediaType;
				}
			}

			this.content.Add(content, parameterName, fileName);
		}
	}

	public class HttpParameterParser {

//		private static HttpParameterParser instance;
//		private static object lockObject = new Object();

		public static readonly HttpParameterParser Instance = new HttpParameterParser();
//			get {
//				if (instance == null) {
//					lock (lockObject) {
//						if (instance == null) {
//							instance =
//						}
//					}
//				}
//				return instance;
//			}
//		}

		public virtual IEnumerable<KeyValuePair<string, string>> Parse(HttpParameters parameters) {
			if (parameters == null) {
				return new KeyValuePair<string, string>[] { };
			}

			var parsedParameters = parameters.SelectMany(pair => {
				var parameterName = pair.Key.ToString();
				var parameterValue = pair.Value;
				var parameterPairs = this.ParseValue(parameterValue);
				var resultList = parameterPairs.Select(innerPair => {
					var key = string.Format("{0}{1}", parameterName, innerPair.Key);
					var value = innerPair.Value;
					var result = new KeyValuePair<string, string>(key, value);
					return result;
				});
				return resultList;
			});

			return parsedParameters;
		}

		private IEnumerable<KeyValuePair<string, string>> ParseValue(object parameterValue) {
			if (parameterValue == null) {
				return new KeyValuePair<string, string>[] { };
			}

			var parameterType = parameterValue.GetType();
			var genericArguments = parameterType.GenericTypeArguments;

			if (genericArguments.Count() == 2) {
				var keyType = genericArguments[0];
				var valueType = genericArguments[1];
				return this.ParseDictionary(parameterValue, keyType, valueType);
			} else if (parameterValue is HttpDictionary) {
				return this.ParseDictionary<object, object>(parameterValue);
			} else if (genericArguments.Count() == 1
			           || parameterType.IsArray) {
				var arrayType = genericArguments.Count() > 0
					? genericArguments[0]
					: parameterType.GetElementType();

				return this.ParseArray(parameterValue, arrayType);
			} else if (parameterValue is HttpArray) {
				return this.ParseArray<object>(parameterValue);
			} else {
				var resultParameterName = string.Empty;
				var resultParameterValue = parameterValue.ToString();
				return new[] { new KeyValuePair<string, string>(resultParameterName, resultParameterValue) };
			}
		}

		#region Generic Types parsing

		private IEnumerable<KeyValuePair<string, string>> ParseDictionary(object parameterValue, Type keyType, Type valueType) {
			if (parameterValue == null) {
				return new KeyValuePair<string, string>[]{ };
			}

			if (keyType == typeof(string)) {
				if (valueType == typeof(int)) {
					return this.ParseDictionary<string, int>(parameterValue);
				} else if (valueType == typeof(long)) {
					return this.ParseDictionary<string, long>(parameterValue);
				} else if (valueType == typeof(double)) {
					return this.ParseDictionary<string, double>(parameterValue);
				} else if (valueType == typeof(float)) {
					return this.ParseDictionary<string, float>(parameterValue);
				} else if (valueType == typeof(string)) {
					return this.ParseDictionary<string, string>(parameterValue);
				} else if (valueType == typeof(object)) {
					return this.ParseDictionary<string, object>(parameterValue);
				}
			} else if (keyType == typeof(object)) {
				if (valueType == typeof(int)) {
					return this.ParseDictionary<object, int>(parameterValue);
				} else if (valueType == typeof(long)) {
					return this.ParseDictionary<object, long>(parameterValue);
				} else if (valueType == typeof(double)) {
					return this.ParseDictionary<object, double>(parameterValue);
				} else if (valueType == typeof(float)) {
					return this.ParseDictionary<object, float>(parameterValue);
				} else if (valueType == typeof(string)) {
					return this.ParseDictionary<object, string>(parameterValue);
				} else if (valueType == typeof(object)) {
					return this.ParseDictionary<object, object>(parameterValue);
				}
			}

			return new KeyValuePair<string, string>[]{ };
		}

		private IEnumerable<KeyValuePair<string, string>> ParseDictionary<T, U>(object parameterValue) {
			var parameterDictionary = parameterValue as IDictionary<T, U>;

			if (parameterDictionary == null) {
				return new KeyValuePair<string, string>[]{ };
			}
			var innerParseResult = parameterDictionary.SelectMany(parameterPair => {
				var key = parameterPair.Key.ToString();
				var parsedItems = this.ParseValue(parameterPair.Value);
				return parsedItems.Select(item => new KeyValuePair<string, string>(string.Format("[{0}]{1}", key, item.Key), item.Value));
			});
			return innerParseResult;
		}

		private IEnumerable<KeyValuePair<string, string>> ParseArray(object parameterValue, Type type) {

			if (parameterValue == null) {
				return new KeyValuePair<string, string>[]{ };
			}

			if (type == typeof(int)) {
				return this.ParseArray<int>(parameterValue);
			} else if (type == typeof(long)) {
				return this.ParseArray<long>(parameterValue);
			} else if (type == typeof(double)) {
				return this.ParseArray<double>(parameterValue);
			} else if (type == typeof(float)) {
				return this.ParseArray<float>(parameterValue);
			} else if (type == typeof(string)) {
				return this.ParseArray<string>(parameterValue);
			} else if (type == typeof(object)) {
				return this.ParseArray<object>(parameterValue);
			}

			return new KeyValuePair<string, string>[]{ };
		}

		private IEnumerable<KeyValuePair<string, string>> ParseArray<T>(object parameterValue) {
			var parameterArray = parameterValue as IEnumerable<T>;

			if (parameterArray == null) {
				return new KeyValuePair<string, string>[]{ };
			}

			var innerParseResult = parameterArray.SelectMany(item => this.ParseValue(item));
			return innerParseResult.Select(parsedPair => new KeyValuePair<string, string>(string.Format("[]{0}", parsedPair.Key), parsedPair.Value));
		}

		#endregion

		public virtual string ParseQuery(HttpParameters parameters) {
			var queryParameters = this.Parse(parameters);
			var queryArray = queryParameters.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value));
			return string.Join("&", queryArray);
		}
	}

	public class HttpRequest: IWebRequest<HttpMethod> {

		public enum HttpMethod {
			GET,
			POST,
			PUT,
			DELETE,
			HEAD,
			OPTIONS
		}

		#region IWebRequest

		public string Path { get; set; }

		public HttpMethod Method { get; set; }

		public TimeSpan Timeout { get; set; }

		public bool UseBaseUrl { get; set; }

		public Uri BuildRequestUri(string baseUrl = null) {
			try {
				Uri resultUri;
				UriBuilder uriBuilder;

				if ((baseUrl?.Length ?? 0) != 0
					&& this.UseBaseUrl) {
					uriBuilder = new UriBuilder(baseUrl);
					uriBuilder.Path = this.Path;
				} else {
					uriBuilder = new UriBuilder(this.Path);
				}

				if (this.Method == HttpMethod.GET
				    && (this.Parameters?.Count ?? 0) != 0) {
					var queryString = this.parameterParser.ParseQuery(this.Parameters);
					uriBuilder.Query = queryString;
				}

				resultUri = uriBuilder.Uri;

				return resultUri;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return null;
			}
		}

		#endregion

		public HttpHeaders Headers;
		public HttpParameters Parameters;
		public HttpFileConstrictionAction ConstructionAction;

		protected HttpParameterParser parameterParser;

		public HttpRequest() : this(HttpParameterParser.Instance) {
		}

		public HttpRequest(HttpParameterParser parser) {
			this.Path = null;
			this.Method = HttpMethod.GET;
			this.Headers = null;
			this.Parameters = null;
			this.ConstructionAction = null;
			this.Timeout = TimeSpan.FromSeconds(100);

			this.parameterParser = parser;

			this.UseBaseUrl = true;
		}

		public HttpRequestMessage HttpRequest {
			get {
				var resultRequest = new HttpRequestMessage();
				resultRequest.Method = this.Method.HttpMethod();

				if ((this.Headers?.Count ?? 0) != 0) {
					foreach (var header in this.Headers) {
						resultRequest.Headers.Add(header.Key, header.Value);
					}
				}

				if (this.Method != HttpMethod.GET
				    && (this.Parameters?.Count ?? 0) != 0) {

					if (this.ConstructionAction != null) {
						var multipartContent = new MultipartFormDataContent();

						var parsedParameters = this.parameterParser.Parse(this.Parameters);
						foreach (var pair in parsedParameters) {
							var pairContent = new StringContent(pair.Value);
							pairContent.Headers.ContentType = null;
							multipartContent.Add(pairContent, pair.Key);
						}

						var fileConstructor = new HttpFileConstructor(multipartContent);
						this.ConstructionAction(fileConstructor);

						resultRequest.Content = multipartContent;

					} else {
						var parsedParameters = this.parameterParser.Parse(this.Parameters);
						var formContent = new FormUrlEncodedContent(parsedParameters);
						resultRequest.Content = formContent;
					}
				}

				return resultRequest;
			}
		}
	}

	public class HttpPerformer: IWebPerformer<HttpResponse, HttpRequest> {

		private NativeCookieHandler cookieHandler;

		public HttpClient HttpClient {
			get {
				var httpMessageHandler = new NativeMessageHandler(false, false, this.cookieHandler);
				httpMessageHandler.AllowAutoRedirect = true;
				var client = new HttpClient(httpMessageHandler);

				if ((this.Headers?.Count ?? 0) != 0) {
					foreach (var header in this.Headers) {
						client.DefaultRequestHeaders.Add(header.Key, header.Value);
					}
				}

				return client;
			}
		}

		#region IWebPerformer

		public string BaseUrl { get; set; }

		public async Task<HttpResponse> Perform(HttpRequest request) {
			if (request == null) {
				return HttpResponse.Empty;
			}

			var requestUri = request.BuildRequestUri(this.BaseUrl);

			if (requestUri == null) {
				return HttpResponse.Empty;
			}

			try {
				var client = this.HttpClient;
				var clientRequest = request.HttpRequest;
				clientRequest.RequestUri = requestUri;
				client.Timeout = request.Timeout;

				var clientResponse = client.SendAsync(clientRequest);
				var resultResponse = new HttpResponse(await clientResponse);

				return resultResponse;
			} catch (WebException serverException) {
				Debug.WriteLine(serverException);
				var statusCode = serverException.HttpStatusCode();
				return HttpResponse.EmptyWithStatus(statusCode);
			} catch (TaskCanceledException timeout) {
				Debug.WriteLine(timeout);
				return HttpResponse.EmptyWithStatus(HttpStatusCode.RequestTimeout);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return HttpResponse.Empty;
			}
		}

		#endregion

		public HttpHeaders Headers;

		public HttpPerformer() : this(null) {
		}

		public HttpPerformer(NativeCookieHandler cookieHandler) {
			this.BaseUrl = null;
			this.Headers = null;
			this.cookieHandler = cookieHandler;
		}
	}

	[DependsOn(typeof(IOService))]
	public abstract class HttpService : Service {

		protected NativeCookieHandler cookieHandler;

		public HttpPerformer RequestPerformer {
			private set;
			get;
		}

		#region Service

		public override async Task Initialize() {
            await Task.Run(() => {
                this.cookieHandler = new NativeCookieHandler();
			    this.RequestPerformer = new HttpPerformer(this.cookieHandler);
            });
		}

		#endregion

		#region Cookies methods

		public virtual void SetCookie(Cookie cookie) {
			this.SetCookies(new []{ cookie });
		}

		public virtual void SetCookies(IEnumerable<Cookie> cookies) {
			this.cookieHandler.SetCookies(cookies);
		}

		public virtual void DeleteCookies(string name = null, string domain = null) {
			var cookieList = this.GetCookies(name, domain);
			this.DeleteCookies(cookieList);
		}

		public virtual void DeleteCookie(Cookie cookie) {
			this.DeleteCookies(new [] { cookie });
		}

		public abstract void DeleteCookies(IEnumerable<Cookie> cookieList);

		public virtual IEnumerable<Cookie> GetCookies(string name = null, string domain = null) {
			var comparison = StringComparison.CurrentCultureIgnoreCase;
			var cookieList = this.cookieHandler.Cookies
				.Where(item => (name == null || string.Equals(item.Name, name, comparison))
			                 && (domain == null || string.Equals(item.Domain, domain, comparison)));

			return cookieList;
		}

		#endregion

		#region Request methods

		public Task<HttpResponse> Perform(HttpRequest request) {
			return this.RequestPerformer?.Perform(request);
		}

		#endregion
	}
}
