using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChartSightAI.MVVM.Models;
using static ChartSightAI.MVVM.Models.Enums;
using ChartSightAI.DTO_S.AI_S;

namespace ChartSightAI.Services
{
    public sealed class AssistantClient
    {
        private readonly string _apiKey;
        private readonly string _model;

        private const string SystemPrompt =
        """
        You are ChartSightAI. Analyze TRADING CHARTS and return ONLY this JSON object (no prose, no code fences):
        {
          "summary": string,
          "trend_analysis": string,
          "pattern": string,
          "support_resistance": [ { "type":"Support"|"Resistance", "price": number, "confidence": number } ],
          "indicators": [string],
          "risk": string,
          "trade_idea": { "entry": number, "stop_loss": number, "targets": [number], "rationale": string },
          "explainability": string
        }
        Confidence must be a calibrated probability between 0 and 1 with two decimals, typical range 0.55–0.85.
        Avoid 0.95+ except when extremely certain; never return 1.00 unless trivial.
        If the image is clearly not a trading chart, return the same object with:
        summary = "Image is not a trading price chart.", other fields empty/null.
        Keys may be snake_case or camelCase. Numbers must be numbers, not strings.
        """;

        private const string ForceAnalyzeText =
        "Assume the image IS a trading price chart. Do not return the 'not a trading price chart' message. Analyze and output the JSON.";

        private sealed class Settings
        {
            [JsonProperty("apiKey")] public string? ApiKey { get; set; }
            [JsonProperty("model")] public string? Model { get; set; }
        }

        private AssistantClient(string apiKey, string model)
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model;
        }

        public static async Task<AssistantClient> CreateFromAssetAsync(string fileName = "ai.settings.json")
        {
            var (apiKey, model) = await LoadSettingsAsync(fileName);
            apiKey ??= Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException($"Missing OpenAI API key. Put {fileName} in Resources/Raw (Build Action: MauiAsset), or place it in {FileSystem.AppDataDirectory}, or set environment variable OPENAI_API_KEY.");
            return new AssistantClient(apiKey, string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model!);
        }

        private static async Task<(string? apiKey, string? model)> LoadSettingsAsync(string fileName)
        {
            Settings? cfg = null;
            try
            {
                using var s = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var r = new StreamReader(s, Encoding.UTF8);
                cfg = JsonConvert.DeserializeObject<Settings>(await r.ReadToEndAsync());
                if (!string.IsNullOrWhiteSpace(cfg?.ApiKey)) return (cfg!.ApiKey, cfg.Model);
            }
            catch { }
            try
            {
                var alt = Path.Combine(FileSystem.AppDataDirectory, fileName);
                if (File.Exists(alt))
                {
                    var json = await File.ReadAllTextAsync(alt, Encoding.UTF8);
                    cfg = JsonConvert.DeserializeObject<Settings>(json);
                    if (!string.IsNullOrWhiteSpace(cfg?.ApiKey)) return (cfg!.ApiKey, cfg.Model);
                }
            }
            catch { }
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var res = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
                if (res != null)
                {
                    using var s = asm.GetManifestResourceStream(res)!;
                    using var r = new StreamReader(s, Encoding.UTF8);
                    cfg = JsonConvert.DeserializeObject<Settings>(await r.ReadToEndAsync());
                    if (!string.IsNullOrWhiteSpace(cfg?.ApiKey)) return (cfg!.ApiKey, cfg.Model);
                }
            }
            catch { }
            return (null, null);
        }

        public async Task<AiAnalysisResult?> AnalyzeAsync(AnalysisRequest req, CancellationToken ct = default)
        {
            var first = await AnalyzeOnceAsync(req, false, ct);
            if (first != null && IsNotChart(first) && !string.IsNullOrWhiteSpace(req.PreviewImage) && File.Exists(req.PreviewImage))
            {
                var second = await AnalyzeOnceAsync(req, true, ct);
                return second ?? first;
            }
            return first;
        }

        private async Task<AiAnalysisResult?> AnalyzeOnceAsync(AnalysisRequest req, bool forceAnalyze, CancellationToken ct)
        {
            var body = new
            {
                model = _model,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user",   content = BuildUserContent(req, forceAnalyze) }
                }
            };

            var json = JsonConvert.SerializeObject(body);
            using var http = CreateHttpClient();
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions", content, ct);
            var payload = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new Exception($"AI request failed ({resp.StatusCode}): {payload}");

            var root = JObject.Parse(payload);
            var text = (root["choices"]?[0]?["message"]?["content"]?.ToString() ?? "").trim();
            if (text.StartsWith("```", StringComparison.Ordinal))
            {
                var i = text.IndexOf('\n');
                if (i >= 0 && i + 1 < text.Length) text = text[(i + 1)..];
                if (text.EndsWith("```", StringComparison.Ordinal)) text = text[..^3];
                text = text.Trim();
            }
            if (string.IsNullOrWhiteSpace(text)) return null;

            if (!TryParseStrict(text, out var obj, out var parseErr))
            {
                var repaired = RepairJsonText(text);
                if (!TryParseStrict(repaired, out obj, out parseErr))
                    throw new Exception($"Invalid AI JSON: {parseErr}\n\n{text}");
            }

            ExpandFlattenedPaths(obj);

            var result = new AiAnalysisResult
            {
                Summary = S(obj, "summary"),
                TrendAnalysis = S(obj, "trend_analysis", "trendAnalysis"),
                Pattern = S(obj, "pattern"),
                Explainability = S(obj, "explainability"),
                Risk = S(obj, "risk"),
                Indicators = req.Indicators?.ToList() ?? new List<string>()
            };

            var srToken = obj["support_resistance"] ?? obj["supportResistance"];
            var srList = new List<SupportResistanceLevel>();
            if (srToken is JArray arr)
            {
                foreach (var t in arr)
                {
                    var typeStr = S(t, "type");
                    var price = D(t, "price");
                    var conf = D(t, "confidence");
                    if (!string.IsNullOrEmpty(typeStr) && price.HasValue)
                    {
                        var st = ParseSupportType(typeStr);
                        srList.Add(new SupportResistanceLevel
                        {
                            Type = st,
                            Price = price.Value,
                            Confidence = NormalizeConfidence(conf)
                        });
                    }
                }
            }
            result.SupportResistance = srList;

            var tiToken = obj["trade_idea"] ?? obj["tradeIdea"];
            if (tiToken is JObject tio)
            {
                var entry = D(tio, "entry");
                var sl = D(tio, "stop_loss", "stopLoss");
                var targetsT = tio["targets"];
                var rationale = S(tio, "rationale", "reason");
                var targets = new List<double>();
                if (targetsT is JArray ta)
                {
                    foreach (var tt in ta)
                    {
                        var v = AsDouble(tt);
                        if (v.HasValue) targets.Add(v.Value);
                    }
                }
                result.TradeIdea = new TradeIdea
                {
                    Entry = entry ?? 0d,
                    StopLoss = sl ?? 0d,
                    Targets = targets,
                    Rationale = rationale
                };
            }
            else
            {
                result.TradeIdea = new TradeIdea { Targets = new List<double>() };
            }

            return result;
        }

        private static bool TryParseStrict(string text, out JObject obj, out string? error)
        {
            try { obj = JObject.Parse(text); error = null; return true; }
            catch (Exception ex) { obj = new JObject(); error = ex.Message; return false; }
        }

        private static string RepairJsonText(string s)
        {
            var t = s.Replace('“', '"').Replace('”', '"').Replace('’', '\'');
            t = Regex.Replace(t, @"(?<!\\)\'", "\"");
            t = Regex.Replace(t, @"(?m)^\s*([A-Za-z_][A-Za-z0-9_\-\.\[\]]*)\s*:", "\"$1\":");
            t = Regex.Replace(t, @",\s*(?=[}\]])", "");
            t = t.Replace("NaN", "null").Replace("Infinity", "null").Replace("-Infinity", "null");
            return t;
        }

        private static void ExpandFlattenedPaths(JObject root)
        {
            var props = root.Properties().ToList();
            foreach (var p in props)
            {
                var mArr = Regex.Match(p.Name, @"^([A-Za-z_][A-Za-z0-9_]*)\[(\d+)\]\.([A-Za-z_][A-Za-z0-9_]*)$");
                if (mArr.Success)
                {
                    var arrName = mArr.Groups[1].Value;
                    var idx = int.Parse(mArr.Groups[2].Value, CultureInfo.InvariantCulture);
                    var field = mArr.Groups[3].Value;

                    var arr = root[arrName] as JArray;
                    if (arr == null)
                    {
                        arr = new JArray();
                        root[arrName] = arr;
                    }
                    while (arr.Count <= idx) arr.Add(new JObject());
                    var obj = (JObject)arr[idx];
                    obj[field] = p.Value;
                    p.Remove();
                    continue;
                }

                var mObj = Regex.Match(p.Name, @"^([A-Za-z_][A-Za-z0-9_]*)\.([A-Za-z_][A-Za-z0-9_]*)$");
                if (mObj.Success)
                {
                    var objName = mObj.Groups[1].Value;
                    var field = mObj.Groups[2].Value;

                    var child = root[objName] as JObject;
                    if (child == null)
                    {
                        child = new JObject();
                        root[objName] = child;
                    }
                    child[field] = p.Value;
                    p.Remove();
                }
            }
        }

        private static bool IsNotChart(AiAnalysisResult r)
        {
            if (string.IsNullOrWhiteSpace(r.Summary)) return false;
            var s = r.Summary.Trim().ToLowerInvariant();
            return s == "image is not a trading price chart." || s == "not a trading chart" || s.Contains("doesn’t look like a price chart");
        }

        private static List<object> BuildUserContent(AnalysisRequest req, bool forceAnalyze)
        {
            var parts = new List<object>();
            var preface =
                $"Market: {req.MarketType}\n" +
                $"Timeframe: {req.TimeFrame}\n" +
                $"Direction: {req.TradeDirection}\n" +
                $"Indicators: {string.Join(", ", req.Indicators ?? Array.Empty<string>())}\n" +
                (string.IsNullOrWhiteSpace(req.Notes) ? "" : $"Notes: {req.Notes}\n") +
                (forceAnalyze ? ForceAnalyzeText : "Analyze the image and return only the strict JSON object described by the system message.");
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
                parts.Add(new { type = "image_url", image_url = new { url = dataUrl } });
            }
            return parts;
        }

        private HttpClient CreateHttpClient()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        }

        private static string? S(JToken parent, params string[] keys)
        {
            foreach (var k in keys)
            {
                var v = parent[k];
                if (v != null && v.Type != JTokenType.Null) return v.ToString();
            }
            return null;
        }

        private static double? D(JToken parent, params string[] keys)
        {
            foreach (var k in keys)
            {
                var v = parent[k];
                var d = AsDouble(v);
                if (d.HasValue) return d;
            }
            return null;
        }

        private static double? AsDouble(JToken? t)
        {
            if (t == null || t.Type == JTokenType.Null) return null;
            if (t.Type == JTokenType.Float || t.Type == JTokenType.Integer) return t.Value<double>();
            var s = t.ToString().Trim();
            if (s.EndsWith("%", StringComparison.Ordinal)) s = s[..^1];
            var s2 = s.Replace(" ", "");
            if (Regex.IsMatch(s2, @"^\d{1,3}(,\d{3})+(\.\d+)?$")) s2 = s2.Replace(",", "");
            if (Regex.IsMatch(s2, @"^\d+,\d+$")) s2 = s2.Replace(",", ".");
            if (double.TryParse(s2, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            return null;
        }

        private static double NormalizeConfidence(double? c)
        {
            var v = c ?? 0.68;
            if (v > 1.0 && v <= 100.0) v = v / 100.0;
            if (v < 0) v = 0;
            if (v > 1) v = 1;
            return Math.Round(v, 2, MidpointRounding.AwayFromZero);
        }

        private static SupportType ParseSupportType(string s)
        {
            var v = s.Trim().ToLowerInvariant();
            if (v == "support" || v == "demand") return SupportType.Support;
            if (v == "resistance" || v == "supply") return SupportType.Resistance;
            if (Enum.TryParse<SupportType>(s, true, out var e)) return e;
            return SupportType.Support;
        }
    }

    static class StringExt
    {
        public static string trim(this string s) => s?.Trim() ?? string.Empty;
    }
}