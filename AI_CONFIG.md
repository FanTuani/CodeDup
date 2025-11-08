# DeepSeek API 配置说明

## 获取 API Key

1. 访问 DeepSeek 官网：https://platform.deepseek.com/
2. 注册/登录账号
3. 进入"API Keys"页面
4. 创建新的 API Key
5. 复制 API Key（格式：`sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`）

## 配置步骤

1. 打开文件：`CodeDup.Core/Services/AiAnalysisService.cs`
2. 找到第 10 行：
   ```csharp
   private const string API_KEY = "sk-your-api-key-here";
   ```
3. 将 `"sk-your-api-key-here"` 替换为你的真实 API Key
4. 保存文件
5. 重新编译项目

## 使用说明

1. 在主窗口中点击"分析重复代码"按钮
2. 设置参数并点击"开始分析"
3. 分析完成后，点击"生成 AI 分析报告（前 10 个片段）"按钮
4. 等待 10-30 秒，AI 会生成详细的分析报告

## 费用说明

- DeepSeek API 价格：约 0.002 元/1K tokens
- 每次分析大约消耗 1000-2000 tokens
- 每次分析费用约 0.002-0.004 元（非常便宜）

## 常见问题

**Q: API 调用失败怎么办？**
A: 检查：
1. API Key 是否正确配置
2. 网络连接是否正常
3. DeepSeek 账户是否有余额

**Q: 可以用其他 AI 吗？**
A: 可以，修改 `AiAnalysisService.cs` 中的 `API_ENDPOINT` 和请求格式即可支持 OpenAI、通义千问等。

**Q: 报告生成很慢？**
A: 正常现象，AI 分析需要 10-30 秒，请耐心等待。
