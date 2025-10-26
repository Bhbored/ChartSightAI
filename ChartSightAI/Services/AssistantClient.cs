using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Storage;
using ChartSightAI.MVVM.Models;
using ChartSightAI.DTO_S.AI_S;

namespace ChartSightAI.Services
{
    public sealed class AssistantClient
    {
        #region Fields
        private readonly string _apiKey;
        private readonly string _model;
        private readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private const string SystemPrompt =
        """
        You are ChartSightAI. Analyze TRADING CHARTS and return STRICT JSON matching AiAnalysisResult.

        Return only:
        {
          "summary": string,
          "trendAnalysis": string,
          "pattern": string,
          "supportResistance": [ { "type":"Support"|"Resistance", "price": number, "confidence": number } ],
          "indicators": [string],
          "risk": string,
          "tradeIdea": { "entry": number, "stopLoss": number, "targets": [number], "rationale": string },
          "explainability": string
        }

        If the image is clearly not a trading price chart, set:
          summary = "Image is not a trading price chart."
          trendAnalysis = "" ; pattern = "" ; supportResistance = [] ; indicators = [] ; risk = "" ;
          tradeIdea = null ; explainability = ""
        """;
        #endregion

        #region Ctor/Factory
        private AssistantClient(string apiKey, string model)
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;
        }

        private sealed record Settings(string? ApiKey, string? Model);

        public static async Task<AssistantClient> CreateFromAssetAsync(string fileName = "ai.settings.json")
        {
            var (apiKey, model) = await LoadSettingsAsync(fileName);

            apiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    $"Missing OpenAI API key. Put {fileName} in Resources/Raw (Build Action: MauiAsset), " +
                    $"or place it in {FileSystem.AppDataDirectory}, or set environment variable OPENAI_API_KEY.");

            return new AssistantClient(apiKey, model ?? "gpt-4o-mini");
        }
        #endregion

        #region Settings Loader
        private static async Task<(string? apiKey, string? model)> LoadSettingsAsync(string fileName)
        {
            Settings? cfg = null;

            // 1) MAUI asset: Resources/Raw/ai.settings.json
            try
            {
                using var s = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var r = new StreamReader(s, Encoding.UTF8);
                cfg = JsonSerializer.Deserialize<Settings>(await r.ReadToEndAsync());
                if (!string.IsNullOrWhiteSpace(cfg?.ApiKey))
                    return (cfg!.ApiKey, cfg.Model);
            }
            catch { /* ignore */ }

            // 2) AppDataDirectory (runtime-writable)
            try
            {
                var alt = Path.Combine(FileSystem.AppDataDirectory, fileName);
                if (File.Exists(alt))
                {
                    var json = await File.ReadAllTextAsync(alt, Encoding.UTF8);
                    cfg = JsonSerializer.Deserialize<Settings>(json);
                    if (!string.IsNullOrWhiteSpace(cfg?.ApiKey))
                        return (cfg!.ApiKey, cfg.Model);
                }
            }
            catch { /* ignore */ }

            // 3) Embedded resource fallback
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var res = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                if (res != null)
                {
                    using var s = asm.GetManifestResourceStream(res)!;
                    using var r = new StreamReader(s, Encoding.UTF8);
                    cfg = JsonSerializer.Deserialize<Settings>(await r.ReadToEndAsync());
                    if (!string.IsNullOrWhiteSpace(cfg?.ApiKey))
                        return (cfg!.ApiKey, cfg.Model);
                }
            }
            catch { /* ignore */ }

            return (null, null);
        }
        #endregion

        #region Public API
        public async Task<AiAnalysisResult?> AnalyzeAsync(AnalysisRequest req, CancellationToken ct = default)
        {
            var body = new
            {
                model = _model,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = BuildUserContent(req) }
                }
            };

            var json = JsonSerializer.Serialize(body);
            using var http = CreateHttpClient();
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions", content, ct);
            var payload = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"AI request failed ({resp.StatusCode}): {payload}");

            var envelope = JsonSerializer.Deserialize<ChatEnvelope>(payload, _jsonOpts);
            var text = envelope?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            text = text.Trim();
            if (text.StartsWith("```", StringComparison.Ordinal)) text = StripCodeFence(text);

            var result = JsonSerializer.Deserialize<AiAnalysisResult>(text, _jsonOpts);
            return result;
        }
        #endregion

        #region Builders
        private static List<object> BuildUserContent(AnalysisRequest req)
        {
            var parts = new List<object>();

            var preface =
                $"Market: {req.MarketType}\n" +
                $"Timeframe: {req.TimeFrame}\n" +
                $"Direction: {req.TradeDirection}\n" +
                $"Indicators: {string.Join(", ", req.Indicators ?? Array.Empty<string>())}\n" +
                (string.IsNullOrWhiteSpace(req.Notes) ? "" : $"Notes: {req.Notes}\n") +
                "Analyze the image and return ONLY the strict JSON object described by the system message.";

            parts.Add(new { type = "text", text = preface });

            if (!string.IsNullOrWhiteSpace(req.PreviewImage) && File.Exists(req.PreviewImage))
            {
                var bytes = File.ReadAllBytes(req.PreviewImage);
                var ext = Path.GetExtension(req.PreviewImage).ToLowerInvariant();
                var mime = ext switch
                {
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "image/*"
                };
                var dataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

                parts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = dataUrl }
                });
            }

            return parts;
        }
        #endregion

        #region Http/Utils
        private HttpClient CreateHttpClient()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        }

        private static string StripCodeFence(string s)
        {
            if (!s.StartsWith("```", StringComparison.Ordinal)) return s;
            var i = s.IndexOf('\n');
            if (i >= 0 && i + 1 < s.Length) s = s[(i + 1)..];
            if (s.EndsWith("```", StringComparison.Ordinal)) s = s[..^3];
            return s.Trim();
        }
        #endregion
    }
}
