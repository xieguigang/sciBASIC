' --- 1. 最小化 Pickle 协议4解析器 ---
Imports System.Text

''' <summary>
''' Python pickle data reader helper
''' </summary>
Public Class MinimalPickleUnpickler
    ' Pickle 协议4 操作码
    Private Const PROTO As Byte = &H80
    Private Const FRAME As Byte = &H95
    Private Const EMPTY_DICT As Byte = &H7D
    Private Const EMPTY_LIST As Byte = &H5D
    Private Const MEMOIZE As Byte = &H94
    Private Const SHORT_BINUNICODE As Byte = &H8C
    Private Const SHORT_BININT As Byte = &H4B
    Private Const BININT As Byte = &H4C
    Private Const BINFLOAT As Byte = &H47
    Private Const SETITEM As Byte = &H73
    Private Const APPEND As Byte = &H61
    Private Const MARK As Byte = &H28
    Private Const APPENDS As Byte = &H65
    Private Const [Stop] As Byte = &H2E

    ' 用于标记栈中的特殊位置
    Private Class MarkObject
    End Class

    Public Shared Function Unpickle(base64Str As String) As Object
        Dim data As Byte() = Convert.FromBase64String(base64Str)
        Dim stack As New Stack(Of Object)()
        Dim memo As New List(Of Object)()
        Dim i As Integer = 0

        While i < data.Length
            Dim opcode As Byte = data(i)
            i += 1

            Select Case opcode
                Case PROTO ' 协议头，跳过1字节版本号
                    i += 1
                Case FRAME ' 帧头，跳过8字节长度
                    i += 8
                Case EMPTY_DICT ' 压入空字典
                    stack.Push(New Dictionary(Of Object, Object)())
                Case EMPTY_LIST ' 压入空列表
                    stack.Push(New List(Of Object)())
                Case MEMOIZE ' 记录栈顶对象
                    memo.Add(stack.Peek())
                Case SHORT_BINUNICODE ' 读取短字符串
                    Dim len As Integer = data(i)
                    i += 1
                    Dim str As String = Encoding.UTF8.GetString(data, i, len)
                    i += len
                    stack.Push(str)
                Case SHORT_BININT ' 读取1字节整数
                    stack.Push(CInt(data(i)))
                    i += 1
                Case BININT ' 读取4字节整数
                    stack.Push(BitConverter.ToInt32(data, i))
                    i += 4
                Case BINFLOAT ' 读取8字节大端浮点数
                    Dim floatBytes(7) As Byte
                    Array.Copy(data, i, floatBytes, 0, 8)
                    Array.Reverse(floatBytes) ' 转为小端序
                    stack.Push(BitConverter.ToDouble(floatBytes, 0))
                    i += 8
                Case MARK ' 压入标记对象
                    stack.Push(New MarkObject())
                Case SETITEM ' 为字典赋值 (单个键值对)
                    Dim val As Object = stack.Pop()
                    Dim key As Object = stack.Pop()
                    Dim dict As Dictionary(Of Object, Object) = CType(stack.Peek(), Dictionary(Of Object, Object))
                    dict(key) = val
                Case APPEND ' 为列表追加元素 (单个元素)
                    Dim val As Object = stack.Pop()
                    Dim lst As List(Of Object) = CType(stack.Peek(), List(Of Object))
                    lst.Add(val)
                Case APPENDS ' 为列表批量追加元素 (到Mark为止)
                    Dim items As New List(Of Object)()
                    Do While Not TypeOf stack.Peek() Is MarkObject
                        items.Insert(0, stack.Pop())
                    Loop
                    stack.Pop() ' 弹出MarkObject
                    Dim lst As List(Of Object) = CType(stack.Peek(), List(Of Object))
                    lst.AddRange(items)
                Case [Stop] ' 结束解析
                    Return stack.Pop()
                Case Else
                    Throw New Exception($"不支持的 Pickle 操作码: {opcode:X2}")
            End Select
        End While

        Return Nothing
    End Function
End Class