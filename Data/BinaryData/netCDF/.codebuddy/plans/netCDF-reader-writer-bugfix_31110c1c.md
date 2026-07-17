---
name: netCDF-reader-writer-bugfix
overview: 审查并修复 netCDF VB.NET 读写库中本仓库代码内的确定性错误：Header 记录维度 id 取值、字节属性值读取被破坏、以及 CDFWriter.Save 重复调用的防重入保护。
todos:
  - id: fix-header-recordid
    content: 修正 Components/Header.vb 第 142 行记录维度 id 的 If 取值，改用 HasValue 判断
    status: completed
  - id: fix-byte-attr
    content: 在 Data/StructureParser.vb 的 attributesList 中扩展 Byte() 单值提取，修复字节属性读取
    status: completed
  - id: fix-save-reentry
    content: 为 CDFWriter.vb 的 Save() 增加 saved 标志位防止重复写入
    status: completed
  - id: verify-build
    content: 构建 netCDF 项目并做最小读写往返校验（含 NC_BYTE 属性与记录变量）
    status: completed
    dependencies:
      - fix-header-recordid
      - fix-byte-attr
      - fix-save-reentry
---

## 用户需求

对本仓库（VB.NET 实现的 netCDF 经典格式 CDF-1/2/5 读写库）进行代码审查，找出本仓库代码内的真实错误并修正。外部依赖库（Microsoft.VisualBasic.Core、binarydata）不在本次修改范围内。

## 核心修复内容

- 修复记录（无限）维度 id 在 id=0 时被错误置为 -100 的取值逻辑错误。
- 修复字节（NC_BYTE/NC_UBYTE）类型全局/变量属性在读取时值被破坏（变为 "System.Byte[]"）导致 `getObjectValue()` 解析抛异常的往返失败问题。
- 为 `CDFWriter.Save()` 增加防重入保护，避免显式 `Save()` 后又经 `Dispose()` 二次写入导致文件结构损坏。

## 技术栈与约束

- 语言：VB.NET（项目 `netCDF.vbproj`，目标框架 net10.0，`Option Strict` 默认关闭，故允许隐式数值与布尔互转）。
- 依赖：外部 `Microsoft.VisualBasic.Core`、`binarydata`（BinaryDataReader/Writer、扩展方法 `Fill/reverse`），本次不改动。
- 规范：netCDF classic 布局（magic + version + numrecs + dim_list + gatt_list + var_list），非记录变量连续存放、记录变量按 record 交错存放，4 字节对齐，大端字节序；CDF-1/2/5 各字段宽度已核对正确，不在修复范围。

## 修复方案

### 错误 1：`Components/Header.vb` 记录维度 id 取值

原代码 `Me.recordDimension.id = If(dimList.recordId, dimList.recordId, -100)` 利用 `Integer?` 隐式转布尔，当记录维度 id 恰为 0 时误判为 `False` 返回 -100。改为基于 `HasValue` 的显式判断：`If(dimList.recordId.HasValue, dimList.recordId.Value, -100)`。仅影响显示（`recordDimension.ToString`），不改变读取逻辑，风险极低。

### 错误 2：`Data/StructureParser.vb` 字节属性读取

`Utils.readType` 对 `NC_BYTE/NC_UBYTE` 返回 `Byte()`，而 `attribute.value` 为 `String` 类型，赋值后得到字符串 `"System.Byte[]"`；随后 `attribute.getObjectValue()` 执行 `Byte.Parse("System.Byte[]")` 抛 `FormatException`。写端 `CDFWriter.writeAttributes` 对 `NC_BYTE` 属性只写 1 个元素，故读取侧在现有 `Boolean()` 单值提取分支旁扩展，增加对 `Byte()` 的处理：`ElseIf TypeOf val Is Byte() Then val = DirectCast(val, Byte())(Scan0)`，保证与写端一致地往返为标量字符串。

### 错误 3：`CDFWriter.vb` Save 防重入

`Save()` 当前无重入保护，被 `Dispose(disposing)` 调用；若用户先显式 `Save()` 再 `Dispose()`，第二次写入以 EOF 为基准重算 offset 并重复写出数据区，导致 header 指向错误位置、文件损坏。新增模块级字段 `Dim saved As Boolean = False`，在 `Save()` 开头 `If saved Then Return`，在正常写完数据区后置于 `True`，避免重复写出。

## 实施要点

- 仅修改本仓库三处文件，保持既有命名/注释风格与文件结构，不做无关重构。
- 错误 2 修复需与 `attribute.getObjectValue()`（NC_BYTE 分支 `Byte.Parse(value)`）保持字符串契约一致。
- 错误 3 的 `saved` 标志放在 `CDFWriter` 类作用域，与现有 `disposedValue` 类似声明方式。
- 修改后核对 `CDFWriter.writeAttributes`（仅写 1 元素字节）与读取分支 Symmetry 一致。

## 验证（best-effort）

- 尝试构建 `netCDF.vbproj`（依赖外部项目），若本地无法构建则以静态核对为准。
- 构造最小往返用例：写入含一个 `NC_BYTE` 全局属性与一个记录变量的文件，再用 `netCDFReader` 读回，确认该属性 `getObjectValue()` 返回正确数值而非 `"System.Byte[]"`，且记录变量可正常读取。