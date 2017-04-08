#Region "Microsoft.VisualBasic::20935a2e73eab6da7823820aa2c04079, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\StringSimilarity\Levenshtein\LevenshteinDistance.vb"

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

Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.ComponentModel.DataStructures

Namespace Text.Levenshtein

    ''' <summary>
    ''' Levenshtein Edit Distance Algorithm for measure string distance
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Tips/697588/Levenshtein-Edit-Distance-Algorithm
    ''' </remarks>
    '''
    <PackageNamespace("Distance.Levenshtein",
                  Description:="Implement the Levenshtein Edit Distance algorithm and result data visualization.",
                  Publisher:="furkanavcu",
                  Category:=APICategories.UtilityTools,
                  Url:="http://www.codeproject.com/Tips/697588/Levenshtein-Edit-Distance-Algorithm")>
    <Cite(Title:="Binary codes capable of correcting deletions, insertions, and reversals",
      Pages:="707–710", StartPage:=707, Issue:="8", Volume:=10, Authors:="Levenshtein,
Vladimir I",
      Journal:="Soviet Physics Doklady", Year:=1966)>
    Public Module LevenshteinDistance

        ''' <summary>
        ''' Creates distance table for data visualization
        ''' </summary>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="cost"></param>
        ''' <returns></returns>
        Private Function __createTable(reference As Integer(), hypotheses As Integer(), cost As Double) As Double(,)
            Return CreateTable(Of Integer)(reference, hypotheses, cost, AddressOf __int32Equals)
        End Function

        Private Function __int32Equals(a As Integer, b As Integer) As Boolean
            Return a = b
        End Function

        Public Delegate Function Equals(Of T)(a As T, b As T) As Boolean

        ''' <summary>
        ''' 用于泛型的序列相似度比较
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="cost"></param>
        ''' <param name="equals">泛型化的元素等价性的比较方法</param>
        ''' <returns></returns>
        Public Function CreateTable(Of T)(reference As T(), hypotheses As T(), cost As Double, equals As Equals(Of T)) As Double(,)
            Dim distTable As Double(,) = New Double(reference.Length, hypotheses.Length) {}

            For i As Integer = 0 To reference.Length
                distTable(i, 0) = i * cost
            Next

            For j As Integer = 0 To hypotheses.Length
                distTable(0, j) = j * cost
            Next

            For i As Integer = 1 To reference.Length
                For j As Integer = 1 To hypotheses.Length

                    If equals(reference(i - 1), hypotheses(j - 1)) Then
                        '  if the letters are same
                        distTable(i, j) = distTable(i - 1, j - 1)
                    Else ' if not add 1 to its neighborhoods and assign minumun of its neighborhoods
                        Dim n As Double = Math.Min(distTable(i - 1, j - 1) + 1, distTable(i - 1, j) + cost)
                        distTable(i, j) = Math.Min(n, distTable(i, j - 1) + cost)
                    End If
                Next
            Next

            Return distTable
        End Function

        ''' <summary>
        ''' 泛型序列的相似度的比较计算方法，这个函数返回的是距离
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="equals"></param>
        ''' <param name="cost"></param>
        ''' <returns></returns>
        Public Function ComputeDistance(Of T)(reference As T(), hypotheses As T(), equals As Equals(Of T), Optional cost As Double = 0.7) As Double
            If hypotheses Is Nothing Then hypotheses = New T() {}
            If reference Is Nothing Then reference = New T() {}

            Dim distTable As Double(,) = CreateTable(reference, hypotheses, cost, equals)
            Dim i As Integer = reference.Length, j As Integer = hypotheses.Length

            Return distTable(i, j)
        End Function

        ''' <summary>
        ''' 泛型序列的相似度的比较计算方法，这个会返回所有的数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="equals"></param>
        ''' <param name="asChar">这个只是用于进行显示输出的</param>
        ''' <param name="cost"></param>
        ''' <returns></returns>
        Public Function ComputeDistance(Of T)(reference As T(), hypotheses As T(), equals As Equals(Of T), asChar As ToChar(Of T), Optional cost As Double = 0.7) As DistResult
            If hypotheses Is Nothing Then hypotheses = New T() {}
            If reference Is Nothing Then reference = New T() {}

            Dim distTable As Double(,) =
                CreateTable(reference, hypotheses, cost, equals)
            Dim i As Integer = reference.Length,
                j As Integer = hypotheses.Length
            Dim sHyp As String = New String(hypotheses.ToArray(Function(x) asChar(x)))
            Dim sRef As String = New String(reference.ToArray(Function(x) asChar(x)))
            Dim result As New DistResult With {
                .Hypotheses = sHyp,
                .Reference = sRef
            }
            Return __computeRoute(sHyp, result, i, j, distTable)
        End Function

        <ExportAPI("ToHTML", Info:="View distance evolve route of the Levenshtein Edit Distance calculation.")>
        Public Function GetVisulization(res As DistResult) As String
            Return res.htmlVisualize
        End Function

        <ExportAPI("Write.Xml.DistResult")>
        Public Function SaveMatch(result As DistResult, SaveTo As String) As Boolean
            Return result.GetXml.SaveTo(SaveTo)
        End Function

        Public Delegate Function ToChar(Of T)(x As T) As Char

        ''' <summary>
        ''' Implement the Levenshtein Edit Distance algorithm between string.
        ''' </summary>
        ''' <param name="reference">The reference string ASCII cache.</param>
        ''' <param name="hypotheses"></param>
        ''' <param name="cost"></param>
        ''' <returns></returns>
        <ExportAPI("ComputeDistance", Info:="Implement the Levenshtein Edit Distance algorithm.")>
        Public Function ComputeDistance(reference As Integer(), hypotheses As String, Optional cost As Double = 0.7) As DistResult
            If hypotheses Is Nothing Then hypotheses = ""
            If reference Is Nothing Then reference = New Integer() {}

            Dim distTable#(,) = __createTable(reference,
                                              hypotheses.ToArray(Function(ch) Asc(ch)),
                                              cost)
            Dim i As Integer = reference.Length,
                j As Integer = hypotheses.Length
            Dim result As New DistResult With {
                .Hypotheses = hypotheses,
                .Reference = Nothing
            }
            Return __computeRoute(hypotheses, result, i, j, distTable)
        End Function

        Const a As Integer = Asc("a"c)

        Public Function Similarity(Of T)(query As T(), subject As T(), Optional penalty As Double = 0.75) As Double
            If query.IsNullOrEmpty OrElse subject.IsNullOrEmpty Then
                Return 0
            End If

            Dim distinct As T() =
                (New [Set](query) + New [Set](subject)) _
                .ToArray _
                .ToArray(Function(x) DirectCast(x, T))
            Dim dict = (From index As Integer
                        In distinct.Sequence(offSet:=a)
                        Select ch = ChrW(index),
                            obj = distinct(index - a)) _
                            .ToDictionary(Function(x) x.obj,
                                          Function(x) x.ch)
            Dim ref As String = New String(query.ToArray(Function(x) dict(x)))
            Dim sbj As String = New String(subject.ToArray(Function(x) dict(x)))

            If String.IsNullOrEmpty(ref) OrElse String.IsNullOrEmpty(sbj) Then
                Return 0
            End If

            Dim result As DistResult = ComputeDistance(ref, sbj, penalty)
            If result Is Nothing Then
                Return 0
            Else
                Return result.Score
            End If
        End Function

        ''' <summary>
        ''' 计算lev编辑的变化路径
        ''' </summary>
        ''' <param name="hypotheses"></param>
        ''' <param name="result"></param>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <param name="distTable"></param>
        ''' <returns></returns>
        Private Function __computeRoute(hypotheses$,
                                        result As DistResult,
                                        i%, j%,
                                        distTable#(,)) As DistResult

            Dim css As New List(Of Coords)
            Dim evolve As List(Of Char) = New List(Of Char)
            Dim edits As New List(Of Char)

            While True

                Call css.Add({i - 1, j})

                If i = 0 AndAlso j = 0 Then
                    Dim evolveRoute As Char() = evolve.ToArray
                    Call Array.Reverse(evolveRoute)
                    Call css.Add({i, j})

                    result.DistTable = distTable _
                        .ToVectorList _
                        .ToArray(Function(vec) New Streams.Array.Double With {
                            .Values = vec
                        })
                    result.DistEdits = New String(evolveRoute)
                    result.CSS = css.ToArray
                    result.Matches = New String(edits.ToArray.Reverse.ToArray)

                    Exit While

                ElseIf i = 0 AndAlso j > 0 Then   ' delete
                    Call evolve.Add("d"c)
                    Call css.Add({i - 1, j})
                    Call edits.Add("-"c)
                    j -= 1

                ElseIf i > 0 AndAlso j = 0 Then
                    Call evolve.Add("i"c)         ' insert
                    Call css.Add({i - 1, j})
                    Call edits.Add("-"c)

                    i -= 1

                Else
                    If distTable(i - 1, j - 1) <= distTable(i - 1, j) AndAlso
                    distTable(i - 1, j - 1) <= distTable(i, j - 1) Then
                        Call css.Add({i - 1, j})
                        If distTable(i - 1, j - 1) = distTable(i, j) Then
                            Call evolve.Add("m"c) ' match
                            Call edits.Add(hypotheses(j - 1))
                        Else
                            Call evolve.Add("s"c) ' substitue
                            Call edits.Add("-"c)
                        End If

                        i -= 1
                        j -= 1

                    ElseIf distTable(i - 1, j) < distTable(i, j - 1) Then
                        Call css.Add({i - 1, j})
                        Call evolve.Add("i")      ' insert
                        Call edits.Add("-"c)
                        i -= 1

                    ElseIf distTable(i, j - 1) < distTable(i - 1, j) Then
                        Call css.Add({i - 1, j})
                        Call evolve.Add("d")      ' delete
                        Call edits.Add("-"c)
                        j -= 1

                    End If
                End If

                If css.Count > 1024 AndAlso css.Count - evolve.Count > 128 Then
                    ' Call $"{reference} ==> {hypotheses} stack could not be solve, operation abort!".__DEBUG_ECHO
                    Return Nothing
                End If
            End While

            Return result
        End Function

        ''' <summary>
        ''' The edit distance between two strings is defined as the minimum number of
        ''' edit operations required to transform one string into another.
        ''' (请注意，这函数是大小写敏感的。如果需要大小写不敏感，在使用前，请先将函数的两个字符串参数都转换为小写形式)
        ''' </summary>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="cost"></param>
        ''' <returns></returns>
        <ExportAPI("ComputeDistance", Info:="Implement the Levenshtein Edit Distance algorithm.")>
        Public Function ComputeDistance(reference$, hypotheses$, Optional cost# = 0.7) As DistResult

            If hypotheses Is Nothing Then hypotheses = ""
            If reference Is Nothing Then reference = ""

            Dim distTable As Double(,) = __createTable(
                reference.ToArray(Function(ch) AscW(ch)),
                hypotheses.ToArray(Function(ch) AscW(ch)),
                cost)
            Dim i As Integer = reference.Length,
                j As Integer = hypotheses.Length
            Dim result As New DistResult With {
                .Hypotheses = hypotheses,
                .Reference = reference
            }

            Return __computeRoute(hypotheses, result, i, j, distTable)
        End Function
    End Module
End Namespace
