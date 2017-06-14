//
// Extension.cs
//
// Created by Sergey Minakov on 18.03.2016.
//
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

//TODO: summary, comments.
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace mapp.core {

	using FtpMethod = FtpRequest.FtpMethod;
	using HttpMethod = HttpRequest.HttpMethod;
	using SystemHttpMethod = System.Net.Http.HttpMethod;
	using SystemStatusCode = System.Net.HttpStatusCode;

	public static partial class GlobalVariables {
		public const int ByteReadLength = 4096;
	}

	public static partial class LinqExtensions {
		///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
		///<param name="items">The enumerable to search.</param>
		///<param name="predicate">The expression to test the items against.</param>
		///<returns>The index of the first matching item, or -1 if no items match.</returns>
		public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
			if (items == null) throw new ArgumentNullException("items");
			if (predicate == null) throw new ArgumentNullException("predicate");

			int retVal = 0;
			foreach (var item in items) {
				if (predicate(item)) return retVal;
				retVal++;
			}
			return -1;
		}
		///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
		///<param name="items">The enumerable to search.</param>
		///<param name="item">The item to find.</param>
		///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
		public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }
	}

    public static partial class TypeConvertionExtensions
    {
        /// <summary>
        /// Try cast <paramref name="obj"/> value to type <typeparamref name="T"/>,
        /// if can't will return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T To<T>(this object obj, T defaultValue)
        {
            if (obj == null)
                return defaultValue;

            if (obj is T)
                return (T)obj;

            Type type = typeof(T);

            // Place convert to reference types here

            if (type == typeof(string))
            {
                return (T)(object)obj.ToString();
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, defaultValue, underlyingType);
            }

            return To(obj, defaultValue, type);
        }

        private static T To<T>(object obj, T defaultValue, Type type)
        {
            if (obj is bool && type.IsNumericType())
            {
                return (bool)obj ? (T)Convert.ChangeType(1, type) : (T)Convert.ChangeType(0, type);
            }

            if (type == typeof(int))
            {
                int intValue;
                if (int.TryParse(obj.ToString(), out intValue))
                    return (T)(object)intValue;
                return defaultValue;
            }

            if (type == typeof(decimal))
            {
                decimal decimalValue;
                if (decimal.TryParse(obj.ToString(), out decimalValue))
                    return (T)(object)decimalValue;
                return defaultValue;
            }

            if (type == typeof(double))
            {
                double doubleValue;
                if (double.TryParse(obj.ToString(), out doubleValue))
                    return (T)(object)doubleValue;
                return defaultValue;
            }

            if (type == typeof(long))
            {
                long intValue;
                if (long.TryParse(obj.ToString(), out intValue))
                    return (T)(object)intValue;
                return defaultValue;
            }

            if (type == typeof(bool))
            {
                if (obj.GetType().IsNumericType())
                    return (T)(object)(Convert.ToInt64(obj) != 0);
                bool bValue;
                if (bool.TryParse(obj.ToString(), out bValue))
                    return (T)(object)bValue;
                return defaultValue;
            }

            if (type == typeof(byte))
            {
                byte byteValue;
                if (byte.TryParse(obj.ToString(), out byteValue))
                    return (T)(object)byteValue;
                return defaultValue;
            }

            if (type == typeof(short))
            {
                short shortValue;
                if (short.TryParse(obj.ToString(), out shortValue))
                    return (T)(object)shortValue;
                return defaultValue;
            }

            if (type == typeof(DateTime))
            {
                DateTime dateValue;
                if (DateTime.TryParse(obj.ToString(), out dateValue))
                    return (T)(object)dateValue;
                return defaultValue;
            }

            if (type == typeof(Guid))
            {
                Guid guidValue;
                if (Guid.TryParse(obj.ToString(), out guidValue))
                    return (T)(object)guidValue;
                return defaultValue;
            }

            if (type.IsEnum())
            {
                if (Enum.IsDefined(type, obj))
                    return (T)Enum.Parse(type, obj.ToString());
                return defaultValue;
            }

            throw new NotSupportedException(string.Format("Couldn't parse to Type {0}", typeof(T)));
        }

        private static bool IsNumericType(this Type type)
        {
            return type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
                   type == typeof(float) || type == typeof(double) || type == typeof(ushort) || type == typeof(uint) ||
                   type == typeof(ulong);
        }
    }

	public static partial class Math {
		public static T Clamp<T>(T value, T min, T max)
			where T : IComparable<T> {
			var resultValue = (value.CompareTo(min) < 0) ? min : (value.CompareTo(max) > 0) ? max : value;
			return resultValue;
		}
	}

	public static class XamarinEx {
		public static async Task LoadFromXaml<T>(this T view, string path, bool isUrl = false) where T: BindableObject {
			string fullPath;

			var repo = App.Get<RepositoryService>();

			if (isUrl) {
				fullPath = await repo.RequestResoursePath(path);
			} else {
				fullPath = path;
			}

			await XamarinEx.LoadFromXaml(view, fullPath);
		}

		public static async Task LoadFromXaml<T>(this T view, string bundle, string fileName) where T: BindableObject {

			var repo = App.Get<RepositoryService>();

			var fullPath = await repo.RequestResoursePath(bundle, fileName);

			await XamarinEx.LoadFromXaml(view, fullPath);
		}

		public static void LoadFromXamlSync<T>(this T view, string bundle, string fileName) where T: BindableObject {

			var repo = App.Get<RepositoryService>();

			var fullPath = Task.Run( async () => await repo.RequestResoursePath(bundle, fileName)).Result;

			XamarinEx.LoadFromXamlSync(view, fullPath);
		}

		private static void LoadFromXamlSync<T>(this T view, string fullPath) where T: BindableObject {
			var io = App.Get<IOService>();
			using (var xaml = io.OpenRead(fullPath, true)) {
				if (xaml == null) {
					return;
				}

				var content = xaml.ReadToEnd();
				App.Instance.LoadViewFromXaml(view, content);
			}
		}

		private static async Task LoadFromXaml<T>(this T view, string fullPath) where T: BindableObject {
			var io = App.Get<IOService>();
			using (var xaml = io.OpenRead(fullPath, true)) {
				if (xaml == null) {
					return;
				}

				var content = await xaml.ReadToEndAsync();
				App.Instance.LoadViewFromXaml(view, content);
			}
		}
	}

	public class Image {

		public async static  Task<ImageSource> FromRepository(string repoUrl) {
			var repo = App.Get<RepositoryService>();
			var path = await repo.RequestResoursePath(repoUrl);
			var imageSource = ImageSource.FromFile(path);
			return imageSource;
		}
	}

	public static partial class Path {

		public static string Repository() {
			return IOService.REPOSITORY_FOLDER;
		}

		public static string Repository(params string[] components) {
			if (components != null) {
				var combined = Path.Combine(components);
				return Path.Combine(IOService.REPOSITORY_FOLDER, combined);
			}

			return IOService.REPOSITORY_FOLDER;
		}
	}

    public static partial class FTPExtensions
    {

        public static string FtpMethod(this FtpMethod method)
        {
            var resultMethod = "NLST";

            switch (method)
            {
                case FtpMethod.ListDirectoryDetails:
                    resultMethod = "LIST";
                    break;
                case FtpMethod.PrintWorkingDirectory:
                    resultMethod = "PWD";
                    break;
                case FtpMethod.DownloadFile:
                    resultMethod = "RETR";
                    break;
                case FtpMethod.GetFileSize:
                    resultMethod = "SIZE";
                    break;
                case FtpMethod.GetDateTimestamp:
                    resultMethod = "MDTM";
                    break;
                default:
                    break;
            }

            return resultMethod;
        }
    }
    public static partial class StringExtensions
    {
        public static string Fmt(this string template, params object[] args)
        {
            return string.Format(template, args);
        }
    }

    public static partial class DictionaryExtensions {

		public static void AddRange<T, U>(this IDictionary<T, U> source, IDictionary<T, U> other) {
			foreach (var item in other) {
				source.Add(item.Key, item.Value);
			}
		}
	}

	public static partial class StreamExtensions {
		public static byte[] ByteArray(this Stream stream) {
			using (var streamReader = new BinaryReader(stream)) {
				if (!stream.CanSeek) {
					List<byte> byteList = new List<byte>();
					byte[] readByte;

					do {
						readByte = streamReader.ReadBytes(GlobalVariables.ByteReadLength);
						byteList.AddRange(readByte);
					} while (readByte.Length == GlobalVariables.ByteReadLength);

					return byteList.ToArray();
				} else {
					byte[] byteArray = streamReader.ReadBytes((int)stream.Length);
					return byteArray;
				}
			}
		}
	}

	public static partial class HTTPExtensions {


		public static SystemStatusCode HttpStatusCode(this WebException exception) {
			var resultStatusCode = SystemStatusCode.InternalServerError;

			var httpResponse = exception.Response as HttpWebResponse;
			if (httpResponse != null) {
				resultStatusCode = httpResponse.StatusCode;
			} else {
				/*
				 * Success = 0,
				 * NameResolutionFailure = 1,
				 * ConnectFailure = 2,
				 * ReceiveFailure = 3,
				 * SendFailure = 4,
				 * PipelineFailure = 5,
				 * RequestCanceled = 6,
				 * ProtocolError = 7,
				 * ConnectionClosed = 8,
				 * TrustFailure = 9,
				 * SecureChannelFailure = 10,
				 * ServerProtocolViolation = 11,
				 * KeepAliveFailure = 12,
				 * Pending = 13,
				 * Timeout = 14,
				 * ProxyNameResolutionFailure = 15,
				 * UnknownError = 16,
				 * MessageLengthLimitExceeded = 17,
				 * CacheEntryNotFound = 18,
				 * RequestProhibitedByCachePolicy = 19,
				 * RequestProhibitedByProxy = 20
				*/
				switch ((int)exception.Status) {
				case 1:
					resultStatusCode = SystemStatusCode.NotFound;
					break;
				case 2:
					resultStatusCode = SystemStatusCode.BadGateway;
					break;
				case 4:
					resultStatusCode = SystemStatusCode.BadRequest;
					break;
				case 7:
					resultStatusCode = SystemStatusCode.Unauthorized;
					break;
				case 14:
					resultStatusCode = SystemStatusCode.RequestTimeout;
					break;
				default:
					break;
				}
			}

			return resultStatusCode;
		}

		public static string CookieDomain(this string possibleUrl) {
			var builder = new UriBuilder(possibleUrl);
			return builder.Host;
		}

		public static SystemHttpMethod HttpMethod(this HttpMethod method) {
			var resultMethod = SystemHttpMethod.Get;

			switch (method) {
			case HttpMethod.POST:
				resultMethod = SystemHttpMethod.Post;
				break;
			case HttpMethod.DELETE:
				resultMethod = SystemHttpMethod.Delete;
				break;
			case HttpMethod.PUT:
				resultMethod = SystemHttpMethod.Put;
				break;
			case HttpMethod.HEAD:
				resultMethod = SystemHttpMethod.Head;
				break;
			case HttpMethod.OPTIONS:
				resultMethod = SystemHttpMethod.Options;
				break;
			default:
				break;
			}
			return resultMethod;
		}
	}

}
