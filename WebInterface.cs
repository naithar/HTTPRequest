////
//// WebInterfaces.cs
////
//// Created by Sergey Minakov on 17.03.2016.
////
////

//using System;
//using System.Threading.Tasks;
//using System.IO;
//using System.Net;

//namespace mapp.core {

//	public enum WebResponseStatus {
//		Success,
//		Failed
//	}

//	public abstract class IWebResponse<T> {

//		Task<String> String {
//			get;
//		}

//		Task<Stream> Stream {
//			get;
//		}

//		WebResponseStatus Status {
//			get;
//		}

//		HttpStatusCode StatusCode {
//			get;
//		}

//		T Response {
//			get;
//		}
//	}

//	public interface IWebRequest<T> {

//		string Path {
//			get;
//			set;
//		}

//		T Method {
//			get;
//			set;
//		}

//		Uri BuildRequestUri(string baseUrl = null);

//		TimeSpan Timeout {
//			get;
//			set;
//		}

//		bool UseBaseUrl {
//			get;
//			set;
//		}
//	}

//	public interface IWebPerformer<T, U> {

//		string BaseUrl {
//			get;
//			set;
//		}

//		Task<T> Perform(U request);
//	}

//}
