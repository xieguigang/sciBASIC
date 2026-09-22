# 网络图文件 I/O 与第三方格式互操作

## 引言

网络图的「输入」来源极其多样：上游分析程序可能给 CSV、给 JSON，也可能直接给 Cytoscape 或 Gephi 的导出文件；而「输出」目标同样多样——要发给合作者，可能要 Sigma.js 的网页格式，也可能要 graphology 的 JSON。

本包是 `sciBASIC#` 网络可视化栈的 **I/O 层**：把各种格式统一读入 `NetworkGraph`，也能把 `NetworkGraph` 写成任意支持的格式。

## 设计目标

- **格式即插件**：新增一种格式只需新增一个读写模块，不影响其他格式；
- **原生格式优先**：CSV / JSON 这类通用格式作为「中间格式」，便于与外部工具交换；
- **浏览器可直接渲染**：内置 Sigma.js 与 graphology 输出，省去前端转换步骤。

## 核心特性

- **原生格式**：
  - 表格（CSV 风格）：`StreamTable`、`TabularCreator`、`ModelLoader`、`ModelExtensions`；
  - JSON：`FileStream.Json`；
  - 通用入口：`FileStream.Generic`。
- **第三方互操作**：
  - **Gephi / GML**：`edge`、`GML`、`graphics`、`node` —— 读取 GML 文档及其节点、边与图形属性；
  - **Cytoscape**：`Cytoscape`、`CytoscapeTableLoader`；
  - **Javascript**：`graphology` 与 `SigmaJs` —— 直接产出浏览器端可视化所需的文档。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.visualize.Network`（根） | 入口与 Sigma.js 输出 |
| `....Network.IO`（+ `.FileStream` / `.FileStream.Json`） | 表格与 JSON 读写 |
| `....Network.Cytoscape` | Cytoscape 读取 |
| `....Network.Gephi` | GML 读取 |
| `....Network.Javascript` | Sigma.js / graphology 输出 |

## 关键类型与 API

- `...Network.IO.FileStream.ModelLoader` —— 按内容自动分派格式并载入图；
- `...Network.IO.FileStream.StreamTable` / `TabularCreator` —— 表格形式的节点 / 边读写；
- `...Network.IO.FileStream.Json` —— JSON 序列化；
- `...Network.Cytoscape.CytoscapeTableLoader` —— Cytoscape 网络文件读取；
- `...Network.Gephi.GML` —— GML 图文档读取；
- `...Network.Javascript.graphology` / `SigmaJs` —— 浏览器端格式输出。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network.IO.FileStream

' 读取：按内容自动识别格式
Dim g = ModelLoader.Load("./network.csv")

' 写出：转成 JSON 交给下游
Call g.SaveJson("./network.json")
```

## 实现要点

- **中间格式的重要性**：有了「表格 + JSON」这对中间格式，任意两个格式之间只需各自实现「↔ 中间格式」两次转换，而不是两两互转——这是本包能低成本支持多种格式的关键。
- **图形属性也属于数据**：GML / Gephi 文件常把节点坐标、颜色写进图形属性；读取时保留这些属性，才能让导入的图保持原有外观。
- **浏览器格式的必要性**：Sigma.js 与 graphology 使用不同的 JSON 结构；直接输出可省去前端手写转换。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.visualize.Network.IO.Extensions`
- TargetFramework：`net10.0`
- Tags：`scibasic;network-io;cytoscape;gephi;gml;sigma-js;graphology;file-format`
- 许可：GPL-3.0-or-later
