    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    namespace Kheti.KhetiUtils
    {
        public class KhaltiPayment
        {
            public static async Task<string> InitiatePayment(int purchaseOrderId, int totalAmountInPaisa, string returnUrl)
            {
                try
                {
                    var url = "https://a.khalti.com/api/v2/epayment/initiate/";

                    var payload = new
                    {
                        return_url = returnUrl,
                        website_url = "https://localhost:7108/",
                        amount = 1000.ToString(),
                        purchase_order_id = purchaseOrderId,
                        purchase_order_name = "Product Name",
                    };

                    var jsonPayload = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using var client = new HttpClient();
                /*client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455");*/
                /*client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455");*/
                client.DefaultRequestHeaders.Add("Authorization", "test_public_key_6256b6173edb41a0bf932235503e4f71");

                var response = await client.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        // Handle non-success status code
                        throw new HttpRequestException($"Failed to initiate payment. Status code: {response.StatusCode}");
                    }

                    // Deserialize the response to get the payment URL
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string paymentUrl = responseObject.payment_url;

                    return paymentUrl;
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Error initiating payment: {ex.Message}");
                    throw; // Rethrow the exception
                }
            }
        }
    }
