Imports System.Globalization

Namespace Numpy

    ''' <summary>
    ''' numpy 数据类型的字节序标记（对应 dtype 描述文本的首字符）。
    ''' </summary>
    Public Enum NpyEndian As Byte

        ''' <summary>
        ''' <c>|</c>：与字节序无关（布尔、字节串、对象引用、原始字节）
        ''' </summary>
        NotApplicable = 0

        ''' <summary>
        ''' <c>&lt;</c>：小端序
        ''' </summary>
        LittleEndian = 1

        ''' <summary>
        ''' <c>&gt;</c>：大端序
        ''' </summary>
        BigEndian = 2

        ''' <summary>
        ''' <c>=</c> 或者描述文本省略了字节序标记：本机字节序
        ''' </summary>
        [Native] = 3
    End Enum

    ''' <summary>
    ''' numpy dtype 的类型族（对应 dtype 描述文本中的类型字符）。
    ''' </summary>
    Public Enum NpyTypeKind As Byte

        ''' <summary>
        ''' <c>b1</c> / <c>?</c>：布尔
        ''' </summary>
        Bool = 0

        ''' <summary>
        ''' <c>i</c>：有符号整数
        ''' </summary>
        Int = 1

        ''' <summary>
        ''' <c>u</c>：无符号整数
        ''' </summary>
        UInt = 2

        ''' <summary>
        ''' <c>f</c>：浮点数
        ''' </summary>
        Float = 3

        ''' <summary>
        ''' <c>c</c>：复数
        ''' </summary>
        Complex = 4

        ''' <summary>
        ''' <c>S</c>：定长字节串
        ''' </summary>
        Bytes = 5

        ''' <summary>
        ''' <c>U</c>：定长 Unicode 字符串（UTF-32 码元）
        ''' </summary>
        Unicode = 6

        ''' <summary>
        ''' <c>O</c>：Python 对象引用（数据区为 pickle 字节流）
        ''' </summary>
        PyObject = 7

        ''' <summary>
        ''' <c>V</c>：无解释的原始字节，同时用于结构化记录类型
        ''' </summary>
        Void = 8

        ''' <summary>
        ''' <c>M</c>：datetime64
        ''' </summary>
        Datetime = 9

        ''' <summary>
        ''' <c>m</c>：timedelta64
        ''' </summary>
        Timedelta = 10
    End Enum

    ''' <summary>
    ''' 结构化 dtype 的一个字段。
    ''' </summary>
    Public Class NpyField

        ''' <summary>
        ''' 字段名称。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 字段的数据类型。
        ''' </summary>
        Public Property DType As NpyDType

        ''' <summary>
        ''' 字段在单条记录中的字节偏移量。
        ''' </summary>
        Public Property Offset As Integer

        ''' <summary>
        ''' 返回 <c>name: dtype</c> 形式的描述文本。
        ''' </summary>
        Public Overrides Function ToString() As String
            Return $"{Name}: {DType}"
        End Function
    End Class

    ''' <summary>
    ''' numpy 数据类型模型：描述一个 .npy 文件中数据元素的类型、字节宽度与字节序。
    ''' </summary>
    ''' <remarks>
    ''' 支持 numpy 的 <c>dtype.str</c> 形式（例如 <c>&lt;f8</c>、<c>|O</c>、<c>&lt;U5</c>、<c>|S10</c>、
    ''' <c>&lt;c16</c>、<c>|V16</c>、<c>&lt;M8[ns]</c>）以及结构化记录的列表形式
    ''' （例如 <c>[('a', '&lt;i4'), ('b', '&lt;f8')]</c>）。
    ''' 结构化记录只保证可识别与其字节宽度正确，元素值需要按原始字节使用。
    ''' </remarks>
    Public Class NpyDType

        ''' <summary>
        ''' dtype 的原始描述文本（结构化类型为重新生成的列表文本）。
        ''' </summary>
        Public Property Descriptor As String

        ''' <summary>
        ''' 字节序标记。
        ''' </summary>
        Public Property Endian As NpyEndian

        ''' <summary>
        ''' 类型族。
        ''' </summary>
        Public Property Kind As NpyTypeKind

        ''' <summary>
        ''' 单个元素的字节宽度。
        ''' </summary>
        Public Property ItemSize As Integer

        ''' <summary>
        ''' 结构化记录的字段列表；非结构化类型为 <c>Nothing</c>。
        ''' </summary>
        Public Property Fields As NpyField()

        ''' <summary>
        ''' datetime64 / timedelta64 的时间单位（例如 <c>ns</c>），其它类型为 <c>Nothing</c>。
        ''' </summary>
        Public Property Unit As String

        ''' <summary>
        ''' 是否为结构化记录类型。
        ''' </summary>
        Public ReadOnly Property IsStructured As Boolean
            Get
                Return Fields IsNot Nothing AndAlso Fields.Length > 0
            End Get
        End Property

        ''' <summary>
        ''' 是否为数值类型（布尔、整数、浮点、复数）。
        ''' </summary>
        Public ReadOnly Property IsNumeric As Boolean
            Get
                Select Case Kind
                    Case NpyTypeKind.Bool, NpyTypeKind.Int, NpyTypeKind.UInt, NpyTypeKind.Float, NpyTypeKind.Complex
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 是否为字符串类型（定长字节串或定长 Unicode）。
        ''' </summary>
        Public ReadOnly Property IsStringType As Boolean
            Get
                Return Kind = NpyTypeKind.Bytes OrElse Kind = NpyTypeKind.Unicode
            End Get
        End Property

        ''' <summary>
        ''' 是否需要在读取/写入时反转字节序。
        ''' </summary>
        Public ReadOnly Property NeedsByteSwap As Boolean
            Get
                Select Case Endian
                    Case NpyEndian.LittleEndian
                        Return Not BitConverter.IsLittleEndian
                    Case NpyEndian.BigEndian
                        Return BitConverter.IsLittleEndian
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        ''' <summary>
        ''' 定长 Unicode 类型的字符容量（字节宽度除以 4）。
        ''' </summary>
        Public ReadOnly Property CharCapacity As Integer
            Get
                If Kind = NpyTypeKind.Unicode Then
                    Return ItemSize \ 4
                Else
                    Return ItemSize
                End If
            End Get
        End Property

        ''' <summary>
        ''' 元素值的 .NET 等价类型。
        ''' </summary>
        Public Function GetClrType() As Type
            Select Case Kind
                Case NpyTypeKind.Bool
                    Return GetType(Boolean)
                Case NpyTypeKind.Int, NpyTypeKind.Datetime, NpyTypeKind.Timedelta
                    Return GetType(Long)
                Case NpyTypeKind.UInt
                    Return GetType(ULong)
                Case NpyTypeKind.Float
                    Return GetType(Double)
                Case NpyTypeKind.Complex
                    Return GetType(System.Numerics.Complex)
                Case NpyTypeKind.Bytes, NpyTypeKind.Unicode
                    Return GetType(String)
                Case Else
                    Return GetType(Object)
            End Select
        End Function

        ''' <summary>
        ''' 解析 numpy 的 dtype 描述文本。
        ''' </summary>
        ''' <param name="descr">例如 <c>&lt;f8</c>、<c>f8</c>、<c>|O</c>、<c>&lt;U5</c>、<c>[(a, &lt;i4)]</c></param>
        ''' <returns>数据类型描述对象</returns>
        Public Shared Function Parse(descr As String) As NpyDType
            If descr Is Nothing Then
                Throw New ArgumentNullException(NameOf(descr))
            End If

            Dim text As String = descr.Trim()

            If text.Length = 0 Then
                Throw New ArgumentException("numpy dtype descriptor can not be empty!", NameOf(descr))
            End If

            If text.StartsWith("[", StringComparison.Ordinal) Then
                Return BuildStructured(PyLiteral.Parse(text), text)
            End If

            If text.StartsWith("{", StringComparison.Ordinal) Then
                ' numpy 在记录类型存在对齐填充时会使用 dict 形式，这里暂不支持
                Throw New NotSupportedException($"unsupported numpy dtype descriptor: {text}")
            End If

            Dim result As New NpyDType With {.Descriptor = text}
            Dim pos As Integer = 0
            Dim first As Char = text(0)

            If first = "<"c OrElse first = ">"c OrElse first = "="c OrElse first = "|"c Then
                result.Endian = ParseEndian(first)
                pos = 1
            Else
                result.Endian = NpyEndian.Native
            End If

            If pos >= text.Length Then
                Throw New ArgumentException($"invalid numpy dtype descriptor: {text}", NameOf(descr))
            End If

            Dim code As Char = text(pos)

            pos += 1

            Dim sizeStart As Integer = pos

            Do While pos < text.Length AndAlso Char.IsDigit(text(pos))
                pos += 1
            Loop

            Dim sizeText As String = text.Substring(sizeStart, pos - sizeStart)
            Dim size As Integer = If(sizeText.Length = 0, 0, Integer.Parse(sizeText, CultureInfo.InvariantCulture))

            If pos < text.Length AndAlso text(pos) = "["c Then
                Dim closing As Integer = text.IndexOf("]"c, pos)

                If closing > pos Then
                    result.Unit = text.Substring(pos + 1, closing - pos - 1)
                End If
            End If

            result.Kind = KindOf(code)

            Select Case result.Kind
                Case NpyTypeKind.Unicode
                    result.ItemSize = 4 * size
                Case NpyTypeKind.Bytes
                    result.ItemSize = size
                Case NpyTypeKind.PyObject
                    result.ItemSize = If(size > 0, size, IntPtr.Size)
                Case Else
                    result.ItemSize = If(size > 0, size, DefaultItemSize(code))
            End Select

            If result.ItemSize <= 0 Then
                Throw New NotSupportedException($"unsupported numpy dtype descriptor: {text}")
            End If

            Return result
        End Function

        ''' <summary>
        ''' 从已经解析完成的 Python 字面量对象构造 dtype（字符串交给 <see cref="Parse"/>，
        ''' 列表则按结构化记录处理）。
        ''' </summary>
        ''' <param name="literal">字符串或者 <c>List(Of Object)</c></param>
        Friend Shared Function FromLiteral(literal As Object) As NpyDType
            Dim text As String = TryCast(literal, String)

            If text IsNot Nothing Then
                Return Parse(text)
            Else
                Return BuildStructured(literal, Nothing)
            End If
        End Function

        Private Shared Function BuildStructured(literal As Object, rawText As String) As NpyDType
            Dim items As List(Of Object) = TryCast(literal, List(Of Object))

            If items Is Nothing Then
                Throw New NotSupportedException($"unsupported numpy structured dtype: {rawText}")
            End If

            Dim fields As New List(Of NpyField)()
            Dim offset As Integer = 0
            Dim textList As New List(Of String)()

            For Each item In items
                Dim pair As Object() = TryCast(item, Object())

                If pair Is Nothing OrElse pair.Length < 2 Then
                    Throw New NotSupportedException($"unsupported numpy structured dtype field: {item}")
                End If

                Dim fieldDType As NpyDType = FromLiteral(pair(1))

                fields.Add(New NpyField With {
                    .Name = CStr(pair(0)),
                    .DType = fieldDType,
                    .Offset = offset
                })

                offset += fieldDType.ItemSize
                textList.Add($"('{CStr(pair(0))}', '{fieldDType.Descriptor}')")
            Next

            Return New NpyDType With {
                .Descriptor = If(rawText IsNot Nothing, rawText, "[" & String.Join(", ", textList) & "]"),
                .Endian = NpyEndian.NotApplicable,
                .Kind = NpyTypeKind.Void,
                .ItemSize = offset,
                .Fields = fields.ToArray
            }
        End Function

        Private Shared Function ParseEndian(c As Char) As NpyEndian
            Select Case c
                Case "<"c
                    Return NpyEndian.LittleEndian
                Case ">"c
                    Return NpyEndian.BigEndian
                Case "|"c
                    Return NpyEndian.NotApplicable
                Case Else
                    Return NpyEndian.Native
            End Select
        End Function

        Private Shared Function KindOf(code As Char) As NpyTypeKind
            Select Case code
                Case "?"c, "b"c
                    ' numpy 的 dtype.str 中 '|b1' 即布尔（'?' 是其 char 形式）
                    Return NpyTypeKind.Bool
                Case "i"c, "h"c, "l"c, "q"c, "p"c
                    Return NpyTypeKind.Int
                Case "u"c, "B"c, "H"c, "I"c, "L"c, "Q"c, "P"c
                    Return NpyTypeKind.UInt
                Case "f"c, "d"c, "e"c, "g"c
                    Return NpyTypeKind.Float
                Case "c"c, "F"c, "D"c, "G"c
                    Return NpyTypeKind.Complex
                Case "S"c, "a"c
                    Return NpyTypeKind.Bytes
                Case "U"c
                    Return NpyTypeKind.Unicode
                Case "O"c
                    Return NpyTypeKind.PyObject
                Case "V"c, "x"c
                    Return NpyTypeKind.Void
                Case "M"c
                    Return NpyTypeKind.Datetime
                Case "m"c
                    Return NpyTypeKind.Timedelta
                Case Else
                    Throw New NotSupportedException($"unsupported numpy dtype type code: '{code}'")
            End Select
        End Function

        Private Shared Function DefaultItemSize(code As Char) As Integer
            Select Case code
                Case "f"c
                    Return 4
                Case "d"c
                    Return 8
                Case "e"c
                    Return 2
                Case "h"c
                    Return 2
                Case "i"c, "I"c
                    Return 4
                Case "l"c, "L"c, "q"c, "Q"c, "p"c, "P"c
                    Return 8
                Case "u"c, "B"c
                    Return 1
                Case "b"c, "?"c
                    Return 1
                Case "M"c, "m"c
                    Return 8
                Case "O"c
                    Return IntPtr.Size
                Case Else
                    Return 0
            End Select
        End Function

        ''' <summary>
        ''' 返回 dtype 的描述文本。
        ''' </summary>
        Public Overrides Function ToString() As String
            Return Descriptor
        End Function

    End Class

End Namespace
