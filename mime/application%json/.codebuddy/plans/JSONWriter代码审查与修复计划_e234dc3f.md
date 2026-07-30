---
name: JSONWriter代码审查与修复计划
overview: 针对 Serializer\JSONWriter.vb 进行代码审查，发现并修复：1) 非数值 IFormattable 类型（Char/Guid/TimeSpan/Enum/Uri/Version/ObjectId 等）被序列化为未加引号的裸 token，产生非法 JSON；2) 缩进模式下 Object 类型数组内含标量元素时逗号前置且 ] 前出现空行（影响 round-trip 重序列化）；3) JsonValue 字符串值 "null" 被误写为 JSON null 关键字（数据歧义/丢失）；4) 数组格式不一致（数值向量单行、字符串/对象数组强制多行）等次要问题。
todos:
  - id: fix-value-quoting
    content: 重构 jsonValueString 收尾分支，非数值类型加引号、仅数值型保留未引号
    status: completed
  - id: fix-null-string
    content: 修正 BuildJSONString 的 null 判定，使字符串 "null" 正确输出
    status: completed
  - id: fix-array-format
    content: 修正 jsonArrayString 中 Object 标量数组的逗号前置与空行
    status: completed
  - id: add-review-tests
    content: 在 writerReviewTest.vb 新增回归用例并运行验证全部通过
    status: completed
    dependencies:
      - fix-value-quoting
      - fix-null-string
      - fix-array-format
---

## 用户需求

对 VB.NET 项目中的 `Serializer\JSONWriter.vb` 模块（负责把 JsonElement 对象树生成为 JSON 字符串）进行代码审查，识别潜在缺陷并制定修复计划。

## 核心问题（待修复）

- **非法 JSON 输出**：`jsonValueString` 收尾分支对非数值的 `IFormattable` 类型（如 `Char`、`Guid`、`TimeSpan`、`Enum`、`Uri`、`Version`）以及 `ObjectId` 类型，输出未加引号的裸 token，生成非法 JSON。
- **缩进数组格式错误（影响 round-trip 重序列化）**：缩进模式下，`UnderlyingType = Object` 且元素为标量的数组，逗号被写到下一行行首，且 `]` 前多出一个空行，导致用 `JsonParser` 重新解析时出现元素错位或失败。
- **字符串 `"null"` 数据丢失**：`BuildJSONString` 用 `IsLiteralNull` 判定 null，而 `JsonValue.IsLiteralNull` 在字符串值恰好为 `"null"` 时也返回 True，于是该字符串被序列化为 JSON `null` 关键字而非字符串 `"null"`，造成数据歧义与丢失。

## 不在本次范围（既已正确，不再报修）

既有 `test\writerReviewTest.vb` 已固化的项（不变文化数字格式、ISO 日期、NaN/±Infinity 引号、控制字符转义、非 ASCII 仅 unicode 路径转义、缩进数组 `]` 前无空行）当前代码已满足，本计划不予重复。

## 技术栈

- 语言/框架：VB.NET（目标框架 .NET 5，工程 `JSON-netcore5.vbproj`），无新增第三方依赖。
- 既有模式：复用 `escapeString` / `encodeString`（不变文化）、`JSONSerializerOptions`（indent/unicodeEscape 等）、`JsonValue`/`JsonObject`/`JsonArray` 数据模型与 `JsonParser` 校验。

## 实现方案

### 总体策略

仅对 `JSONWriter.vb` 做局部修正，不引入新架构；保持与 `JsonValue.NULL`（value=Nothing 输出 `null` 关键字）、不变文化格式化、既有测试断言的向后兼容。

### 关键修复点

1. **`jsonValueString` 收尾分支（约 171–176 行）**：以“是否为数值类型”作为唯一不加引号的条件。显式识别 `Byte/SByte/Int16/UInt16/Int32/UInt32/Int64/UInt64/Decimal/Double/Single` 输出未引号数字（不变文化）；其余类型（`Char`、`Guid`、`TimeSpan`、`Enum`、`Uri`、`Version`、`ObjectId` 等）一律通过 `encodeString(Convert.ToString(value, InvariantCulture))` 输出为引号字符串。`ObjectId` 当前返回 `ObjectId("...")` 裸 token，应改为引号字符串。
2. **`BuildJSONString` 的 null 判定（约 103 行）**：由 `DirectCast(json, JsonValue).IsLiteralNull` 改为 `DirectCast(json, JsonValue).value Is Nothing`，仅当底层值真正为 Nothing 时才输出 `null` 关键字；字符串 `"null"` 走 `jsonValueString` 正确输出为 `"null"`。注意 `JsonValue.NULL` 与解析得到的 null 其 value 均为 Nothing，语义不变。
3. **`jsonArrayString` 的 `GetType(Object)` 分支（约 287–305 行）**：为每个元素调用 `BuildJSONString` 时新增可选参数 `trailingNewline As Boolean = True`；在该数组分支内传 `trailingNewline:=False`，由本方法统一在元素后写 `,`（缩进时 `,` + 换行，非缩进时 `, `）；并去掉循环后的多余 `json.WriteLine()`，使 `]` 紧贴最后元素，消除空行与前置逗号。

### 性能与可靠性

- 仅在类型判定分支增加轻量 `TypeOf` 比较，复杂度 O(1)，不引入额外遍历或分配。
- 复用现有 `StringBuilder`/`TextWriter` 缓冲，不改变 I/O 行为；`leaveOpen`/Dispose 逻辑保持不变。
- 维持不变文化输出，确保跨语言环境生成一致且合法的 JSON 数字与日期。

### 可选（低优先，不阻塞）

- `escapeString` 将补充字符平面（surrogate pair）合并为单个 `\u` 码点转义（当前逐码元转义为两个 `\uXXXX`，合法但可优化）。
- 统一数组格式（数值向量单行、字符串/对象数组多行）属设计选择，默认保持现状并在注释中说明。

## 目录结构

```
mime/application%json/
├── Serializer/
│   └── JSONWriter.vb        # [MODIFY] 修正 jsonValueString 收尾分支（加引号规则）、BuildJSONString 的 null 判定、jsonArrayString 的 Object 数组逗号/空行逻辑；新增 BuildJSONString 可选参数 trailingNewline
└── test/
    └── writerReviewTest.vb  # [MODIFY] 新增回归用例：Object 标量数组缩进格式、非数值类型加引号、字符串 "null" 正确输出；复用 JsonParser 校验可解析性
```

## 关键接口变更

- `Private Sub BuildJSONString(json As JsonElement, indent As Integer, Optional trailingNewline As Boolean = True)`：新增可选参数，控制元素写完后是否追加换行（供数组内联排版使用）。