Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module ExpressionBuilder

    Public Function Build(query$, assertion As AssertionProvider) As Expression
        Dim tks As New Pointer(Of Token(Of Tokens))(Parser(query$))
        Return Build(tks, assertion)
    End Function

    Public Function Build(tks As Pointer(Of Token(Of Tokens)), assertion As AssertionProvider) As Expression
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

    Public Function IsOperator(x As Token(Of Tokens)) As Boolean
        Dim t As Tokens = x.Type

        Return t = SyntaxParser.Tokens.op_AND OrElse
            t = SyntaxParser.Tokens.op_NOT OrElse
            t = SyntaxParser.Tokens.op_OR
    End Function
End Module
