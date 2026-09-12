#Region "Microsoft.VisualBasic::e573300fb4960ea5f45f8ed5b86b5237, vs_solutions\VBS\src\VBScript\TupleDestructuring.vb"

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

    '   Total Lines: 146
    '    Code Lines: 97 (66.44%)
    ' Comment Lines: 22 (15.07%)
    '    - Xml Docs: 68.18%
    ' 
    '   Blank Lines: 27 (18.49%)
    '     File Size: 5.59 KB


    '     Module TupleDestructuring
    ' 
    '         Function: Expand, FindTopLevelAs, IndexOfComment, SplitTopLevel
    ' 
    '         Sub: ExpandNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Namespace Script

    Public Module TupleDestructuring

        ''' <summary>
        ''' 预处理 VB 源码中的元组分解语法
        ''' 支持:
        '''   Dim (a, b) = expr
        '''   Dim (a As Integer, b As String) = expr
        '''   Dim (a, , c) = expr           ' 跳过中间项
        '''   Dim (a, (b, c)) = expr        ' 嵌套分解
        '''   保留行尾注释与前后空白
        ''' </summary>
        Public Function Expand(source As String) As String
            Dim i As i32 = 1
            Dim outLines As New List(Of String)

            For Each raw As String In source.LineTokens
                ' 提取前导空白和行尾注释
                Dim lead As String = Regex.Match(raw, "^\s*").Value
                Dim body As String = raw.Substring(lead.Length)
                Dim comment As String = ""
                Dim cIdx As Integer = IndexOfComment(body)

                If cIdx >= 0 Then
                    comment = body.Substring(cIdx)
                    body = body.Substring(0, cIdx).TrimEnd()
                End If

                Dim m As Match = Regex.Match(
                    body, "^Dim\s*\((?<names>.+)\)\s*=\s*(?<expr>.+)$",
                    RegexOptions.IgnoreCase)

                If Not m.Success Then
                    Call outLines.Add(raw)
                    Continue For
                End If

                Dim expr As String = m.Groups("expr").Value.Trim()
                Dim tmp As String = $"__tuple{++i}"
                Dim declIdx As Integer = 0

                ' ---- 生成临时元组变量声明 ----
                Call outLines.Add($"{lead}Dim {tmp} = {expr}{comment}")

                ' ---- 递归展开分解变量 ----
                Call ExpandNames(m.Groups("names").Value, tmp, lead, outLines, declIdx)
            Next

            Return String.Join(vbCrLf, outLines)
        End Function

        ''' <summary>
        ''' 递归展开左侧变量名列表(支持嵌套)
        ''' </summary>
        Private Sub ExpandNames(namesText As String, tmp As String,
                                lead As String, outLines As List(Of String),
                                ByRef index As Integer)

            ' 切分顶层逗号(忽略括号内的逗号)
            For Each part As String In SplitTopLevel(namesText)
                Dim p As String = part.Trim()

                If String.IsNullOrEmpty(p) OrElse p = "_"c Then
                    index += 1
                    Continue For
                End If

                ' ---- 嵌套元组: 递归展开 ----
                If p.StartsWith("("c) Then
                    Dim inner As String = p.Trim("("c, ")"c)
                    Dim nestedTmp As String = $"{tmp}._n{index}"
                    Call outLines.Add($"{lead}Dim {nestedTmp} = {tmp}.Item{index + 1}")
                    Call ExpandNames(inner, nestedTmp, lead, outLines, 0)
                    index += 1
                    Continue For
                End If

                ' ---- 命名别名: alias:=varName ----
                Dim varName As String = p
                Dim colonIdx As Integer = p.IndexOf(":"c)
                If colonIdx > 0 Then
                    varName = p.Substring(colonIdx + 1).Trim()
                End If

                ' ---- 类型声明: varName As Type ----
                Dim typeClause As String = ""
                Dim asIdx As Integer = FindTopLevelAs(varName)
                If asIdx >= 0 Then
                    typeClause = " " & varName.Substring(asIdx).Trim()
                    varName = varName.Substring(0, asIdx).Trim()
                End If

                Call outLines.Add(
                    $"{lead}Dim {varName}{typeClause} = {tmp}.Item{index + 1}")

                index += 1
            Next
        End Sub

        ''' <summary>按顶层逗号切分(不切括号内的)</summary>
        Private Iterator Function SplitTopLevel(s As String) As IEnumerable(Of String)
            Dim depth As Integer = 0
            Dim start As Integer = 0

            For i As Integer = 0 To s.Length - 1
                Select Case s(i)
                    Case "("c : depth += 1
                    Case ")"c : depth -= 1
                    Case ","c
                        If depth = 0 Then
                            Yield s.Substring(start, i - start)
                            start = i + 1
                        End If
                End Select
            Next

            Yield s.Substring(start)
        End Function

        ''' <summary>定位 As 关键字(排除字符串内的)</summary>
        Private Function FindTopLevelAs(s As String) As Integer
            Return Regex.Match(s, "\s+As\s+", RegexOptions.IgnoreCase).Index
        End Function

        ''' <summary>定位行尾注释的起点(排除字符串内的)</summary>
        Private Function IndexOfComment(s As String) As Integer
            Dim inStr As Boolean = False

            For i As Integer = 0 To s.Length - 1
                Dim c As Char = s(i)
                If c = """"c AndAlso i > 0 AndAlso s(i - 1) <> "\"c Then
                    inStr = Not inStr
                ElseIf c = "'"c AndAlso Not inStr Then
                    Return i
                End If
            Next

            Return -1
        End Function
    End Module

End Namespace
