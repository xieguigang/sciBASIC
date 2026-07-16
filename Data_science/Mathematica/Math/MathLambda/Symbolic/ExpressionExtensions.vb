' /********************************************************************************/
'
'     Module ExpressionExtensions
' 
'         The shared infrastructure for the symbolic computation engine:
'         a recursive expression rewriter, deep clone, structural equality,
'         variable collection, constant detection and node builders.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Symbolic

    ''' <summary>
    ''' Shared helpers that operate on the immutable <see cref="Expression"/> tree.
    ''' All transformations are functional: they return a new node and never mutate
    ''' the input tree.
    ''' </summary>
    Public Module ExpressionExtensions

        ' -----------------------------------------------------------------------
        ' Deep clone
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' Make a deep copy of the given expression node.
        ''' </summary>
        Public Function Clone(expr As Expression) As Expression
            If expr Is Nothing Then
                Return Nothing
            ElseIf TypeOf expr Is Literal Then
                Return New Literal(DirectCast(expr, Literal).number)
            ElseIf TypeOf expr Is SymbolExpression Then
                Return New SymbolExpression(DirectCast(expr, SymbolExpression).symbolName)
            ElseIf TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                Return New BinaryExpression(Clone(b.left), Clone(b.right), b.operator)
            ElseIf TypeOf expr Is FunctionInvoke Then
                Dim f = DirectCast(expr, FunctionInvoke)
                Return New FunctionInvoke(f.funcName, f.parameters.Select(AddressOf Clone).ToArray)
            ElseIf TypeOf expr Is UnaryExpression Then
                Dim u = DirectCast(expr, UnaryExpression)
                Return New UnaryExpression With {.[operator] = u.operator, .value = Clone(u.value)}
            ElseIf TypeOf expr Is UnaryNot Then
                Dim n = DirectCast(expr, UnaryNot)
                Return New UnaryNot With {.value = Clone(n.value)}
            ElseIf TypeOf expr Is LogicalLiteral Then
                Return New LogicalLiteral(DirectCast(expr, LogicalLiteral).logical.ToString)
            ElseIf TypeOf expr Is Factorial Then
                Return New Factorial(Clone(DirectCast(expr, Factorial).factor).ToString)
            Else
                Throw New NotSupportedException($"Unsupported expression type: {expr.GetType.Name}")
            End If
        End Function

        ' -----------------------------------------------------------------------
        ' Structural equality
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' Compare two expression trees structurally (ignoring parentheses which are
        ''' not represented in the tree).
        ''' </summary>
        Public Function ExprEquals(a As Expression, b As Expression) As Boolean
            If a Is b Then
                Return True
            ElseIf a Is Nothing OrElse b Is Nothing Then
                Return False
            ElseIf TypeOf a Is Literal AndAlso TypeOf b Is Literal Then
                Return DirectCast(a, Literal).number = DirectCast(b, Literal).number
            ElseIf TypeOf a Is SymbolExpression AndAlso TypeOf b Is SymbolExpression Then
                Return DirectCast(a, SymbolExpression).symbolName = DirectCast(b, SymbolExpression).symbolName
            ElseIf TypeOf a Is BinaryExpression AndAlso TypeOf b Is BinaryExpression Then
                Dim x = DirectCast(a, BinaryExpression), y = DirectCast(b, BinaryExpression)
                Return x.operator = y.operator AndAlso ExprEquals(x.left, y.left) AndAlso ExprEquals(x.right, y.right)
            ElseIf TypeOf a Is FunctionInvoke AndAlso TypeOf b Is FunctionInvoke Then
                Dim x = DirectCast(a, FunctionInvoke), y = DirectCast(b, FunctionInvoke)
                If x.funcName <> y.funcName OrElse x.parameters.Length <> y.parameters.Length Then
                    Return False
                End If
                For i As Integer = 0 To x.parameters.Length - 1
                    If Not ExprEquals(x.parameters(i), y.parameters(i)) Then
                        Return False
                    End If
                Next
                Return True
            ElseIf TypeOf a Is UnaryExpression AndAlso TypeOf b Is UnaryExpression Then
                Dim x = DirectCast(a, UnaryExpression), y = DirectCast(b, UnaryExpression)
                Return x.operator = y.operator AndAlso ExprEquals(x.value, y.value)
            ElseIf TypeOf a Is UnaryNot AndAlso TypeOf b Is UnaryNot Then
                Return ExprEquals(DirectCast(a, UnaryNot).value, DirectCast(b, UnaryNot).value)
            ElseIf TypeOf a Is LogicalLiteral AndAlso TypeOf b Is LogicalLiteral Then
                Return DirectCast(a, LogicalLiteral).logical = DirectCast(b, LogicalLiteral).logical
            ElseIf TypeOf a Is Factorial AndAlso TypeOf b Is Factorial Then
                Return ExprEquals(DirectCast(a, Factorial).factor, DirectCast(b, Factorial).factor)
            Else
                Return False
            End If
        End Function

        ' -----------------------------------------------------------------------
        ' Variable collection / constant detection
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' All distinct variable symbol names that appear in the expression.
        ''' </summary>
        <Extension>
        Public Function GetSymbols(expr As Expression) As String()
            Dim list As New List(Of String)

            If expr Is Nothing Then
                Return list.ToArray
            End If

            For Each s In expr.GetVariableSymbols
                If Not list.Contains(s) Then
                    list.Add(s)
                End If
            Next

            Return list.ToArray
        End Function

        ''' <summary>
        ''' Test if the expression contains no variable symbols (i.e. it is a pure
        ''' constant that can be evaluated numerically).
        ''' </summary>
        <Extension>
        Public Function IsConstant(expr As Expression) As Boolean
            If expr Is Nothing Then
                Return True
            End If
            Return expr.GetVariableSymbols.ToArray.Length = 0
        End Function

        ''' <summary>
        ''' Test if the expression still references the given variable symbol.
        ''' </summary>
        <Extension>
        Public Function DependsOn(expr As Expression, var$) As Boolean
            Return Array.IndexOf(GetSymbols(expr), var) >= 0
        End Function

        ' -----------------------------------------------------------------------
        ' Node builders
        ' -----------------------------------------------------------------------

        Public Function MakeLiteral(x As Double) As Literal
            Return New Literal(x)
        End Function

        Public Function Add(a As Expression, b As Expression) As Expression
            Return New BinaryExpression(a, b, "+")
        End Function

        Public Function Subt(a As Expression, b As Expression) As Expression
            Return New BinaryExpression(a, b, "-")
        End Function

        Public Function Mul(a As Expression, b As Expression) As Expression
            Return New BinaryExpression(a, b, "*")
        End Function

        Public Function Div(a As Expression, b As Expression) As Expression
            Return New BinaryExpression(a, b, "/")
        End Function

        Public Function Pow(a As Expression, b As Expression) As Expression
            Return New BinaryExpression(a, b, "^")
        End Function

        Public Function Negate(a As Expression) As Expression
            Return New UnaryExpression With {.[operator] = "-", .value = a}
        End Function

        Public Function Reciprocal(a As Expression) As Expression
            Return Div(MakeLiteral(1), a)
        End Function

        ' -----------------------------------------------------------------------
        ' Sum / product flattening
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' Flatten nested additions into a list of summands. A subtraction
        ''' ``a - b`` is represented as ``a + (-b)``.
        ''' </summary>
        Public Function FlattenSum(expr As Expression) As List(Of Expression)
            Dim terms As New List(Of Expression)
            flattenSum(expr, terms)
            Return terms
        End Function

        Private Sub flattenSum(expr As Expression, terms As List(Of Expression))
            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                If b.operator = "+"c Then
                    flattenSum(b.left, terms)
                    flattenSum(b.right, terms)
                    Return
                ElseIf b.operator = "-"c Then
                    flattenSum(b.left, terms)
                    terms.Add(Negate(b.right))
                    Return
                End If
            End If
            terms.Add(expr)
        End Sub

        ''' <summary>
        ''' Flatten nested multiplications into a list of factors. A division
        ''' ``a / b`` is represented as ``a * (1/b)``.
        ''' </summary>
        Public Function FlattenProduct(expr As Expression) As List(Of Expression)
            Dim factors As New List(Of Expression)
            flattenProduct(expr, factors)
            Return factors
        End Function

        Private Sub flattenProduct(expr As Expression, factors As List(Of Expression))
            If TypeOf expr Is BinaryExpression Then
                Dim b = DirectCast(expr, BinaryExpression)
                If b.operator = "*"c Then
                    flattenProduct(b.left, factors)
                    flattenProduct(b.right, factors)
                    Return
                ElseIf b.operator = "/"c Then
                    flattenProduct(b.left, factors)
                    factors.Add(Reciprocal(b.right))
                    Return
                End If
            End If
            factors.Add(expr)
        End Sub

        ' -----------------------------------------------------------------------
        ' Coefficient extraction
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' Split a term into a numeric coefficient and the remaining body, such that
        ''' ``term = coeff * body``. Only a leading numeric factor is recognised:
        ''' ``2*x^2`` yields coeff=2, body=x^2; ``x*2`` yields coeff=2, body=x.
        ''' </summary>
        Public Sub SplitCoefficient(term As Expression, ByRef coeff As Double, ByRef body As Expression)
            If TypeOf term Is Literal Then
                coeff = DirectCast(term, Literal).number
                body = MakeLiteral(1)
            ElseIf TypeOf term Is BinaryExpression Then
                Dim b = DirectCast(term, BinaryExpression)
                If b.operator = "*"c Then
                    If TypeOf b.left Is Literal Then
                        coeff = DirectCast(b.left, Literal).number
                        body = b.right
                    ElseIf TypeOf b.right Is Literal Then
                        coeff = DirectCast(b.right, Literal).number
                        body = b.left
                    Else
                        coeff = 1.0
                        body = term
                    End If
                Else
                    coeff = 1.0
                    body = term
                End If
            Else
                coeff = 1.0
                body = term
            End If
        End Sub

        ''' <summary>
        ''' If the expression is a literal number, returns its value; otherwise
        ''' returns <see langword="Nothing"/>.
        ''' </summary>
        Public Function NumericValue(expr As Expression) As Double?
            If TypeOf expr Is Literal Then
                Return DirectCast(expr, Literal).number
            End If
            Return Nothing
        End Function

        ' -----------------------------------------------------------------------
        ' Recursive rewriter base class
        ' -----------------------------------------------------------------------

        ''' <summary>
        ''' A reusable recursive visitor that rewrites an expression tree. Override
        ''' the typed ``Rewrite*`` hooks; the base implementation returns the node
        ''' unchanged (after recursively rewriting children).
        ''' </summary>
        Public MustInherit Class ExpressionRewriter

            Public Function Rewrite(expr As Expression) As Expression
                If expr Is Nothing Then
                    Return Nothing
                ElseIf TypeOf expr Is Literal Then
                    Return RewriteLiteral(DirectCast(expr, Literal))
                ElseIf TypeOf expr Is SymbolExpression Then
                    Return RewriteSymbol(DirectCast(expr, SymbolExpression))
                ElseIf TypeOf expr Is BinaryExpression Then
                    Dim b = DirectCast(expr, BinaryExpression)
                    Return RewriteBinary(New BinaryExpression(Rewrite(b.left), Rewrite(b.right), b.operator))
                ElseIf TypeOf expr Is FunctionInvoke Then
                    Dim f = DirectCast(expr, FunctionInvoke)
                    Return RewriteFunction(f.funcName, f.parameters.Select(AddressOf Rewrite).ToArray)
                ElseIf TypeOf expr Is UnaryExpression Then
                    Dim u = DirectCast(expr, UnaryExpression)
                    Return RewriteUnary(u.operator, Rewrite(u.value))
                ElseIf TypeOf expr Is UnaryNot Then
                    Dim n = DirectCast(expr, UnaryNot)
                    Return RewriteNot(Rewrite(n.value))
                ElseIf TypeOf expr Is LogicalLiteral Then
                    Return RewriteLogical(DirectCast(expr, LogicalLiteral))
                ElseIf TypeOf expr Is Factorial Then
                    Return RewriteFactorial(Rewrite(DirectCast(expr, Factorial).factor))
                Else
                    Return expr
                End If
            End Function

            Protected Overridable Function RewriteLiteral(x As Literal) As Expression
                Return x
            End Function

            Protected Overridable Function RewriteSymbol(x As SymbolExpression) As Expression
                Return x
            End Function

            Protected Overridable Function RewriteBinary(x As BinaryExpression) As Expression
                Return x
            End Function

            Protected Overridable Function RewriteFunction(name$, args As Expression()) As Expression
                Return New FunctionInvoke(name, args)
            End Function

            Protected Overridable Function RewriteUnary(op$, v As Expression) As Expression
                Return New UnaryExpression With {.[operator] = op, .value = v}
            End Function

            Protected Overridable Function RewriteNot(v As Expression) As Expression
                Return New UnaryNot With {.value = v}
            End Function

            Protected Overridable Function RewriteLogical(x As LogicalLiteral) As Expression
                Return x
            End Function

            Protected Overridable Function RewriteFactorial(v As Expression) As Expression
                Return New Factorial(v.ToString)
            End Function
        End Class
    End Module
End Namespace
