---
name: LLM Markdown Block 提示词
overview: 在项目 JSONSchema 文件夹下新建一份中文为主的提示词文档（.md），教会 LLM 如何使用 Block 数据模型把 markdown 内容表达为 JSON 块数组。覆盖 6 种核心块（heading/paragraph/code/list/blockquote/table）外加常用扩展（image/link/hr），以自然语言说明各块字段并配以完整 JSON 示例。
todos:
  - id: map-block-fields
    content: 梳理 9 种块字段映射与渲染行为，依据 Block.vb 与 BlockRenderer.vb 作为事实依据
    status: completed
  - id: write-prompt-core
    content: 编写 LLMPrompt.md 概述、通用约定与 9 种块逐一说明及最小示例
    status: completed
    dependencies:
      - map-block-fields
  - id: write-prompt-examples
    content: 编写综合示例文档与注意事项，并校验示例字段与 JSONRenderer.Parse 一致
    status: completed
    dependencies:
      - write-prompt-core
---

## 用户需求

用户维护一个用 VB.NET 编写的 markdown 处理模块（JSONSchema 文件夹下已用 Block 类定义了一套以 JSON 对象描述 markdown 文档内容的数据模型）。现在需要编写一份**中文提示词（prompt）**，让 LLM 学会把待生成的 markdown 内容转换为「Block 对象组成的 JSON 数组」，以便现有 `JSONRenderer.Parse` 解析后渲染成 markdown/HTML。

## 产品概述

交付一份独立的 Markdown 文档（建议命名为 `LLMPrompt.md`，置于 JSONSchema 文件夹下），作为面向 LLM 的系统提示词。文档以中文为主，采用「自然语言说明 + 示例」形式，不包含机器可读 JSON Schema。

## 核心功能

- 角色与任务说明：LLM 需将用户提供的 markdown 内容需求转换为 Block JSON 数组。
- 通用格式约定：顶层为 JSON 数组；`type` 必填且统一小写；未使用字段可省略；不支持复杂嵌套。
- 9 种块逐一说明（6 核心 + 3 常用）：heading、paragraph、code、list、blockquote、table，外加 image、link、hr。每种给出字段说明与一个最小 JSON 示例。
- 一个综合示例：串联 heading/paragraph/list/code/table/blockquote/image/link/hr，体现编排顺序。
- 注意事项：content 中的换行与特殊字符处理、table 的 `rows` 与 `headers` 列数一致、`list` 的 `ordered` 与 `items` 搭配、`type` 取值须与 Block.vb 一致等。
- 所有示例必须能被 `JSONRenderer.Parse` 正确解析（字段名、type 取值与 Block.vb / BlockRenderer.vb 实现一致）。