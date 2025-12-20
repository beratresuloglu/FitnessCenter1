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
        private const string GroqApiKey = "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";

        private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiTrainerViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(AiTrainerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                string userPrompt = $"Ben {model.Age} yaşında, {model.Weight} kg ağırlığında, {model.Height} cm boyunda bir {model.Gender} bireyim. " +
                                    $"Hareket seviyem: {model.ActivityLevel}. Temel hedefim: {model.Goal}. " +
                                    $"Bana maddeler halinde, emojiler kullanarak samimi bir spor hocası gibi haftalık antrenman ve beslenme tavsiyesi ver. Cevabı Türkçe ver.";

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
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GroqApiKey}");

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
                        model.AiResponse = $"Hata oluştu ({response.StatusCode}): {errorMsg}";
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