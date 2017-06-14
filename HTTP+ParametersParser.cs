using System;
using System.Collections.Generic;
using System.Linq;

namespace naithar
{

    public partial class HTTP {
        
		public class ParametersParser
		{

			public static IEnumerable<KeyValuePair<string, string>> Parse(HTTP.Parameters parameters)
			{
				if (parameters == null)
				{
					return new KeyValuePair<string, string>[] { };
				}

				var parsedParameters = parameters.SelectMany(pair =>
				{
					var parameterName = pair.Key;
					var parameterValue = pair.Value;
					var parameterPairs = ParametersParser.ParseValue(parameterValue);
					var resultList = parameterPairs.Select(innerPair =>
					{
						var key = string.Format("{0}{1}", parameterName, innerPair.Key);
						var value = innerPair.Value;
						var result = new KeyValuePair<string, string>(key, value);
						return result;
					});
					return resultList;
				});

				return parsedParameters;
			}

			private static IEnumerable<KeyValuePair<string, string>> ParseValue(object parameterValue)
			{
				if (parameterValue == null)
				{
					return new KeyValuePair<string, string>[] { };
				}

				var parameterType = parameterValue.GetType();
				var genericArguments = parameterType.GenericTypeArguments;

				if (genericArguments.Count() == 2)
				{
					var keyType = genericArguments[0];
					var valueType = genericArguments[1];
					return ParametersParser.ParseDictionary(parameterValue, keyType, valueType);
				}
				else if (parameterValue is HTTP.Dictionary)
				{
					return ParametersParser.ParseDictionary<object, object>(parameterValue);
				}
				else if (genericArguments.Count() == 1
						 || parameterType.IsArray)
				{
					var arrayType = genericArguments.Count() > 0
						? genericArguments[0]
						: parameterType.GetElementType();

					return ParametersParser.ParseArray(parameterValue, arrayType);
				}
				else if (parameterValue is HTTP.Array)
				{
					return ParametersParser.ParseArray<object>(parameterValue);
				}
				else
				{
					var resultParameterName = string.Empty;
					var resultParameterValue = parameterValue.ToString();
					return new[] { new KeyValuePair<string, string>(resultParameterName, resultParameterValue) };
				}
			}

			#region Generic Types parsing

			private static IEnumerable<KeyValuePair<string, string>> ParseDictionary(object parameterValue, Type keyType, Type valueType)
			{
				if (parameterValue == null)
				{
					return new KeyValuePair<string, string>[] { };
				}

				if (keyType == typeof(string))
				{
					if (valueType == typeof(int))
					{
						return ParametersParser.ParseDictionary<string, int>(parameterValue);
					}
					else if (valueType == typeof(long))
					{
						return ParametersParser.ParseDictionary<string, long>(parameterValue);
					}
					else if (valueType == typeof(double))
					{
						return ParametersParser.ParseDictionary<string, double>(parameterValue);
					}
					else if (valueType == typeof(float))
					{
						return ParametersParser.ParseDictionary<string, float>(parameterValue);
					}
					else if (valueType == typeof(string))
					{
						return ParametersParser.ParseDictionary<string, string>(parameterValue);
					}
					else if (valueType == typeof(object))
					{
						return ParametersParser.ParseDictionary<string, object>(parameterValue);
					}
				}
				else if (keyType == typeof(object))
				{
					if (valueType == typeof(int))
					{
						return ParametersParser.ParseDictionary<object, int>(parameterValue);
					}
					else if (valueType == typeof(long))
					{
						return ParametersParser.ParseDictionary<object, long>(parameterValue);
					}
					else if (valueType == typeof(double))
					{
						return ParametersParser.ParseDictionary<object, double>(parameterValue);
					}
					else if (valueType == typeof(float))
					{
						return ParametersParser.ParseDictionary<object, float>(parameterValue);
					}
					else if (valueType == typeof(string))
					{
						return ParametersParser.ParseDictionary<object, string>(parameterValue);
					}
					else if (valueType == typeof(object))
					{
						return ParametersParser.ParseDictionary<object, object>(parameterValue);
					}
				}

				return new KeyValuePair<string, string>[] { };
			}

			private static IEnumerable<KeyValuePair<string, string>> ParseDictionary<T, U>(object parameterValue)
			{
				var parameterDictionary = parameterValue as IDictionary<T, U>;

				if (parameterDictionary == null)
				{
					return new KeyValuePair<string, string>[] { };
				}
				var innerParseResult = parameterDictionary.SelectMany(parameterPair =>
				{
					var key = parameterPair.Key.ToString();
					var parsedItems = ParametersParser.ParseValue(parameterPair.Value);
					return parsedItems.Select(item => new KeyValuePair<string, string>(string.Format("[{0}]{1}", key, item.Key), item.Value));
				});
				return innerParseResult;
			}

			private static IEnumerable<KeyValuePair<string, string>> ParseArray(object parameterValue, Type type)
			{

				if (parameterValue == null)
				{
					return new KeyValuePair<string, string>[] { };
				}

				if (type == typeof(int))
				{
					return ParametersParser.ParseArray<int>(parameterValue);
				}
				else if (type == typeof(long))
				{
					return ParametersParser.ParseArray<long>(parameterValue);
				}
				else if (type == typeof(double))
				{
					return ParametersParser.ParseArray<double>(parameterValue);
				}
				else if (type == typeof(float))
				{
					return ParametersParser.ParseArray<float>(parameterValue);
				}
				else if (type == typeof(string))
				{
					return ParametersParser.ParseArray<string>(parameterValue);
				}
				else if (type == typeof(object))
				{
					return ParametersParser.ParseArray<object>(parameterValue);
				}

				return new KeyValuePair<string, string>[] { };
			}

			private static IEnumerable<KeyValuePair<string, string>> ParseArray<T>(object parameterValue)
			{
				var parameterArray = parameterValue as IEnumerable<T>;

				if (parameterArray == null)
				{
					return new KeyValuePair<string, string>[] { };
				}

				var innerParseResult = parameterArray.SelectMany(item => ParametersParser.ParseValue(item));
				return innerParseResult.Select(parsedPair => new KeyValuePair<string, string>(string.Format("[]{0}", parsedPair.Key), parsedPair.Value));
			}

			#endregion

			public static string ParseQuery(HTTP.Parameters parameters)
			{
				var queryParameters = ParametersParser.Parse(parameters);
				var queryArray = queryParameters.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value));
				return string.Join("&", queryArray);
			}
		}
    }        
}