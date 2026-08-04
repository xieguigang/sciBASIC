---
name: fix-docx-module
overview: 修复 ImageHelper.vb 中 WriteChunk 方法的数组越界 bug，确保 docx 文档生成和纯文本提取测试能成功运行。
todos:
  - id: fix-crc-array
    content: 修复 ImageHelper.vb 中 WriteChunk 方法的 CRC 数组大小计算错误（第186行），将 New Byte(3 + dataLen - 1) 改为 New Byte(4 + dataLen - 1)
    status: completed
  - id: verify-full-pipeline
    content: 编译并运行测试项目，验证 docx 文档生成和纯文本提取全流程通过
    status: completed
    dependencies:
      - fix-crc-array
---

## 用户需求

调试 VB.NET 的 docx 文档处理模块，修复运行时崩溃问题，使测试程序成功运行，实现以下核心功能：

- 成功生成 .docx 文档（含样式、标题、段落、表格、代码块、图片、引用、列表等）
- 从已生成的 .docx 文档中成功提取纯文本文字和元数据

## 当前问题

项目编译通过（0 errors），但运行时在 `ImageHelper.vb` 第188行的 `Array.Copy` 调用中崩溃，错误信息为 `Destination array was not long enough`，导致整个测试流水线中断。

## 技术栈

- **语言**: VB.NET
- **运行时**: .NET 10.0
- **核心依赖**: Microsoft.VisualBasic.Core, System.IO.Compression, System.Xml.Linq

## 根因分析

`ImageHelper.vb` 第186行的 CRC 计算缓冲区大小存在差一错误：

```
' 当前错误代码（第186行）
crcData = New Byte(3 + dataLen - 1) {}
```

在 VB.NET 中，`New Byte(n)` 创建的数组上限为 `n`，长度为 `n + 1`。此处：

- 当前：上限 = `dataLen + 2`，数组长度 = `dataLen + 3`
- 需要：4字节（type 字段）+ dataLen 字节（data 字段）= **`dataLen + 4`** 长度
- 少分配了 1 字节，导致 `Array.Copy(data, 0, crcData, 4, dataLen)` 写入越界

## 修复方案

将第186行改为：

```
crcData = New Byte(4 + dataLen - 1) {}
```

即数组上限为 `dataLen + 3`，长度为 `dataLen + 4`，满足 CRC 计算所需空间。

## 影响范围

此修复仅涉及 `ImageHelper.vb` 的 `WriteChunk` 私有方法中的数组初始化，不影响任何外部 API 合约或调用方。

## 验证方式

修复后重新运行 `dotnet run --project test/test.vbproj`，验证三个 Demo 场景全部执行成功：

1. DemoFullFeatures：生成含样式、表格、代码块的全功能报告
2. DemoBlockModel：通过 Block 模型生成文档
3. DemoTextExtraction：从生成的 docx 提取元数据和纯文本