' ---------------------------------------------------------------------------
' 表达式节点：字面量 / 参数 / 局部 / 一元 / 二元 / 转换 / 调用 /
'             数组取元素 / 数组长度 / 三元 / SSA phi
' ---------------------------------------------------------------------------

Imports System.Reflection

Namespace IL

    ''' <summary>字面量常量（ldc.i4 / ldc.r4 / ldc.r8 / ldstr / ldnull ...）</summary>
    Public Class LiteralExpression : Inherits Expression

        ''' <summary>常量的托管值（Integer / Single / Double / Long / String / Nothing）</summary>
        Public Property Value As Object

        Public Sub New()
            MyBase.New(SyntaxKind.Literal)
        End Sub

        Public Sub New(value As Object, valueType As System.Type)
            MyBase.New(SyntaxKind.Literal)
            Me.Value = value
            Me.Type = valueType
        End Sub

        Public Overrides Function ToString() As String
            If Value Is Nothing Then Return "null"
            If TypeOf Value Is Single Then Return CSng(Value).ToString("0.0######", Globalization.CultureInfo.InvariantCulture) & "F"
            If TypeOf Value Is Double Then Return CDbl(Value).ToString("0.0######", Globalization.CultureInfo.InvariantCulture)
            Return Value.ToString()
        End Function
    End Class

    ''' <summary>方法形参引用（ldarg / ldarga）</summary>
    Public Class ParameterExpression : Inherits Expression

        ''' <summary>参数下标；实例方法的 ldarg.0 为 this</summary>
        Public Property Index As Integer
        ''' <summary>参数名（无 PDB 时用 argN 兜底）</summary>
        Public Property Name As String

        Public Sub New()
            MyBase.New(SyntaxKind.Parameter)
        End Sub

        Public Sub New(index As Integer, name As String, parameterType As System.Type)
            MyBase.New(SyntaxKind.Parameter)
            Me.Index = index
            Me.Name = name
            Me.Type = parameterType
        End Sub

        Public Overrides Function ToString() As String
            Return If(Name, "arg" & Index)
        End Function
    End Class

    ''' <summary>局部变量引用（ldloc / ldloca）</summary>
    Public Class LocalExpression : Inherits Expression

        ''' <summary>原始局部变量下标（与 MethodBody.Locals 对应）</summary>
        Public Property Index As Integer
        ''' <summary>SSA 版本号；未做 SSA 时为 0</summary>
        Public Property Version As Integer
        ''' <summary>变量名（SSA 之后形如 V_0_1，无 PDB 时用 V_N 兜底）</summary>
        Public Property Name As String

        Public Sub New()
            MyBase.New(SyntaxKind.Local)
        End Sub

        Public Sub New(index As Integer, name As String, localType As System.Type, Optional version As Integer = 0)
            MyBase.New(SyntaxKind.Local)
            Me.Index = index
            Me.Name = name
            Me.Type = localType
            Me.Version = version
        End Sub

        ''' <summary>SSA 之后的唯一名字（同一变量的不同版本互不相同）</summary>
        Public ReadOnly Property SsaName As String
            Get
                Dim base = If(Name, "V_" & Index)
                Return If(Version > 0, base & "_" & Version, base)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return SsaName
        End Function
    End Class

    ''' <summary>二元运算</summary>
    Public Class BinaryExpression : Inherits Expression

        Public Property [Operator] As BinaryOperator
        Public Property Left As Expression
        Public Property Right As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.Binary)
        End Sub

        Public Sub New(op As BinaryOperator, left As Expression, right As Expression, resultType As System.Type)
            MyBase.New(SyntaxKind.Binary)
            Me.Operator = op
            Me.Left = left
            Me.Right = right
            Me.Type = resultType
        End Sub

        Public Overrides Function ToString() As String
            Return $"({Left} {[Operator]} {Right})"
        End Function
    End Class

    ''' <summary>一元运算</summary>
    Public Class UnaryExpression : Inherits Expression

        Public Property [Operator] As UnaryOperator
        Public Property Operand As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.Unary)
        End Sub

        Public Sub New(op As UnaryOperator, operand As Expression, resultType As System.Type)
            MyBase.New(SyntaxKind.Unary)
            Me.Operator = op
            Me.Operand = operand
            Me.Type = resultType
        End Sub

        Public Overrides Function ToString() As String
            Return $"({[Operator]} {Operand})"
        End Function
    End Class

    ''' <summary>数值/引用转换（conv.r4 / conv.r8 / conv.i4 ...）</summary>
    Public Class ConvertExpression : Inherits Expression

        Public Property Operand As Expression
        ''' <summary>目标类型</summary>
        Public Property TargetType As System.Type
        ''' <summary>是否为带溢出检查的 conv.ovf.*</summary>
        Public Property IsChecked As Boolean

        Public Sub New()
            MyBase.New(SyntaxKind.Convert)
        End Sub

        Public Sub New(operand As Expression, targetType As System.Type, Optional isChecked As Boolean = False)
            MyBase.New(SyntaxKind.Convert)
            Me.Operand = operand
            Me.TargetType = targetType
            Me.Type = targetType
            Me.IsChecked = isChecked
        End Sub

        Public Overrides Function ToString() As String
            Return $"CType({Operand}, {If(TargetType Is Nothing, "?", TargetType.Name)})"
        End Function
    End Class

    ''' <summary>方法调用（call / callvirt，仅白名单内的纯函数）</summary>
    Public Class CallExpression : Inherits Expression

        ''' <summary>被调用的方法；无法解析时为 Nothing</summary>
        Public Property Method As MethodInfo
        ''' <summary>方法名（无法解析时用作兜底标识）</summary>
        Public Property MethodName As String
        ''' <summary>声明类型全名；用于白名单匹配</summary>
        Public Property DeclaringTypeName As String
        Public Property Arguments As New List(Of Expression)
        ''' <summary>实例方法的 this 参数（静态方法为 Nothing）</summary>
        Public Property Instance As Expression
        Public Property IsStaticCall As Boolean

        Public Sub New()
            MyBase.New(SyntaxKind.Invoke)
        End Sub

        ''' <summary>形如 System.Math.Sqrt 的完全限定名，供发射器查表</summary>
        Public ReadOnly Property FullName As String
            Get
                If Method IsNot Nothing Then
                    Return If(Method.DeclaringType Is Nothing, Method.Name, Method.DeclaringType.FullName & "." & Method.Name)
                End If

                Return If(DeclaringTypeName Is Nothing, MethodName, DeclaringTypeName & "." & MethodName)
            End Get
        End Property

        ''' <summary>参与求值的全部表达式（实例方法的 this 排在第一个）</summary>
        Public ReadOnly Property AllArguments As List(Of Expression)
            Get
                If Instance Is Nothing Then Return Arguments

                Dim all As New List(Of Expression) From {Instance}
                all.AddRange(Arguments)

                Return all
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{FullName}({String.Join(", ", AllArguments)})"
        End Function
    End Class

    ''' <summary>数组取元素（ldelem.*）</summary>
    Public Class ArrayIndexExpression : Inherits Expression

        ''' <summary>数组表达式</summary>
        Public Property Array As Expression
        ''' <summary>下标表达式</summary>
        Public Property Index As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.ArrayIndex)
        End Sub

        Public Sub New(array As Expression, index As Expression, elementType As System.Type)
            MyBase.New(SyntaxKind.ArrayIndex)
            Me.Array = array
            Me.Index = index
            Me.Type = elementType
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Array}[{Index}]"
        End Function
    End Class

    ''' <summary>数组长度（ldlen）</summary>
    Public Class ArrayLengthExpression : Inherits Expression

        Public Property Array As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.ArrayLength)
            Me.Type = GetType(Integer)
        End Sub

        Public Sub New(array As Expression)
            MyBase.New(SyntaxKind.ArrayLength)
            Me.Array = array
            Me.Type = GetType(Integer)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Array}.Length"
        End Function
    End Class

    ''' <summary>三元条件表达式（cond ? a : b）</summary>
    Public Class TernaryExpression : Inherits Expression

        Public Property Condition As Expression
        Public Property WhenTrue As Expression
        Public Property WhenFalse As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.Ternary)
        End Sub

        Public Sub New(condition As Expression, whenTrue As Expression, whenFalse As Expression, resultType As System.Type)
            MyBase.New(SyntaxKind.Ternary)
            Me.Condition = condition
            Me.WhenTrue = whenTrue
            Me.WhenFalse = whenFalse
            Me.Type = resultType
        End Sub

        Public Overrides Function ToString() As String
            Return $"({Condition} ? {WhenTrue} : {WhenFalse})"
        End Function
    End Class

    ''' <summary>SSA phi 的一个入边：来自哪个基本块、取哪个值</summary>
    Public Class PhiArgument

        ''' <summary>来源基本块编号</summary>
        Public Property BlockId As Integer
        Public Property Value As Expression

        Public Sub New()
        End Sub

        Public Sub New(blockId As Integer, value As Expression)
            Me.BlockId = blockId
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"bb{BlockId}:{Value}"
        End Function
    End Class

    ''' <summary>
    ''' SSA phi 节点。结构化还原阶段会把它就地折叠成"分支末尾赋值 + 声明提升"，
    ''' 因此正常情况下最终 AST 里不应残留 phi。
    ''' </summary>
    Public Class PhiExpression : Inherits Expression

        Public Property Arguments As New List(Of PhiArgument)
        ''' <summary>该 phi 对应的变量（用于折叠后定位目标）</summary>
        Public Property TargetName As String

        Public Sub New()
            MyBase.New(SyntaxKind.Phi)
        End Sub

        Public Overrides Function ToString() As String
            Return $"phi({String.Join(", ", Arguments)})"
        End Function
    End Class
End Namespace
