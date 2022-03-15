#Region "Microsoft.VisualBasic::d018f2030021b919112b982948b92f60, sciBASIC#\Data\SearchEngine\SearchEngine\Evaluation\AssertionProvider.vb"

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

    '   Total Lines: 102
    '    Code Lines: 77
    ' Comment Lines: 10
    '   Blank Lines: 15
    '     File Size: 4.04 KB


    ' Module AssertionProvider
    ' 
    '     Function: ContainsAny, MustContains
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 提供对字符串的计算方法
''' </summary>
Public Module AssertionProvider

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="t"></param>
    ''' <returns></returns>
    ''' <remarks>得分最高</remarks>
    Public Function MustContains(t As CodeToken(Of Tokens), Optional caseSensitive As Boolean = True) As IAssertion
        Dim term$ = t.text.GetString
        Dim evaluate As Func(Of String, Boolean) =
            term$.CompileMustSearch(caseSensitive)

        Return Function(def, obj)
                   For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                       If evaluate(x.Value) Then
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

    Public Function ContainsAny(t As CodeToken(Of Tokens), Optional allowInstr As Boolean = True, Optional caseSensitive As Boolean = True) As IAssertion
        Dim term$ = t.text.GetString("'")

        If Not term.Contains(":"c) Then  ' 并不是对特定的域进行搜索
            Dim evaluate As Func(Of String, Boolean) =
                term.CompileNormalSearch(allowInstr, caseSensitive)

            Return Function(def, obj)
                       For Each x As NamedValue(Of String) In def.EnumerateFields(obj)
                           If evaluate(x.Value) Then
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

        term = fieldSearch.Value

        If fieldSearch.Value.IsMustExpression Then
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
                                    .Value = searchIn
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
