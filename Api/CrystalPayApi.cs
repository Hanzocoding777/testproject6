using System.Text;
using Newtonsoft.Json;
using Tools;

namespace Premium.Api;

public class CrystalPayApi
{
    public CrystalPayApi(string authLogin, string authSecret)
    {
        AuthLogin = authLogin;
        AuthSecret = authSecret;
    }

    private string AuthLogin { get; set; }
    private string AuthSecret { get; set; }
    private string Url => "https://api.crystalpay.io/v2";

# nullable disable
    public class CreateInvoiceEntry
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
# nullable enable
    
    public async Task<CreateInvoiceEntry> CreateInvoice(int amount, int lifeTime = 15, string extra = "")
    {
        try
        {
            using HttpClient client = new();

            var content =
                $"{{\"auth_login\":\"{AuthLogin}\",\"auth_secret\":\"{AuthSecret}\",\"amount\":{amount},\"type\":\"purchase\",\"lifetime\":{lifeTime},\"extra\":\"{extra}\"}}";

            var httpResponse = await client.PostAsync($"{Url}/invoice/create/", new StringContent(content, Encoding.UTF8, "application/json"));
            var answer = JsonConvert.DeserializeObject<CreateInvoiceEntry>(
                await httpResponse.Content.ReadAsStringAsync(),
                settings: Converters.JsonConverter.Settings);

            if (answer == null)
                throw new Exception("Invoice wasn't created");
            
            if (answer.Error == true)
                throw new Exception("Error while creating invoice");

            return answer;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
# nullable disable
    public class InvoiceInfoEntry
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("required_method")]
        public object RequiredMethod { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("service_commission")]
        public int ServiceCommission { get; set; }

        [JsonProperty("extra_commission")]
        public int ExtraCommission { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("pay_amount")]
        public int PayAmount { get; set; }

        [JsonProperty("remaining_amount")]
        public int RemainingAmount { get; set; }

        [JsonProperty("balance_amount")]
        public int BalanceAmount { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("callback_url")]
        public object CallbackUrl { get; set; }

        [JsonProperty("extra")]
        public object Extra { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("expired_at")]
        public string ExpiredAt { get; set; }
    }
# nullable enable
    

    public async Task<InvoiceInfoEntry> InvoiceInfo(string id)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        var content =
            $"{{\n\"auth_login\": \"{AuthLogin}\",\n\"auth_secret\": \"{AuthSecret}\",\n\"id\": {id}\n}}";
        
        var httpResponse = await client.PostAsync($"{Url}/invoice/info/", new StringContent(content));
        var answer = JsonConvert.DeserializeObject<InvoiceInfoEntry>(await httpResponse.Content.ReadAsStringAsync(),
            settings: Converters.JsonConverter.Settings);

        if (answer == null)
            throw new Exception("Invoice info wasn't deserialized");

        return answer;
    }
}