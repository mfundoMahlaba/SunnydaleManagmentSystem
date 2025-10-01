using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebApplication3.Services
{
    public class FacePlusPlusService
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly HttpClient _client;

        public FacePlusPlusService(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _client = new HttpClient();
        }

        public async Task<string> DetectFaceTokenAsync(string base64Image)
        {
            var url = "https://api-us.faceplusplus.com/facepp/v3/detect";
            var values = new Dictionary<string, string>
            {
                {"api_key", _apiKey},
                {"api_secret", _apiSecret},
                {"image_base64", base64Image}
            };

            var content = new FormUrlEncodedContent(values);
            var resp = await _client.PostAsync(url, content);
            var json = await resp.Content.ReadAsStringAsync();

            var j = JObject.Parse(json);
            var faces = j["faces"] as JArray;
            if (faces != null && faces.Count > 0)
                return faces[0]["face_token"].ToString();

            return null;
        }

        public async Task<double?> CompareToFaceTokenAsync(string base64Image, string faceToken)
        {
            var url = "https://api-us.faceplusplus.com/facepp/v3/compare";
            var values = new Dictionary<string, string>
            {
                {"api_key", _apiKey},
                {"api_secret", _apiSecret},
                {"image_base64_1", base64Image},
                {"face_token2", faceToken}
            };

            var content = new FormUrlEncodedContent(values);
            var resp = await _client.PostAsync(url, content);
            var json = await resp.Content.ReadAsStringAsync();

            var j = JObject.Parse(json);
            if (j["confidence"] != null)
                return double.Parse(j["confidence"].ToString());

            return null;
        }
    }
}
