using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace naithar
{

    public partial class HTTP
    {

		public interface Encoder
		{
			void Encode(System.Net.Http.HttpRequestMessage request, HTTP.Parameters parameters);
		}

        public class JSONEncoder : HTTP.Encoder {

            public static readonly JSONEncoder Instance = new JSONEncoder();

            public void Encode(System.Net.Http.HttpRequestMessage request, HTTP.Parameters parameters)
            {
                var json = JsonConvert.SerializeObject(parameters);
                request.Content = new StringContent(json);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }                
        }

        public class MultipartEncoder : HTTP.Encoder {

            public class File {

                public string key { private set; get; }
                public string filename { private set; get; }
                public string mimeType { private set; get; }
                public HttpContent content { private set; get; }

                public File(string key, string filename, string mimeType = null) {
                    this.key = key;
                    this.filename = filename;
                    this.mimeType = mimeType;
                    this.content = null;
                }

                public File(string key, string filename, HttpContent content, string mimeType = null) 
                    : this(key, filename, mimeType)
                {
                    this.content = content;
                }

                public File(string key, string filename, byte[] bytes, string mimeType = null)
                    : this(key, filename, new ByteArrayContent(bytes), mimeType)
                { }

				public File(string key, string filename, Stream stream, string mimeType = null)
					: this(key, filename, new StreamContent(stream), mimeType)
				{ }

                public File(string key, HttpContent content, string mimeType = null) 
                    : this(key: key, filename: null, mimeType: mimeType)
				{
					this.content = content;
				}

				public File(string key, byte[] bytes, string mimeType = null)
					: this(key, new ByteArrayContent(bytes), mimeType)
				{ }

				public File(string key, Stream stream, string mimeType = null)
					: this(key, new StreamContent(stream), mimeType)
				{ }
			}

            private File[] files = {};

			public MultipartEncoder()
			{
			}

            public MultipartEncoder(File[] files) {
                this.files = files;
            }

            public void Encode(System.Net.Http.HttpRequestMessage request, HTTP.Parameters parameters)
            {
                if (request.Method == System.Net.Http.HttpMethod.Get) { return; }

                var content = new MultipartFormDataContent();

				var parsedParameters = HTTP.ParametersParser.Parse(parameters);
				foreach (var pair in parsedParameters)
				{
					var pairContent = new StringContent(pair.Value);
					pairContent.Headers.ContentType = null;
					content.Add(pairContent, pair.Key);
				}

                foreach (var file in this.files) {
                    if (file.content == null) { continue; }
					if (file.mimeType != null)
					{
						MediaTypeHeaderValue mediaType;
						if (MediaTypeHeaderValue.TryParse(file.mimeType, out mediaType))
						{
                            file.content.Headers.ContentType = mediaType;
						}
					}

                    content.Add(file.content, file.key, file.filename ?? "");
                }

                request.Content = content;
            }                
        }

		public class URLEncoder : HTTP.Encoder
		{

			public static readonly URLEncoder Instance = new URLEncoder();

			public URLEncoder()
			{

			}

			public void Encode(System.Net.Http.HttpRequestMessage request, HTTP.Parameters parameters)
			{
				if (request.Method == System.Net.Http.HttpMethod.Get)
				{
					var builder = new UriBuilder(request.RequestUri);
					var queryString = HTTP.ParametersParser.ParseQuery(parameters);
					builder.Query = queryString;

					request.RequestUri = builder.Uri;
				}
				else
				{
					var parsedParameters = HTTP.ParametersParser.Parse(parameters);
					var content = new FormUrlEncodedContent(parsedParameters);
					request.Content = content;
				}
			}
		}
    }
}