using Microsoft.AspNetCore.Mvc;
using FitnessCenterWebApplication.Models.ViewModels;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterWebApplication.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        // Konfigürasyon dosyalarına (appsettings.json) erişmek için gerekli servis
        private readonly IConfiguration _configuration;

        // Groq API Adresi (Sabit kalabilir)
        private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        // Constructor (Yapıcı Metot): Dependency Injection ile Configuration'ı alıyoruz
        public AiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiTrainerViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(AiTrainerViewModel model)
        {
            // 1. ADIM: API Key'i güvenli alandan okuyoruz
            string groqApiKey = _configuration["GroqApiKey"];

            // Key kontrolü: Eğer ayarlanmamışsa hata dönelim
            if (string.IsNullOrEmpty(groqApiKey))
            {
                model.AiResponse = "Sistem Hatası: API Anahtarı bulunamadı. Lütfen yönetici ile iletişime geçin (appsettings kontrolü).";
                return View("Index", model);
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // 2. Prompt Hazırlığı
                string userPrompt = $"Ben {model.Age} yaşında, {model.Weight} kg ağırlığında, {model.Height} cm boyunda bir {model.Gender} bireyim. " +
                                    $"Hareket seviyem: {model.ActivityLevel}. Temel hedefim: {model.Goal}. " +
                                    $"Bana maddeler halinde, emojiler kullanarak samimi bir spor hocası gibi haftalık antrenman ve beslenme tavsiyesi ver. Cevabı Türkçe ver.";

                // 3. İstek Verisi (En güncel Llama 3.3 modeli ile)
                var requestData = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "user", content = userPrompt }
                    }
                };

                using (var client = new HttpClient())
                {
                    // Key'i değişkenden alıp Header'a ekliyoruz
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {groqApiKey}");

                    var jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(ApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic responseJson = JsonConvert.DeserializeObject(responseString);

                        string aiText = responseJson.choices[0].message.content;
                        model.AiResponse = aiText;
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        model.AiResponse = $"API Bağlantı Hatası ({response.StatusCode}): {errorMsg}";
                    }
                }
            }
            catch (Exception ex)
            {
                model.AiResponse = $"Sistem hatası: {ex.Message}";
            }

            return View("Index", model);
        }
    }
}