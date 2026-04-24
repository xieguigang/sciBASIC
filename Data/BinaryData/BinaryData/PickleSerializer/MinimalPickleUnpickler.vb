Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace Pickle

    ' =====================================================================
    ' 扩展的 Pickle 反序列化器
    ' =====================================================================

    ''' <summary>
    ''' 扩展的 Pickle 协议反序列化器，支持协议 0-5 的大部分操作码。
    ''' 能够将 Python Pickle 二进制数据反序列化为对应的 .NET 对象。
    ''' 
    ''' 类型映射关系：
    '''   Python None     → VB.NET Nothing
    '''   Python bool     → VB.NET Boolean
    '''   Python int      → VB.NET Long (或 Integer，取决于大小)
    '''   Python float    → VB.NET Double
    '''   Python str      → VB.NET String
    '''   Python bytes    → VB.NET Byte()
    '''   Python list     → VB.NET List(Of Object)
    '''   Python dict     → VB.NET Dictionary(Of Object, Object)
    '''   Python tuple    → PythonTuple
    '''   Python set      → PythonSet
    '''   Python complex  → ComplexNumber
    '''   Python datetime → VB.NET DateTime
    '''   其他自定义对象  → PythonObject
    ''' </summary>
    Public Class MinimalPickleUnpickler

        ' ===== Pickle 操作码常量 =====

        ' ----- 协议控制 -----
        Private Const PROTO As Byte = &H80              ' 协议版本头 (协议2+)
        Private Const FRAME As Byte = &H95              ' 帧头 (协议4+)
        Private Const [STOP] As Byte = &H2E             ' 结束标记 '.'

        ' ----- 栈操作 -----
        Private Const MARK As Byte = &H28               ' 压入标记 '('
        Private Const POP As Byte = &H30                ' 弹出栈顶 '0'
        Private Const POP_MARK As Byte = &H31           ' 弹出到标记 '1'
        Private Const DUP As Byte = &H32                ' 复制栈顶 '2'

        ' ----- Memo 操作 -----
        Private Const PUT As Byte = &H70                ' 存入memo（十进制索引）'p'
        Private Const BINPUT As Byte = &H71             ' 存入memo（1字节索引）'q'
        Private Const LONG_BINPUT As Byte = &H72        ' 存入memo（4字节索引）'r'
        Private Const [GET] As Byte = &H67              ' 从memo读取（十进制索引）'g'
        Private Const BINGET As Byte = &H68             ' 从memo读取（1字节索引）'h'
        Private Const LONG_BINGET As Byte = &H6A        ' 从memo读取（4字节索引）'j'
        Private Const MEMOIZE As Byte = &H94            ' 自动memo（协议4+）

        ' ----- None 和布尔 -----
        Private Const NONE As Byte = &H4E               ' None 'N'
        Private Const NEWTRUE As Byte = &H88            ' True (协议2+)
        Private Const NEWFALSE As Byte = &H89           ' False (协议2+)

        ' ----- 整数 -----
        Private Const INT As Byte = &H49                ' 十进制整数字符串 'I'
        Private Const BININT As Byte = &H4A             ' 4字节有符号整数 'J'
        Private Const BININT1 As Byte = &H4B            ' 1字节无符号整数 'K' (原 SHORT_BININT)
        Private Const BININT2 As Byte = &H4D            ' 2字节无符号整数 'M'
        Private Const LONG1 As Byte = &H8A              ' 变长长整数 (协议2+)
        Private Const LONG4 As Byte = &H8B              ' 4字节长度长整数 (协议2+)

        ' ----- 浮点数 -----
        Private Const FLOAT As Byte = &H46              ' 十进制浮点字符串 'F'
        Private Const BINFLOAT As Byte = &H47           ' 8字节大端浮点 'G'

        ' ----- 文本字符串 -----
        Private Const SHORT_BINUNICODE As Byte = &H8C   ' 1字节长度UTF8字符串 (协议4+)
        Private Const BINUNICODE As Byte = &H58         ' 4字节长度UTF8字符串 'X'
        Private Const UNICODE As Byte = &H56            ' raw-unicode-escape字符串 'V'

        ' ----- 旧式字符串（协议0兼容）-----
        Private Const SHORT_BINSTRING As Byte = &H55    ' 1字节长度字节字符串 'U'
        Private Const BINSTRING As Byte = &H54          ' 4字节长度字节字符串 'T'
        Private Const [STRING] As Byte = &H53           ' 引号字符串 'S'

        ' ----- 字节 -----
        Private Const SHORT_BINBYTES As Byte = &H43     ' 1字节长度字节 'C' (协议3+)
        Private Const BINBYTES As Byte = &H42           ' 4字节长度字节 'B' (协议3+)
        Private Const BYTEARRAY8 As Byte = &H96         ' 8字节长度字节数组 (协议5+)

        ' ----- 列表 -----
        Private Const EMPTY_LIST As Byte = &H5D         ' 空列表 ']' (协议1+)
        Private Const LIST As Byte = &H6C               ' 从栈构建列表 'l'
        Private Const APPEND As Byte = &H61             ' 追加单个元素 'a'
        Private Const APPENDS As Byte = &H65            ' 批量追加到标记 'e'

        ' ----- 字典 -----
        Private Const EMPTY_DICT As Byte = &H7D         ' 空字典 '}'
        Private Const DICT As Byte = &H64               ' 从栈构建字典 'd'
        Private Const SETITEM As Byte = &H73            ' 设置单个键值对 's'
        Private Const SETITEMS As Byte = &H75           ' 批量设置键值对 'u'

        ' ----- 元组 -----
        Private Const EMPTY_TUPLE As Byte = &H29        ' 空元组 ')' (协议1+)
        Private Const TUPLE As Byte = &H74              ' 从栈构建元组 't'
        Private Const TUPLE1 As Byte = &H85             ' 1元素元组 (协议2+)
        Private Const TUPLE2 As Byte = &H86             ' 2元素元组 (协议2+)
        Private Const TUPLE3 As Byte = &H87             ' 3元素元组 (协议2+)

        ' ----- 对象构造 -----
        Private Const [GLOBAL] As Byte = &H63             ' 全局引用 'c'
        Private Const REDUCE As Byte = &H52             ' 调用可调用对象 'R'
        Private Const BUILD As Byte = &H62              ' 构建对象状态 'b'
        Private Const NEWOBJ As Byte = &H81             ' 构建新对象 (协议2+)
        Private Const INST As Byte = &H69               ' 实例化 'i'
        Private Const OBJ As Byte = &H6F                ' 构建对象 'o'

        ' ----- 持久化ID -----
        Private Const PERSID As Byte = &H50             ' 持久化ID（字符串）'P'
        Private Const BINPERSID As Byte = &H51          ' 持久化ID（二进制）'Q'

        ' ----- 扩展注册表 -----
        Private Const EXT1 As Byte = &H82               ' 1字节扩展代码 (协议2+)
        Private Const EXT2 As Byte = &H83               ' 2字节扩展代码 (协议2+)
        Private Const EXT4 As Byte = &H84               ' 4字节扩展代码 (协议2+)

        ' 兼容旧代码的常量别名
        Private Const SHORT_BININT As Byte = BININT1    ' &H4B，旧名保持兼容

        ' 栈中的标记对象
        Private Class MarkObject
        End Class

        ' ===== 公共方法 =====

        ''' <summary>
        ''' 从 Base64 字符串反序列化 Pickle 数据。
        ''' 这是最便捷的入口方法，适合从文本传输或存储中恢复 Python 对象。
        ''' </summary>
        ''' <param name="base64Str">Base64 编码的 Pickle 数据</param>
        ''' <returns>反序列化后的 .NET 对象</returns>
        Public Shared Function Unpickle(base64Str As String) As Object
            Dim data As Byte() = Convert.FromBase64String(base64Str)
            Return UnpickleFromBytes(data)
        End Function

        ''' <summary>
        ''' 从字节数组反序列化 Pickle 数据。
        ''' 适合从文件读取或网络接收的原始二进制数据。
        ''' </summary>
        ''' <param name="data">Pickle 格式的字节数组</param>
        ''' <returns>反序列化后的 .NET 对象</returns>
        Public Shared Function UnpickleFromBytes(data As Byte()) As Object
            Using ms As New MemoryStream(data)
                Return UnpickleFromStream(ms)
            End Using
        End Function

        ''' <summary>
        ''' 从流反序列化 Pickle 数据。
        ''' 支持任意 Stream 来源，包括文件流、网络流、内存流等。
        ''' </summary>
        ''' <param name="stream">包含 Pickle 数据的流</param>
        ''' <returns>反序列化后的 .NET 对象</returns>
        Public Shared Function UnpickleFromStream(stream As Stream) As Object
            Dim stack As New Stack(Of Object)()
            Dim memo As New Dictionary(Of Integer, Object)()
            Dim reader As New BinaryReader(stream, Encoding.UTF8, leaveOpen:=True)

            While stream.Position < stream.Length
                Dim opcode As Byte = reader.ReadByte()

                Select Case opcode
                ' ============================================================
                ' 协议控制
                ' ============================================================
                    Case PROTO
                        ' 协议头：1字节版本号。支持0-5，更高版本可能不兼容。
                        Dim version = reader.ReadByte()
                        If version > 5 Then
                            Throw New NotSupportedException($"不支持的 Pickle 协议版本: {version}，最高支持版本5")
                        End If

                    Case FRAME
                        ' 协议4帧头：8字节小端序长度，表示帧内数据字节数。
                        ' 帧是协议4引入的优化，允许流式读取大对象。
                        reader.ReadBytes(8)

                    Case [STOP]
                        ' 结束标记：弹出栈顶对象作为最终结果返回
                        Return If(stack.Count > 0, stack.Pop(), Nothing)

                ' ============================================================
                ' 栈操作
                ' ============================================================
                    Case MARK
                        ' 压入标记对象，用于后续的 APPENDS/SETITEMS/TUPLE 等批量操作
                        stack.Push(New MarkObject())

                    Case POP
                        ' 弹出栈顶元素并丢弃
                        stack.Pop()

                    Case POP_MARK
                        ' 弹出栈中到 MarkObject 为止的所有元素（含 MarkObject 本身）并丢弃
                        Do While stack.Count > 0 AndAlso Not TypeOf stack.Peek() Is MarkObject
                            stack.Pop()
                        Loop
                        If stack.Count > 0 Then stack.Pop() ' 弹出 MarkObject

                    Case DUP
                        ' 复制栈顶元素并再次压入（用于 MEMOIZE 前保留栈顶）
                        stack.Push(stack.Peek())

                ' ============================================================
                ' Memo 操作（对象引用共享和循环引用支持）
                ' ============================================================
                    Case PUT
                        ' 协议0：存入memo，索引为十进制字符串（以换行符结尾）
                        Dim idxStr = ReadLine(reader)
                        memo(Integer.Parse(idxStr, CultureInfo.InvariantCulture)) = stack.Peek()

                    Case BINPUT
                        ' 协议1：存入memo，索引为1字节无符号整数
                        memo(CInt(reader.ReadByte())) = stack.Peek()

                    Case LONG_BINPUT
                        ' 协议1：存入memo，索引为4字节有符号整数（用于超大memo）
                        memo(reader.ReadInt32()) = stack.Peek()

                    Case [GET]
                        ' 协议0：从memo读取，索引为十进制字符串
                        Dim idxStr = ReadLine(reader)
                        stack.Push(memo(Integer.Parse(idxStr, CultureInfo.InvariantCulture)))

                    Case BINGET
                        ' 协议1：从memo读取，索引为1字节
                        stack.Push(memo(CInt(reader.ReadByte())))

                    Case LONG_BINGET
                        ' 协议1：从memo读取，索引为4字节
                        stack.Push(memo(reader.ReadInt32()))

                    Case MEMOIZE
                        ' 协议4：自动将栈顶对象存入memo的下一个可用索引
                        memo(memo.Count) = stack.Peek()

                ' ============================================================
                ' None 和布尔
                ' ============================================================
                    Case NONE
                        ' Python None 映射为 VB.NET Nothing
                        stack.Push(Nothing)

                    Case NEWTRUE
                        ' 协议2+：Python True
                        stack.Push(True)

                    Case NEWFALSE
                        ' 协议2+：Python False
                        stack.Push(False)

                ' ============================================================
                ' 整数
                ' ============================================================
                    Case INT
                        ' 协议0：十进制整数字符串（以换行符结尾）
                        ' 特殊值：I01\n = True, I00\n = False（旧式布尔编码）
                        Dim intStr = ReadLine(reader).Trim()
                        If intStr = "01" Then
                            stack.Push(True)
                        ElseIf intStr = "00" Then
                            stack.Push(False)
                        Else
                            stack.Push(Long.Parse(intStr, CultureInfo.InvariantCulture))
                        End If

                    Case BININT
                        ' 协议1：4字节小端序有符号整数
                        stack.Push(CLng(reader.ReadInt32()))

                    Case BININT1
                        ' 协议1：1字节无符号整数 (0-255)
                        stack.Push(CLng(reader.ReadByte()))

                    Case BININT2
                        ' 协议1：2字节小端序无符号整数 (0-65535)
                        stack.Push(CLng(reader.ReadUInt16()))

                    Case LONG1
                        ' 协议2：变长小端序有符号长整数
                        ' 第1字节为后续字节数，0表示值为0
                        Dim len = reader.ReadByte()
                        If len = 0 Then
                            stack.Push(0L)
                        Else
                            stack.Push(DecodeLong(reader.ReadBytes(len)))
                        End If

                    Case LONG4
                        ' 协议2：4字节长度前缀的小端序有符号长整数
                        ' 前4字节为后续字节数，用于非常大的整数
                        Dim len = reader.ReadInt32()
                        If len = 0 Then
                            stack.Push(0L)
                        Else
                            stack.Push(DecodeLong(reader.ReadBytes(len)))
                        End If

                ' ============================================================
                ' 浮点数
                ' ============================================================
                    Case FLOAT
                        ' 协议0：十进制浮点字符串（以换行符结尾）
                        Dim floatStr = ReadLine(reader).Trim()
                        stack.Push(Double.Parse(floatStr, CultureInfo.InvariantCulture))

                    Case BINFLOAT
                        ' 协议1：8字节大端序 IEEE 754 双精度浮点数
                        Dim floatBytes(7) As Byte
                        reader.Read(floatBytes, 0, 8)
                        Array.Reverse(floatBytes) ' 大端转小端
                        stack.Push(BitConverter.ToDouble(floatBytes, 0))

                ' ============================================================
                ' 文本字符串
                ' ============================================================
                    Case SHORT_BINUNICODE
                        ' 协议4：1字节长度前缀的UTF-8字符串 (最长255字节)
                        Dim len = reader.ReadByte()
                        stack.Push(Encoding.UTF8.GetString(reader.ReadBytes(len)))

                    Case BINUNICODE
                        ' 协议1：4字节长度前缀的UTF-8字符串 (最长2GB)
                        Dim len = reader.ReadInt32()
                        stack.Push(Encoding.UTF8.GetString(reader.ReadBytes(len)))

                    Case UNICODE
                        ' 协议0：raw-unicode-escape编码字符串（以换行符结尾）
                        ' 支持 \uXXXX 和 \UXXXXXXXX 转义序列
                        Dim rawStr = ReadLine(reader)
                        stack.Push(RawUnicodeEscapeDecode(rawStr))

                ' ============================================================
                ' 旧式字符串（协议0兼容，通常为 Latin-1 编码）
                ' ============================================================
                    Case SHORT_BINSTRING
                        ' 1字节长度前缀的字节字符串，按 Latin-1 解码
                        Dim len = reader.ReadByte()
                        stack.Push(Encoding.GetEncoding("Latin1").GetString(reader.ReadBytes(len)))

                    Case BINSTRING
                        ' 4字节长度前缀的字节字符串，按 Latin-1 解码
                        Dim len = reader.ReadInt32()
                        stack.Push(Encoding.GetEncoding("Latin1").GetString(reader.ReadBytes(len)))

                    Case [STRING]
                        ' 引号包围的字符串，支持转义序列
                        Dim line = ReadLine(reader).Trim()
                        If line.Length >= 2 Then
                            line = line.Substring(1, line.Length - 2) ' 去掉首尾引号
                        End If
                        stack.Push(EscapeDecode(line))

                ' ============================================================
                ' 字节类型
                ' ============================================================
                    Case SHORT_BINBYTES
                        ' 协议3：1字节长度前缀的字节对象 (最长255字节)
                        Dim len = reader.ReadByte()
                        stack.Push(reader.ReadBytes(len))

                    Case BINBYTES
                        ' 协议3：4字节长度前缀的字节对象
                        Dim len = reader.ReadInt32()
                        stack.Push(reader.ReadBytes(len))

                    Case BYTEARRAY8
                        ' 协议5：8字节长度前缀的字节数组
                        Dim len = reader.ReadInt64()
                        stack.Push(reader.ReadBytes(CInt(len)))

                ' ============================================================
                ' 列表
                ' ============================================================
                    Case EMPTY_LIST
                        ' 协议1：压入空列表
                        stack.Push(New List(Of Object)())

                    Case LIST
                        ' 协议0：从栈中弹出 Mark 以来的所有元素构建列表
                        Dim items = PopToMark(stack)
                        stack.Push(New List(Of Object)(items))

                    Case APPEND
                        ' 弹出栈顶值，追加到栈中的列表
                        Dim val = stack.Pop()
                        DirectCast(stack.Peek(), IList).Add(val)

                    Case APPENDS
                        ' 弹出 Mark 以来的所有值，批量追加到栈中的列表
                        Dim items = PopToMark(stack)
                        Dim lst = DirectCast(stack.Peek(), IList)
                        For Each item In items
                            lst.Add(item)
                        Next

                ' ============================================================
                ' 字典
                ' ============================================================
                    Case EMPTY_DICT
                        ' 压入空字典
                        stack.Push(New Dictionary(Of Object, Object)())

                    Case DICT
                        ' 协议0：从栈中弹出 Mark 以来的键值对构建字典
                        Dim items = PopToMark(stack)
                        Dim dict As New Dictionary(Of Object, Object)()
                        For i = 0 To items.Count - 2 Step 2
                            dict(items(i)) = items(i + 1)
                        Next
                        stack.Push(dict)

                    Case SETITEM
                        ' 弹出值和键，设置到栈中的字典
                        Dim val = stack.Pop()
                        Dim key = stack.Pop()
                        DirectCast(stack.Peek(), IDictionary)(key) = val

                    Case SETITEMS
                        ' 弹出 Mark 以来的键值对，批量设置到栈中的字典
                        Dim items = PopToMark(stack)
                        Dim dict = DirectCast(stack.Peek(), IDictionary)
                        For i = 0 To items.Count - 2 Step 2
                            dict(items(i)) = items(i + 1)
                        Next

                ' ============================================================
                ' 元组
                ' ============================================================
                    Case EMPTY_TUPLE
                        ' 协议1：压入空元组
                        stack.Push(New PythonTuple(Array.Empty(Of Object)()))

                    Case TUPLE
                        ' 从栈中弹出 Mark 以来的所有元素构建元组
                        Dim items = PopToMark(stack)
                        stack.Push(New PythonTuple(items.ToArray()))

                    Case TUPLE1
                        ' 协议2：弹出1个元素构建单元素元组
                        Dim item1 = stack.Pop()
                        stack.Push(New PythonTuple(New Object() {item1}))

                    Case TUPLE2
                        ' 协议2：弹出2个元素构建双元素元组
                        Dim item2 = stack.Pop()
                        Dim item1 = stack.Pop()
                        stack.Push(New PythonTuple(New Object() {item1, item2}))

                    Case TUPLE3
                        ' 协议2：弹出3个元素构建三元素元组
                        Dim item3 = stack.Pop()
                        Dim item2 = stack.Pop()
                        Dim item1 = stack.Pop()
                        stack.Push(New PythonTuple(New Object() {item1, item2, item3}))

                ' ============================================================
                ' 对象构造（REDUCE/GLOBAL/NEWOBJ/BUILD 模式）
                ' ============================================================
                    Case [GLOBAL]
                        ' 读取模块名和类名（各占一行），压入全局引用
                        Dim moduleName = ReadLine(reader)
                        Dim className = ReadLine(reader)
                        stack.Push(New PythonGlobalRef(moduleName, className))

                    Case REDUCE
                        ' 弹出参数元组和可调用引用，构造对象
                        Dim args = stack.Pop()
                        Dim callable = stack.Pop()
                        stack.Push(ReduceCallable(callable, args))

                    Case BUILD
                        ' 弹出状态对象和目标对象，将状态应用到目标对象
                        Dim state = stack.Pop()
                        Dim obj = stack.Pop()
                        BuildObject(obj, state)
                        stack.Push(obj)

                    Case NEWOBJ
                        ' 协议2：弹出新式类参数和类引用，构造空对象
                        Dim args = stack.Pop()
                        Dim cls = stack.Pop()
                        stack.Push(NewObjf(cls, args))

                    Case INST
                        ' 协议0：读取模块名和类名，弹出 Mark 以来的参数，构造旧式类实例
                        Dim moduleName = ReadLine(reader)
                        Dim className = ReadLine(reader)
                        Dim args = PopToMark(stack)
                        stack.Push(NewObjf(New PythonGlobalRef(moduleName, className), New PythonTuple(args.ToArray())))

                    Case OBJ
                        ' 协议0：弹出 Mark 以来的元素，第一个为类引用，其余为参数
                        Dim items = PopToMark(stack)
                        If items.Count >= 2 Then
                            Dim cls = items(0)
                            Dim args = items.Skip(1).ToArray()
                            stack.Push(NewObjf(cls, New PythonTuple(args)))
                        ElseIf items.Count = 1 Then
                            stack.Push(NewObjf(items(0), New PythonTuple(Array.Empty(Of Object)())))
                        Else
                            Throw New InvalidDataException("OBJ 操作码需要至少一个类引用")
                        End If

                ' ============================================================
                ' 持久化ID（需要外部持久化存储支持，此处简化处理）
                ' ============================================================
                    Case PERSID
                        ' 协议0：读取持久化ID字符串，简化处理为直接返回ID
                        Dim pid = ReadLine(reader)
                        stack.Push(pid)

                    Case BINPERSID
                        ' 协议1：弹出栈顶作为持久化ID，简化处理为直接返回
                        Dim pid = stack.Pop()
                        stack.Push(pid)

                ' ============================================================
                ' 扩展注册表（copyreg 机制，需要注册表支持）
                ' ============================================================
                    Case EXT1
                        Dim extCode = reader.ReadByte()
                        Throw New NotSupportedException($"不支持的 Pickle 扩展代码: {extCode}（需要 copyreg 注册表支持）")

                    Case EXT2
                        Dim extCode = reader.ReadUInt16()
                        Throw New NotSupportedException($"不支持的 Pickle 扩展代码: {extCode}（需要 copyreg 注册表支持）")

                    Case EXT4
                        Dim extCode = reader.ReadInt32()
                        Throw New NotSupportedException($"不支持的 Pickle 扩展代码: {extCode}（需要 copyreg 注册表支持）")

                    Case Else
                        Throw New NotSupportedException($"不支持的 Pickle 操作码: 0x{opcode:X2} ('{ChrW(opcode)}')")
                End Select
            End While

            Return Nothing
        End Function

        ' ===== 私有辅助方法 =====

        ''' <summary>
        ''' 读取一行文本（以 LF 换行符结尾）。协议0使用换行符作为分隔符。
        ''' </summary>
        Private Shared Function ReadLine(reader As BinaryReader) As String
            Dim sb As New StringBuilder()
            Try
                Do
                    Dim b As Byte = reader.ReadByte()
                    If b = &HA Then Exit Do ' LF 换行符
                    sb.Append(ChrW(b))
                Loop
            Catch ex As EndOfStreamException
                ' 到达流末尾，返回已读取的内容
            End Try
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 弹出栈中到 MarkObject 为止的所有元素（不包括 MarkObject 本身），
        ''' 返回的列表保持原始压栈顺序（先压入的在前）。
        ''' </summary>
        Private Shared Function PopToMark(stack As Stack(Of Object)) As List(Of Object)
            Dim items As New List(Of Object)()
            Do While stack.Count > 0 AndAlso Not TypeOf stack.Peek() Is MarkObject
                items.Insert(0, stack.Pop()) ' 逆序插入以保持原始顺序
            Loop
            If stack.Count > 0 Then stack.Pop() ' 弹出 MarkObject
            Return items
        End Function

        ''' <summary>
        ''' 解码小端序有符号长整数。
        ''' Python 使用任意长度的补码表示法，最高字节的最高位为符号位。
        ''' </summary>
        Private Shared Function DecodeLong(bytes As Byte()) As Long
            If bytes.Length = 0 Then Return 0L

            ' 读取无符号值
            Dim value As Long = 0
            For i = 0 To bytes.Length - 1
                value = value Or (CLng(bytes(i)) << (8 * i))
            Next

            ' 处理符号扩展：如果最高字节的最高位为1，则为负数
            If (bytes(bytes.Length - 1) And &H80) <> 0 Then
                value = value - (1L << (8 * bytes.Length))
            End If

            Return value
        End Function

        ''' <summary>
        ''' 解码 Python raw-unicode-escape 格式字符串。
        ''' 支持 \uXXXX（4位十六进制）和 \UXXXXXXXX（8位十六进制）转义序列，
        ''' 以及 \\ 反斜杠转义。
        ''' </summary>
        Private Shared Function RawUnicodeEscapeDecode(s As String) As String
            Dim sb As New StringBuilder(s.Length)
            Dim i = 0
            While i < s.Length
                If s(i) = "\"c AndAlso i + 1 < s.Length Then
                    If s(i + 1) = "u"c AndAlso i + 5 < s.Length Then
                        ' \uXXXX - BMP 字符
                        Dim hex = s.Substring(i + 2, 4)
                        sb.Append(ChrW(Integer.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture)))
                        i += 6
                        Continue While
                    ElseIf s(i + 1) = "U"c AndAlso i + 9 < s.Length Then
                        ' \UXXXXXXXX - 完整 Unicode 码点
                        Dim hex = s.Substring(i + 2, 8)
                        sb.Append(Char.ConvertFromUtf32(Integer.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture)))
                        i += 10
                        Continue While
                    ElseIf s(i + 1) = "\"c Then
                        ' \\ - 反斜杠字面量
                        sb.Append("\"c)
                        i += 2
                        Continue While
                    End If
                End If
                sb.Append(s(i))
                i += 1
            End While
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 解码 Python 字符串转义序列。
        ''' 支持 \n, \r, \t, \\, \', \", \xHH 和八进制转义 \OOO。
        ''' </summary>
        Private Shared Function EscapeDecode(s As String) As String
            Dim sb As New StringBuilder(s.Length)
            Dim i = 0
            While i < s.Length
                If s(i) = "\"c AndAlso i + 1 < s.Length Then
                    Select Case s(i + 1)
                        Case "n"c : sb.Append(vbLf) : i += 2
                        Case "r"c : sb.Append(vbCr) : i += 2
                        Case "t"c : sb.Append(vbTab) : i += 2
                        Case "\"c : sb.Append("\"c) : i += 2
                        Case "'"c : sb.Append("'"c) : i += 2
                        Case """"c : sb.Append(""""c) : i += 2
                        Case "a"c : sb.Append(ChrW(7)) : i += 2  ' BEL
                        Case "b"c : sb.Append(ChrW(8)) : i += 2  ' BS
                        Case "f"c : sb.Append(ChrW(12)) : i += 2 ' FF
                        Case "v"c : sb.Append(ChrW(11)) : i += 2 ' VT
                        Case "0"c To "7"c
                            ' 八进制转义 \OOO（最多3位）
                            Dim octalStr = ""
                            Dim j = i + 1
                            While j < s.Length AndAlso j < i + 4 AndAlso IsOctalDigit(s(j))
                                octalStr &= s(j)
                                j += 1
                            End While
                            sb.Append(ChrW(Convert.ToInt32(octalStr, 8)))
                            i = j
                        Case "x"c
                            ' 十六进制转义 \xHH
                            If i + 3 < s.Length Then
                                Dim hex = s.Substring(i + 2, 2)
                                sb.Append(ChrW(Integer.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture)))
                                i += 4
                            Else
                                sb.Append(s(i)) : i += 1
                            End If
                        Case Else
                            sb.Append(s(i)) : i += 1
                    End Select
                Else
                    sb.Append(s(i)) : i += 1
                End If
            End While
            Return sb.ToString()
        End Function

        Private Shared Function IsOctalDigit(c As Char) As Boolean
            Return c >= "0"c AndAlso c <= "7"c
        End Function

        ''' <summary>
        ''' 处理 REDUCE 操作码：调用可调用对象并传入参数。
        ''' 如果可调用对象是 PythonGlobalRef，尝试构造对应的 .NET 对象；
        ''' 否则返回 PythonObject 包装器。
        ''' </summary>
        Private Shared Function ReduceCallable(callable As Object, args As Object) As Object
            Dim globalRef = TryCast(callable, PythonGlobalRef)
            If globalRef IsNot Nothing Then
                Return ConstructPythonObject(globalRef, args)
            End If

            ' 无法识别的可调用对象，返回 PythonObject 包装器
            Dim argArray = ExtractArgs(args)
            Return New PythonObject("unknown", If(callable?.ToString(), "callable"), argArray)
        End Function

        ''' <summary>
        ''' 处理 NEWOBJ 操作码：使用类引用和参数构造新式类实例。
        ''' </summary>
        Private Shared Function NewObjf(cls As Object, args As Object) As Object
            Dim globalRef = TryCast(cls, PythonGlobalRef)
            If globalRef IsNot Nothing Then
                Return ConstructPythonObject(globalRef, args)
            End If

            Dim argArray = ExtractArgs(args)
            Return New PythonObject("unknown", If(cls?.ToString(), "class"), argArray)
        End Function

        ''' <summary>
        ''' 从参数对象中提取参数数组。支持 PythonTuple、IList 和 Object()。
        ''' </summary>
        Private Shared Function ExtractArgs(args As Object) As Object()
            If TypeOf args Is PythonTuple Then
                Return DirectCast(args, PythonTuple).Items
            ElseIf TypeOf args Is IList Then
                Dim lst = DirectCast(args, IList)
                Dim arr(lst.Count - 1) As Object
                lst.CopyTo(arr, 0)
                Return arr
            ElseIf TypeOf args Is Object() Then
                Return DirectCast(args, Object())
            Else
                Return New Object() {args}
            End If
        End Function

        ''' <summary>
        ''' 根据全局引用构造 Python 对象的 .NET 等价物。
        ''' 对常见的 Python 内置类型和标准库类型进行映射，
        ''' 未知类型则返回 PythonObject 包装器以保留信息。
        ''' </summary>
        Private Shared Function ConstructPythonObject(globalRef As PythonGlobalRef, args As Object) As Object
            Dim argArray = ExtractArgs(args)
            Dim fullName = $"{globalRef.ModuleName}.{globalRef.ClassName}"

            Select Case fullName
            ' ----- 集合类型 -----
                Case "collections.OrderedDict", "collections.defaultdict"
                    ' OrderedDict 和 defaultdict 都映射为 Dictionary（.NET Dictionary 保持插入顺序）
                    Dim dict As New Dictionary(Of Object, Object)()
                    If argArray.Length > 0 Then
                        If TypeOf argArray(0) Is IList Then
                            ' OrderedDict([(k1,v1), (k2,v2), ...]) 形式
                            For Each item In DirectCast(argArray(0), IList)
                                If TypeOf item Is PythonTuple Then
                                    Dim t = DirectCast(item, PythonTuple)
                                    If t.Length >= 2 Then dict(t(0)) = t(1)
                                ElseIf TypeOf item Is IList Then
                                    Dim l = DirectCast(item, IList)
                                    If l.Count >= 2 Then dict(l(0)) = l(1)
                                End If
                            Next
                        ElseIf TypeOf argArray(0) Is IDictionary Then
                            ' defaultdict(None, {k1:v1, ...}) 形式
                            For Each entry As DictionaryEntry In DirectCast(argArray(0), IDictionary)
                                dict(entry.Key) = entry.Value
                            Next
                        End If
                    End If
                    ' defaultdict 的第二个参数可能是初始字典
                    If fullName = "collections.defaultdict" AndAlso argArray.Length > 1 Then
                        If TypeOf argArray(1) Is IDictionary Then
                            For Each entry As DictionaryEntry In DirectCast(argArray(1), IDictionary)
                                dict(entry.Key) = entry.Value
                            Next
                        End If
                    End If
                    Return dict

                Case "collections.Counter"
                    ' Counter 是 dict 的子类，映射为 Dictionary
                    Dim dict As New Dictionary(Of Object, Object)()
                    If argArray.Length > 0 AndAlso TypeOf argArray(0) Is IDictionary Then
                        For Each entry As DictionaryEntry In DirectCast(argArray(0), IDictionary)
                            dict(entry.Key) = entry.Value
                        Next
                    End If
                    Return dict

            ' ----- 日期时间类型 -----
                Case "datetime.datetime"
                    ' datetime(year, month, day, hour, minute, second, microsecond)
                    If argArray.Length >= 3 Then
                        Dim year = CInt(argArray(0))
                        Dim month = CInt(argArray(1))
                        Dim day = CInt(argArray(2))
                        Dim hour = If(argArray.Length > 3, CInt(argArray(3)), 0)
                        Dim minute = If(argArray.Length > 4, CInt(argArray(4)), 0)
                        Dim second = If(argArray.Length > 5, CInt(argArray(5)), 0)
                        Dim millisecond = If(argArray.Length > 6, CInt(argArray(6)) \ 1000, 0)
                        Return New DateTime(year, month, day, hour, minute, second, millisecond)
                    End If
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

                Case "datetime.date"
                    ' date(year, month, day)
                    If argArray.Length >= 3 Then
                        Return New DateTime(CInt(argArray(0)), CInt(argArray(1)), CInt(argArray(2)))
                    End If
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

                Case "datetime.time"
                    ' time(hour, minute, second, microsecond)
                    If argArray.Length >= 1 Then
                        Dim hour = CInt(argArray(0))
                        Dim minute = If(argArray.Length > 1, CInt(argArray(1)), 0)
                        Dim second = If(argArray.Length > 2, CInt(argArray(2)), 0)
                        Return New TimeSpan(hour, minute, second)
                    End If
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

                Case "datetime.timedelta"
                    ' timedelta(days, seconds, microseconds)
                    If argArray.Length >= 1 Then
                        Dim days = CLng(argArray(0))
                        Dim seconds = If(argArray.Length > 1, CLng(argArray(1)), 0L)
                        Dim microseconds = If(argArray.Length > 2, CLng(argArray(2)), 0L)
                        Return TimeSpan.FromDays(days).Add(TimeSpan.FromSeconds(seconds)).Add(TimeSpan.FromTicks(microseconds * 10))
                    End If
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

            ' ----- 数值类型 -----
                Case "decimal.Decimal"
                    If argArray.Length > 0 Then
                        Return Decimal.Parse(argArray(0).ToString(), CultureInfo.InvariantCulture)
                    End If
                    Return 0D

                Case "fractions.Fraction"
                    If argArray.Length >= 2 Then
                        Dim numerator = Convert.ToDouble(argArray(0))
                        Dim denominator = Convert.ToDouble(argArray(1))
                        If denominator <> 0 Then Return numerator / denominator
                    End If
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

            ' ----- 集合类型 -----
                Case "builtins.set", "__builtin__.set"
                    Dim s As New PythonSet()
                    If argArray.Length > 0 AndAlso TypeOf argArray(0) Is IList Then
                        For Each item In DirectCast(argArray(0), IList)
                            s.Add(item)
                        Next
                    End If
                    Return s

                Case "builtins.frozenset", "__builtin__.frozenset"
                    Dim s As New PythonSet()
                    If argArray.Length > 0 AndAlso TypeOf argArray(0) Is IList Then
                        For Each item In DirectCast(argArray(0), IList)
                            s.Add(item)
                        Next
                    End If
                    Return s

            ' ----- 字节类型 -----
                Case "builtins.bytes", "__builtin__.bytes"
                    If argArray.Length > 0 Then
                        If TypeOf argArray(0) Is Byte() Then
                            Return argArray(0)
                        ElseIf TypeOf argArray(0) Is IList Then
                            Dim lst = DirectCast(argArray(0), IList)
                            Dim bytes(lst.Count - 1) As Byte
                            For i = 0 To lst.Count - 1
                                bytes(i) = CByte(lst(i))
                            Next
                            Return bytes
                        End If
                    End If
                    Return Array.Empty(Of Byte)()

                Case "builtins.bytearray", "__builtin__.bytearray"
                    If argArray.Length > 0 Then
                        If TypeOf argArray(0) Is Byte() Then
                            Return DirectCast(argArray(0), Byte()).Clone()
                        End If
                    End If
                    Return Array.Empty(Of Byte)()

            ' ----- 复数 -----
                Case "builtins.complex", "__builtin__.complex"
                    If argArray.Length >= 2 Then
                        Return New ComplexNumber(Convert.ToDouble(argArray(0)), Convert.ToDouble(argArray(1)))
                    ElseIf argArray.Length = 1 Then
                        Return New ComplexNumber(Convert.ToDouble(argArray(0)), 0.0)
                    End If
                    Return New ComplexNumber(0.0, 0.0)

            ' ----- 范围类型 -----
                Case "builtins.range", "__builtin__.range", "builtins.xrange", "__builtin__.xrange"
                    ' range 映射为 List(Of Integer)
                    If argArray.Length = 1 Then
                        Return Enumerable.Range(0, CInt(argArray(0))).Cast(Of Object)().ToList()
                    ElseIf argArray.Length = 2 Then
                        Return Enumerable.Range(CInt(argArray(0)), CInt(argArray(1)) - CInt(argArray(0))).Cast(Of Object)().ToList()
                    ElseIf argArray.Length >= 3 Then
                        Dim start = CInt(argArray(0))
                        Dim [stop] = CInt(argArray(1))
                        Dim [step] = CInt(argArray(2))
                        Dim lst As New List(Of Object)()
                        If [step] > 0 Then
                            For v = start To [stop] - 1 Step [step]
                                lst.Add(v)
                            Next
                        ElseIf [step] < 0 Then
                            For v = start To [stop] + 1 Step [step]
                                lst.Add(v)
                            Next
                        End If
                        Return lst
                    End If
                    Return New List(Of Object)()

            ' ----- 枚举类型 -----
                Case "builtins.enumerate", "__builtin__.enumerate"
                    ' enumerate 对象无法完整还原，返回空列表
                    Return New List(Of Object)()

            ' ----- re 模块 -----
                Case "re.Pattern", "_sre.SRE_Pattern"
                    ' 正则表达式对象无法直接映射，返回 PythonObject
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)

                Case Else
                    ' 未知类型，返回 PythonObject 包装器以保留完整信息
                    Return New PythonObject(globalRef.ModuleName, globalRef.ClassName, argArray)
            End Select
        End Function

        ''' <summary>
        ''' 处理 BUILD 操作码：将状态应用到已构造的对象上。
        ''' 对于 PythonObject，将状态字典合并到 State 属性中；
        ''' 对于字典类型，将状态字典的键值对合并到目标字典中。
        ''' </summary>
        Private Shared Sub BuildObject(obj As Object, state As Object)
            If TypeOf obj Is PythonObject Then
                Dim pyObj = DirectCast(obj, PythonObject)
                If TypeOf state Is IDictionary Then
                    For Each entry As DictionaryEntry In DirectCast(state, IDictionary)
                        pyObj.State(entry.Key) = entry.Value
                    Next
                End If
            ElseIf TypeOf obj Is IDictionary Then
                ' 某些 Python 类的 __dict__ 通过 BUILD 设置到已有字典对象上
                If TypeOf state Is IDictionary Then
                    Dim dict = DirectCast(obj, IDictionary)
                    For Each entry As DictionaryEntry In DirectCast(state, IDictionary)
                        dict(entry.Key) = entry.Value
                    Next
                End If
            End If
            ' 对于其他已知类型（如 DateTime、数值类型等），BUILD 通常不需要特殊处理
        End Sub

    End Class

End Namespace