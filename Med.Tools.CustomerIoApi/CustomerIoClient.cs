using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace Med.Tools.CustomerIoApi
{
    public class CustomerIoClient
    {
        private const string ApiUrl = "https://track.customer.io/api/v1/customers/";

        private string ApiKey { get; set; }
        private string SiteId { get; set; }
        private HttpClient Client { get; set; }

        public CustomerIoClient(string siteId, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                throw new ArgumentNullException("siteId");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException("apiKey");

            ApiKey = apiKey;
            SiteId = siteId;

            Client = new HttpClient();
        }

        public async Task<bool> IdentifyCustomer(string customerId, string email, DateTime? dateCreatedUtc = null, Dictionary<string, string> extraData = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId may not be null or empty");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException("email may not be null or empty");


            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(ApiUrl + customerId));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", SiteId, ApiKey))));

            var dict = extraData == null ? new Dictionary<string, string>() : extraData.ToDictionary(d => d.Key, d => d.Value);

            if (dict.ContainsKey("email"))
            {
                throw new ArgumentOutOfRangeException("extraData may not contain a key called 'email'");
            }

            dict.Add("email", email);

            dict.Add("created_at",
                dateCreatedUtc != null
                    ? DateTimeToUnixTimestamp(dateCreatedUtc.Value).ToString("####")
                    : DateTimeToUnixTimestamp(DateTime.UtcNow).ToString("####"));

            request.Content = new FormUrlEncodedContent(dict);

            var response = await Client.SendAsync(request).ConfigureAwait(false);
            return ProcessResponseAsync(response);
        }

        public async Task<bool> DeleteCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId may not be null or empty");

            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(ApiUrl + customerId));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", SiteId, ApiKey))));

            var response = await Client.SendAsync(request).ConfigureAwait(false);

            return ProcessResponseAsync(response);
        }

        public async Task<bool> TrackEvent(string customerId, string eventName, Dictionary<string, string> extraData = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException("customerId may not be null or empty");

            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException("eventName may not be null or empty");

            var dict = extraData == null ? new Dictionary<string, string>() : extraData.ToDictionary(d => d.Key, d => d.Value);

            if (dict.ContainsKey("name"))
            {
                throw new ArgumentOutOfRangeException("extraData may not contain a key called 'name'");
            }

            dict.Add("name", eventName);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(ApiUrl + customerId + "/events"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", SiteId, ApiKey))));

            request.Content = new FormUrlEncodedContent(dict);

            var response = await Client.SendAsync(request).ConfigureAwait(false);
            return ProcessResponseAsync(response);
        }

        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        private static bool ProcessResponseAsync(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }

            return true;
        }
    }
}