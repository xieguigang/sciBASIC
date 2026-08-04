---
name: git-weekly-log-parser
overview: 在 VersionControl\git 模块新增可复用的解析类，解析 `git --no-pager log --since="1 week ago" -p` 的输出，结构化获取最近一周每个 commit 的元数据、改动文件清单、具体 diff 内容以及增删行统计，便于撰写工作周报。
todos:
  - id: create-weeklylog
    content: 新增 weeklyLog.vb，定义 commitEntry 聚合类与增删行统计
    status: completed
  - id: implement-parse
    content: 实现 ParseWeeklyLogText 按 commit 块切分并组合 log/diff 解析
    status: completed
    dependencies:
      - create-weeklylog
  - id: add-entry-method
    content: 新增 GetWeeklyLog 便捷入口调用 git log -p 并解析
    status: completed
    dependencies:
      - implement-parse
  - id: verify-build
    content: 检查代码与现有模式一致性并确认可编译
    status: completed
    dependencies:
      - add-entry-method
---

## 用户需求

在项目的 `VersionControl\git` 模块中，新增对 `git --no-pager log --since="1 week ago" -p` 命令输出的解析能力，以获得最近一周的代码修改内容，辅助撰写工作周报。

## 产品概述

基于现有 `log.vb`（commit 元数据解析）与 `diff.vb`（patch 解析）的能力，组合实现一个可复用的解析类，将“带 patch 的 git log”原始文本解析为结构化对象。解析结果需包含每条 commit 的提交元数据、本次提交改动的文件清单、各文件的具体 diff 内容，以及新增/删除行的统计。

## 核心功能

- 解析 `git log ... -p` 原始文本，按 commit 边界（`commit ` 开头行）切分各提交块。
- 提取每个 commit 的元数据：commit hash、author、date、message（复用现有 `log` 类结构）。
- 提取每个 commit 紧随其后的 patch（从 `diff --git a/... b/...` 起），解析为改动文件清单与具体 diff 内容（复用 `diff.vb` 的 `ParseDiffText`）。
- 对每个 commit 提供新增行数、删除行数的统计汇总（基于 `FileChange` 的 Hunks 计算）。
- 提供从仓库目录直接调用 git 命令（`--no-pager log --since="1 week ago" -p`）并解析的便捷入口方法。

## 技术栈选型

- 语言/框架：VB.NET（与现有项目一致，.vb 源码）
- 命名空间：`VersionControl.Git`（保持与 `log.vb`、`diff.vb` 一致）
- 复用工具：`Microsoft.VisualBasic.Text`（`LineIterators`、`Split`、`GetTagValue`、`JoinBy`、`StringEmpty`、`LineTokens`）、`Microsoft.VisualBasic.Language`（`Iterator`、`Scan0`）、`PipelineProcess.Call`（执行 git 命令）

## 实现思路

### 总体策略

采用“组合复用”策略：不重写日志与 diff 解析，而是新增一个 `weeklyLog.vb` 文件，定义 `commitEntry` 类（聚合一个 `log` 实例 + 一个 `DiffResult` + 增删行统计），并提供一个解析入口 `ParseWeeklyLogText`，按 commit 块切分后，对每个块先调用已有 `log.ParseGitLogText` 的逻辑解析元数据，再将该块中 `diff --git` 起始的部分截取后交给 `diff.ParseDiffText` 解析，最后基于 `FileChange.Hunks` 统计增删行。

### 关键技术决策

1. **复用而非重复**：`ParseGitLogText` 与 `ParseDiffText` 已是成熟解析器。`-p` 输出本质是其串联，因此新代码只做“分块 + 分流”，避免逻辑重复与回归风险。
2. **commit 块切分**：沿用 `log.vb` 的 `text.LineIterators.Split(Function(line) line.StartsWith("commit "), DelimiterLocation.NextFirst)` 方式切分；每个块内，元数据部分为前若干行（commit/author/date/message），patch 部分从首次出现 `diff --git` 行开始到块末尾。
3. **数据结构设计**：新增 `commitEntry` 聚合类，避免修改现有 `log`/`DiffResult` 定义，保持向后兼容（开放封闭原则）。
4. **增删行统计**：遍历 `DiffResult.Files` 下每个 `Hunk.Lines`，按 `DiffLineType.Added`/`Deleted` 计数，封装为 `commitEntry.AddedLines`/`DeletedLines` 属性，便于周报直接取用。

### 性能与可靠性

- 文本按行解析为 O(N) 线性扫描，一次遍历完成切分与分流；`ParseDiffText` 本身也是线性解析，整体复杂度 O(N)，无 N+1 或重复遍历。
- 边界处理：commit 块内若无 patch（如 merge commit 或 `--since` 边界异常），`DiffResult.Files` 为空列表，统计为 0，不抛异常。
- 命令执行失败时（`PipelineProcess.Call` 返回空），返回空序列，与 `diff.GetDiff` 现有行为一致。

## 实现注意事项

- 严格保持 `VersionControl.Git` 命名空间与既有代码风格（Region 头、XML 注释、`Shared Iterator Function`）。
- 不要修改 `log.vb`/`diff.vb` 现有公共 API，仅在必要时沿用其内部解析方法（均为 `Public Shared`，可直接调用）。
- 命令参数为 `--no-pager log --since="1 week ago" -p`，`workdir` 传仓库目录，复用 `PipelineProcess.Call(git, args, workdir:=directory)`。
- 解析时对 commit 块内 message 与 diff 的边界需准确判断（以首个 `diff --git` 行为界），防止 message 多行把 patch 误吞。

## 架构设计

### 系统结构

在现有 `VersionControl.Git` 命名空间下新增组合层，连接 `log`（元数据）与 `diff`（patch）两个已有解析器：

```mermaid
graph TD
    A[git raw log -p text] --> B[weeklyLog.ParseWeeklyLogText]
    B --> C[Split by 'commit ' lines]
    C --> D[Each commit block]
    D --> E[log.ParseGitLogText 解析元数据]
    D --> F[截取 diff 部分 -> diff.ParseDiffText]
    F --> G[DiffResult: Files/Hunks/Lines]
    E --> H[commitEntry]
    G --> H
    H --> I[AddedLines / DeletedLines 统计]
    B --> J[IEnumerable(Of commitEntry)]
```

## 目录结构

```
g:\GCModeller\src\runtime\sciBASIC#\vs_solutions\dev\VisualStudio\VersionControl\git\
├── log.vb          # [MODIFY] 无需修改，仅复用其 Public Shared ParseGitLogText
├── diff.vb         # [MODIFY] 无需修改，仅复用其 Public Shared ParseDiffText / DiffResult
└── weeklyLog.vb    # [NEW] 新增。定义 commitEntry 类（聚合 log 元数据 + DiffResult + AddedLines/DeletedLines 统计）与 Shared Iterator Function ParseWeeklyLogText(text) 以及 GetWeeklyLog(directory, Optional since:="1 week ago", Optional git:="git") 便捷入口。按 commit 块切分并分别复用 log/diff 解析器，统计增删行后产出 IEnumerable(Of commitEntry)。
```

## 关键代码结构

```
Namespace VersionControl.Git

    ''' <summary>
    ''' 单个 commit 的完整解析结果（元数据 + 改动文件 + 差异 + 增删行统计）
    ''' </summary>
    Public Class commitEntry
        Public Property meta As log
        Public Property changes As DiffResult
        Public Property AddedLines As Integer
        Public Property DeletedLines As Integer
    End Class

End Namespace
```