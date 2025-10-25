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