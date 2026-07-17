---
name: Feather写入后读回乱码的根因排查与修复
overview: 针对 VB.NET 的 Feather 读写模块：读取已有(如 R 生成)文件正常，但本项目写出的文件再读回出现乱码（尤其中文多字节 UTF-8 文本）。目标是通过构建并运行已有的轮转测试(test/Program.vb)与字节级 dump，定位写入侧缺陷并修复，使写-读 round-trip 正确（含 5000 行规模场景）。
todos:
  - id: reproduce
    content: 构建并运行 test/Program.vb，复现乱码并捕获失败场景（A–D）与具体字符串字节偏差
    status: completed
  - id: dump-compare
    content: 在测试中 dump 写出文件字节，比对各列元数据 Offset/Length 与磁盘 offsets 表、variable data 位置，并与 R 示例对照
    status: completed
    dependencies:
      - reproduce
  - id: locate-fix
    content: 依据比对结果定位写入侧根因（FeatherWriter.vb / MultiStreamProvider.vb）并修复变长字符串写入与多路复用回放
    status: completed
    dependencies:
      - dump-compare
  - id: verify
    content: 重新运行 A–E 全部场景，确认字符串正确、ALL PASS，且第三方文件读取不受影响
    status: completed
    dependencies:
      - locate-fix
---

## 用户需求

- 当前项目是用 VB.NET 实现的 Feather 格式数据框读写模块（读：`FeatherReader.vb`/`DataFrame.vb`；写：`FeatherWriter.vb` + `Impl/MultiStreamProvider.vb`）。
- 现象：能正确读取第三方（如 R 生成）的 Feather 文件，但【由本项目生成的 Feather 文件再读回时数据乱码】，尤其表现为多字节 UTF-8 中文字符串变成乱码。
- 目标：审查源代码，分析乱码可能的原因，并尽量修复。

## 核心特征

- 读路径与第三方文件共用且经验证正确（Scenario E 仅校验 shape，但字符串解码逻辑静态无误），因此问题唯一确定地落在“仅写入侧”的代码路径（reader 不共享 `BinaryDataWriter`/`MultiStreamProvider`）。
- 写入采用三逻辑流（Data/Null/Variable）经 `MultiStreamProvider` 多路复用到单一物理文件，按绝对偏移回放；字符串以 UTF-8 写入、offsets 表 + 8 字节对齐的 variable data 布局，与 reader 的 `ReadString` 计算在静态层面一致。
- 需通过运行时复现 + 字节级比对，定位是 offsets 写错、variable data 写错位置，还是多路复用回放在特定规模/列顺序下错位。

## 技术栈

- 语言/框架：VB.NET（.NET，项目 `FeatherFormat.vbproj`），读写核心为 `FeatherReader.vb`、`FeatherWriter.vb` 及 `Impl/` 下组件。
- 二进制写入：`Microsoft.VisualBasic.Data.IO.BinaryDataWriter`（裸写字节数组、按 `ByteOrder` 写整型，已确认正确）。
- 多路复用：`Impl/MultiStreamProvider.vb`（三逻辑流按绝对 `WriteAtPosition` 排序回放到单一 `FileStream`）。
- 字符编码：字符串以 `Encoding.UTF8` 编码；读取以 `Encoding.UTF8.GetString` 解码。

## 实现思路与关键结论

1. **读路径已排除**：`ReadString`（DataFrame.vb 617–694）对 offsets 表读取、8 字节对齐 padding、`ByteBuffer` 复用、`UTF8.GetString` 均正确；`HeapSizeOf.int=4` 在读写两侧一致（R 文件可读佐证），非错位根因。
2. **写路径为唯一可疑区**：`FeatherWriter.vb` 的 `WriteVariableSizedData`/`WriteNullableVariableSizedData`/`CopyString*` 布局逻辑与 reader 对称；`BinaryDataWriter`、`MultiStreamProvider`（含 `WriteToStream` 绝对偏移回放、`PendingEntry` 缓冲独占无别名）静态逻辑自洽。
3. **静态分析无法定位唯一故障行**：因 writer 与 reader 在偏移/对齐/编码上完全对称，需运行时取证。最可疑环节为 `MultiStreamProvider` 的“写入中途经 `AdvanceStreamTo`→`Flush`→`RequestFlush` 可能触发 `WriteToStream`”的时序，以及 5000 行规模（Scenario D）下多流复用回放的健壮性——这正是构造函数注释所指“规模下 UTF-8 乱码”的回归点。
4. **诊断优于猜测**：先构建运行 `test/Program.vb` 复现，再 dump 写出文件字节，比对“元数据各列 `PrimitiveArray.Offset/Length` ↔ 磁盘 offsets 表位置 ↔ variable data 实际位置”，并与 `examples/r-feather-test.feather` 字节级对照，从而精确判定故障类型（offsets 错 / 数据错位 / 回放错位）。

## 实施要点（降低返工）

- 复用现有 `test/Program.vb`（A/B/C/D 四场景中文轮转 + E 第三方文件）作为回归基线，不新建测试框架。
- dump 比对应覆盖最小可复现（Scenario A：4 行中文 `String()`）与规模场景（Scenario D：5000 行两列中文），以区分“恒错”与“规模相关”。
- 修复只改写入侧（`FeatherWriter.vb` / `Impl/MultiStreamProvider.vb`），保持 reader 与 R 文件兼容；不改动读路径与编码方式。
- 若定位到 `MultiStreamProvider`，优先采用“仅在最后一个子流释放时统一回放、写入过程中不触发中途 `WriteToStream`”的稳健化方案，避免绝对位置回放竞态。

## 架构与文件

- 诊断/修复涉及文件：
- `FeatherWriter.vb` [MODIFY]：字符串/变长数据写入与列偏移计算（`WriteColumn`、`WriteVariableSizedData`、`WriteNullableVariableSizedData`、`CopyString*`、`WriteMetadata`、`SerializePending`）。
- `Impl/MultiStreamProvider.vb` [MODIFY]：三流多路复用到单一物理流的回放时序（`WriteToStream`、`RequestFlush`、`AdvanceTo`）。
- `test/Program.vb` [MODIFY]：临时增加字节 dump 以辅助比对（验证后移除）。
- `examples/r-feather-test.feather` [参考]：作为字节级正确基线。