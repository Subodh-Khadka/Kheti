using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kheti.KhetiUtils
{
    // Class for handling Khalti payment operations
    public class KhaltiPayment
    {
        // Method to initiate payment for an order
        public static async Task<string> InitiateOrderPayment(int purchaseOrderId, int totalAmountInPaisa, string returnUrl, string purchase_order_name)
        {
            try
            {
                // Define the Khalti API endpoint
                var url = "https://a.khalti.com/api/v2/epayment/initiate/";

                var payload = new  // Prepare payload for the API request
                {
                    return_url = returnUrl,
                    website_url = "https://localhost:7108/",
                    amount = 1000.ToString(),
                    purchase_order_id = purchaseOrderId,
                    purchase_order_name = purchase_order_name,
                };

                // Serialize payload to JSON
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var client = new HttpClient();  // Create HttpClient instance
                /*client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455");*/
                /*client.DefaultRequestHeaders.Add("Authorization", "key test_secret_key_a7bcc8829c0a4fda8212857e8a935049");*/
                // Set Khalti API key
                client.DefaultRequestHeaders.Add("Authorization", "key 52e3cf49646a4e1a85b47cd3e0b56a8e");

                var response = await client.PostAsync(url, content);  // Send API request to Khalti
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) // Check if API request was successful
                {
                    throw new HttpRequestException($"Failed to initiate payment. Status code: {response.StatusCode}");
                }

                // Deserialize the response to get the payment URL
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string paymentUrl = responseObject.payment_url;
                int a = 10;

                return paymentUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating payment: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> InitiateBookingPayment(Guid purchaseOrderId, int totalAmountInPaisa, string returnUrl,string purchase_order_name)
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
                    purchase_order_name = purchase_order_name,
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                /*client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455");*/
                /*client.DefaultRequestHeaders.Add("Authorization", "key test_secret_key_a7bcc8829c0a4fda8212857e8a935049");*/
                client.DefaultRequestHeaders.Add("Authorization", "key 52e3cf49646a4e1a85b47cd3e0b56a8e");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to initiate payment. Status code: {response.StatusCode}");
                }

                // Deserialize the response to get the payment URL
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string paymentUrl = responseObject.payment_url;

                return paymentUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating payment: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> InitiateRemainingBokkingPayment(Guid purchaseOrderId, int totalAmountInPaisa, string returnUrl, string purchase_order_name)
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
                    purchase_order_name = purchase_order_name,
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                /*client.DefaultRequestHeaders.Add("Authorization", "key live_secret_key_68791341fdd94846a146f0457ff7b455");*/
                /*client.DefaultRequestHeaders.Add("Authorization", "key test_secret_key_a7bcc8829c0a4fda8212857e8a935049");*/
                client.DefaultRequestHeaders.Add("Authorization", "key 52e3cf49646a4e1a85b47cd3e0b56a8e");

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to initiate payment. Status code: {response.StatusCode}");
                }

                // Deserialize the response to get the payment URL
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string paymentUrl = responseObject.payment_url;

                return paymentUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating payment: {ex.Message}");
                throw;
            }
        }
    }
}
