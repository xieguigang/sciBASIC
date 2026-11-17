---
name: 向 scibasic.net.slnx 添加缺失的 Microsoft.VisualBasic 项目
overview: 扫描全部 vbproj 文件，筛选出 SDK 风格且 RootNamespace 以 Microsoft.VisualBasic 开头的项目，与 scibasic.net.slnx 中已有条目比对，将缺失的 6 个项目按语义归类添加到对应解决方案文件夹中。
todos:
  - id: add-data-ml-projects
    content: 在 scibasic.net.slnx 的 /packages/data/ 添加 Parquet、Protobuf，在 /packages/machine_learning/ 添加 DirectML 条目
    status: completed
  - id: add-math-vision-projects
    content: 在 /packages/math.dll/ 添加 GCN、GLMNet，在 /packages/machine_learning/vision/ 添加 Cloudy.Test 条目
    status: completed
  - id: verify-solution
    content: 用只读脚本复核 110 个候选项目全部入列且 XML 结构完整
    status: completed
    dependencies:
      - add-data-ml-projects
      - add-math-vision-projects
---

## 产品概述

扫描工作区全部 `.vbproj` 文件，检查 SDK 风格且命名空间以 `Microsoft.VisualBasic` 开头的项目是否已纳入解决方案，并将缺失项补入。

## 核心功能

- 筛选同时满足以下两条件的项目：项目节点 `Sdk` 属性为 `Microsoft.NET.Sdk`；`RootNamespace` 值以 `Microsoft.VisualBasic` 起始
- 与 `scibasic.net.slnx` 中已有条目按完整路径比对，识别缺失项目
- 将缺失项目按语义归类添加至对应解决方案文件夹，沿用既有条目格式
- 验证补入后全部候选项目均被覆盖，且解决方案 XML 结构完整有效

## Tech Stack Selection

- 目标文件：`g:/GCModeller/src/runtime/sciBASIC#/scibasic.net.slnx`，XML 格式的 .slnx 解决方案文件（3125 行）
- 编辑方式：直接文本编辑，沿用文件既有 2 空格层级缩进与正斜杠 `/` 相对路径约定
- 验证方式：PowerShell 只读扫描脚本，无需引入任何新依赖或工具

## Implementation Approach

已完成只读分析：工作区共 250 个 `.vbproj`，其中 110 个满足双条件，104 个已在 slnx 中，确认 6 个缺失。策略是按同类兄弟项目所在 `Folder` 分组，将新条目插入对应位置，并复用文件中 25 个"最小条目"已有的 `<Platform Solution="*|x64" Project="x64" />` 结构，避免臆造 BuildType 配置。该方式改动面最小、与现有约定完全一致，插入操作时间复杂度为 O(n)（n 为文件行数），无性能瓶颈。

关键取舍：不采用自闭合 `<Project .../>` 或空条目，因为现有 120 个条目均含子元素；不为新项目编造大量 BuildType 映射，因为同类最小条目仅含 Platform 映射即可正常工作。

## Implementation Notes

- 路径统一使用正斜杠，相对 slnx 所在目录，例如 `Data/BinaryData/Parquet/Parquet.vbproj`
- 新增条目必须使用 `<Project Path="...">` 包裹 `<Platform Solution="*|x64" Project="x64" />` 子元素
- 严格限定改动范围：不修改任何已有条目、`<Configurations>`、`<Folder>` 名称或末尾 `</Solution>` 结构
- 插入位置参考：SQLite3 条目结束约第 430 行；Cloudy.vbproj 约第 1493 行；GNN 条目第 1576-1578 行
- 修改后重新运行只读比对脚本：110 个候选项目应全部匹配，且 Folder/Project 标签配对正确

## Architecture Design

`slnx` 为分层 XML：根 `<Solution>` 下含 `<Configurations>`、17 个 `<Folder Name="...">`（各自包含若干 `<Project Path="...">`）及根级 `<Project>` 条目。本次仅在既有 Folder 内部追加 Project 子节点，不改变层级关系。

## Directory Structure

```
g:/GCModeller/src/runtime/sciBASIC#/
└── scibasic.net.slnx  # [MODIFY] 在四个 Folder 内共新增 6 个 Project 条目：
                       #   /packages/data/ → Parquet.vbproj、Protobuf.vbproj
                       #   /packages/machine_learning/ → DirectML.vbproj
                       #   /packages/math.dll/ → GCN.vbproj、GLMNet.vbproj
                       #   /packages/machine_learning/vision/ → Cloudy.Test.vbproj
                       # 每条仅含 <Platform Solution="*|x64" Project="x64" /> 子元素
```