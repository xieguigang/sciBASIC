Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Delegate Function IAssertion(data As IObject) As Boolean

Public Module AssertionProvider

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
End Module
