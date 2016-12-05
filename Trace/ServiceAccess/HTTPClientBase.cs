﻿using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ModernHttpClient;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;

namespace Trace {
	public class HTTPClientBase : HttpClient {
		private const string NO_CONNECTION_ERROR_MSG = "Could not connect to the server.";

		/// <summary>
		/// Calls the HttpClient with a handler that calls the stack optimized for each platform.
		/// </summary>
		public HTTPClientBase() :
			base(new NativeMessageHandler()) { }


		public async Task<JObject> GetAsyncJSON(string uri) {
			string content = "";
			if(CrossConnectivity.Current.IsConnected) {
				content = await GetStringAsync(uri);
			}
			return await Task.Run(() => JObject.Parse(content));
		}


		public async Task<JObject> PostAsyncJSON(string uri, string data) {
			var content = new StringContent(data, Encoding.UTF8, "application/json");
			HttpResponseMessage response = null;
			string result = JObject.FromObject(new WSResult { error = NO_CONNECTION_ERROR_MSG }).ToString();
			if(CrossConnectivity.Current.IsConnected) {
				response = await PostAsync(uri, content);

				try {
					response.EnsureSuccessStatusCode();
				}
				catch(HttpRequestException e) {
					return await Task.Run(() => JObject.FromObject(new WSResult { success = false, error = e.Message }));
				}

				result = await response.Content.ReadAsStringAsync();
			}
			return await Task.Run(() => JObject.Parse(result));
		}


		public async Task<JObject> GetAsyncFormURL(string uri, FormUrlEncodedContent query) {
			string result = JObject.FromObject(new WSResult { error = NO_CONNECTION_ERROR_MSG }).ToString();
			if(CrossConnectivity.Current.IsConnected) {
				var response = await GetAsync(uri + query.ReadAsStringAsync().Result);

				try {
					response.EnsureSuccessStatusCode();
				}
				catch(HttpRequestException e) {
					return await Task.Run(() => JObject.FromObject(new WSResult { error = e.Message }));
				}

				result = await response.Content.ReadAsStringAsync();
			}
			return await Task.Run(() => JObject.Parse(result));
		}


		public async Task<JObject> PostAsyncFormURL(string uri, FormUrlEncodedContent data) {
			string result = JObject.FromObject(new WSResult { error = NO_CONNECTION_ERROR_MSG }).ToString();
			if(CrossConnectivity.Current.IsConnected) {
				var response = await PostAsync(uri, data);

				try {
					response.EnsureSuccessStatusCode();
				}
				catch(HttpRequestException e) {
					return await Task.Run(() => JObject.FromObject(new WSResult { success = false, error = e.Message }));
				}

				result = await response.Content.ReadAsStringAsync();
			}
			return await Task.Run(() => JObject.Parse(result));
		}


		public async Task<byte[]> DownloadImageAsync(string url) {
			byte[] result = null;
			if(CrossConnectivity.Current.IsConnected) {
				result = await GetByteArrayAsync(url);
			}
			return result;
		}
	}
}
