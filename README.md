# CodeDup

CodeDup 是一个功能强大的代码查重工具，支持多种编程语言和文本格式，可以帮助你快速发现项目中的代码重复现象。

## 主要特性

- **多算法支持**
  - Winnowing 算法：基于局部哈希的代码相似度检测
  - SimHash 算法：基于 Google 的 SimHash 实现的相似度检测
  - ShingleCosine 算法：基于 n-gram 的余弦相似度计算

- **多格式支持**
  - 编程语言：C#、Python、HTML、JavaScript、Java、C++、C 等
  - 文档格式：TXT、DOCX、PDF

- **强大的分析功能**
  - 两两文件对比
  - 中心文件聚类
  - 重复文件分组

- **友好的用户界面**
  - 拖拽导入文件
  - 项目管理
  - 实时文件对比
  - 结果导出

- **AI 分析报告**（可选）
  - 基于 DeepSeek API 的智能分析
  - 自动识别重复代码的严重程度和成因

## 配置 API Key（可选）

如果需要使用 AI 分析功能，请按以下步骤配置：

1. 复制 `appsettings.local.example.json` 为 `appsettings.local.json`
   ```bash
   copy appsettings.local.example.json appsettings.local.json
   ```

2. 在 `appsettings.local.json` 中填入你的 DeepSeek API Key：
   ```json
   {
     "DeepSeek": {
       "ApiKey": "your-api-key-here",
       "Endpoint": "https://api.deepseek.com/v1/chat/completions",
       "Model": "deepseek-chat"
     }
   }
   ```

3. `appsettings.local.json` 已被添加到 `.gitignore`，不会被提交到 Git

> 注意：如果不配置 API Key，其他功能仍可正常使用，只是无法生成 AI 分析报告。

## 使用方法

   - 创建新项目
   - 导入需要查重的文件
   - 选择查重算法和阈值
   - 点击"开始查重"
   - 查看并分析结果

## 项目结构

- **CodeDup.Algorithms**: 核心算法实现
- **CodeDup.Core**: 核心模型和存储接口
- **CodeDup.Text**: 文本处理和提取
- **CodeDup.App**: WPF 界面应用