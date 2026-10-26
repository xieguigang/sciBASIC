#Region "Microsoft.VisualBasic::b440bb1336d97708ca44a2956128622f, Data\BinaryData\BinaryData\PickleSerializer\MinimalPicklePickler.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 701
    '    Code Lines: 399 (56.92%)
    ' Comment Lines: 205 (29.24%)
    '    - Xml Docs: 65.37%
    ' 
    '   Blank Lines: 97 (13.84%)
    '     File Size: 27.70 KB


    '     Class MinimalPicklePickler
    ' 
    '         Function: EncodeLong, Pickle, PickleToBase64, Serialize
    ' 
    '         Sub: AddToMemo, (+2 Overloads) PickleToFile, SerializeArray, SerializeAsDict, SerializeBoolean
    '              SerializeBytes, SerializeComplex, SerializeDateTime, SerializeDict, SerializeFloat
    '              SerializeList, SerializeLong, SerializeObject, SerializePythonObject, SerializeSet
    '              SerializeString, SerializeTimeSpan, SerializeTuple, SerializeULong
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports std = System.Math

Namespace Pickle

    ' =====================================================================
    ' Pickle 序列化器
    ' =====================================================================

    ''' <summary>
    ''' Pickle 序列化器，将 .NET 对象序列化为 Pickle 协议4格式。
    ''' 生成的数据可被 Python 的 pickle.loads() 正确反序列化。
    ''' 
    ''' 支持的 .NET 类型到 Python 类型的映射：
    '''   Nothing / null    → None
    '''   Boolean           → bool
    '''   整数类型          → int
    '''   Single/Double     → float
    '''   Decimal           → float (精度可能损失)
    '''   String / Char     → str
    '''   Byte()            → bytes
    '''   IDictionary       → dict
    '''   IList / Array     → list
    '''   PythonTuple       → tuple
    '''   PythonSet         → list (简化处理，Python 可再转换)
    '''   PythonObject      → 通过 GLOBAL+REDUCE+BUILD 还原
    '''   ComplexNumber     → complex
    '''   DateTime          → datetime.datetime
    '''   Enum              → 底层整数值
    '''   Type              → 程序集限定名字符串（反序列化端还原为 Type）
    '''   Guid/Uri/DateTimeOffset/DateOnly/TimeOnly → 字符串
    '''   其他 CLR 对象     → GLOBAL(clr:程序集) + REDUCE + BUILD 字段快照，
    '''                       可被本库的反序列化端还原为原始 CLR 类型
    ''' 
    ''' 特性：
    '''   - 支持 memo 机制，正确处理共享引用和循环引用
    '''   - 使用协议4格式，兼容 Python 3.4+
    '''   - 自动选择最优的整数编码（BININT1/BININT2/BININT/LONG1）
    '''   - 自动选择最优的字符串编码（SHORT_BINUNICODE/BINUNICODE）
    '''   - 自定义 CLR 类型以"字段快照"保存：委托与 NonSerialized 字段被跳过，
    '''     只带参数化构造函数的类型通过未初始化对象 + 字段回填还原
    ''' </summary>
    Public Class MinimalPicklePickler

        ' ===== Pickle 协议4 操作码 =====

        ' 协议控制
        Private Const PROTO As Byte = &H80
        Private Const FRAME As Byte = &H95
        Private Const [STOP] As Byte = &H2E

        ' 栈操作
        Private Const MARK As Byte = &H28

        ' Memo 操作
        Private Const MEMOIZE As Byte = &H94
        Private Const BINGET As Byte = &H68
        Private Const LONG_BINGET As Byte = &H6A

        ' None 和布尔
        Private Const NONE As Byte = &H4E
        Private Const NEWTRUE As Byte = &H88
        Private Const NEWFALSE As Byte = &H89

        ' 整数
        Private Const BININT As Byte = &H4A
        Private Const BININT1 As Byte = &H4B
        Private Const BININT2 As Byte = &H4D
        Private Const LONG1 As Byte = &H8A
        Private Const LONG4 As Byte = &H8B

        ' 浮点数
        Private Const BINFLOAT As Byte = &H47

        ' 文本字符串
        Private Const SHORT_BINUNICODE As Byte = &H8C
        Private Const BINUNICODE As Byte = &H58

        ' 字节
        Private Const SHORT_BINBYTES As Byte = &H43
        Private Const BINBYTES As Byte = &H42

        ' 列表
        Private Const EMPTY_LIST As Byte = &H5D
        Private Const APPENDS As Byte = &H65

        ' 字典
        Private Const EMPTY_DICT As Byte = &H7D
        Private Const SETITEMS As Byte = &H75

        ' 元组
        Private Const EMPTY_TUPLE As Byte = &H29
        Private Const TUPLE As Byte = &H74
        Private Const TUPLE1 As Byte = &H85
        Private Const TUPLE2 As Byte = &H86
        Private Const TUPLE3 As Byte = &H87

        ' 对象构造
        Private Const [GLOBAL] As Byte = &H63
        Private Const REDUCE As Byte = &H52
        Private Const BUILD As Byte = &H62
        Private Const NEWOBJ As Byte = &H81

        ' ===== 序列化上下文 =====
        Private _output As New List(Of Byte)()

        ' memo 使用引用相等比较：覆盖了 Equals 的类如果使用默认比较器，
        ' 两个不同实例会被误判成同一个 memo 条目，导致还原后的共享引用错乱
        Private _memo As New Dictionary(Of Object, Integer)(ReferenceComparer.Instance)
        Private _memoIndex As Integer = 0

        ' ===== 公共方法 =====

        ''' <summary>
        ''' 将 .NET 对象序列化为 Pickle 格式的字节数组。
        ''' 这是最核心的序列化方法，生成的数据可被 Python 的 pickle.loads() 直接读取。
        ''' </summary>
        ''' <param name="obj">要序列化的 .NET 对象</param>
        ''' <returns>Pickle 协议4格式的字节数组</returns>
        Public Shared Function Pickle(obj As Object) As Byte()
            Dim pickler As New MinimalPicklePickler()
            Return pickler.Serialize(obj)
        End Function

        ''' <summary>
        ''' 将 .NET 对象序列化为 Base64 编码字符串。
        ''' 适合在文本协议（如 JSON、XML）中传输 Pickle 数据。
        ''' </summary>
        ''' <param name="obj">要序列化的 .NET 对象</param>
        ''' <returns>Base64 编码的 Pickle 数据</returns>
        Public Shared Function PickleToBase64(obj As Object) As String
            Dim bytes = Pickle(obj)
            Return Convert.ToBase64String(bytes)
        End Function

        ''' <summary>
        ''' 将 .NET 对象序列化并写入文件。
        ''' 生成的文件可被 Python 的 pickle.load(open('file', 'rb')) 读取。
        ''' </summary>
        ''' <param name="obj">要序列化的 .NET 对象</param>
        ''' <param name="filePath">输出文件路径</param>
        Public Shared Sub PickleToFile(obj As Object, filePath As String)
            Dim bytes = Pickle(obj)
            File.WriteAllBytes(filePath, bytes)
        End Sub

        Public Shared Sub PickleToFile(obj As Object, file As Stream)
            Dim bytes = Pickle(obj)

            Call file.Write(bytes, Scan0, bytes.Length)
            Call file.Flush()
        End Sub

        ' ===== 内部序列化方法 =====

        ''' <summary>
        ''' 执行实际的序列化过程：写入协议头、序列化根对象、写入结束标记。
        ''' </summary>
        Private Function Serialize(obj As Object) As Byte()
            _output.Clear()
            _memo.Clear()
            _memoIndex = 0

            ' 写入协议4头
            _output.Add(PROTO)
            _output.Add(4)

            ' 序列化根对象
            SerializeObject(obj)

            ' 写入结束标记
            _output.Add([STOP])

            Return _output.ToArray()
        End Function

        ''' <summary>
        ''' 核心序列化分派方法。根据对象类型选择合适的 Pickle 操作码进行序列化。
        ''' 处理 memo 查找（共享引用和循环引用），以及类型分派。
        ''' </summary>
        Private Sub SerializeObject(obj As Object)
            ' ----- 处理 None / Nothing -----
            If obj Is Nothing Then
                _output.Add(NONE)
                Return
            End If

            ' ----- 检查 memo（共享引用和循环引用）-----
            ' 值类型和字符串不参与 memo（它们是不可变的，不会有循环引用）
            Dim objType = obj.GetType()
            If Not objType.IsValueType AndAlso Not (TypeOf obj Is String) Then
                If _memo.ContainsKey(obj) Then
                    Dim idx = _memo(obj)
                    If idx <= Byte.MaxValue Then
                        _output.Add(BINGET)
                        _output.Add(CByte(idx))
                    Else
                        _output.Add(LONG_BINGET)
                        _output.AddRange(BitConverter.GetBytes(idx))
                    End If
                    Return
                End If
            End If

            ' ----- 根据类型分派序列化 -----
            Select Case True
            ' 布尔必须在整数之前检查（Boolean 是 ValueType 的子类）
                Case TypeOf obj Is Boolean
                    SerializeBoolean(CBool(obj))

            ' 枚举：以底层整数值保存（反序列化端按目标字段的枚举类型还原）
                Case TypeOf obj Is [Enum]
                    Dim underlying As Type = [Enum].GetUnderlyingType(obj.GetType())

                    If underlying Is GetType(ULong) Then
                        SerializeULong(Convert.ToUInt64(obj, CultureInfo.InvariantCulture))
                    Else
                        SerializeLong(Convert.ToInt64(obj, CultureInfo.InvariantCulture))
                    End If

            ' 整数类型：统一转为 Long 处理
                Case TypeOf obj Is SByte
                    SerializeLong(CLng(CSByte(obj)))
                Case TypeOf obj Is Byte
                    SerializeLong(CLng(CByte(obj)))
                Case TypeOf obj Is Short
                    SerializeLong(CLng(CShort(obj)))
                Case TypeOf obj Is UShort
                    SerializeLong(CLng(CUShort(obj)))
                Case TypeOf obj Is Integer
                    SerializeLong(CLng(CInt(obj)))
                Case TypeOf obj Is UInteger
                    SerializeLong(CLng(CUInt(obj)))
                Case TypeOf obj Is Long
                    SerializeLong(CLng(obj))
                Case TypeOf obj Is ULong
                    ' ULong 可能超出 Long 范围，需要特殊处理
                    SerializeULong(CULng(obj))

            ' 浮点数类型
                Case TypeOf obj Is Single
                    SerializeFloat(CDbl(CSng(obj)))
                Case TypeOf obj Is Double
                    SerializeFloat(CDbl(obj))
                Case TypeOf obj Is Decimal
                    ' Decimal 转为 Double 可能损失精度，但 Python 没有等价的 Decimal 操作码
                    SerializeFloat(CDbl(CDec(obj)))

            ' 字符串类型
                Case TypeOf obj Is String
                    SerializeString(CStr(obj))
                Case TypeOf obj Is Char
                    SerializeString(obj.ToString())

            ' 字节数组
                Case TypeOf obj Is Byte()
                    SerializeBytes(DirectCast(obj, Byte()))

            ' Python 特殊类型
                Case TypeOf obj Is PythonTuple
                    SerializeTuple(DirectCast(obj, PythonTuple))
                Case TypeOf obj Is PythonSet
                    SerializeSet(DirectCast(obj, PythonSet))
                Case TypeOf obj Is PythonObject
                    SerializePythonObject(DirectCast(obj, PythonObject))
                Case TypeOf obj Is ComplexNumber
                    SerializeComplex(DirectCast(obj, ComplexNumber))

            ' 日期时间类型
                Case TypeOf obj Is DateTime
                    SerializeDateTime(CDate(obj))
                Case TypeOf obj Is TimeSpan
                    SerializeTimeSpan(DirectCast(obj, TimeSpan))

            ' ----- 以字符串形式保存"无字段可序列化"或跨程序集难以重建的结构 -----
                Case TypeOf obj Is Type
                    ' 反序列化端由 ClrObjectCodec 按程序集限定名还原为 Type
                    SerializeString(DirectCast(obj, Type).AssemblyQualifiedName)
                Case TypeOf obj Is [Delegate]
                    ' 委托绑定的是方法与其宿主实例，无法跨进程重建，写 None
                    _output.Add(NONE)
                Case TypeOf obj Is IntPtr
                    SerializeLong(DirectCast(obj, IntPtr).ToInt64())
                Case TypeOf obj Is UIntPtr
                    SerializeULong(DirectCast(obj, UIntPtr).ToUInt64())
                Case TypeOf obj Is Guid
                    SerializeString(DirectCast(obj, Guid).ToString("D"))
                Case TypeOf obj Is Uri
                    SerializeString(DirectCast(obj, Uri).OriginalString)
                Case TypeOf obj Is DateTimeOffset
                    SerializeString(DirectCast(obj, DateTimeOffset).ToString("o", CultureInfo.InvariantCulture))
                Case TypeOf obj Is DateOnly
                    SerializeString(DirectCast(obj, DateOnly).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                Case TypeOf obj Is TimeOnly
                    SerializeString(DirectCast(obj, TimeOnly).ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture))

            ' 集合类型
                Case TypeOf obj Is IDictionary
                    SerializeDict(DirectCast(obj, IDictionary))
                Case TypeOf obj Is IList
                    SerializeList(DirectCast(obj, IList))
                Case TypeOf obj Is Array
                    SerializeArray(DirectCast(obj, Array))

                    ' 其他类型：通过反射采集字段快照
                Case Else
                    SerializeAsDict(obj)
            End Select
        End Sub

        ''' <summary>
        ''' 序列化布尔值。使用 NEWTRUE/NEWFALSE 操作码（协议2+）。
        ''' </summary>
        Private Sub SerializeBoolean(value As Boolean)
            If value Then
                _output.Add(NEWTRUE)
            Else
                _output.Add(NEWFALSE)
            End If
        End Sub

        ''' <summary>
        ''' 序列化有符号长整数。自动选择最紧凑的编码方式：
        '''   0-255       → BININT1 (2字节)
        '''   0-65535     → BININT2 (3字节)
        '''   Int32范围   → BININT (5字节)
        '''   超出范围    → LONG1 (变长)
        ''' </summary>
        Private Sub SerializeLong(value As Long)
            If value >= 0 AndAlso value <= &HFF Then
                _output.Add(BININT1)
                _output.Add(CByte(value))
            ElseIf value >= 0 AndAlso value <= &HFFFF Then
                _output.Add(BININT2)
                _output.AddRange(BitConverter.GetBytes(CUShort(value)))
            ElseIf value >= Integer.MinValue AndAlso value <= Integer.MaxValue Then
                _output.Add(BININT)
                _output.AddRange(BitConverter.GetBytes(CInt(value)))
            Else
                ' 超出 Int32 范围，使用 LONG1 变长编码
                Dim bytes = EncodeLong(value)
                If bytes.Length <= Byte.MaxValue Then
                    _output.Add(LONG1)
                    _output.Add(CByte(bytes.Length))
                    _output.AddRange(bytes)
                Else
                    _output.Add(LONG4)
                    _output.AddRange(BitConverter.GetBytes(bytes.Length))
                    _output.AddRange(bytes)
                End If
            End If
        End Sub

        ''' <summary>
        ''' 序列化无符号长整数（ULong）。处理超出 Long 范围的情况。
        ''' </summary>
        Private Sub SerializeULong(value As ULong)
            If value <= Long.MaxValue Then
                SerializeLong(CLng(value))
            Else
                ' 超出 Long 范围，需要使用 LONG1 编码
                ' ULong 最大值 18446744073709551615 需要8字节
                Dim bytes As New List(Of Byte)()
                Dim v = value
                Do While v > 0
                    bytes.Add(CByte(v And &HFF))
                    v >>= 8
                Loop
                ' 确保最高位不被误读为负数符号
                If (bytes(bytes.Count - 1) And &H80) <> 0 Then
                    bytes.Add(0)
                End If
                _output.Add(LONG1)
                _output.Add(CByte(bytes.Count))
                _output.AddRange(bytes)
            End If
        End Sub

        ''' <summary>
        ''' 序列化双精度浮点数。使用 BINFLOAT 操作码（8字节大端序 IEEE 754）。
        ''' </summary>
        Private Sub SerializeFloat(value As Double)
            _output.Add(BINFLOAT)
            Dim bytes = BitConverter.GetBytes(value)
            Array.Reverse(bytes) ' 小端转大端
            _output.AddRange(bytes)
        End Sub

        ''' <summary>
        ''' 序列化 UTF-8 字符串。自动选择长度前缀大小：
        '''   UTF8字节长度 &lt;= 255  → SHORT_BINUNICODE (2字节头)
        '''   UTF8字节长度 > 255   → BINUNICODE (5字节头)
        ''' </summary>
        Private Sub SerializeString(value As String)
            Dim utf8Bytes = Encoding.UTF8.GetBytes(value)
            If utf8Bytes.Length <= Byte.MaxValue Then
                _output.Add(SHORT_BINUNICODE)
                _output.Add(CByte(utf8Bytes.Length))
                _output.AddRange(utf8Bytes)
            Else
                _output.Add(BINUNICODE)
                _output.AddRange(BitConverter.GetBytes(utf8Bytes.Length))
                _output.AddRange(utf8Bytes)
            End If
        End Sub

        ''' <summary>
        ''' 序列化字节数组。自动选择长度前缀大小：
        '''   长度 &lt;= 255  → SHORT_BINBYTES (2字节头)
        '''   长度 > 255   → BINBYTES (5字节头)
        ''' </summary>
        Private Sub SerializeBytes(value As Byte())
            If value.Length <= Byte.MaxValue Then
                _output.Add(SHORT_BINBYTES)
                _output.Add(CByte(value.Length))
                _output.AddRange(value)
            Else
                _output.Add(BINBYTES)
                _output.AddRange(BitConverter.GetBytes(value.Length))
                _output.AddRange(value)
            End If
        End Sub

        ''' <summary>
        ''' 序列化字典。使用 EMPTY_DICT + MEMOIZE + MARK + 键值对 + SETITEMS 模式。
        ''' 先将空字典注册到 memo（处理循环引用），然后批量设置键值对。
        ''' </summary>
        Private Sub SerializeDict(dict As IDictionary)
            ' 压入空字典并立即 memoize（处理循环引用）
            _output.Add(EMPTY_DICT)
            AddToMemo(dict)

            If dict.Count = 0 Then Return

            ' 批量设置键值对
            _output.Add(MARK)
            For Each entry As DictionaryEntry In dict
                SerializeObject(entry.Key)
                SerializeObject(entry.Value)
            Next
            _output.Add(SETITEMS)
        End Sub

        ''' <summary>
        ''' 序列化列表。使用 EMPTY_LIST + MEMOIZE + MARK + 元素 + APPENDS 模式。
        ''' </summary>
        Private Sub SerializeList(list As IList)
            _output.Add(EMPTY_LIST)
            AddToMemo(list)

            If list.Count = 0 Then Return

            _output.Add(MARK)
            For Each item In list
                SerializeObject(item)
            Next
            _output.Add(APPENDS)
        End Sub

        ''' <summary>
        ''' 序列化数组。与列表相同的 Pickle 表示（Python 没有区分数组和列表）。
        ''' </summary>
        Private Sub SerializeArray(arr As Array)
            _output.Add(EMPTY_LIST)
            AddToMemo(arr)

            If arr.Length = 0 Then Return

            _output.Add(MARK)
            For Each item In arr
                SerializeObject(item)
            Next
            _output.Add(APPENDS)
        End Sub

        ''' <summary>
        ''' 序列化 PythonTuple 元组。根据元素数量选择最优编码：
        '''   0元素 → EMPTY_TUPLE
        '''   1元素 → TUPLE1
        '''   2元素 → TUPLE2
        '''   3元素 → TUPLE3
        '''   4+元素 → MARK + items + TUPLE
        ''' </summary>
        Private Sub SerializeTuple(tuple As PythonTuple)
            Select Case tuple.Length
                Case 0
                    _output.Add(EMPTY_TUPLE)

                Case 1
                    SerializeObject(tuple(0))
                    _output.Add(TUPLE1)

                Case 2
                    SerializeObject(tuple(0))
                    SerializeObject(tuple(1))
                    _output.Add(TUPLE2)

                Case 3
                    SerializeObject(tuple(0))
                    SerializeObject(tuple(1))
                    SerializeObject(tuple(2))
                    _output.Add(TUPLE3)

                Case Else
                    _output.Add(MARK)
                    For i = 0 To tuple.Length - 1
                        SerializeObject(tuple(i))
                    Next
                    _output.Add(MinimalPicklePickler.TUPLE)
            End Select

            AddToMemo(tuple)
        End Sub

        ''' <summary>
        ''' 序列化 PythonSet 集合。
        ''' 由于 Python pickle 对 set 的序列化使用 GLOBAL+REDUCE 模式较复杂，
        ''' 此处简化为序列化为列表。Python 端可用 set() 转换回来。
        ''' </summary>
        Private Sub SerializeSet(pySet As PythonSet)
            _output.Add(EMPTY_LIST)
            AddToMemo(pySet)

            If pySet.Count = 0 Then Return

            _output.Add(MARK)
            For Each item In pySet
                SerializeObject(item)
            Next
            _output.Add(APPENDS)
        End Sub

        ''' <summary>
        ''' 序列化 DateTime 为 Python datetime.datetime 对象。
        ''' 使用 GLOBAL + 参数元组 + REDUCE 模式。
        ''' </summary>
        Private Sub SerializeDateTime(dt As DateTime)
            ' 确保 DateTimeKind 不影响序列化（去除时区偏移）
            Dim dtUnspecified = New DateTime(dt.Ticks, DateTimeKind.Unspecified)

            ' GLOBAL: 引用 datetime.datetime
            _output.Add([GLOBAL])
            _output.AddRange(Encoding.ASCII.GetBytes("datetime" & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes("datetime" & vbLf))

            ' 构造参数元组: (year, month, day, hour, minute, second, microsecond)
            Dim microsecond = dtUnspecified.Millisecond * 1000L ' 毫秒转微秒
            SerializeObject(New PythonTuple(New Object() {
            dtUnspecified.Year, dtUnspecified.Month, dtUnspecified.Day,
            dtUnspecified.Hour, dtUnspecified.Minute, dtUnspecified.Second,
            microsecond
        }))

            ' REDUCE: 调用 datetime.datetime(*args)
            _output.Add(REDUCE)

            AddToMemo(dt)
        End Sub

        ''' <summary>
        ''' 序列化 TimeSpan 为 Python datetime.timedelta 对象。
        ''' </summary>
        Private Sub SerializeTimeSpan(ts As TimeSpan)
            ' GLOBAL: 引用 datetime.timedelta
            _output.Add([GLOBAL])
            _output.AddRange(Encoding.ASCII.GetBytes("datetime" & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes("timedelta" & vbLf))

            ' 构造参数元组: (days, seconds, microseconds)
            Dim totalDays = CLng(std.Truncate(ts.TotalDays))
            Dim remainingSeconds = CLng(std.Truncate(ts.TotalSeconds)) - totalDays * 86400L
            Dim microseconds = (ts.Ticks Mod 10000000L) \ 10L ' 剩余微秒
            SerializeObject(New PythonTuple(New Object() {totalDays, remainingSeconds, microseconds}))

            _output.Add(REDUCE)
            AddToMemo(ts)
        End Sub

        ''' <summary>
        ''' 序列化 ComplexNumber 为 Python complex 对象。
        ''' 使用 GLOBAL + 参数元组 + REDUCE 模式。
        ''' </summary>
        Private Sub SerializeComplex(c As ComplexNumber)
            _output.Add([GLOBAL])
            _output.AddRange(Encoding.ASCII.GetBytes("builtins" & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes("complex" & vbLf))

            SerializeObject(New PythonTuple(New Object() {c.Real, c.Imaginary}))

            _output.Add(REDUCE)
            AddToMemo(c)
        End Sub

        ''' <summary>
        ''' 序列化 PythonObject 自定义对象。
        ''' 使用 GLOBAL + 参数元组 + REDUCE + BUILD 模式，
        ''' 完整还原 Python 对象的类名、构造参数和状态。
        ''' </summary>
        Private Sub SerializePythonObject(pyObj As PythonObject)
            ' GLOBAL: 引用模块.类
            _output.Add([GLOBAL])
            _output.AddRange(Encoding.ASCII.GetBytes(pyObj.ModuleName & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes(pyObj.ClassName & vbLf))

            ' 构造参数元组
            If pyObj.ConstructorArgs IsNot Nothing AndAlso pyObj.ConstructorArgs.Length > 0 Then
                SerializeObject(New PythonTuple(pyObj.ConstructorArgs))
            Else
                _output.Add(EMPTY_TUPLE)
            End If

            ' REDUCE: 调用 类(*args)
            _output.Add(REDUCE)
            AddToMemo(pyObj)

            ' BUILD: 应用对象状态（如果有）
            If pyObj.State IsNot Nothing AndAlso pyObj.State.Count > 0 Then
                SerializeObject(pyObj.State)
                _output.Add(BUILD)
            End If
        End Sub

        ''' <summary>
        ''' 将任意 .NET 对象通过反射序列化为"CLR 字段快照"。
        ''' </summary>
        ''' <remarks>
        ''' 使用 GLOBAL + EMPTY_TUPLE + REDUCE + BUILD 模式：
        ''' 
        ''' <list type="number">
        ''' <item>GLOBAL 携带 <c>clr:程序集名</c> 与类型全名，反序列化端据此解析出 CLR 类型；</item>
        ''' <item>REDUCE 以空参数构造一个"未填充"的实例（无参构造优先，否则未初始化对象）；</item>
        ''' <item>BUILD 以"字段名 → 值"的字典回填全部实例字段（含 Private 与基类字段）。</item>
        ''' </list>
        ''' 
        ''' 只采集字段而不采集属性，可以避免"属性 setter 副作用"与"自动属性重复写两份"的问题；
        ''' 委托字段与 <c>&lt;NonSerialized&gt;</c> 字段会被跳过。
        ''' </remarks>
        Private Sub SerializeAsDict(obj As Object)
            Dim objType = obj.GetType()

            If Not ClrObjectCodec.IsSupportedType(objType) Then
                ' 无法重建的类型（动态类型、反射元数据等）退化成 None，避免写出无法解析的载荷
                _output.Add(NONE)
                Return
            End If

            ' GLOBAL: 携带可解析的 CLR 类型标识
            _output.Add([GLOBAL])
            _output.AddRange(Encoding.ASCII.GetBytes(ClrObjectCodec.GetModuleName(objType) & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes(ClrObjectCodec.GetClassName(objType) & vbLf))

            ' 空构造参数：反序列化端创建一个空实例，随后由 BUILD 回填字段
            _output.Add(EMPTY_TUPLE)
            _output.Add(REDUCE)
            AddToMemo(obj)

            ' 字段快照
            Dim fields As FieldInfo() = ClrObjectCodec.GetSerializableFields(objType)
            Dim stateDict As New Dictionary(Of Object, Object)()

            For Each field As FieldInfo In fields
                Try
                    Dim fieldValue As Object = field.GetValue(obj)

                    If TypeOf fieldValue Is [Delegate] Then
                        Continue For
                    End If

                    stateDict(field.Name) = fieldValue
                Catch ex As Exception
                    ' 跳过无法读取的字段
                End Try
            Next

            If stateDict.Count > 0 Then
                SerializeObject(stateDict)
                _output.Add(BUILD)
            End If
        End Sub

        ' ===== 编码辅助方法 =====

        ''' <summary>
        ''' 将有符号长整数编码为小端序补码字节序列。
        ''' 这是 Python LONG1/LONG4 操作码使用的编码格式。
        ''' 编码规则：
        '''   - 使用小端序（最低有效字节在前）
        '''   - 使用补码表示负数
        '''   - 如果最高字节的最高位与符号不一致，需要额外的符号字节
        ''' </summary>
        Private Shared Function EncodeLong(value As Long) As Byte()
            ' BitConverter 给出的是 8 字节小端补码，正是 Python LONG1/LONG4 使用的编码
            Dim buffer As Byte() = BitConverter.GetBytes(value)
            Dim last As Integer = 7

            If value >= 0 Then
                ' 去掉多余的高位 0 字节（至少保留 1 字节，且最高字节的符号位必须是 0）
                While last > 0 AndAlso buffer(last) = 0 AndAlso (buffer(last - 1) And &H80) = 0
                    last -= 1
                End While

                If (buffer(last) And &H80) <> 0 Then
                    ' 最高字节的符号位为 1 会被读成负数，补一个 0 字节
                    last += 1
                End If
            Else
                ' 去掉多余的高位 0xFF 字节（至少保留 1 字节，且最高字节的符号位必须是 1）
                While last > 0 AndAlso buffer(last) = &HFF AndAlso (buffer(last - 1) And &H80) <> 0
                    last -= 1
                End While

                If (buffer(last) And &H80) = 0 Then
                    ' 最高字节的符号位为 0 会被读成正数，补一个 0xFF 字节
                    last += 1
                End If
            End If

            Dim result As Byte() = New Byte(last) {}

            Call Array.Copy(buffer, 0, result, 0, System.Math.Min(8, last + 1))

            If last > 7 Then
                result(last) = If(value < 0, CByte(&HFF), CByte(0))
            End If

            Return result
        End Function

        ''' <summary>
        ''' 将对象添加到 memo 表，并写入 MEMOIZE 操作码。
        ''' Memo 机制用于处理共享引用（同一个对象被多次引用）
        ''' 和循环引用（对象直接或间接引用自身）。
        ''' 仅对引用类型进行 memo，值类型和字符串不需要。
        ''' </summary>
        Private Sub AddToMemo(obj As Object)
            If obj Is Nothing Then Return

            Dim objType = obj.GetType()
            ' 值类型和字符串不需要 memo（它们是不可变的，不会产生循环引用）
            If objType.IsValueType OrElse TypeOf obj Is String Then Return

            ' 记录到 memo 字典（用于后续 BINGET 引用）
            _memo(obj) = _memoIndex
            _memoIndex += 1

            ' 写入 MEMOIZE 操作码（协议4自动索引）
            _output.Add(MEMOIZE)
        End Sub

    End Class

End Namespace
