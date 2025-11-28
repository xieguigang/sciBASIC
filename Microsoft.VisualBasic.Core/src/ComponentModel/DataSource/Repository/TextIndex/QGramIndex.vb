#Region "Microsoft.VisualBasic::73abc9c60d325e1996ef4fdbbe04c802, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Repository\TextIndex\QGramIndex.vb"

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

'   Total Lines: 144
'    Code Lines: 91 (63.19%)
' Comment Lines: 24 (16.67%)
'    - Xml Docs: 75.00%
' 
'   Blank Lines: 29 (20.14%)
'     File Size: 5.19 KB


'     Class QGramIndex
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: FindSimilar, GenerateQGrams, GetIndexStats, PadString
' 
'         Sub: AddString
' 
'     Class FindResult
' 
'         Properties: index, levenshtein, similarity, text
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein

Namespace ComponentModel.DataSourceModel.Repository

    Public Class QGramIndex

        ''' <summary>
        ''' q-gram的长度
        ''' </summary>
        ReadOnly _q As Integer
        ReadOnly _index As Dictionary(Of String, HashSet(Of Integer))
        ReadOnly _strings As List(Of (str As String, id As Integer))
        ReadOnly _counts As New Dictionary(Of String, Integer)

        Public Sub New(q As Integer)
            _q = q
            _index = New Dictionary(Of String, HashSet(Of Integer))(StringComparer.OrdinalIgnoreCase)
            _strings = New List(Of (String, Integer))
        End Sub

        ''' <summary>
        ''' 向索引中添加字符串
        ''' </summary>
        Public Sub AddString(s As String, Optional index As Integer? = Nothing)
            If String.IsNullOrEmpty(s) Then
                Return
            End If

            ' 生成q-grams
            Dim grams = GenerateQGrams(s)
            Dim id As Integer = If(index, _strings.Count)
            Dim stringIndex As Integer = _strings.Count

            _strings.Add((s, id))
            _counts(s) = grams.Count

            For Each gram As String In grams
                If Not _index.ContainsKey(gram) Then
                    _index(gram) = New HashSet(Of Integer)()
                End If
                _index(gram).Add(stringIndex)
            Next
        End Sub

        ''' <summary>
        ''' 生成字符串的所有q-grams
        ''' </summary>
        Private Function GenerateQGrams(s As String) As List(Of String)
            Dim grams = New List(Of String)()

            ' 对短于q的字符串添加首尾填充
            Dim padded = PadString(s.ToLower)

            For i As Integer = 0 To padded.Length - _q
                grams.Add(padded.Substring(i, _q))
            Next

            Return grams
        End Function

        ''' <summary>
        ''' 为短字符串添加填充字符
        ''' </summary>
        Private Function PadString(s As String) As String
            If s.Length >= _q Then Return s

            ' 在首尾添加特殊字符进行填充
            Dim padding = New String("$"c, _q - 1)
            Return padding & s & padding
        End Function

        ''' <summary>
        ''' 基于q-gram重叠度查找相似字符串
        ''' </summary>
        Public Function FindSimilar(query As String, Optional threshold As Double = 0) As IEnumerable(Of FindResult)
            If String.IsNullOrEmpty(query) Then
                Return New FindResult() {}
            End If

            Dim queryGrams = GenerateQGrams(query)
            Dim candidateCounts = New Dictionary(Of Integer, Integer)()

            ' 统计每个候选字符串的匹配q-gram数量
            For Each gram As String In queryGrams
                If _index.ContainsKey(gram) Then
                    For Each strIndex As Integer In _index(gram)
                        If candidateCounts.ContainsKey(strIndex) Then
                            candidateCounts(strIndex) += 1
                        Else
                            candidateCounts(strIndex) = 1
                        End If
                    Next
                End If
            Next

            Dim results As New List(Of FindResult)

            ' 计算Jaccard相似度并筛选结果
            For Each kvp As KeyValuePair(Of Integer, Integer) In candidateCounts
                Dim strIndex = kvp.Key
                Dim commonGrams = kvp.Value
                Dim targetGrams = _counts(_strings(strIndex).str)
                Dim unionGrams = queryGrams.Count + targetGrams - commonGrams

                Dim similarity = commonGrams / CDbl(unionGrams)

                If similarity > threshold Then
                    ' 计算编辑距离作为二次验证
                    Dim dist = LevenshteinDistance.ComputeDistance(query, _strings(strIndex).str)
                    Dim distance = If(dist Is Nothing, Double.PositiveInfinity, dist.Distance)

                    Call results.Add(New FindResult(_strings(strIndex).str, similarity, distance) With {
                         .index = _strings(strIndex).id
                    })
                End If
            Next

            Return results.OrderByDescending(Function(r) r.similarity)
        End Function

        ''' <summary>
        ''' 获取索引统计信息
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetIndexStats() As (Integer, Integer, Integer)
            Return (_strings.Count, _index.Count, _index.Sum(Function(x) x.Value.Count))
        End Function
    End Class

    Public Class FindResult

        Public Property text As String
        Public Property similarity As Double
        Public Property levenshtein As Double
        Public Property index As Integer

        Sub New()
        End Sub

        Sub New(text As String, similairty As Double, levenshtein As Double)
            _text = text
            _similarity = similairty
            _levenshtein = levenshtein
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] {text} = {similarity}"
        End Function
    End Class
End Namespace
