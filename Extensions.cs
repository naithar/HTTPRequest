////
//// Extension.cs
////
//// Created by Sergey Minakov on 18.03.2016.
////
////
using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace naithar {

    public static class Extensions {

		internal static void AddRange<T, U>(this IDictionary<T, U> source, IDictionary<T, U> other)
		{
			foreach (var item in other)
			{
				source.Add(item.Key, item.Value);
			}
		}

        internal static System.Net.Http.HttpMethod Method(this HTTP.Method m) {
            switch (m) {
                case HTTP.Method.POST:
                    return HttpMethod.Post;
                case HTTP.Method.PUT:
                    return HttpMethod.Put;
                case HTTP.Method.OPTIONS:
                    return HttpMethod.Options;
                case HTTP.Method.HEAD:
                    return HttpMethod.Head;
                case HTTP.Method.DELETE:
                    return HttpMethod.Delete;
                default:
                    return HttpMethod.Get;    
            }
        }

        public static Task<byte[]> Bytes(this System.IO.Stream stream, int chunkSize = 0) {
            return Task.Run(() =>
            {
                var bufferSize = chunkSize > 0 ? chunkSize : 2048;
				using (var reader = new MemoryStream())
				{
					byte[] buffer = new byte[bufferSize];
					int bytesRead;
					while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						reader.Write(buffer, 0, bytesRead);
					}
                    byte[] result = reader.ToArray();
                    return result;
				}
            });
        }
    }
}