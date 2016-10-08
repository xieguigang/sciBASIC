Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Module ExpressionBuilder

    ''' <summary>
    ''' 构建查询表达式的对象模型
    ''' </summary>
    ''' <param name="query$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Build(query$) As Expression
        Dim tks As IEnumerable(Of Token(Of Tokens)) =
            SyntaxParser.Parser(query$)

        If IsEquals(Of Token(Of Tokens))(tks.Count) <=
            From x As Token(Of Tokens)
            In tks
            Where x.TokenName = Tokens.AnyTerm
            Select x Then

            Return New Expression With {
                .Tokens = {
                    New MetaExpression With {
                        .Operator = Tokens.AnyTerm,
                        .Expression = AssertionProvider.ContainsAny(New Token(Of Tokens)() With {
                            .TokenName = Tokens.AnyTerm,
                            .TokenValue = query$
                        })
                    }
                }
            }
        End If

        Try
            Return New Pointer(Of Token(Of Tokens))(tks).Build()
        Catch ex As Exception
            ex = New Exception("$query_expression:= " & query, ex)
            Throw ex
        End Try
    End Function

    <Extension>
    Public Function Build(tks As Pointer(Of Token(Of Tokens))) As Expression
        Dim metas As New List(Of MetaExpression)
        Dim meta As MetaExpression

        Do While Not tks.EndRead
            Dim t = +tks

            meta = New MetaExpression

            Select Case t.Type
                Case SyntaxParser.Tokens.AnyTerm
                    meta.Expression = AssertionProvider.ContainsAny(t)
                Case SyntaxParser.Tokens.MustTerm
                    meta.Expression = AssertionProvider.MustContains(t)
                Case SyntaxParser.Tokens.stackOpen
                    meta.Expression = AddressOf Build(tks).Evaluate
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

    Public Function IsOperator(x As Token(Of Tokens)) As Boolean
        Dim t As Tokens = x.Type

        Return t = SyntaxParser.Tokens.op_AND OrElse
            t = SyntaxParser.Tokens.op_NOT OrElse
            t = SyntaxParser.Tokens.op_OR
    End Function
End Module
