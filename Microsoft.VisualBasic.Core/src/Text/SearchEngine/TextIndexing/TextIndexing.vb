#Region "Microsoft.VisualBasic::b1583e6415793d896e13bce6241bacf7, Microsoft.VisualBasic.Core\src\Text\SearchEngine\TextIndexing\TextIndexing.vb"

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

    '   Total Lines: 204
    '    Code Lines: 131 (64.22%)
    ' Comment Lines: 43 (21.08%)
    '    - Xml Docs: 86.05%
    ' 
    '   Blank Lines: 30 (14.71%)
    '     File Size: 8.57 KB


    '     Class TextIndexing
    ' 
    '         Properties: cache
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __indexing, __workParts, doCache, (+2 Overloads) Found, (+2 Overloads) FuzzyIndex
    '                   IsMatch, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Text.Search

    <Package("Text.Indexing", Publisher:="xie.guigang@gcmodeller.org", Category:=APICategories.UtilityTools)>
    Public Class TextIndexing

        ''' <summary>
        ''' 为了用于加速批量匹配计算的效率而生成的一个缓存对象
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property cache As TextSegment()

        ReadOnly _text As String
        ReadOnly _mMatches As Dictionary(Of Integer, String)
        ReadOnly _max As Integer

        ''' <summary>
        ''' Creates a text index instance object for the statement fuzzy match in the whole text document.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        Sub New(text As String, min As Integer, max As Integer)
            If min = max Then
                cache = doCache(text, max)
            Else
                cache = LinqAPI.Exec(Of TextSegment) _
 _
                    () <= From d As Integer
                          In (max - min) _
                              .Sequence _
                              .AsParallel
                          Let len As Integer = min + d
                          Select doCache(text, len)
            End If

            _text = text
            _max = max
            _mMatches = max _
                .Sequence _
                .ToDictionary(Function(l) l,
                              Function(d) New String("m"c, d))

            Call $"{cache.Length} cache data from length range from {min} to {max}...".debug
        End Sub

        Public Overrides Function ToString() As String
            Return Mid(_text, 1, 120) & "..."
        End Function

        Private Shared Function doCache(text As String, len As Integer) As TextSegment()
            Dim out As New List(Of TextSegment)

            For i As Integer = 1 To text.Length - len
                Dim s As String = Mid(text, i, len)

                ' 通过划窗操作构建当前长度的片段的缓存
                out += New TextSegment(s) With {
                    .Index = i
                }
            Next

            Return out
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="keyword"></param>
        ''' <param name="cutoff"></param>
        ''' <param name="numPartition">每一个分区之中的元素的数量，负数表示不进行分区</param>
        ''' <returns></returns>
        Public Function Found(keyword$, Optional cutoff# = 0.6, Optional numPartition% = 1024) As Map(Of TextSegment, DistResult)()
            If numPartition <= 0 Then
                Dim out = __indexing(cache, keyword, cutoff, True)
                Return out
            Else
                Return __workParts(keyword, cutoff, numPartition)
            End If
        End Function

        Private Function __workParts(keyword$, cutoff#, numPartitions%) As Map(Of TextSegment, DistResult)()
            Dim resultSet As New List(Of Map(Of TextSegment, DistResult))
            Dim partitions = cache.Split(numPartitions)

            Call $"{partitions.Length} partitions...".debug

            Dim LQuery = From part As TextSegment()
                         In partitions.AsParallel
                         Select __indexing(
                             part, keyword, cutoff,
                             parallel:=False)

            For Each part As Map(Of TextSegment, DistResult)() In LQuery
                Call resultSet.Add(part)
            Next

            Return resultSet
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="keyword"></param>
        ''' <param name="cutoff">表示出现连续的m匹配的片段的长度,-1表示所搜索的关键词片段的长度一半</param>
        ''' <returns></returns>
        Public Function Found(keyword$, Optional cutoff% = -1) As Map(Of TextSegment, DistResult)()
            If cutoff = -1 Then
                cutoff = Len(keyword) / 2
            End If

            Dim LQuery = LinqAPI.Exec(Of Map(Of TextSegment, DistResult)) _
 _
                () <= From piece As TextSegment
                      In cache
                      Let levl As DistResult = LevenshteinDistance.ComputeDistance(piece.Array, keyword)
                      Where Not levl Is Nothing AndAlso IsMatch(levl.DistEdits, cutoff)
                      Select New Map(Of TextSegment, DistResult) With {
                          .Key = piece,
                          .Maps = levl
                      }

            Return LQuery
        End Function

        ''' <summary>
        ''' 函数返回最长的匹配的个数，-1表示没有匹配
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="cutoff"></param>
        ''' <returns></returns>
        Public Function IsMatch(m As String, cutoff As Integer) As Integer
            For i As Integer = _max To cutoff Step -1
                If Not _mMatches.ContainsKey(i) Then
                    Continue For
                End If

                ' 由于i是倒序的，所以lev的匹配结果m字符串的长度是从长到短的
                ' 故而第一个匹配上目标输入的结果参数的为最长的匹配结果
                Dim cache As String = _mMatches(i)

                If InStr(m, cache) > 0 Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Shared Function __indexing(part As TextSegment(), keyword$, cutoff#, parallel As Boolean) As Map(Of TextSegment, DistResult)()
            Dim LQuery = From index As TextSegment
                         In part.Populate(parallel)
                         Let levenshtein As DistResult = index Like keyword
                         Where Not levenshtein Is Nothing AndAlso
                             levenshtein.Score >= cutoff
                         Select New Map(Of TextSegment, DistResult) With {
                             .Key = index,
                             .Maps = levenshtein
                         }

            Dim out As Map(Of TextSegment, DistResult)() = LQuery.ToArray
            Return out
        End Function

        <ExportAPI("Index.Fuzzy")>
        Public Shared Function FuzzyIndex(text As String, keyword As String,
                                          Optional cutoff As Double = 0.6,
                                          Optional min As Integer = 3,
                                          Optional max As Integer = 20) As Map(Of TextSegment, DistResult)()
            Dim indexr As New TextIndexing(text, min, max)
            Dim searchs = indexr.Found(keyword, cutoff)
            Return searchs
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="keyword"></param>
        ''' <param name="Matches">
        ''' The continues length of the matches, if this value is ZERO or negative value, then the function will using the expression len(keyword)/2 as the default value.
        ''' </param>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        <ExportAPI("Index.Fuzzy")>
        Public Shared Function FuzzyIndex(text$, keyword$,
                                          <Parameter("Cutoff", "The continues length of the matches, if this value is ZERO or negative value, then the function will using the expression len(keyword)/2 as the default value.")>
                                          Optional matches% = -1,
                                          Optional min% = 3,
                                          Optional max% = 20) As Map(Of TextSegment, DistResult)()

            Dim indexr As New TextIndexing(text, min, max)
            Dim searchs = indexr.Found(keyword, matches)
            Return searchs
        End Function
    End Class
End Namespace
