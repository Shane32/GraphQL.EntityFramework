using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GraphQL.EntityFramework.Testing
{
    public static class ClientQueryExecutor
    {
        static string uri = "graphql";

        public static void SetQueryUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) throw new ArgumentNullException(nameof(uri));
            ClientQueryExecutor.uri = uri;
        }

        public static Task<HttpResponseMessage> ExecutePost(HttpClient client, string query, object variables = null, Action<HttpHeaders> headerAction = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
            query = CompressQuery(query);
            var body = new
            {
                query,
                variables
            };
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(ToJson(body), Encoding.UTF8, "application/json")
            };
            headerAction?.Invoke(request.Headers);
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> ExecuteGet(HttpClient client, string query, object variables = null, Action<HttpHeaders> headerAction = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
            var compressed = CompressQuery(query);
            var variablesString = ToJson(variables);
            var getUri = $"{uri}?query={compressed}&variables={variablesString}";
            var request = new HttpRequestMessage(HttpMethod.Get, getUri);
            headerAction?.Invoke(request.Headers);
            return client.SendAsync(request);
        }

        static string ToJson(object target)
        {
            if (target == null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(target);
        }

        public static string CompressQuery(string query)
        {
            query = Regex.Replace(query ?? throw new ArgumentNullException(nameof(query)), @"\s+", " ");
            return Regex.Replace(query, @"\s*(\[|\]|\{|\}|\(|\)|:|\,)\s*", "$1");
        }
    }
}