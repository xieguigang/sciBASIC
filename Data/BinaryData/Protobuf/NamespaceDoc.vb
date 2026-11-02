Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\NamespaceDoc.vb
' /********************************************************************************/

''' <summary>
''' 纯托管实现的 protobuf 工具链，共四层能力：
'''
''' <list type="bullet">
''' <item><description>线格式读写（<c>Wire</c> 命名空间）：varint、zigzag、定长小端字段、长度前缀、未知字段跳过。</description></item>
''' <item><description>CLR 对象序列化（<c>Serialization</c> 命名空间）：基于 <c>ProtoMember</c> 特性标注的对象与字节流往返。</description></item>
''' <item><description>.proto 解析（<c>Schema</c> 命名空间）：把 .proto 文本解析为描述符树。</description></item>
''' <item><description>代码生成（<c>CodeGen</c> 命名空间）：从描述符树生成 VB / C# 实体类源码。</description></item>
''' </list>
'''
''' 四层可以独立使用，也可以串联成"先写 .proto、再生成类、然后序列化对象"的完整闭环。
''' </summary>
Friend Module NamespaceDoc
End Module
