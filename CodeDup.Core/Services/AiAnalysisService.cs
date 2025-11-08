using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeDup.Core.Models;

namespace CodeDup.Core.Services;

// AI 分析服务 - DeepSeek API
public class AiAnalysisService {
    // ====== 配置区域 - 修改这里的 API Key ======
    private const string API_KEY = "sk-ac5e0f87a85c460b994348ee7ca0b177";  // 替换为你的 DeepSeek API Key
    private const string API_ENDPOINT = "https://api.deepseek.com/v1/chat/completions";
    private const string MODEL_NAME = "deepseek-chat";
    // ==========================================
    
    private readonly HttpClient _httpClient;

    public AiAnalysisService() {
        _httpClient = new HttpClient {
            Timeout = TimeSpan.FromMinutes(2)  // API 调用超时 2 分钟
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
    }

    // 分析重复代码并生成报告（流式传输版本）
    public async IAsyncEnumerable<string> AnalyzeDuplicateCodeStream(DuplicateAnalysisResult result, int topN = 10) {
        if (result.TotalFragments == 0) {
            yield return "没有找到重复代码片段。";
            yield break;
        }

        var prompt = BuildPrompt(result, topN);
        
        await foreach (var chunk in CallDeepSeekApiStream(prompt)) {
            yield return chunk;
        }
    }

    // 构建提示模板
    private string BuildPrompt(DuplicateAnalysisResult result, int topN) {
        var sb = new StringBuilder();
        
        sb.AppendLine("你是一位代码相似性分析专家。请客观分析以下代码重复情况，重点关注重复的性质和可能的原因。");
        sb.AppendLine();
        sb.AppendLine("## 数据统计");
        sb.AppendLine($"- 发现 {result.TotalFragments} 个重复片段");
        sb.AppendLine($"- 共计 {result.TotalOccurrences} 次重复出现");
        sb.AppendLine($"- 最多重复 {result.Fragments.Max(f => f.OccurrenceCount)} 次");
        sb.AppendLine();
        
        sb.AppendLine($"## 重复片段详情（前 {Math.Min(topN, result.TotalFragments)} 个）");
        
        var topFragments = result.Fragments.Take(topN).ToList();
        for (int i = 0; i < topFragments.Count; i++) {
            var fragment = topFragments[i];
            sb.AppendLine();
            sb.AppendLine($"【片段 {i + 1}】");
            sb.AppendLine($"重复次数: {fragment.OccurrenceCount} 次");
            sb.AppendLine($"代码行数: {fragment.LineCount} 行");
            sb.AppendLine($"出现文件: {string.Join(", ", fragment.Locations.Select(l => l.FileName))}");
            sb.AppendLine($"代码内容:");
            // 限制代码长度，避免超过 token 限制
            var codePreview = fragment.Content.Length > 400 
                ? fragment.Content.Substring(0, 400) + "..." 
                : fragment.Content;
            sb.AppendLine(codePreview);
            sb.AppendLine();
        }
        
        sb.AppendLine("## 分析要求");
        sb.AppendLine("请提供简洁的分析报告，包含：");
        sb.AppendLine("1. 重复代码的严重程度（高/中/低）及判断依据");
        sb.AppendLine("2. 重复代码的特征分析（是否为通用代码、业务逻辑、算法实现等）");
        sb.AppendLine("3. 可能的成因判断（正常复用、模板代码、可能存在抄袭等）");
        sb.AppendLine();
        sb.AppendLine("输出格式：纯文本，中文，简洁客观，不使用 Markdown 语法，但要保持结构化的输出，有正确的换行和标号。");
        
        return sb.ToString();
    }

    // 调用 DeepSeek API（流式传输）
    private async IAsyncEnumerable<string> CallDeepSeekApiStream(string prompt) {
        IAsyncEnumerable<string>? stream = null;
        Exception? error = null;
        
        try {
            stream = CallDeepSeekApiStreamInternal(prompt);
        }
        catch (Exception ex) {
            error = ex;
        }
        
        if (error != null) {
            yield return $"\n初始化 API 调用失败：{error.Message}";
            yield break;
        }

        // 逐步返回流内容
        if (stream != null) {
            await foreach (var chunk in stream) {
                yield return chunk;
            }
        }
    }

    // 实际的流式 API 调用实现
    private async IAsyncEnumerable<string> CallDeepSeekApiStreamInternal(string prompt) {
        var requestBody = new {
            model = MODEL_NAME,
            messages = new[] {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000,
            stream = true  // 启用流式传输
        };

        var request = new HttpRequestMessage(HttpMethod.Post, API_ENDPOINT) {
            Content = JsonContent.Create(requestBody)
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode) {
            var error = await response.Content.ReadAsStringAsync();
            yield return $"API 调用失败：{response.StatusCode} - {error}\n请检查网络连接和 API Key 配置。";
            yield break;
        }

        // 读取流式响应
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream) {
            var line = await reader.ReadLineAsync();
            
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) {
                continue;
            }

            // 去掉 "data: " 前缀
            var data = line.Substring(6);
            
            // [DONE] 表示流结束
            if (data == "[DONE]") {
                break;
            }

            // 解析 JSON - 忽略解析错误
            DeepSeekStreamChunk? chunk = null;
            try {
                chunk = JsonSerializer.Deserialize<DeepSeekStreamChunk>(data);
            }
            catch (JsonException) {
                // 忽略无法解析的行
                continue;
            }
            
            if (chunk?.Choices != null && chunk.Choices.Length > 0) {
                var content = chunk.Choices[0].Delta?.Content;
                if (!string.IsNullOrEmpty(content)) {
                    yield return content;
                }
            }
        }
    }

    // 调用 DeepSeek API（非流式，已废弃，保留用于向后兼容）
    private async Task<string> CallDeepSeekApi(string prompt) {
        var requestBody = new {
            model = MODEL_NAME,
            messages = new[] {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 2000  // 限制输出长度
        };

        var response = await _httpClient.PostAsJsonAsync(API_ENDPOINT, requestBody);
        
        if (!response.IsSuccessStatusCode) {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API 返回错误 {response.StatusCode}: {error}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DeepSeekResponse>(jsonResponse);
        
        if (result?.Choices == null || result.Choices.Length == 0) {
            throw new Exception("API 返回空响应");
        }

        return result.Choices[0].Message.Content;
    }

    // DeepSeek API 响应模型（非流式）
    private class DeepSeekResponse {
        [JsonPropertyName("choices")]
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    private class Choice {
        [JsonPropertyName("message")]
        public Message Message { get; set; } = new();
        
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }
    }

    private class Message {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
    
    // 流式响应特有的 Delta 模型
    private class Delta {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
    
    // DeepSeek API 流式响应模型
    private class DeepSeekStreamChunk {
        [JsonPropertyName("choices")]
        public StreamChoice[] Choices { get; set; } = Array.Empty<StreamChoice>();
    }
    
    private class StreamChoice {
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }
    }
}
