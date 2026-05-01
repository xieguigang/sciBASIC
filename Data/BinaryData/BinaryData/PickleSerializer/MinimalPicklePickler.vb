Imports System.IO
Imports System.Reflection
Imports System.Text
Imports std = System.Math
Imports TypeInfo = Microsoft.VisualBasic.Scripting.MetaData.TypeInfo

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
    '''   其他对象          → 通过反射序列化属性为 dict
    ''' 
    ''' 特性：
    '''   - 支持 memo 机制，正确处理共享引用和循环引用
    '''   - 使用协议4格式，兼容 Python 3.4+
    '''   - 自动选择最优的整数编码（BININT1/BININT2/BININT/LONG1）
    '''   - 自动选择最优的字符串编码（SHORT_BINUNICODE/BINUNICODE）
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
        Private _memo As New Dictionary(Of Object, Integer)()
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
            ElseIf TypeOf obj Is Type Then
                obj = New TypeInfo(DirectCast(obj, Type))
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

            ' 集合类型
                Case TypeOf obj Is IDictionary
                    SerializeDict(DirectCast(obj, IDictionary))
                Case TypeOf obj Is IList
                    SerializeList(DirectCast(obj, IList))
                Case TypeOf obj Is Array
                    SerializeArray(DirectCast(obj, Array))

                    ' 其他类型：通过反射序列化属性
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
        ''' 将任意 .NET 对象通过反射序列化。
        ''' 使用 GLOBAL + REDUCE + BUILD 模式，将对象的公共属性作为状态字典传递。
        ''' Python 端需要对应的类定义才能完整还原。
        ''' </summary>
        Private Sub SerializeAsDict(obj As Object)
            Dim objType = obj.GetType()

            ' GLOBAL: 引用命名空间.类名
            _output.Add([GLOBAL])
            Dim ns = If(objType.Namespace, "")
            _output.AddRange(Encoding.ASCII.GetBytes(ns & vbLf))
            _output.AddRange(Encoding.ASCII.GetBytes(objType.Name & vbLf))

            ' 空构造参数
            _output.Add(EMPTY_TUPLE)
            _output.Add(REDUCE)
            AddToMemo(obj)

            ' 将公共可读属性序列化为状态字典
            Dim props = objType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            Dim stateDict As New Dictionary(Of Object, Object)()
            For Each prop In props
                Try
                    If prop.CanRead AndAlso prop.GetIndexParameters().Length = 0 Then
                        stateDict(prop.Name) = prop.GetValue(obj)
                    End If
                Catch
                    ' 跳过无法读取的属性（可能抛出异常的索引器等）
                End Try
            Next

            ' 也序列化公共字段
            Dim fields = objType.GetFields(BindingFlags.Public Or BindingFlags.Instance)
            For Each field In fields
                Try
                    stateDict(field.Name) = field.GetValue(obj)
                Catch
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
            If value = 0 Then Return Array.Empty(Of Byte)()

            Dim isNegative = value < 0
            ' 处理负数：先取绝对值
            Dim absValue As ULong
            If isNegative Then
                ' 对于 Long.MinValue，直接取反会溢出，使用 ULong 中间值
                absValue = CULng(-CLng(value And &H7FFFFFFFFFFFFFFFL)) + If((value And 1) <> 0, 0UL, 0UL)
                ' 更简单的方法：直接处理补码
                absValue = CULng(Not CULng(value)) + 1UL
            Else
                absValue = CULng(value)
            End If

            Dim bytes As New List(Of Byte)()
            Do While absValue > 0
                bytes.Add(CByte(absValue And &HFF))
                absValue >>= 8
            Loop

            ' 检查是否需要额外的符号字节
            If (bytes(bytes.Count - 1) And &H80) <> 0 Then
                ' 最高位已设置，需要额外的符号字节
                bytes.Add(If(isNegative, &HFF, CByte(0)))
            ElseIf isNegative Then
                ' 负数但最高位未设置，设置符号位
                bytes(bytes.Count - 1) = bytes(bytes.Count - 1) Or &H80
            End If

            Return bytes.ToArray()
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