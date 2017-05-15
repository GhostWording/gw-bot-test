using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotGoodMorningEvening.Helpers
{
    public class ProxyHelper
    {
        private static string rootUrl;
        private static string RootUrl => rootUrl ?? (rootUrl = ConfigurationManager.AppSettings["RootUrlApi"]);

        // Send a request on a service and retrieve result
        public static T SendRequest<T>(string service, string operation, HttpMethod method = null, params object[] parameters)
        {
            // Create request
            var request = CreateRequest(service, operation, method, parameters);

            // Use request to get result
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response == null) throw new NullReferenceException();
                using (var responseStream = response.GetResponseStream())
                    return Deserialize<T>(responseStream);
            }
        }

        // Send a request without trying to get a result
        public static void SendRequest(string service, string operation, HttpMethod method = null, params object[] parameters)
        {
            // Create request
            var requete = CreateRequest(service, operation, method, parameters);

            // Use request, result is not used
            using (var reponse = requete.GetResponse() as HttpWebResponse)
            {
                if (reponse == null) throw new NullReferenceException();
            }
        }

        private static HttpWebRequest CreateRequest(string service, string operation, HttpMethod method = null, params object[] parameters)
        {
            // Détermine la méthode HTTP et crée les paramètres
            var currentMethod = method == null ? HttpMethod.Get : method;
            var urlParameters = BuildUrlParameters(parameters, currentMethod);

            // Create the request
            var requestUrl = $"{RootUrl}{service}{(operation == null ? string.Empty : "/" + operation)}{urlParameters}";

            var request = WebRequest.CreateHttp(requestUrl);
            request.Method = currentMethod.Method;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en-EN");

            if (currentMethod != HttpMethod.Get)
                AddBodyParameters(parameters, request.GetRequestStream());
            return request;
        }

        private static string BuildUrlParameters(object[] parameters, HttpMethod currentMethod)
        {
            if (currentMethod == HttpMethod.Get)
                return BuildUrlParameters(parameters);

            if (parameters.Length > 1 && parameters.Last() is byte[])
                return BuildUrlParameters(parameters.Take(parameters.Length - 1));

            return string.Empty;
        }

        private static string BuildUrlParameters(IEnumerable<object> parameters)
        {
            var urlParameters = "";
            var parametersList = parameters.ToList();
            var stringListParameters = new List<string>();

            foreach (var parametre in parametersList)
            {
                if (parametre is KeyValuePair<string, string>)
                {
                    var pair = (KeyValuePair<string, string>)parametre;
                    stringListParameters.Add($"{pair.Key}={WebUtility.UrlEncode(pair.Value)}");
                }
                else
                {
                    if (parametre != null)
                        urlParameters += "/" + WebUtility.UrlEncode(parametre.ToString());
                    else
                        urlParameters += "/%20";
                }
            }

            if (stringListParameters.Count > 0)
            {
                urlParameters += "?";
                urlParameters += string.Join("&", stringListParameters);
            }

            return urlParameters;
        }

        private static void AddBodyParameters(object[] parameters, Stream requestStream)
        {
            if (parameters.Length == 0) return;
            if (parameters.Length > 1)
            {
                if (!(parameters.Last() is byte[]))
                    throw new ArgumentOutOfRangeException(nameof(parameters));
                BinarySerialize(parameters.Last() as byte[], requestStream);
            }
            Serialize(parameters[0], requestStream);
        }

        private static void BinarySerialize(byte[] content, Stream stream)
        {
            stream.Write(content, 0, content.Length);
        }

        private static void Serialize(object value, Stream stream)
        {
            var seralizer = new JsonSerializer { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat };
            using (var streamWriter = new StreamWriter(stream))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                seralizer.Serialize(jsonTextWriter, value);
            }
        }

        private static T Deserialize<T>(Stream stream)
        {
            var jsonDeserializer = new JsonSerializer();
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                // If it's a complex type a deserialization is made
                if (!typeof(T).GetTypeInfo().IsValueType && typeof(T) != typeof(string))
                    return jsonDeserializer.Deserialize<T>(jsonTextReader);

                // Otherwise, the first property is retrieved in order to return a simple value (string, int, Guid...)
                var result = jsonDeserializer.Deserialize(jsonTextReader) as JObject;
                if (result == null || !result.Properties().Any()) throw new ArgumentNullException();

                var value = result.GetValue(result.Properties().First().Name);
                if (value != null) return value.Value<T>();

                throw new ArgumentNullException();
            }
        }
    }
}
