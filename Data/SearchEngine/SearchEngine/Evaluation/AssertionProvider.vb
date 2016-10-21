Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module AssertionProvider

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="t"></param>
    ''' <returns></returns>
    ''' <remarks>得分最高</remarks>
    Public Function MustContains(t As Token(Of Tokens), Optional caseSensitive As Boolean = True) As IAssertion
        Dim term$ = t.Text.GetString
        Dim evaluate As Func(Of String, Boolean) =
            term$.CompileMustSearch(caseSensitive)

        Return Function(def, obj)
                   For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                       If evaluate(x.x) Then
                           Return New Match With {
                               .Field = x,
                               .x = obj,
                               .score = 1
                           }
                       End If
                   Next

                   Return Nothing
               End Function
    End Function

    Public Function ContainsAny(t As Token(Of Tokens), Optional allowInstr As Boolean = True, Optional caseSensitive As Boolean = True) As IAssertion
        Dim term$ = t.Text.GetString("'")

        If Not term.Contains(":"c) Then  ' 并不是对特定的域进行搜索
            Dim evaluate As Func(Of String, Boolean) =
                term.CompileNormalSearch(allowInstr, caseSensitive)

            Return Function(def, obj)
                       For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                           If evaluate(x.x) Then
                               Return New Match With {
                                   .Field = x,
                                   .x = obj,
                                   .score = 0.5
                               }
                           End If
                       Next

                       Return Nothing
                   End Function
        End If

        Dim fieldSearch = term.GetTagValue(":")
        Dim assertion As Func(Of String, Boolean)

        term = fieldSearch.x

        If fieldSearch.x.IsMustExpression Then
            term = term.GetString()
            assertion = term$.CompileMustSearch(caseSensitive)
        Else
            term = term.GetString("'")
            assertion = term.CompileNormalSearch(allowInstr, caseSensitive)
        End If

        Dim fName$ = fieldSearch.Name.ToLower

        Return _
            Function(def, obj)
                For Each key$ In def.Schema.Keys   ' 因为可能会存在大小写的问题，所以在这里不可以直接对字典查询
                    If LCase(key$) = fName$ Then
                        Dim searchIn As String =
                            Scripting.ToString(
                                def.Schema(key$).GetValue(obj))

                        ' 由于是限定搜索的域的，所以在这里直接返回结果了，不需要在匹配失败之后继续遍历域列表
                        If assertion(searchIn) Then
                            Return New Match With {
                                .x = obj,
                                .score = 0.75,
                                .Field = New NamedValue(Of String) With {
                                    .Name = key,
                                    .x = searchIn
                                }
                            }
                        Else
                            Return Nothing
                        End If
                    End If
                Next

                Return Nothing
            End Function
    End Function
End Module
