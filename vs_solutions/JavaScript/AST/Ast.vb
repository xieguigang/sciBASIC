#Region "Microsoft.VisualBasic::029dcc55497ab237b99eb807bf81e46b, vs_solutions\JavaScript\AST\Ast.vb"

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

    '   Total Lines: 807
    '    Code Lines: 533 (66.05%)
    ' Comment Lines: 107 (13.26%)
    '    - Xml Docs: 98.13%
    ' 
    '   Blank Lines: 167 (20.69%)
    '     File Size: 25.99 KB


    ' Class Expression
    ' 
    ' 
    ' 
    ' Class LiteralExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class IdentExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class UnaryExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class UpdateExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class BinaryExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class LogicalExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class AssignExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class DestructureAssignExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class CondExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class CallExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class MemberExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class FuncExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class AwaitExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class YieldExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class RegexExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ThisExpr
    ' 
    ' 
    ' 
    ' Class NewExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ArrayExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ObjectExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Enum ObjectPropertyKind
    ' 
    '     Getter, Normal, Setter, Shorthand, Spread
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class ObjectProperty
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class TemplateExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class SequenceExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class SpreadElement
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class OptionalChainExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class Statement
    ' 
    ' 
    ' 
    ' Class Program
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class BlockStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class VarDeclarant
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class VarStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class FuncDeclStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class IfStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ForStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ForInStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class WhileStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class DoWhileStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class SwitchStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class SwitchCase
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ForOfStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ClassDeclStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ClassExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ClassSpec
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Enum ClassMemberKind
    ' 
    '     Field, Getter, Method, Setter
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class ClassMember
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class SuperExpr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class LabeledStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class BreakStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ContinueStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ReturnStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ThrowStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class TryStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ExprStmt
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class EmptyStmt
    ' 
    ' 
    ' 
    ' Class BindingTarget
    ' 
    ' 
    ' 
    ' Class NameTarget
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class MemberTarget
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ArrayTarget
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class ObjectTarget
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class DefaultTarget
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' Abstract-syntax-tree node types for the supported JavaScript subset.
''' <para>Two hierarchies: <see cref="Expression"/> (values) and
''' <see cref="Statement"/> (effects). <see cref="Program"/> is the root.</para>
''' <para>Both the tree-walking <see cref="Interpreter"/> and the VB.NET
''' <see cref="CodeGenerator"/> operate on this object model.</para>
''' </summary>
Public MustInherit Class Expression

    ''' <summary>
    ''' Source location of this node, stamped by the parser
    ''' (<see cref="SourcePosition.None"/> when unknown). It is what makes a JS stack
    ''' frame point at the real source line of a call.
    ''' </summary>
    Public Property Position As SourcePosition

End Class

''' <summary>number / string / true / false / null / undefined literal.</summary>
Public NotInheritable Class LiteralExpr
    Inherits Expression

    ''' <summary>
    ''' JsValue tagged union: Double/Int32 number, String, Boolean,
    ''' <see cref="JsValue.Null"/> (= null) or <see cref="JsValue.Undef"/> (undefined).
    ''' Stored unboxed — no CLR primitive boxing on the interpreter hot path.
    ''' </summary>
    Public ReadOnly Value As JsValue

    Public Sub New(value As JsValue)
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

''' <summary>Destructuring assignment: <c>[a, b] = expr</c> or <c>({a} = expr)</c>.</summary>
Public NotInheritable Class DestructureAssignExpr
    Inherits Expression

    Public ReadOnly Target As BindingTarget
    Public ReadOnly Value As Expression

    Public Sub New(target As BindingTarget, value As Expression)
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
    ''' <summary>Optional call <c>a?.(args)</c> — short-circuits when the callee is nullish.</summary>
    Public ReadOnly IsOptional As Boolean

    Public Sub New(callee As Expression, args As List(Of Expression), Optional isOptional As Boolean = False)
        Me.Callee = callee
        Me.Arguments = args
        Me.IsOptional = isOptional
    End Sub
End Class

''' <summary>Property access obj.name (computed = false) or obj[expr] (true).</summary>
Public NotInheritable Class MemberExpr
    Inherits Expression

    Public ReadOnly Obj As Expression
    ''' <summary>Member name; expression when <see cref="Computed"/> is true.</summary>
    Public ReadOnly Name As Object
    Public ReadOnly Computed As Boolean
    ''' <summary>Optional access <c>obj?.name</c> — short-circuits when obj is nullish.</summary>
    Public ReadOnly IsOptional As Boolean

    Public Sub New(obj As Expression, name As Object, computed As Boolean, Optional isOptional As Boolean = False)
        Me.Obj = obj
        Me.Name = name
        Me.Computed = computed
        Me.IsOptional = isOptional
    End Sub
End Class

''' <summary>Function expression / declaration body; also used for arrow functions.</summary>
Public NotInheritable Class FuncExpr
    Inherits Expression

    ''' <summary>Function name, or Nothing for anonymous/arrow functions.</summary>
    Public ReadOnly Name As String
    Public ReadOnly Parameters As String()
    ''' <summary>Per-parameter default expressions, aligned with <see cref="Parameters"/>;
    ''' Nothing entry = no default.</summary>
    Public ReadOnly Defaults As Expression()
    ''' <summary>Rest parameter name (<c>...args</c>), or Nothing.</summary>
    Public ReadOnly RestParam As String
    ''' <summary>
    ''' Arrow function? Arrow functions have no own <c>this</c>/<c>arguments</c>
    ''' bindings — both resolve lexically through the enclosing scope.
    ''' </summary>
    Public ReadOnly IsArrow As Boolean
    ''' <summary>async function — calling it yields a Promise.</summary>
    Public ReadOnly IsAsync As Boolean
    ''' <summary>generator function (function*) — calling it yields an iterator.</summary>
    Public ReadOnly IsGenerator As Boolean
    Public ReadOnly Body As List(Of Statement)

    ''' <summary>
    ''' Name inferred from the syntactic position when the function itself is anonymous:
    ''' <c>{ m() {} }</c> → <c>m</c>, <c>var f = function () {}</c> → <c>f</c>,
    ''' <c>class C { m() {} }</c> → <c>m</c>. It mirrors the name inference of V8 and is
    ''' used for JS stack frame names (<c>at m (file:line:col)</c>); it never affects the
    ''' script visible <c>name</c> property nor the generated code.
    ''' </summary>
    Public Property InferredName As String

    Public Sub New(name As String, parameters As String(), body As List(Of Statement),
                   Optional isArrow As Boolean = False,
                   Optional defaults As Expression() = Nothing,
                   Optional restParam As String = Nothing,
                   Optional isAsync As Boolean = False,
                   Optional isGenerator As Boolean = False)
        Me.Name = name
        Me.Parameters = parameters
        Me.Defaults = If(defaults, New Expression(System.Math.Max(0, parameters.Length - 1)) {})
        Me.RestParam = restParam
        Me.IsArrow = isArrow
        Me.IsAsync = isAsync
        Me.IsGenerator = isGenerator
        Me.Body = body
    End Sub
End Class

''' <summary>await expr — unwrap a promise (drains the microtask queue).</summary>
Public NotInheritable Class AwaitExpr
    Inherits Expression

    Public ReadOnly Operand As Expression

    Public Sub New(operand As Expression)
        Me.Operand = operand
    End Sub
End Class

''' <summary>yield expr / yield — record a generator value (eager collection).</summary>
Public NotInheritable Class YieldExpr
    Inherits Expression

    ''' <summary>May be Nothing for a bare <c>yield</c>.</summary>
    Public ReadOnly Operand As Expression

    Public Sub New(operand As Expression)
        Me.Operand = operand
    End Sub
End Class

''' <summary>Regular-expression literal: each evaluation yields a fresh RegExp object.</summary>
Public NotInheritable Class RegexExpr
    Inherits Expression

    Public ReadOnly Source As String
    Public ReadOnly Flags As String

    Public Sub New(source As String, flags As String)
        Me.Source = source
        Me.Flags = flags
    End Sub
End Class

''' <summary><c>this</c> — bound by method calls / construction (tier 2).</summary>
Public NotInheritable Class ThisExpr
    Inherits Expression
End Class

''' <summary><c>new Ctor(args...)</c> — construction (tier 2).</summary>
Public NotInheritable Class NewExpr
    Inherits Expression

    Public ReadOnly Callee As Expression
    Public ReadOnly Arguments As List(Of Expression)

    Public Sub New(callee As Expression, args As List(Of Expression))
        Me.Callee = callee
        Me.Arguments = args
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

    Public ReadOnly Properties As List(Of ObjectProperty)

    Public Sub New(properties As List(Of ObjectProperty))
        Me.Properties = properties
    End Sub
End Class

''' <summary>Property flavours of an object literal.</summary>
Public Enum ObjectPropertyKind
    ''' <summary>key: value (also covers methods — the value is a FuncExpr).</summary>
    Normal
    ''' <summary>Shorthand { a } or { a = default }.</summary>
    Shorthand
    Getter
    Setter
    ''' <summary>{...expr} object spread.</summary>
    Spread
End Enum

''' <summary>
''' One property of an object literal. <see cref="Key"/> is Nothing for spread
''' and computed keys; <see cref="KeyExpr"/> is Nothing unless the key is computed;
''' <see cref="Value"/> is Nothing only for plain shorthand.
''' </summary>
Public NotInheritable Class ObjectProperty

    Public ReadOnly Key As String
    Public ReadOnly KeyExpr As Expression
    Public ReadOnly Value As Expression
    Public ReadOnly Kind As ObjectPropertyKind

    Public Sub New(key As String, keyExpr As Expression, value As Expression, kind As ObjectPropertyKind)
        Me.Key = key
        Me.KeyExpr = keyExpr
        Me.Value = value
        Me.Kind = kind
    End Sub
End Class

''' <summary>Template literal `` `a${x}b` `` — alternating cooked text and expressions.</summary>
Public NotInheritable Class TemplateExpr
    Inherits Expression

    ''' <summary>Chunks in source order: <see cref="String"/> = literal text,
    ''' <see cref="Expression"/> = an embedded placeholder value.</summary>
    Public ReadOnly Chunks As List(Of Object)

    Public Sub New(chunks As List(Of Object))
        Me.Chunks = chunks
    End Sub
End Class

''' <summary>Comma operator <c>a, b</c> — evaluate all operands, return the last.</summary>
Public NotInheritable Class SequenceExpr
    Inherits Expression

    Public ReadOnly Expressions As List(Of Expression)

    Public Sub New(expressions As List(Of Expression))
        Me.Expressions = expressions
    End Sub
End Class

''' <summary>Spread element <c>...expr</c> (call arguments, array/object literals).</summary>
Public NotInheritable Class SpreadElement
    Inherits Expression

    Public ReadOnly Argument As Expression

    Public Sub New(argument As Expression)
        Me.Argument = argument
    End Sub
End Class

''' <summary>
''' Wrapper marking the ROOT of a member/call chain that contains at least one
''' optional link (<c>?.</c>). Evaluation short-circuits the whole chain to
''' undefined when a nullish base is hit (via <see cref="Runtime.OptionalChainShortCircuit"/>).
''' </summary>
Public NotInheritable Class OptionalChainExpr
    Inherits Expression

    Public ReadOnly Chain As Expression

    Public Sub New(chain As Expression)
        Me.Chain = chain
    End Sub
End Class

' ============================ statements ============================

Public MustInherit Class Statement

    ''' <summary>
    ''' Source location of this statement, stamped by the parser; the interpreter copies
    ''' it onto the innermost stack frame while the statement executes.
    ''' </summary>
    Public Property Position As SourcePosition

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
    ''' <summary>Binding target: <see cref="String"/> name or a <see cref="BindingTarget"/> pattern.</summary>
    Public ReadOnly Target As Object
    ''' <summary>Initialiser expression, or Nothing for "var x;".</summary>
    Public ReadOnly Init As Expression

    Public Sub New(target As Object, init As Expression)
        Me.Target = target
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
    ''' <summary>Per-parameter default expressions, aligned with <see cref="Parameters"/>.</summary>
    Public ReadOnly Defaults As Expression()
    ''' <summary>Rest parameter name (<c>...args</c>), or Nothing.</summary>
    Public ReadOnly RestParam As String
    ''' <summary>async function — calling it yields a Promise.</summary>
    Public ReadOnly IsAsync As Boolean
    ''' <summary>generator function (function*) — calling it yields an iterator.</summary>
    Public ReadOnly IsGenerator As Boolean
    Public ReadOnly Body As List(Of Statement)

    Public Sub New(name As String, parameters As String(), body As List(Of Statement),
                   Optional defaults As Expression() = Nothing, Optional restParam As String = Nothing,
                   Optional isAsync As Boolean = False, Optional isGenerator As Boolean = False)
        Me.Name = name
        Me.Parameters = parameters
        Me.Defaults = If(defaults, New Expression(System.Math.Max(0, parameters.Length - 1)) {})
        Me.RestParam = restParam
        Me.IsAsync = isAsync
        Me.IsGenerator = isGenerator
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

''' <summary>switch (disc) { case x: ... default: ... } — with fall-through.</summary>
Public NotInheritable Class SwitchStmt
    Inherits Statement

    Public ReadOnly Discriminant As Expression
    Public ReadOnly Cases As List(Of SwitchCase)

    Public Sub New(discriminant As Expression, cases As List(Of SwitchCase))
        Me.Discriminant = discriminant
        Me.Cases = cases
    End Sub
End Class

''' <summary>One case arm; <see cref="Test"/> = Nothing marks the default arm.</summary>
Public NotInheritable Class SwitchCase

    Public ReadOnly Test As Expression
    Public ReadOnly Body As List(Of Statement)

    Public Sub New(test As Expression, body As List(Of Statement))
        Me.Test = test
        Me.Body = body
    End Sub
End Class

''' <summary>for (const x of iterable) body</summary>
Public NotInheritable Class ForOfStmt
    Inherits Statement

    ''' <summary>"var" / "let" / "const", or Nothing when assigning to an existing target.</summary>
    Public ReadOnly Kind As String
    ''' <summary><see cref="String"/> name or a <see cref="BindingTarget"/> pattern.</summary>
    Public ReadOnly Target As Object
    Public ReadOnly Iterable As Expression
    Public ReadOnly Body As Statement

    Public Sub New(kind As String, target As Object, iterable As Expression, body As Statement)
        Me.Kind = kind
        Me.Target = target
        Me.Iterable = iterable
        Me.Body = body
    End Sub
End Class

''' <summary>class Foo extends Bar { ... } — a class definition statement (not hoisted).</summary>
Public NotInheritable Class ClassDeclStmt
    Inherits Statement

    Public ReadOnly Spec As ClassSpec

    Public Sub New(spec As ClassSpec)
        Me.Spec = spec
    End Sub
End Class

''' <summary>Class expression: <c>var C = class { ... }</c>.</summary>
Public NotInheritable Class ClassExpr
    Inherits Expression

    Public ReadOnly Spec As ClassSpec

    Public Sub New(spec As ClassSpec)
        Me.Spec = spec
    End Sub
End Class

''' <summary>Structural description shared by class declarations and expressions.</summary>
Public NotInheritable Class ClassSpec

    Public ReadOnly Name As String
    ''' <summary>Superclass expression, or Nothing for a base class.</summary>
    Public ReadOnly SuperClass As Expression
    ''' <summary>Constructor body, or Nothing (implicit empty constructor).</summary>
    Public ReadOnly Constructor As FuncExpr
    Public ReadOnly Members As List(Of ClassMember)

    Public Sub New(name As String, superClass As Expression, ctor As FuncExpr, members As List(Of ClassMember))
        Me.Name = name
        Me.SuperClass = superClass
        Me.Constructor = ctor
        Me.Members = members
    End Sub
End Class

Public Enum ClassMemberKind
    ''' <summary>Regular method (instance unless <see cref="ClassMember.IsStatic"/>).</summary>
    Method
    Getter
    Setter
    ''' <summary>Field with an initialiser expression (instance or static).</summary>
    Field
End Enum

Public NotInheritable Class ClassMember

    Public ReadOnly Kind As ClassMemberKind
    Public ReadOnly Name As String
    ''' <summary>FuncExpr for methods/accessors; initialiser expression for fields.</summary>
    Public ReadOnly Value As Expression
    Public ReadOnly IsStatic As Boolean

    Public Sub New(kind As ClassMemberKind, name As String, value As Expression, isStatic As Boolean)
        Me.Kind = kind
        Me.Name = name
        Me.Value = value
        Me.IsStatic = isStatic
    End Sub
End Class

''' <summary>super / super.member inside a derived class.</summary>
Public NotInheritable Class SuperExpr
    Inherits Expression

    ''' <summary>Member name, or Nothing for a bare <c>super(...)</c> constructor call.</summary>
    Public ReadOnly Member As String

    Public Sub New(member As String)
        Me.Member = member
    End Sub
End Class

''' <summary>label: statement — target of labeled break/continue.</summary>
Public NotInheritable Class LabeledStmt
    Inherits Statement

    Public ReadOnly Label As String
    Public ReadOnly Body As Statement

    Public Sub New(label As String, body As Statement)
        Me.Label = label
        Me.Body = body
    End Sub
End Class

Public NotInheritable Class BreakStmt
    Inherits Statement

    ''' <summary>Target label, or Nothing for the innermost loop/switch.</summary>
    Public ReadOnly Label As String

    Public Sub New(Optional label As String = Nothing)
        Me.Label = label
    End Sub
End Class

Public NotInheritable Class ContinueStmt
    Inherits Statement

    ''' <summary>Target label, or Nothing for the innermost loop.</summary>
    Public ReadOnly Label As String

    Public Sub New(Optional label As String = Nothing)
        Me.Label = label
    End Sub
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

' ============================ destructuring patterns ============================

''' <summary>
''' Destructuring binding target — shared by var/let/const declarations,
''' assignment destructuring and for-of loop targets.
''' </summary>
Public MustInherit Class BindingTarget
End Class

''' <summary>Plain variable name.</summary>
Public NotInheritable Class NameTarget
    Inherits BindingTarget

    Public ReadOnly Name As String

    Public Sub New(name As String)
        Me.Name = name
    End Sub
End Class

''' <summary>Member/index assignment target: <c>obj.prop</c> or <c>obj[key]</c>.</summary>
Public NotInheritable Class MemberTarget
    Inherits BindingTarget

    Public ReadOnly Obj As Expression
    ''' <summary>Member name (String) or key expression.</summary>
    Public ReadOnly Name As Object
    Public ReadOnly Computed As Boolean

    Public Sub New(obj As Expression, name As Object, computed As Boolean)
        Me.Obj = obj
        Me.Name = name
        Me.Computed = computed
    End Sub
End Class

''' <summary>Array pattern <c>[a, , b, ...rest]</c>; Nothing element = hole.</summary>
Public NotInheritable Class ArrayTarget
    Inherits BindingTarget

    Public ReadOnly Elements As List(Of BindingTarget)
    Public ReadOnly Rest As BindingTarget

    Public Sub New(elements As List(Of BindingTarget), rest As BindingTarget)
        Me.Elements = elements
        Me.Rest = rest
    End Sub
End Class

''' <summary>Object pattern <c>{ a, b: c, ...rest }</c>.</summary>
Public NotInheritable Class ObjectTarget
    Inherits BindingTarget

    Public ReadOnly Properties As List(Of KeyValuePair(Of String, BindingTarget))
    Public ReadOnly Rest As BindingTarget

    Public Sub New(properties As List(Of KeyValuePair(Of String, BindingTarget)), rest As BindingTarget)
        Me.Properties = properties
        Me.Rest = rest
    End Sub
End Class

''' <summary>Target with a default value: <c>a = 1</c> inside a pattern.</summary>
Public NotInheritable Class DefaultTarget
    Inherits BindingTarget

    Public ReadOnly Target As BindingTarget
    Public ReadOnly [Default] As Expression

    Public Sub New(target As BindingTarget, [default] As Expression)
        Me.Target = target
        Me.[Default] = [default]
    End Sub
End Class
