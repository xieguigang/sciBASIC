Option Strict On
Option Explicit On

''' <summary>
''' Abstract-syntax-tree node types for the supported JavaScript subset.
''' <para>Two hierarchies: <see cref="Expression"/> (values) and
''' <see cref="Statement"/> (effects). <see cref="Program"/> is the root.</para>
''' <para>Both the tree-walking <see cref="Interpreter"/> and the VB.NET
''' <see cref="CodeGenerator"/> operate on this object model.</para>
''' </summary>
Public MustInherit Class Expression
End Class

''' <summary>number / string / true / false / null / undefined literal.</summary>
Public NotInheritable Class LiteralExpr
    Inherits Expression

    ''' <summary>Double, String, Boolean, Nothing (= null) or JsRuntime undef marker.</summary>
    Public ReadOnly Value As Object

    Public Sub New(value As Object)
        Me.Value = value
    End Sub
End Class

Public NotInheritable Class IdentExpr
    Inherits Expression

    Public ReadOnly Name As String

    Public Sub New(name As String)
        Me.Name = name
    End Sub
End Class

''' <summary>Unary prefix operator: ! - + typeof (op text stored as-is).</summary>
Public NotInheritable Class UnaryExpr
    Inherits Expression

    Public ReadOnly Op As String
    Public ReadOnly Operand As Expression

    Public Sub New(op As String, operand As Expression)
        Me.Op = op
        Me.Operand = operand
    End Sub
End Class

''' <summary>Prefix/suffix ++/--.</summary>
Public NotInheritable Class UpdateExpr
    Inherits Expression

    Public ReadOnly Op As String       ' "++" or "--"
    Public ReadOnly Target As Expression
    Public ReadOnly IsPrefix As Boolean

    Public Sub New(op As String, target As Expression, isPrefix As Boolean)
        Me.Op = op
        Me.Target = target
        Me.IsPrefix = isPrefix
    End Sub
End Class

Public NotInheritable Class BinaryExpr
    Inherits Expression

    Public ReadOnly Op As String       ' + - * / % ** < <= > >= == != === !==
    Public ReadOnly Left As Expression
    Public ReadOnly Right As Expression

    Public Sub New(op As String, left As Expression, right As Expression)
        Me.Op = op
        Me.Left = left
        Me.Right = right
    End Sub
End Class

''' <summary>&amp;&amp; / || (short-circuit; result is one of the operands).</summary>
Public NotInheritable Class LogicalExpr
    Inherits Expression

    Public ReadOnly Op As String       ' "&&" or "||"
    Public ReadOnly Left As Expression
    Public ReadOnly Right As Expression

    Public Sub New(op As String, left As Expression, right As Expression)
        Me.Op = op
        Me.Left = left
        Me.Right = right
    End Sub
End Class

''' <summary>Assignment: = += -= *= /= %=. Target is Ident / Member / Index.</summary>
Public NotInheritable Class AssignExpr
    Inherits Expression

    Public ReadOnly Op As String
    Public ReadOnly Target As Expression
    Public ReadOnly Value As Expression

    Public Sub New(op As String, target As Expression, value As Expression)
        Me.Op = op
        Me.Target = target
        Me.Value = value
    End Sub
End Class

''' <summary>Conditional (ternary) expression.</summary>
Public NotInheritable Class CondExpr
    Inherits Expression

    Public ReadOnly Test As Expression
    Public ReadOnly Consequent As Expression
    Public ReadOnly Alternate As Expression

    Public Sub New(test As Expression, consequent As Expression, alternate As Expression)
        Me.Test = test
        Me.Consequent = consequent
        Me.Alternate = alternate
    End Sub
End Class

''' <summary>Call expression: callee(args...).</summary>
Public NotInheritable Class CallExpr
    Inherits Expression

    Public ReadOnly Callee As Expression
    Public ReadOnly Arguments As List(Of Expression)

    Public Sub New(callee As Expression, args As List(Of Expression))
        Me.Callee = callee
        Me.Arguments = args
    End Sub
End Class

''' <summary>Property access obj.name (computed = false) or obj[expr] (true).</summary>
Public NotInheritable Class MemberExpr
    Inherits Expression

    Public ReadOnly Obj As Expression
    ''' <summary>Member name; expression when <see cref="Computed"/> is true.</summary>
    Public ReadOnly Name As Object
    Public ReadOnly Computed As Boolean

    Public Sub New(obj As Expression, name As Object, computed As Boolean)
        Me.Obj = obj
        Me.Name = name
        Me.Computed = computed
    End Sub
End Class

''' <summary>Function expression / declaration body; also used for arrow functions.</summary>
Public NotInheritable Class FuncExpr
    Inherits Expression

    ''' <summary>Function name, or Nothing for anonymous/arrow functions.</summary>
    Public ReadOnly Name As String
    Public ReadOnly Parameters As String()
    Public ReadOnly Body As List(Of Statement)

    Public Sub New(name As String, parameters As String(), body As List(Of Statement))
        Me.Name = name
        Me.Parameters = parameters
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class ArrayExpr
    Inherits Expression

    Public ReadOnly Elements As List(Of Expression)

    Public Sub New(elements As List(Of Expression))
        Me.Elements = elements
    End Sub
End Class

''' <summary>Object literal { key: value, ... } — keys are strings.</summary>
Public NotInheritable Class ObjectExpr
    Inherits Expression

    Public ReadOnly Properties As List(Of KeyValuePair(Of String, Expression))

    Public Sub New(properties As List(Of KeyValuePair(Of String, Expression)))
        Me.Properties = properties
    End Sub
End Class

' ============================ statements ============================

Public MustInherit Class Statement
End Class

''' <summary>Root node: top-level statement list.</summary>
Public NotInheritable Class Program
    Public ReadOnly Body As List(Of Statement)

    Public Sub New(body As List(Of Statement))
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class BlockStmt
    Inherits Statement

    Public ReadOnly Body As List(Of Statement)

    Public Sub New(body As List(Of Statement))
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class VarDeclarant
    Public ReadOnly Name As String
    ''' <summary>Initialiser expression, or Nothing for "var x;".</summary>
    Public ReadOnly Init As Expression

    Public Sub New(name As String, init As Expression)
        Me.Name = name
        Me.Init = init
    End Sub
End Class

''' <summary>var / let / const declaration with one or more declarants.</summary>
Public NotInheritable Class VarStmt
    Inherits Statement

    Public ReadOnly Kind As String         ' "var" | "let" | "const"
    Public ReadOnly Declarations As List(Of VarDeclarant)

    Public Sub New(kind As String, declarations As List(Of VarDeclarant))
        Me.Kind = kind
        Me.Declarations = declarations
    End Sub
End Class

Public NotInheritable Class FuncDeclStmt
    Inherits Statement

    Public ReadOnly Name As String
    Public ReadOnly Parameters As String()
    Public ReadOnly Body As List(Of Statement)

    Public Sub New(name As String, parameters As String(), body As List(Of Statement))
        Me.Name = name
        Me.Parameters = parameters
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class IfStmt
    Inherits Statement

    Public ReadOnly Test As Expression
    Public ReadOnly ThenBranch As Statement
    ''' <summary>May be Nothing.</summary>
    Public ReadOnly ElseBranch As Statement

    Public Sub New(test As Expression, thenBranch As Statement, elseBranch As Statement)
        Me.Test = test
        Me.ThenBranch = thenBranch
        Me.ElseBranch = elseBranch
    End Sub
End Class

Public NotInheritable Class ForStmt
    Inherits Statement

    ''' <summary>VarStmt or expression, or Nothing.</summary>
    Public ReadOnly Init As Object
    ''' <summary>May be Nothing (infinite).</summary>
    Public ReadOnly Test As Expression
    ''' <summary>May be Nothing.</summary>
    Public ReadOnly Update As Expression
    Public ReadOnly Body As Statement

    Public Sub New(init As Object, test As Expression, update As Expression, body As Statement)
        Me.Init = init
        Me.Test = test
        Me.Update = update
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class ForInStmt
    Inherits Statement

    ''' <summary>Declared loop variable name.</summary>
    Public ReadOnly VarName As String
    Public ReadOnly ObjectExpr As Expression
    Public ReadOnly Body As Statement

    Public Sub New(varName As String, objectExpr As Expression, body As Statement)
        Me.VarName = varName
        Me.ObjectExpr = objectExpr
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class WhileStmt
    Inherits Statement

    Public ReadOnly Test As Expression
    Public ReadOnly Body As Statement

    Public Sub New(test As Expression, body As Statement)
        Me.Test = test
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class DoWhileStmt
    Inherits Statement

    Public ReadOnly Body As Statement
    Public ReadOnly Test As Expression

    Public Sub New(body As Statement, test As Expression)
        Me.Body = body
        Me.Test = test
    End Sub
End Class

Public NotInheritable Class BreakStmt
    Inherits Statement
End Class

Public NotInheritable Class ContinueStmt
    Inherits Statement
End Class

Public NotInheritable Class ReturnStmt
    Inherits Statement

    ''' <summary>May be Nothing (plain "return;").</summary>
    Public ReadOnly Value As Expression

    Public Sub New(value As Expression)
        Me.Value = value
    End Sub
End Class

Public NotInheritable Class ThrowStmt
    Inherits Statement

    Public ReadOnly Value As Expression

    Public Sub New(value As Expression)
        Me.Value = value
    End Sub
End Class

Public NotInheritable Class TryStmt
    Inherits Statement

    Public ReadOnly TryBlock As List(Of Statement)
    ''' <summary>May be Nothing (try/finally).</summary>
    Public ReadOnly CatchParam As String
    Public ReadOnly CatchBlock As List(Of Statement)
    ''' <summary>May be Nothing.</summary>
    Public ReadOnly FinallyBlock As List(Of Statement)

    Public Sub New(tryBlock As List(Of Statement), catchParam As String,
                   catchBlock As List(Of Statement), finallyBlock As List(Of Statement))
        Me.TryBlock = tryBlock
        Me.CatchParam = catchParam
        Me.CatchBlock = catchBlock
        Me.FinallyBlock = finallyBlock
    End Sub
End Class

Public NotInheritable Class ExprStmt
    Inherits Statement

    Public ReadOnly Expr As Expression

    Public Sub New(expr As Expression)
        Me.Expr = expr
    End Sub
End Class

Public NotInheritable Class EmptyStmt
    Inherits Statement
End Class
