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
        private readonly IConfiguration _configuration;

        // 1. ANALİZ İÇİN (Metin + Görüş)
        private const string GeminiAnalyzeUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        // 2. ÇİZİM İÇİN (Resim Oluşturma - Imagen 3)
        private const string ImagenUrl = "https://generativelanguage.googleapis.com/v1beta/models/imagen-3.0-generate-001:predict";

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
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> GenerateTransformation(AiTrainerViewModel model)
        {
            string apiKey = _configuration["GeminiApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                ModelState.AddModelError("", "API Key bulunamadı.");
                return View("Index", model);
            }

            if (model.UserImageFile == null || model.UserImageFile.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir fotoğraf yükleyin.");
                return View("Index", model);
            }

            if (model.UserImageFile.Length > 4 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Fotoğraf boyutu çok büyük (Max 4MB).");
                return View("Index", model);
            }

            if (!ModelState.IsValid) return View("Index", model);

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(3);

                    // FOTOĞRAFI HAZIRLA
                    string base64Image;
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            await model.UserImageFile.CopyToAsync(ms);
                            base64Image = Convert.ToBase64String(ms.ToArray());
                        }
                        model.UserImageBase64 = $"data:{model.UserImageFile.ContentType};base64,{base64Image}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Fotoğraf yüklenirken hata: {ex.Message}");
                        return View("Index", model);
                    }

                    // ==========================================================
                    // GEMINI ANALİZ
                    // ==========================================================
                    try
                    {
                        string analysisPrompt = $"Ben {model.Age} yaşında, {model.Weight} kg, {model.Height} cm boyunda, {model.Gender} biriyim. " +
                                                $"Hedefim: {model.Goal}. Hareket seviyem: {model.ActivityLevel}. " +
                                                $"Bu fotoğraftaki vücut tipimi analiz et. Hedefime ulaşmam için Türkçe, emojili, maddeler halinde antrenman ve beslenme programı yaz.";

                        var geminiPayload = new
                        {
                            contents = new[]
                            {
                        new {
                            parts = new object[]
                            {
                                new { text = analysisPrompt },
                                new { inline_data = new { mime_type = model.UserImageFile.ContentType, data = base64Image } }
                            }
                        }
                    }
                        };

                        var textContent = new StringContent(
                            JsonConvert.SerializeObject(geminiPayload),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var textResponse = await client.PostAsync($"{GeminiAnalyzeUrl}?key={apiKey}", textContent);
                        var responseBody = await textResponse.Content.ReadAsStringAsync();

                        if (textResponse.IsSuccessStatusCode)
                        {
                            try
                            {
                                dynamic result = JsonConvert.DeserializeObject(responseBody);

                                // GÜVENLİ PARSE
                                if (result?.candidates != null && result.candidates.Count > 0)
                                {
                                    var candidate = result.candidates[0];
                                    if (candidate?.content?.parts != null && candidate.content.parts.Count > 0)
                                    {
                                        model.AiTextResponse = candidate.content.parts[0].text?.ToString()
                                            ?? "Gemini'den yanıt alındı ama metin boş.";
                                    }
                                    else
                                    {
                                        model.AiTextResponse = "Gemini yanıtı beklenmeyen formatta geldi.";
                                    }
                                }
                                else
                                {
                                    model.AiTextResponse = $"Gemini'den geçersiz yanıt. Ham yanıt: {responseBody.Substring(0, Math.Min(500, responseBody.Length))}";
                                }
                            }
                            catch (Exception parseEx)
                            {
                                model.AiTextResponse = $"Yanıt parse hatası: {parseEx.Message}\n\nHam yanıt: {responseBody.Substring(0, Math.Min(500, responseBody.Length))}";
                            }
                        }
                        else
                        {
                            model.AiTextResponse = $"Gemini API Hatası ({textResponse.StatusCode}):\n{responseBody}";
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        model.AiTextResponse = "⏱️ Gemini analizi zaman aşımına uğradı. Lütfen tekrar deneyin.";
                    }
                    catch (HttpRequestException hex)
                    {
                        model.AiTextResponse = $"🌐 Bağlantı hatası: {hex.Message}";
                    }
                    catch (Exception geminiEx)
                    {
                        model.AiTextResponse = $"❌ Gemini hatası: {geminiEx.Message}\n{geminiEx.StackTrace}";
                    }

                    // ==========================================================
                    // IMAGEN (Opsiyonel - Şimdilik KAPALI)
                    // ==========================================================
                    // IMAGEN'I GEÇİCİ OLARAK KAPATIYORUZ - SORUN BURADA OLABİLİR
                    /*
                    try
                    {
                        // ... imagen kodu ...
                    }
                    catch { }
                    */
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Genel Hata: {ex.Message}\n\nStack Trace: {ex.StackTrace}");
            }

            return View("Index", model);
        }



    }
}