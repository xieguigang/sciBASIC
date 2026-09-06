' ---------------------------------------------------------------------------
' 语句节点：块 / 变量声明 / 赋值 / 表达式语句 / 返回 / if / while / for /
'           break / continue / nop，以及整个方法的语法树 MethodSyntax
' ---------------------------------------------------------------------------

Imports System.Reflection

Namespace IL

    ''' <summary>语句块（同时用作方法体与 if / while / for 的 body）</summary>
    Public Class BlockStatement : Inherits Statement

        Public Property Statements As New List(Of Statement)

        Public Sub New()
            MyBase.New(SyntaxKind.Block)
        End Sub

        Public Sub New(statements As IEnumerable(Of Statement))
            MyBase.New(SyntaxKind.Block)
            If statements IsNot Nothing Then Me.Statements.AddRange(statements)
        End Sub
    End Class

    ''' <summary>局部变量声明（可带初值）</summary>
    Public Class VariableDeclarationStatement : Inherits Statement

        ''' <summary>SSA 之后的唯一名字</summary>
        Public Property Name As String
        ''' <summary>原始局部变量下标；SSA 产生的提升变量为 -1</summary>
        Public Property LocalIndex As Integer
        ''' <summary>声明的类型</summary>
        Public Property VariableType As System.Type
        ''' <summary>初值表达式；可为 Nothing（仅声明）</summary>
        Public Property Initializer As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.VariableDeclaration)
        End Sub

        Public Sub New(name As String, variableType As System.Type,
                       Optional initializer As Expression = Nothing,
                       Optional localIndex As Integer = -1)
            MyBase.New(SyntaxKind.VariableDeclaration)
            Me.Name = name
            Me.VariableType = variableType
            Me.Initializer = initializer
            Me.LocalIndex = localIndex
        End Sub

        Public Overrides Function ToString() As String
            Dim typeName = If(VariableType Is Nothing, "?", VariableType.Name)
            Return If(Initializer Is Nothing,
                      $"Dim {Name} As {typeName}",
                      $"Dim {Name} As {typeName} = {Initializer}")
        End Function
    End Class

    ''' <summary>赋值：target = value</summary>
    Public Class AssignmentStatement : Inherits Statement

        ''' <summary>赋值目标：LocalExpression / ParameterExpression / ArrayIndexExpression</summary>
        Public Property Target As Expression
        Public Property Value As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.Assignment)
        End Sub

        Public Sub New(target As Expression, value As Expression)
            MyBase.New(SyntaxKind.Assignment)
            Me.Target = target
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Target} = {Value}"
        End Function
    End Class

    ''' <summary>只有副作用、丢弃结果的表达式语句（例如 stelem、无返回值的 call）</summary>
    Public Class ExpressionStatement : Inherits Statement

        ''' <summary>注意：属性名不能叫 Expression（与 Expression 类名冲突）</summary>
        Public Property Value As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.ExpressionStatement)
        End Sub

        Public Sub New(value As Expression)
            MyBase.New(SyntaxKind.ExpressionStatement)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return If(Value Is Nothing, "", Value.ToString())
        End Function
    End Class

    ''' <summary>return 语句；Value 为 Nothing 表示 Sub 的隐式返回</summary>
    Public Class ReturnStatement : Inherits Statement

        Public Property Value As Expression

        Public Sub New()
            MyBase.New(SyntaxKind.ReturnStmt)
        End Sub

        Public Sub New(value As Expression)
            MyBase.New(SyntaxKind.ReturnStmt)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return If(Value Is Nothing, "Return", $"Return {Value}")
        End Function
    End Class

    ''' <summary>if / else</summary>
    Public Class IfStatement : Inherits Statement

        Public Property Condition As Expression
        Public Property ThenBody As BlockStatement
        ''' <summary>else 分支；没有 else 时为 Nothing</summary>
        Public Property ElseBody As BlockStatement

        Public Sub New()
            MyBase.New(SyntaxKind.IfStmt)
            Me.ThenBody = New BlockStatement()
        End Sub

        Public Sub New(condition As Expression, thenBody As BlockStatement, Optional elseBody As BlockStatement = Nothing)
            MyBase.New(SyntaxKind.IfStmt)
            Me.Condition = condition
            Me.ThenBody = If(thenBody, New BlockStatement())
            Me.ElseBody = elseBody
        End Sub
    End Class

    ''' <summary>while 循环（由带单一回边的自然循环归约而来）</summary>
    Public Class WhileStatement : Inherits Statement

        Public Property Condition As Expression
        Public Property Body As BlockStatement
        ''' <summary>条件在循环头还是循环尾（do-while 形态）</summary>
        Public Property IsPostCondition As Boolean

        Public Sub New()
            MyBase.New(SyntaxKind.While)
            Me.Body = New BlockStatement()
        End Sub

        Public Sub New(condition As Expression, body As BlockStatement, Optional isPostCondition As Boolean = False)
            MyBase.New(SyntaxKind.While)
            Me.Condition = condition
            Me.Body = If(body, New BlockStatement())
            Me.IsPostCondition = isPostCondition
        End Sub
    End Class

    ''' <summary>for 循环（归纳变量形态的 while 提升而来）</summary>
    Public Class ForStatement : Inherits Statement

        Public Property VariableName As String
        Public Property Initializer As Expression
        ''' <summary>循环条件，形如 i &lt; n</summary>
        Public Property Condition As Expression
        ''' <summary>步进表达式，形如 i + 1（即 i = i + step）</summary>
        Public Property Increment As Expression
        Public Property Body As BlockStatement

        Public Sub New()
            MyBase.New(SyntaxKind.For)
            Me.Body = New BlockStatement()
        End Sub
    End Class

    Public Class BreakStatement : Inherits Statement
        Public Sub New()
            MyBase.New(SyntaxKind.Break)
        End Sub

        Public Overrides Function ToString() As String
            Return "Exit While"
        End Function
    End Class

    Public Class ContinueStatement : Inherits Statement
        Public Sub New()
            MyBase.New(SyntaxKind.Continue)
        End Sub

        Public Overrides Function ToString() As String
            Return "Continue While"
        End Function
    End Class

    ''' <summary>空语句（nop / 未归约出来的占位）</summary>
    Public Class NopStatement : Inherits Statement
        Public Sub New()
            MyBase.New(SyntaxKind.Nop)
        End Sub

        Public Overrides Function ToString() As String
            Return ""
        End Function
    End Class

    ''' <summary>一个方法形参的声明</summary>
    Public Class ParameterDeclaration

        Public Property Index As Integer
        Public Property Name As String
        Public Property ParameterType As System.Type

        Public Sub New()
        End Sub

        Public Sub New(index As Integer, name As String, parameterType As System.Type)
            Me.Index = index
            Me.Name = name
            Me.ParameterType = parameterType
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} As {If(ParameterType Is Nothing, "?", ParameterType.Name)}"
        End Function
    End Class

    ''' <summary>一个局部变量的声明（方法级符号表）</summary>
    Public Class LocalDeclaration

        Public Property Index As Integer
        Public Property Name As String
        Public Property LocalType As System.Type

        Public Sub New()
        End Sub

        Public Sub New(index As Integer, name As String, localType As System.Type)
            Me.Index = index
            Me.Name = name
            Me.LocalType = localType
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} As {If(LocalType Is Nothing, "?", LocalType.Name)}"
        End Function
    End Class

    ''' <summary>
    ''' 一个方法反编译之后的完整语法树。
    ''' </summary>
    Public Class MethodSyntax

        Public Property Name As String
        Public Property Method As MethodInfo
        Public Property ReturnType As System.Type
        Public Property IsStatic As Boolean
        Public Property Parameters As New List(Of ParameterDeclaration)
        Public Property Locals As New List(Of LocalDeclaration)
        ''' <summary>方法体（顶层语句块）</summary>
        Public Property Body As BlockStatement
        ''' <summary>反编译过程中的结构化诊断（警告不阻断，只记录）</summary>
        Public Property Diagnostics As DecompileDiagnostics

        Public Overrides Function ToString() As String
            Dim params = String.Join(", ", Parameters)
            Dim ret = If(ReturnType Is Nothing, "Void", ReturnType.Name)

            Return $"{(If(IsStatic, "Shared ", ""))}Function {Name}({params}) As {ret}"
        End Function
    End Class
End Namespace
