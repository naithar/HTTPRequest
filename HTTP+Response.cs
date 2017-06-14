using System;
using System.Threading.Tasks;

namespace naithar {

    public partial class HTTP {

        public class Response
        {
            public System.Net.Http.HttpResponseMessage response { private set; get; }

            public System.Net.HttpStatusCode StatusCode {
                get {
                    if (this.response == null) { return System.Net.HttpStatusCode.Unused; }
                    return this.response.StatusCode;
                }
            }    

			public bool isEmpty
			{
				get
				{
					return this.response == null;
				}
			}

			public Task<System.IO.Stream> Stream
			{
				get
				{
					if (this.response == null)
					{
						return Task.Run(() => { return (System.IO.Stream)(new System.IO.MemoryStream()); });
					}
					return this.response.Content.ReadAsStreamAsync();
				}
			}

			public Task<string> String
			{
				get
				{
					if (this.response == null)
					{
						return Task.Run(() => { return ""; });
					}
					return this.response.Content.ReadAsStringAsync();
				}
			}

			public Task<byte[]> Bytes
			{
				get
				{
					if (this.response == null)
					{
						return Task.Run(() => { return new byte[] { }; });
					}
					return this.response.Content.ReadAsByteArrayAsync();
				}
			}

			public Response()
			{
                this.response = null;
			}

			public Response(System.Net.Http.HttpResponseMessage response)
			{
				this.response = response;
			}
		}
    }
}