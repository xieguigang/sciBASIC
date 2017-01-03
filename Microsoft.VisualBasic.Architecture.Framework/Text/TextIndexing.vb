#Region "Microsoft.VisualBasic::0ef67ef9a3bae1648490af531f31a347, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\TextIndexing.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Language

Namespace Text

    ''' <summary>
    ''' 文本之中的一个片段
    ''' </summary>
    Public Class TextSegment

        Dim _text As String

        ''' <summary>
        ''' 当前的这个文本片段的字符串值
        ''' </summary>
        ''' <returns></returns>
        Public Property Segment As String
            Get
                Return _text
            End Get
            Set(value As String)
                Dim ascii As Integer() = value.ToArray(Function(c) AscW(c))
                _Array = ascii
                _text = value
            End Set
        End Property

        ''' <summary>
        ''' ASCII值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Array As Integer()
        ''' <summary>
        ''' 在原始文本之中的左端起始位置
        ''' </summary>
        ''' <returns></returns>
        Public Property Index As Integer

        Sub New(Optional value As String = "")
            Segment = value
        End Sub

        Public Overrides Function ToString() As String
            Return Segment
        End Function

        Public Overloads Shared Narrowing Operator CType(segment As TextSegment) As String
            Return segment.Segment
        End Operator
    End Class

    <PackageNamespace("Text.Indexing", Publisher:="xie.guigang@gcmodeller.org", Category:=APICategories.UtilityTools)>
    Public Class TextIndexing

        ''' <summary>
        ''' 为了用于加速批量匹配计算的效率而生成的一个缓存对象
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PreCache As TextSegment()
            Get
                Return _preCaches
            End Get
        End Property

        ReadOnly _preCaches As TextSegment()
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
            Dim LQuery As TextSegment() =
                LinqAPI.Exec(Of TextSegment) <= From d As Integer
                                                In (max - min).Sequence.AsParallel
                                                Let len As Integer = min + d
                                                Select __cache(text, len)
            If min = max Then
                _preCaches = __cache(text, max)
            Else
                _preCaches = LQuery
            End If

            _text = text
            _max = max
            _mMatches = (From d As Integer
                         In max.Sequence
                         Select len = d,
                             m = New String("m"c, d)) _
                            .ToDictionary(Function(x) x.len,
                                          Function(x) x.m)

            Call $"{_preCaches.Length} cache data from length range from {min} to {max}...".__DEBUG_ECHO
        End Sub

        Public Overrides Function ToString() As String
            Return _text
        End Function

        Private Shared Function __cache(text As String, len As Integer) As TextSegment()
            Dim lstCache As New List(Of TextSegment)

            For i As Integer = 1 To text.Length - len
                Dim piece As String = Mid(text, i, len)
                lstCache += New TextSegment(piece) With {
                    .Index = i
                }
            Next

            Return lstCache.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="keyword"></param>
        ''' <param name="cutoff"></param>
        ''' <param name="NumPartitions">负数表示不进行分区</param>
        ''' <returns></returns>
        Public Function Found(keyword As String,
               Optional cutoff As Double = 0.6,
               Optional NumPartitions As Integer = 1024) _
                                      As Dictionary(Of TextSegment, DistResult)

            If NumPartitions <= 0 Then
                Dim resultSet As New Dictionary(Of TextSegment, DistResult)
                Call __indexing(Me._preCaches, resultSet, keyword, cutoff)
                Return resultSet
            Else
                Return __workParts(keyword, cutoff, NumPartitions)
            End If
        End Function

        Private Function __workParts(keyword As String,
                                 cutoff As Double,
                                 NumPartitions As Integer) As Dictionary(Of TextSegment, DistResult)
            Dim resultSet As New Dictionary(Of TextSegment, DistResult)
            Dim partitions = Me._preCaches.Split(1024)

            Call $"{partitions.Length} partitions...".__DEBUG_ECHO

            For Each part In partitions
                Call __indexing(part, resultSet, keyword, cutoff)
            Next

            Return resultSet
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="keyword"></param>
        ''' <param name="cutoff">表示出现连续的m匹配的片段的长度,-1表示所搜索的关键词片段的长度一半</param>
        ''' <returns></returns>
        Public Function Found(keyword As String, Optional cutoff As Integer = -1) As Dictionary(Of TextSegment, DistResult)
            If cutoff = -1 Then
                cutoff = Len(keyword) / 2
            End If

            Dim LQuery = (From piece As TextSegment
                          In Me._preCaches
                          Let levl As DistResult =
                              LevenshteinDistance.ComputeDistance(piece.Array, keyword)
                          Where Not levl Is Nothing AndAlso
                              IsMatch(levl.DistEdits, cutoff)
                          Select piece,
                              levl) _
                             .ToDictionary(Function(x) x.piece,
                                           Function(x) x.levl)
            Call Console.Write(".")
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

                Dim cache As String = _mMatches(i)
                If InStr(m, cache) > 0 Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Shared Sub __indexing(part As TextSegment(),
                                  ByRef resultSet As Dictionary(Of TextSegment, DistResult),
                                  keyword As String,
                                  cutoff As Double)
            Call Console.Write(".")

            Dim LQuery = From piece As TextSegment
                         In part'.AsParallel
                         Let lev As DistResult =
                             LevenshteinDistance.ComputeDistance(piece.Array, keyword)
                         Where Not lev Is Nothing AndAlso
                             lev.Score >= cutoff
                         Select piece, lev

            For Each x In LQuery
                Call resultSet.Add(x.piece, x.lev)
            Next
            ' Call FlushMemory()
        End Sub

        <ExportAPI("Index.Fuzzy")>
        Public Shared Function FuzzyIndex(text As String, keyword As String,
                                          Optional cutoff As Double = 0.6,
                                          Optional min As Integer = 3,
                                          Optional max As Integer = 20) As Dictionary(Of TextSegment, DistResult)
            Dim indexr As New TextIndexing(text, min, max)
            Dim searchs = indexr.Found(keyword, cutoff)
            Return searchs
        End Function

        <ExportAPI("Index.Fuzzy")>
        Public Shared Function FuzzyIndex(text As String, keyword As String,
                                      <Parameter("Cutoff",
                                                 "The continues length of the matches, if this value is ZERO or negative value, then the function will using the expression len(keyword)/2 as the default value.")>
                                      Optional Matches As Integer = -1,
                                      Optional min As Integer = 3,
                                      Optional max As Integer = 20) As Dictionary(Of TextSegment, DistResult)
            Dim indexr As New TextIndexing(text, min, max)
            Dim searchs = indexr.Found(keyword, Matches)
            Return searchs
        End Function
    End Class
End Namespace
