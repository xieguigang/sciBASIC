---
name: sqlite3-reader-test-and-fix
overview: 在 test/test.vbproj 中编写针对真实数据库 G:\compounds_2-copy.sqlite 的读取测试程序，定位并修复 SQLite3 模块的读取缺陷，运行时输出控制台摘要并生成 Markdown 测试报告。
todos:
  - id: test-harness
    content: 编写 test\Program.vb：环境与表结构输出、全量+抽样用例、异常捕获、控制台摘要与 Markdown 报告生成
    status: completed
  - id: run-baseline
    content: 构建并运行基线测试，记录失败用例与错误数据（未知声明类型、NULL 误读、BOOLEAN 恒真等）
    status: completed
    dependencies:
      - test-harness
  - id: fix-decode
    content: 用 [skill:lsp-code-analysis] 确认影响面后重写 Sqlite3Table.ParseRow，按 serial type 解码并正确处理 NULL/Float/Boolean/Text/Blob 与列边界
    status: completed
    dependencies:
      - run-baseline
  - id: fix-schema
    content: 用 [subagent:code-explorer] 核对 Core 助手签名后扩展 DataTypeParser 支持 BOOLEAN 等并容错，修正 ReadVarInt 计数与 cellOffsets 行序
    status: completed
    dependencies:
      - run-baseline
  - id: rerun-report
    content: 复测全部用例确认修复有效，输出 test\TEST-REPORT.md 报告（含问题与修复记录及复测结论）
    status: completed
    dependencies:
      - fix-decode
      - fix-schema
---

## 产品概述

这是一个用 VB.NET 编写的、针对 SQLite3 数据库文件的纯托管读取模块。需要在 `test\test.vbproj` 中编写验证程序，基于真实数据库文件 `G:\compounds_2-copy.sqlite` 测试读取能力，修复测试暴露的读取缺陷，并输出测试报告。

## 核心功能

- 测试程序打开目标数据库，输出文件头信息（页大小、文本编码、页数、Schema 格式、预留空间等）并列出全部表的名称、类型、根页与 CREATE TABLE 结构。
- 结构 + 抽样校验：对 `registries`、`compound_identifiers`、`compound_microspecies`、`magnesium_dissociation_constant` 等小表全量遍历，统计行数并按列统计 NULL/类型分布；对 `compounds` 大表抽样读取，重点覆盖长文本（inchi）、BLOB、溢出页、可空列 NULL、BOOLEAN 等边界场景。
- 捕获并汇总读取过程中抛出的异常与错误数据，运行时输出控制台摘要。
- 修复测试暴露的缺陷（可空列 NULL 处理、按真实存储类型解码、BOOLEAN/FLOAT 等类型、行序等），保证公共 API 行为不变。
- 在 `test\` 下生成 Markdown 测试报告：环境信息、测试用例与结果、发现的问题与修复内容、复测结论。

## 技术选型

- 语言/运行时：VB.NET + .NET 10（`net10.0`），与现有 `SQLite3.vbproj`、`test.vbproj` 一致，环境已具备 `dotnet 10.0.401`。
- 工程形态：复用 `test` 项目（`OutputType=Exe`）作为控制台测试程序，直接以 `ProjectReference` 引用 `SQLite3.vbproj`。
- 依赖：不新增第三方包；仅复用现有 `Microsoft.VisualBasic.Core`（`GetJson`/`Index`/`SeqIterator`/`Scripting.ToString` 等）与已引用的 `dataframework`、`binarydata` 项目。
- 测试对象仅使用公开 API（`Sqlite3Database.OpenFile/GetTables/GetTable`、`Sqlite3Table.EnumerateRows`、`Sqlite3Row`），不触碰 `Friend` 内部类型。

## 实现思路

采用“测试驱动修复”：先编写覆盖性测试程序 → 运行取得基线失败/错误数据 → 对模块做最小必要修复 → 复测并生成报告。

核心解码改造（关键决策）：`Tables\Sqlite3Table.vb` 的 `ParseRow` 当前用**声明类型**（schema 亲和性）决定解码方式，而 SQLite 是动态类型、且记录头中的 serial type 才是每列真实存储类型的唯一依据。改为**按记录头 serial type 逐列解码**，这是正确性与完整性的根本修复。映射规则：

| serial type | 含义 | 读取方式 |
| --- | --- | --- |
| 0 | NULL | 返回 `Nothing`（不再沿用上一行残留长度） |
| 1/2/3/4/5/6 | 1/2/3/4/6/8 字节整数 | `ReadInteger(n)` |
| 7 | 8 字节 IEEE 浮点 | `Int64BitsToDouble(ReadInteger(8))` |
| 8 / 9 | 整数 0 / 1（布尔） | `False` / `True` |
| 10/11 | 保留 | 视为 NULL 并告警 |
| N>=12 且为偶 | BLOB，长度 `(N-12)/2` | 读字节数组（或按 `blobAsBase64` 转 Base64） |
| N>=13 且为奇 | TEXT，长度 `(N-13)/2` | `ReadString(len)` |


配套修复：

- `DataTypeParser.TryParse`：新增 `BOOLEAN/BOOL/[bool]/bit/date/datetime/numeric/real/text` 等声明类型，并对未知声明类型按 SQLite 亲和性规则给出合理回退，**不再抛 `NotImplementedException`**（当前 `is_prefixed BOOLEAN`、`is_major BOOLEAN` 会导致 `GetTable` 构造 schema 直接失败）。
- `ReaderBase.ReadVarInt`：修正 9 字节 varint 时 `readBytes` 计数多算 1 的问题。
- `BTreePage.Parse`：移除对 `cellOffsets` 的 `Array.Sort`，按页内 cell 指针数组顺序（即 key/rowid 顺序）遍历，避免行序被物理偏移排序打乱。
- 消除 `metaInfo` 在行间共享并残留 `length` 的状态问题；header 列数与 schema 列数不一致时做边界保护而非越界。

性能与可靠性：沿用现有 `Iterator` + `ReadonlyStream` 流式读取，逐行处理、不全表入内存；`compounds` 默认只读前 N 行（默认 5000，可命令行/常量调整）；控制台只打印样例，避免逐行输出。读取复杂度 O(行数)，主要成本在随机页 I/O，抽样可将大表验证耗时从分钟级降到秒级。

## 实现要点（执行注意）

- 复用现有模式：`.vb` 采用 `Iterator`、`Using`、`SqliteDataStream` 的读写方式；日志/输出沿用 `Console.WriteLine`，报告写 UTF-8。
- 兼容性优先：保持 `Sqlite3Row` 取值语义、`ColumnDataMeta` 类型、`SqliteDataType` 既有枚举值与 `blobAsBase64` 设置不变；新增逻辑放在内部解码路径。
- 影响面控制：改动仅限 SQLite3 模块内部 + `test\Program.vb`，不做无关重构；`GetTables/GetTable/ExportTable` 等调用方不受破坏。
- 报告内容：由程序自动生成环境与用例结果区，并在程序中维护一份“发现的问题/修复记录”清单写入报告，保证基线失败与复测结论同源可追溯。

## 架构设计

`test\Program.vb`（用例编排、断言、结果汇总、报告生成） → 调用 `Sqlite3Database` → `Subscribe` 表枚举 → `Sqlite3Table.EnumerateRows`（`BTreeTools.WalkTableBTree` 递归遍历 B 树 + `SqliteDataStream` 合并溢出页 + 改造后的 `ParseRow` serial-type 解码）→ `Sqlite3Row` 取值；`Schema/SQLParser/DataTypeParser` 负责列名与声明类型解析。各测试用例把结果写入 `List(Of TestResult)`，最后统一输出控制台摘要与 Markdown。

## 目录结构

```
SQLite3/
├── test/
│   ├── Program.vb                 # [MODIFY] 重写为完整测试程序：头部/表结构输出，小表全量、compounds 抽样校验，
│   │                              #          覆盖 NULL/BOOLEAN/长文本/BLOB 溢出页/类型校验，异常捕获与结果汇总，
│   │                              #          控制台摘要 + 生成 Markdown 报告（含问题与修复记录）
│   ├── test.vbproj                # [保持] 已引用 SQLite3/Core/DataFrame/BinaryData，无需改动
│   └── TEST-REPORT.md             # [NEW] 运行生成的测试报告（环境、用例结果、问题与修复、复测结论）
├── Tables/
│   └── Sqlite3Table.vb            # [MODIFY] 重写 ParseRow：按记录头 serial type 解码，正确处理 NULL/Float/Boolean/
│                                  #          Text/Blob 与长度；消除列 meta 残留状态与列数越界
├── Objects/
│   ├── Enums/SqliteDataType.vb    # [MODIFY] 扩展 DataTypeParser：支持 BOOLEAN 等声明类型并容错（不抛异常）
│   └── BTreePage.vb               # [MODIFY] 移除 cellOffsets 排序，恢复按 rowid 顺序遍历
├── Internal/
│   └── ReaderBase.vb              # [MODIFY] 修正 ReadVarInt 9 字节读取计数（readBytes 应为 9 而非 10）
└── Schema/
    └── (Schema.vb / SQLParser.vb) # [按需] 仅当测试暴露 VARCHAR NOT NULL/异常声明类型解析问题时最小修正
```

## Agent Extensions

### Skill

- **lsp-code-analysis**
- Purpose: 在修改 `Sqlite3Table.ParseRow` 与 `DataTypeParser.TryParse` 前，定位符号定义、查找引用与调用层级，确认改动影响面（谁调用 ParseRow、谁使用 `SqliteDataType`/`TryParse`）。
- Expected outcome: 得到准确的调用点清单与影响范围，保证最小且完整的修改，不破坏 `GetTables/GetTable/ExportTable` 等调用方。

### SubAgent

- **code-explorer**
- Purpose: 在实施修复前核对 `Microsoft.VisualBasic.Core` 中被复用助手的真实签名与语义（如 `HeaderSchema.GetOrdinal`、`NamedValue`、`Convert.ChangeType` 用法、`ReadonlyStream` 契约），避免因签名不符导致编译或行为错误。
- Expected outcome: 确认现有助手 API 的正确用法，使补丁可直接编译通过且行为符合既有约定。