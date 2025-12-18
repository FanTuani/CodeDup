# Repository Guidelines

## 项目结构与模块

- `CodeDup.App`：WPF 前端（视图、视图模型、控件），使用 AvalonEdit/DiffPlex；从仓库根目录复制 `appsettings.local*.json` 配置到输出。
- `CodeDup.Algorithms`：核心相似度算法（Winnowing、SimHash、ShingleCosine）。
- `CodeDup.Core`：领域模型、服务与存储抽象。
- `CodeDup.Text`：通用文本工具。
- `Data` / `TestFiles`：手动校验的示例输入；`Exports`：生成的报表/输出。

## 构建、测试与运行

- 还原与构建：`dotnet restore`，然后 `dotnet build CodeDup.sln -c Release`。
- 本地运行界面：`dotnet run --project CodeDup.App`（Windows，已安装 .NET 8 与 WPF 工作负载）。
- 发布（便携构建）：`dotnet publish CodeDup.App -c Release -r win-x64 --self-contained false`。
- AI 报告配置：复制 `appsettings.local.example.json` 为 `appsettings.local.json` 并填写 `DeepSeek`；该文件已被 gitignore。

## 代码风格与命名

- C# 8+/net8.0，启用可空与隐式 using；优先 4 空格缩进。
- 公共类型/成员用 PascalCase；局部/字段用 camelCase，必要时私有字段可加 `_` 前缀（与文件现有风格保持一致）。
- 异步方法以 `Async` 结尾；避免在 UI 线程阻塞任务。
- WPF 绑定命名清晰（如 `MainWindow.xaml`、`ViewModels/*`）；XAML 按层级缩进。
- 业务逻辑尽量放在 `CodeDup.Core`/`CodeDup.Algorithms`，UI 逻辑放到视图模型，减少 code-behind。

## 测试指南

- 目前无自动化测试项目；如新增，推荐在 `CodeDup.Tests` 使用 `xUnit`，运行 `dotnet test`。
- 短期验证：用 `TestFiles` 作为输入，检查 `Exports` 输出；手动核对算法阈值与 UI 流程（拖拽、差异视图、导出）。
- 回归问题请附最小示例数据到 `TestFiles`（勿包含敏感信息）。

## 提交与 PR 规范

- 历史提交偏好简短动作式消息（如 `upd ...`、`add ...`）；建议保持简洁祈使句，中英皆可，不超过 ~50 字符（例：`add winnowing threshold control`）。
- PR 需说明动机与方案，列出关键变更，涉及 UI 的请附截图/GIF。
- 关联相关 issue/任务，注明配置需求（`appsettings.local.json`）与已执行的手工验证步骤。
