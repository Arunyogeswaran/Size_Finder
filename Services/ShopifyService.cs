using Newtonsoft.Json;
using Size_Finder.Models;

namespace Size_Finder.Services
{
    public class ShopifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _shopDomain;
        private readonly string _accessToken;

        public ShopifyService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _shopDomain = config["Shopify:ShopDomain"];
            _accessToken = config["Shopify:AccessToken"];
        }

        public async Task<ShopifyProduct> GetProductAsync(string productId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://{_shopDomain}/admin/api/2024-01/products/{productId}.json");
            request.Headers.Add("X-Shopify-Access-Token", _accessToken);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ShopifyProductResponse>(json);
            return result?.Product;
        }

        public async Task<ShopifyVariant> FindVariantBySize(string productId, string size)
        {
            var product = await GetProductAsync(productId);
            if (product == null) return null;
            return product.Variants.FirstOrDefault(v =>
                v.Option1.Equals(size, StringComparison.OrdinalIgnoreCase) ||
                v.Title.Contains(size, StringComparison.OrdinalIgnoreCase));
        }
    }
}