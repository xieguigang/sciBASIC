# JSON / BSON 解析、序列化与 Javascript 对象模型

## 引言

JSON 已经成为数据交换的事实标准，但它其实对应着三种截然不同的使用场景，而大多数库只擅长其中一种：

1. **解析大文档** —— 需要流式、可中断，内存占用与文档大小解耦；
2. **序列化 .NET 对象** —— 需要反射，需要处理集合与扩展字典（动态对象）；
3. **处理「不完美」的 JSON** —— 例如大语言模型（LLM）输出的截断片段、单引号字符串、尾随逗号。

`Microsoft.VisualBasic.MIME.application.json` 把这三件事放在同一个包里，另外还提供了 **BSON 二进制编解码** 与一个 **jsonLogic 规则引擎**。

## 设计目标

- **流式优先**：解析器按 token 推进，适合大文件与网络流；
- **宽容但不放弃正确性**：`LenientJson` 是一条独立的解析路径，标准解析器依然严格；
- **动态对象友好**：对派生自 `Dictionary(Of String, T)` 的类型，既能序列化字典内容，也能通过扩展序列化器保留额外的 CLR 属性。

## 核心特性

- **JSON 解析**：流式 JSON tokenizer / parser（`JsonParser`、`Token`、`TokenIcer`）；
- **JSON 写出与格式化**：`JSONWriter` 与可配置的缩进格式化器（由一系列单字符策略组成的状态机）；
- **宽容解析**：`LenientJson` 通过跟踪字符串上下文容忍并修复畸形 JSON（截断、单引号、注释等），专门服务于 LLM 输出场景；
- **BSON**：`BSONValue`、`Encoder`、`Decoder`、`ValueType` 与 `ObjectId` 等扩展二进制类型；
- **jsonLogic**：`jsonLogic` 与 `TreeBuilder` 用于构建与求值 jsonLogic 规则表达式；
- **对象序列化**：基于反射的 `JSONSerializer` / `Variant`，以及能够往返扩展字典的序列化器。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.application.json`（根） | 入口：`JSONTextParser`、`JsonParser`、`JSONWriter`、`JSONSerializer`、`Variant` |
| `...application.json.Javascript` | JSON / BSON 的 Javascript 对象模型 |
| `...application.json.BSON` | BSON 二进制编解码与扩展值类型 |
| `...application.json.JSONLogic` | jsonLogic 规则表达式引擎 |
| `...application.json.LenientJson` | 宽容 / 修复型 JSON 解析器 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.application.json

' 标准解析
Dim obj = JSONTextParser.Parse(jsonText)

' 宽容解析（面向 LLM 输出的截断 / 单引号 JSON）
Dim repaired = LenientJson.LenientJsonParser.Parse(llmOutput)

' 序列化 .NET 对象
Dim text$ = JSONSerializer.Serialize(myObject)
```

## 深入：Dynamics Dictionary 的序列化

以下内容说明本包的**扩展字典序列化**能力。使用该序列化器时，首先导入对应命名空间：

```vbnet
Imports Microsoft.VisualBasic.MIME.JSON.ExtendedDictionary
```

下面是一个 dynamics 对象示例（一个继承自字典类型的对象）：

```vbnet
' An object that extends a dictionary type
Public Class TestDynamicsObject : Inherits Dictionary(Of String, NamedValue(Of Integer()))
    Public Property Tarray As Double()
    Public Property str As String
    Public Property Tarray2 As String()
End Class
```

用如下代码初始化该对象：

```vbnet
Dim t As New TestDynamicsObject With {
    .Tarray = {1, 2, 3, 4, 5, 6, 7, 8},
    .str = "12345" & vbCrLf & "67890",
    .Tarray2 = {
        "xxoo", "1234", "6789", "50"
    }
}

Call t.Add("1234", New NamedValue(Of Integer())("x1", {100, 200, 3}))
Call t.Add("2333", New NamedValue(Of Integer())("x2", {-10, 203, 3}))
```

如果直接序列化这个继承自字典类型的对象，只会得到字典数据：

```vbnet
Call t.GetJson(True).SaveTo("./test_out.json")
```

扩展属性不会出现在输出 JSON 中：

```json
{
  "1234": {
    "Description": null,
    "Name": "x1",
    "x": [ 100, 200, 3 ]
  },
  "2333": {
    "Description": null,
    "Name": "x2",
    "x": [ -10, 203, 3 ]
  }
}
```

改用**扩展序列化函数**即可保留动态对象的额外属性：

```vbnet
Dim json$ = GetExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(t)
Call json.SaveTo("./test_out2.json")
```

输出结果如下 —— 字典条目与 CLR 属性同时被写出：

```json
{
  "1234": {
    "Description": null,
    "Name": "x1",
    "x": [ 100, 200, 3 ]
  },
  "2333": {
    "Description": null,
    "Name": "x2",
    "x": [ -10, 203, 3 ]
  },
  "Tarray": [ 1, 2, 3, 4, 5, 6, 7, 8 ],
  "str": "12345\u000d\u000a67890",
  "Tarray2": [ "xxoo", "1234", "6789", "50" ]
}
```

这正是期望的结果。反向反序列化同样简单：

```vbnet
Dim t2 = LoadExtendedJson(Of NamedValue(Of Integer()), TestDynamicsObject)(json)

' [DEBUG 2016/11/9 20:46:09] [1,2,3,4,5,6,7,8]
Call t2.Tarray.GetJson.debug
' [DEBUG 2016/11/9 20:46:10] ["xxoo","1234","6789","50"]
Call t2.Tarray2.GetJson.debug
```

动态数据被正确还原到了对应属性上。

## 实现要点

- **格式化器是一个状态机**：缩进格式化由一组「单字符策略」（`OpenBracketStrategy`、`CommaCharacterStrategy`、`ColonCharacterStrategy`…）驱动，便于扩展自定义排版风格。
- **宽容解析的代价被隔离**：`LenientJson` 独立于严格解析器，只有显式调用时才会启用修复逻辑，因此标准路径的性能与正确性不受影响。

## 与 sciBASIC# 生态的关系

本包是 `sciBASIC#` 的数据交换基础层：`mime` 家族中的 XML、YAML、Excel、GraphQuery 等模块都直接或间接依赖其对象模型；`Data` / `DataFrame` 的配置与元数据也以 JSON 形式持久化。

## 参考资料

- ESDS-RFC-022v1（NASA Earthdata Conduit 数据格式规范）：https://cdn.earthdata.nasa.gov/conduit/upload/497/ESDS-RFC-022v1.pdf

## 包信息

- Assembly：`Microsoft.VisualBasic.Mime.application.json`
- TargetFramework：`net10.0`
- Tags：`scibasic;json;bson;json-parser;json-serializer;lenient-json;jsonlogic;object-serialization;llm-output`
- 许可：GPL-3.0-or-later
