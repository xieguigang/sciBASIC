Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Delegate Function IAssertion(def As IObject, obj As Object) As Boolean

Public Module AssertionProvider

    Public Function MustContains(t As Token(Of Tokens)) As IAssertion
        Dim term$ = t.Text.GetString
        Dim evaluate As Func(Of String, Boolean) =
            term$.CompileMustSearch

        Return Function(def, obj)
                   For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                       If evaluate(x.x) Then
                           Return True
                       End If
                   Next

                   Return False
               End Function
    End Function

    Public Function ContainsAny(t As Token(Of Tokens)) As IAssertion
        Dim term$ = t.Text.GetString("'")

        If Not term.Contains(":"c) Then
            Dim evaluate As Func(Of String, Boolean) = term.CompileNormalSearch

            Return Function(def, obj)
                       For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                           If evaluate(x.x) Then
                               Return True
                           End If
                       Next

                       Return False
                   End Function
        End If

        Dim fieldSearch = term.GetTagValue(":")
        Dim assertion As Func(Of String, Boolean)

        term = fieldSearch.x

        If fieldSearch.x.IsMustExpression Then
            term = term.GetString()
            assertion = term$.CompileMustSearch
        Else
            term = term.GetString("'")
            assertion = term.CompileNormalSearch
        End If

        Dim fName$ = fieldSearch.Name.ToLower

        Return _
            Function(def, obj)
                For Each key$ In def.Schema.Keys
                    If LCase(key$) = fName$ Then
                        Dim searchIn As String =
                        Scripting.ToString(
                        def.Schema(key$).GetValue(obj))

                        ' 由于是限定搜索的域的，所以在这里直接返回结果了，不需要在匹配失败之后继续遍历域列表
                        Return assertion(searchIn)
                    End If
                Next

                Return False
            End Function
    End Function
End Module
