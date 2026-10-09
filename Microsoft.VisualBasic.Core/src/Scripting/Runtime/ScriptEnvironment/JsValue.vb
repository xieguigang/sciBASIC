
' /********************************************************************************/
'
'   tagged union value type for the scripting runtime:
'
'   a stack allocated value type which represents the script value with
'   zero boxing/unboxing cost on the CLR primitive types (Double, Int32,
'   Boolean, ...). Designed as the internal storage unit of
'   <see cref="ScriptSlot"/> and as the universal value carrier for
'   interpreter hot paths.
'
'       TypeCode.Empty  => undefined
'       TypeCode.DBNull => null (Nothing)
'
' /********************************************************************************/

Namespace Scripting.Runtime

    ''' <summary>
    ''' 值语义的标签联合体（tagged union value type）。
    '''
    ''' CLR 基础类型（Double/Int32/Boolean/...）以独立的强类型字段存储，
    ''' 在解释器热路径上按值传递时不会发生任何 box/unbox 或者堆分配。
    ''' 只有在跨越宿主边界（Object API）时才需要通过 <see cref="AsObject"/>
    ''' 执行一次装箱。
    ''' </summary>
    ''' <remarks>
    ''' 类型标签约定：
    ''' <list type="bullet">
    ''' <item><see cref="TypeCode.Empty"/> => undefined（未定义）</item>
    ''' <item><see cref="TypeCode.DBNull"/> => null（空值）</item>
    ''' <item><see cref="TypeCode.Single"/> 的数据存放在 <c>_dbl</c> 字段中（Single → Double 转换无损）</item>
    ''' <item><see cref="TypeCode.DateTime"/> 的数据以 100ns Ticks 形式存放在 <c>_lng</c> 字段中（零装箱，
    ''' 注意往返不保留 <see cref="DateTimeKind"/> 元数据）</item>
    ''' </list>
    ''' </remarks>
    Public Structure JsValue

        ''' <summary>当前值的数据类型标签</summary>
        Public ReadOnly Property VarType As TypeCode

        Friend ReadOnly _dbl As Double
        Friend ReadOnly _int As Integer
        Friend ReadOnly _lng As Long
        Friend ReadOnly _bool As Boolean
        Friend ReadOnly _str As String
        Friend ReadOnly _obj As Object

        ''' <summary>JS undefined 值</summary>
        Public Shared ReadOnly Undef As JsValue = New JsValue(TypeCode.Empty, 0, 0, 0, False, Nothing, Nothing)

        ''' <summary>JS null 值</summary>
        Public Shared ReadOnly Null As JsValue = New JsValue(TypeCode.DBNull, 0, 0, 0, False, Nothing, Nothing)

        Private Sub New(t As TypeCode, dbl As Double, i As Integer, lng As Long, b As Boolean, s As String, o As Object)
            _VarType = t
            _dbl = dbl
            _int = i
            _lng = lng
            _bool = b
            _str = s
            _obj = o
        End Sub

        ' ========================================================
        ' 工厂方法（无装箱）
        ' ========================================================

        ''' <summary>JS number（Double）</summary>
        Public Shared Function Number(d As Double) As JsValue
            Return New JsValue(TypeCode.Double, d, 0, 0, False, Nothing, Nothing)
        End Function

        ''' <summary>32位整型快速路径（避免整型运算经过 Double 往返）</summary>
        Public Shared Function Int32(i As Integer) As JsValue
            Return New JsValue(TypeCode.Int32, 0, i, 0, False, Nothing, Nothing)
        End Function

        ''' <summary>64位整型</summary>
        Public Shared Function Int64(l As Long) As JsValue
            Return New JsValue(TypeCode.Int64, 0, 0, l, False, Nothing, Nothing)
        End Function

        ''' <summary>32位单精度浮点</summary>
        Public Shared Function Single_(s As Single) As JsValue
            ' Single → Double 转换无损，借用 _dbl 字段存储
            Return New JsValue(TypeCode.Single, s, 0, 0, False, Nothing, Nothing)
        End Function

        ''' <summary>布尔值</summary>
        Public Shared Function Boolean_(b As Boolean) As JsValue
            Return New JsValue(TypeCode.Boolean, 0, 0, 0, b, Nothing, Nothing)
        End Function

        ''' <summary>字符串值</summary>
        Public Shared Function Str(s As String) As JsValue
            Return New JsValue(TypeCode.String, 0, 0, 0, False, s, Nothing)
        End Function

        ''' <summary>日期值（以 100ns Ticks 形式零装箱存储于 <c>_lng</c> 字段）</summary>
        Public Shared Function Date_(d As Date) As JsValue
            Return New JsValue(TypeCode.DateTime, 0, 0, d.Ticks, False, Nothing, Nothing)
        End Function

        ''' <summary>任意 CLR 引用类型对象</summary>
        Public Shared Function OfObject(o As Object) As JsValue
            If o Is Nothing Then
                Return Null
            End If

            Dim t = o.GetType()

            If t Is GetType(Integer) Then
                Return Int32(DirectCast(o, Integer))
            ElseIf t Is GetType(Double) Then
                Return Number(DirectCast(o, Double))
            ElseIf t Is GetType(Boolean) Then
                Return Boolean_(DirectCast(o, Boolean))
            ElseIf t Is GetType(String) Then
                Return Str(DirectCast(o, String))
            ElseIf t Is GetType(Single) Then
                Return Single_(DirectCast(o, Single))
            ElseIf t Is GetType(Long) Then
                Return Int64(DirectCast(o, Long))
            ElseIf t Is GetType(Date) Then
                Return Date_(DirectCast(o, Date))
            Else
                Return New JsValue(TypeCode.Object, 0, 0, 0, False, Nothing, o)
            End If
        End Function

        ' ========================================================
        ' 类型标签测试
        ' ========================================================

        ''' <summary>JS undefined？</summary>
        Public Function IsUndef() As Boolean
            Return VarType = TypeCode.Empty
        End Function

        ''' <summary>JS null？</summary>
        Public Function IsNull() As Boolean
            Return VarType = TypeCode.DBNull
        End Function

        ''' <summary>JS number？（Double/Int32/Single/Int64 标签）</summary>
        Public Function IsNumber() As Boolean
            Select Case VarType
                Case TypeCode.Double, TypeCode.Int32,
                     TypeCode.Single, TypeCode.Int64 : Return True
                Case Else : Return False
            End Select
        End Function

        ''' <summary>字符串值？</summary>
        Public Function IsString() As Boolean
            Return VarType = TypeCode.String
        End Function

        ''' <summary>布尔值？</summary>
        Public Function IsBoolean() As Boolean
            Return VarType = TypeCode.Boolean
        End Function

        ''' <summary>CLR 引用类型对象？</summary>
        Public Function IsObject() As Boolean
            Return VarType = TypeCode.Object
        End Function

        ' ========================================================
        ' 强类型访问器（无装箱，热路径专用）
        ' ========================================================

        ''' <summary>
        ''' 按数值语义读取 Double。整型标签会被隐式转换为 Double。
        ''' </summary>
        Public Function AsDouble() As Double
            Select Case VarType
                Case TypeCode.Double, TypeCode.Single : Return _dbl
                Case TypeCode.Int32 : Return CDbl(_int)
                Case TypeCode.Int64 : Return CDbl(_lng)
                Case Else
                    Throw New InvalidOperationException(
                        $"JsValue is not a number (VarType={VarType})")
            End Select
        End Function

        ''' <summary>
        ''' 按整数语义读取 Integer。Double 标签会被隐式截断转换。
        ''' </summary>
        Public Function AsInt32() As Integer
            Select Case VarType
                Case TypeCode.Int32 : Return _int
                Case TypeCode.Double, TypeCode.Single : Return CInt(_dbl)
                Case TypeCode.Int64 : Return CInt(_lng)
                Case Else
                    Throw New InvalidOperationException(
                        $"JsValue is not a number (VarType={VarType})")
            End Select
        End Function

        Public Function AsInt64() As Long
            Select Case VarType
                Case TypeCode.Int64 : Return _lng
                Case TypeCode.Int32 : Return CLng(_int)
                Case TypeCode.Double, TypeCode.Single : Return CLng(_dbl)
                Case Else
                    Throw New InvalidOperationException(
                        $"JsValue is not a number (VarType={VarType})")
            End Select
        End Function

        Public Function AsSingle() As Single
            Select Case VarType
                Case TypeCode.Single, TypeCode.Double : Return CSng(_dbl)
                Case TypeCode.Int32 : Return CSng(_int)
                Case TypeCode.Int64 : Return CSng(_lng)
                Case Else
                    Throw New InvalidOperationException(
                        $"JsValue is not a number (VarType={VarType})")
            End Select
        End Function

        Public Function AsBoolean() As Boolean
            If VarType = TypeCode.Boolean Then
                Return _bool
            End If
            Throw New InvalidOperationException(
                $"JsValue is not a boolean (VarType={VarType})")
        End Function

        Public Function AsString() As String
            If VarType = TypeCode.String Then
                Return _str
            End If
            Throw New InvalidOperationException(
                $"JsValue is not a string (VarType={VarType})")
        End Function

        ''' <summary>
        ''' 跨越宿主边界时使用：装箱为 Object（这是唯一可能发生装箱的位置）。
        ''' </summary>
        Public Function AsObject() As Object
            Select Case VarType
                Case TypeCode.Empty : Return Nothing
                Case TypeCode.DBNull : Return Nothing
                Case TypeCode.Boolean : Return _bool
                Case TypeCode.Int32 : Return _int
                Case TypeCode.Double : Return _dbl
                Case TypeCode.Single : Return CSng(_dbl)
                Case TypeCode.Int64 : Return _lng
                Case TypeCode.String : Return _str
                Case TypeCode.DateTime : Return New Date(_lng)
                Case TypeCode.Object : Return _obj
                Case Else : Return Nothing
            End Select
        End Function

        ' ========================================================
        ' 类型转换语义（JS truthy）
        ' ========================================================

        ''' <summary>
        ''' JS truthy 语义：undefined/null/0/NaN/空字符串为 falsy，其余为 truthy。
        ''' </summary>
        Public Function Truthy() As Boolean
            Select Case VarType
                Case TypeCode.Empty, TypeCode.DBNull : Return False
                Case TypeCode.Boolean : Return _bool
                Case TypeCode.Int32 : Return _int <> 0
                Case TypeCode.Int64 : Return _lng <> 0
                Case TypeCode.Double, TypeCode.Single : Return _dbl <> 0 AndAlso Not Double.IsNaN(_dbl)
                Case TypeCode.String : Return Not String.IsNullOrEmpty(_str)
                Case Else : Return True   ' Object / DateTime
            End Select
        End Function

        Public Overrides Function ToString() As String
            Select Case VarType
                Case TypeCode.Empty : Return "undefined"
                Case TypeCode.DBNull : Return "null"
                Case TypeCode.Boolean : Return _bool.ToString
                Case TypeCode.Int32 : Return _int.ToString
                Case TypeCode.Int64 : Return _lng.ToString
                Case TypeCode.Double, TypeCode.Single : Return _dbl.ToString
                Case TypeCode.String : Return _str
                Case TypeCode.DateTime : Return New Date(_lng).ToString
                Case Else : Return _obj?.ToString
            End Select
        End Function
    End Structure
End Namespace
