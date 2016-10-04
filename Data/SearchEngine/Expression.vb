Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' The query expression
''' </summary>
Public Class Expression

    Public Property Tokens As MetaExpression()

    Public Function Evaluate(x As IObject) As Boolean

    End Function

    Public Shared Function Build(query$, assertion As AssertionProvider) As Expression
        Dim tks As New Pointer(Of Token(Of Tokens))(Parser(query$))
        Return Build(tks, assertion)
    End Function

    Public Shared Function Build(tks As Pointer(Of Token(Of Tokens)), assertion As AssertionProvider) As Expression
        Dim metas As New List(Of MetaExpression)
        Dim meta As MetaExpression

        Do While Not tks.EndRead
            Dim t = +tks

            meta = New MetaExpression

            Select Case t.Type
                Case SyntaxParser.Tokens.AnyTerm
                    meta.Expression = assertion.ContainsAny(t)
                Case SyntaxParser.Tokens.MustTerm
                    meta.Expression = assertion.MustContains(t)
                Case SyntaxParser.Tokens.stackOpen
                    meta.Expression = AddressOf Build(tks, assertion).Evaluate
                Case SyntaxParser.Tokens.stackClose
                    Return New Expression With {
                        .Tokens = metas
                    }
                Case SyntaxParser.Tokens.op_NOT ' 表达式的可能是以NOT操作符开始的
                    meta = New MetaExpression With {
                        .Operator = SyntaxParser.Tokens.op_NOT
                    }
                    metas += meta

                    Continue Do
            End Select

            If tks.EndRead Then
                metas += meta
                Exit Do
            Else
                t = +tks
            End If

            Select Case t.Type
                Case SyntaxParser.Tokens.stackClose
                    metas += meta

                    Return New Expression With {
                        .Tokens = metas
                    }
                Case SyntaxParser.Tokens.op_AND
                    meta.Operator = SyntaxParser.Tokens.op_AND
                Case SyntaxParser.Tokens.op_NOT
                    meta.Operator = SyntaxParser.Tokens.op_NOT
                Case SyntaxParser.Tokens.op_OR
                    meta.Operator = SyntaxParser.Tokens.op_OR
                Case Else
                    Throw New SyntaxErrorException
            End Select

            metas += meta
        Loop

        Return New Expression With {
            .Tokens = metas
        }
    End Function

    Public Shared Function IsOperator(x As Token(Of Tokens)) As Boolean
        Dim t As Tokens = x.Type

        Return t = SyntaxParser.Tokens.op_AND OrElse
            t = SyntaxParser.Tokens.op_NOT OrElse
            t = SyntaxParser.Tokens.op_OR
    End Function
End Class

Public Class AssertionProvider

    Public Function MustContains(t As Token(Of Tokens)) As IAssertion
        Dim term$ = t.Text.GetString

        Return Function(data)
                   For Each x As NamedValue(Of String) In data.EnumerateFields
                       If Evaluator.MustContains(term, x.x) Then
                           Return True
                       End If
                   Next

                   Return False
               End Function
    End Function

    Public Function ContainsAny(t As Token(Of Tokens)) As IAssertion
        Dim term$ = t.Text.GetString("'")

        If Not term.Contains(":"c) Then
            Return Function(data)
                       For Each x As NamedValue(Of String) In data.EnumerateFields
                           If Evaluator.ContainsAny(term, x.x) Then
                               Return True
                           End If
                       Next

                       Return False
                   End Function
        End If

        Dim fieldSearch = term.GetTagValue(":")
        Dim assertion As Func(Of String, String, Boolean)

        term = fieldSearch.x

        If fieldSearch.x.IsMustExpression Then
            assertion = AddressOf Evaluator.MustContains
            term = term.GetString()
        Else
            assertion = AddressOf Evaluator.ContainsAny
            term = term.GetString("'")
        End If

        Dim fName$ = fieldSearch.Name.ToLower

        Return _
            Function(data)
                For Each key$ In data.Schema.Keys
                    If LCase(key$) = fName$ Then
                        Dim searchIn As String =
                        Scripting.ToString(
                        data.Schema(key$).GetValue(data.x))

                        If assertion(term, searchIn) Then
                            Return True
                        Else
                            Return False
                        End If
                    End If
                Next

                Return False
            End Function
    End Function
End Class

Public Class IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of BindProperty(Of Field))
    Public Property x As Object

    Sub New(type As Type)
        Me.Type = type
    End Sub

    ''' <summary>
    ''' 返回: ``FiledName: value_string``
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function EnumerateFields() As IEnumerable(Of NamedValue(Of String))

    End Function
End Class

Public Delegate Function IAssertion(data As IObject) As Boolean

''' <summary>
''' ``&lt;expr> &lt;opr>``，假如是以NOT操作符起始的元表达式，则<see cref="MetaExpression.Expression"/>属性为空
''' </summary>
Public Class MetaExpression

    Public Property [Operator] As Tokens
    ''' <summary>
    ''' Public <see cref="System.Delegate"/> Function <see cref="IAssertion"/>(data As <see cref="IObject"/>) As <see cref="Boolean"/>.
    ''' (这个可能是包含有括号运算的表达式)
    ''' </summary>
    ''' <returns></returns>
    Public Property Expression As IAssertion

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class