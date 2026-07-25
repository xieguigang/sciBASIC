' ============================================================================
'  PdfObject.vb  -  PDF 对象模型
'  ----------------------------------------------------------------------------
'  PDF 文件由一组"间接对象"组成，每个对象有如下基本类型：
'    - Boolean / Number / Name / String / Null
'    - Dictionary (<< ... >>)
'    - Array     ([ ... ])
'    - Stream    (dictionary + 二进制数据)
'    - Reference (num gen R)
'  本文件定义所有这些类型的轻量级类，不依赖任何第三方 PDF 库。
' ============================================================================
Imports std = System.Math

''' <summary>所有 PDF 对象的抽象基类。</summary>
Public MustInherit Class PdfObject
End Class

''' <summary>PDF null 对象。</summary>
Public Class PdfNull
    Inherits PdfObject
    Public Shared ReadOnly Instance As New PdfNull()
End Class

''' <summary>布尔值。</summary>
Public Class PdfBoolean
    Inherits PdfObject
    Public ReadOnly Value As Boolean
    Public Sub New(v As Boolean)
        Value = v
    End Sub
End Class

''' <summary>数值（PDF 中数值统一为浮点，需要整数时取整）。</summary>
Public Class PdfNumber
    Inherits PdfObject
    Public ReadOnly Value As Double
    Public Sub New(v As Double)
        Value = v
    End Sub
    Public ReadOnly Property IntegerValue As Integer
        Get
            Return CInt(std.Truncate(Value))
        End Get
    End Property
End Class

''' <summary>名称对象，例如 /Type、/Pages。内部存储不带前导斜杠。</summary>
Public Class PdfName
    Inherits PdfObject
    Public ReadOnly Value As String
    Public Sub New(v As String)
        Value = v
    End Sub
    Public Overrides Function ToString() As String
        Return "/" & Value
    End Function
    Public Overrides Function Equals(obj As Object) As Boolean
        Dim other = TryCast(obj, PdfName)
        Return other IsNot Nothing AndAlso other.Value = Me.Value
    End Function
    Public Overrides Function GetHashCode() As Integer
        Return If(Value, "").GetHashCode()
    End Function
    Public Shared Operator =(a As PdfName, b As PdfName) As Boolean
        If a Is Nothing AndAlso b Is Nothing Then Return True
        If a Is Nothing OrElse b Is Nothing Then Return False
        Return a.Value = b.Value
    End Operator
    Public Shared Operator <>(a As PdfName, b As PdfName) As Boolean
        Return Not (a = b)
    End Operator
End Class

''' <summary>字符串对象。同时保留原始字节和解码后的文本。</summary>
Public Class PdfString
    Inherits PdfObject
    Public ReadOnly Value As String
    Public ReadOnly RawBytes As Byte()
    Public Sub New(v As String, raw As Byte())
        Value = v
        RawBytes = raw
    End Sub
End Class

''' <summary>间接引用：num gen R。</summary>
Public Class PdfReference
    Inherits PdfObject
    Public ReadOnly ObjectNumber As Integer
    Public ReadOnly GenerationNumber As Integer
    Public Sub New(objNum As Integer, genNum As Integer)
        ObjectNumber = objNum
        GenerationNumber = genNum
    End Sub
    Public Overrides Function ToString() As String
        Return ObjectNumber & " " & GenerationNumber & " R"
    End Function
End Class

''' <summary>字典对象 &lt;&lt; /Key Value ... &gt;&gt;。</summary>
Public Class PdfDictionary
    Inherits PdfObject
    Private ReadOnly _map As New Dictionary(Of String, PdfObject)()
    Public ReadOnly Property Names As IEnumerable(Of String)
        Get
            Return _map.Keys
        End Get
    End Property
    Public Sub Add(name As String, obj As PdfObject)
        _map(name) = obj
    End Sub
    Public Function [Get](name As String) As PdfObject
        Dim o As PdfObject = Nothing
        _map.TryGetValue(name, o)
        Return o
    End Function
    Public Function Contains(name As String) As Boolean
        Return _map.ContainsKey(name)
    End Function
End Class

''' <summary>数组对象 [a b c ...]。</summary>
Public Class PdfArray
    Inherits PdfObject
    Private ReadOnly _items As New List(Of PdfObject)()
    Public ReadOnly Property Items As IReadOnlyList(Of PdfObject)
        Get
            Return _items
        End Get
    End Property
    Public Sub Add(obj As PdfObject)
        _items.Add(obj)
    End Sub
    Default Public ReadOnly Property Item(index As Integer) As PdfObject
        Get
            Return _items(index)
        End Get
    End Property
    Public ReadOnly Property Count As Integer
        Get
            Return _items.Count
        End Get
    End Property
End Class

''' <summary>流对象：字典 + 二进制数据。</summary>
Public Class PdfStream
    Inherits PdfObject
    Public ReadOnly Dictionary As PdfDictionary
    Public ReadOnly Data As Byte()
    Public Sub New(dict As PdfDictionary, data As Byte())
        Me.Dictionary = dict
        Me.Data = data
    End Sub
End Class

''' <summary>间接对象：num gen obj &lt;content&gt; endobj。</summary>
Public Class PdfIndirectObject
    Public ReadOnly ObjectNumber As Integer
    Public ReadOnly GenerationNumber As Integer
    Public ReadOnly Content As PdfObject
    Public Sub New(objNum As Integer, genNum As Integer, content As PdfObject)
        Me.ObjectNumber = objNum
        Me.GenerationNumber = genNum
        Me.Content = content
    End Sub
End Class


