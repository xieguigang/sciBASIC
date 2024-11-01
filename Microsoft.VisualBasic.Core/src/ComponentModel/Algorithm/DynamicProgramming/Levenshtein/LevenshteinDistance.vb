#Region "Microsoft.VisualBasic::3a7255cb8049d8a93b664cfacb4dfa42, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\Levenshtein\LevenshteinDistance.vb"

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

    '   Total Lines: 326
    '    Code Lines: 205 (62.88%)
    ' Comment Lines: 72 (22.09%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 49 (15.03%)
    '     File Size: 14.70 KB


    '     Module LevenshteinDistance
    ' 
    '         Function: (+2 Overloads) ComputeDistance, createMatrix, CreateTable, i32Equals, SaveMatch
    '         Delegate Function
    ' 
    '             Function: (+2 Overloads) ComputeDistance, computeRouteImpl, Similarity
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports std = System.Math

Namespace ComponentModel.Algorithm.DynamicProgramming.Levenshtein

    ''' <summary>
    ''' Levenshtein Edit Distance Algorithm for measure string distance
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Tips/697588/Levenshtein-Edit-Distance-Algorithm
    ''' </remarks>
    '''
    <Package("Distance.Levenshtein",
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
        Private Function createMatrix(reference As Integer(), hypotheses As Integer(), cost As Double) As Double(,)
            Return CreateTable(Of Integer)(reference, hypotheses, DynamicProgramming.Cost(Of Integer).DefaultCost(cost), AddressOf i32Equals)
        End Function

        Private Function i32Equals(a As Integer, b As Integer) As Boolean
            Return a = b
        End Function

        ''' <summary>
        ''' 用于泛型的序列相似度比较
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="reference"></param>
        ''' <param name="hypotheses"></param>
        ''' <param name="cost"></param>
        ''' <param name="equals">泛型化的元素等价性的比较方法</param>
        ''' <returns></returns>
        Public Function CreateTable(Of T)(reference As T(), hypotheses As T(), cost As Cost(Of T), equals As GenericLambda(Of T).IEquals) As Double(,)
            Dim distTable As Double(,) = New Double(reference.Length, hypotheses.Length) {}

            For i As Integer = 0 To reference.Length - 1
                distTable(i, 0) = i * cost.insert(reference(i))
            Next

            For j As Integer = 0 To hypotheses.Length - 1
                distTable(0, j) = j * cost.delete(hypotheses(j))
            Next

            distTable(reference.Length, 0) = cost.insert(Nothing)
            distTable(0, hypotheses.Length) = cost.delete(Nothing)

            'd[i,j] <- min( d[i-1,j] + delete.fun(source.vec[i-1]),
            'd[i,j-1] + insert.fun(target.vec[j-1]),
            'd[i-1,j-1] + substitute.fun(source.vec[i-1], target.vec[j-1]) );

            For i As Integer = 1 To reference.Length
                For j As Integer = 1 To hypotheses.Length

                    If equals(reference(i - 1), hypotheses(j - 1)) Then
                        '  if the letters are same
                        distTable(i, j) = distTable(i - 1, j - 1)
                    Else ' if not add 1 to its neighborhoods and assign minumun of its neighborhoods
                        Dim n As Double = std.Min(
                            distTable(i - 1, j - 1) + cost.substitute(reference(i - 1), hypotheses(j - 1)),
                            distTable(i - 1, j) + cost.delete(reference(i - 1)))
                        distTable(i, j) = std.Min(n, distTable(i, j - 1) + cost.insert(hypotheses(j - 1)))
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
        Public Function ComputeDistance(Of T)(reference As T(), hypotheses As T(), equals As GenericLambda(Of T).IEquals, Optional cost As Double = 0.7) As Double
            If hypotheses Is Nothing Then hypotheses = New T() {}
            If reference Is Nothing Then reference = New T() {}

            Dim distTable#(,) = CreateTable(Of T)(reference, hypotheses, DynamicProgramming.Cost(Of T).DefaultCost(cost), equals)
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
        Public Function ComputeDistance(Of T)(reference As T(), hypotheses As T(),
                                              equals As GenericLambda(Of T).IEquals,
                                              asChar As ToChar(Of T),
                                              Optional cost As Double = 0.7) As DistResult

            If hypotheses Is Nothing Then hypotheses = New T() {}
            If reference Is Nothing Then reference = New T() {}

            Dim distTable#(,) = CreateTable(Of T)(reference, hypotheses, DynamicProgramming.Cost(Of T).DefaultCost(cost), equals)
            Dim i As Integer = reference.Length,
                j As Integer = hypotheses.Length
            Dim sHyp As String = New String(hypotheses.Select(Function(x) asChar(x)).ToArray)
            Dim sRef As String = New String(reference.Select(Function(x) asChar(x)).ToArray)
            Dim result As New DistResult With {
                .Hypotheses = sHyp,
                .Reference = sRef
            }

            Return computeRouteImpl(sHyp, result, i, j, distTable)
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
        <ExportAPI("ComputeDistance")>
        Public Function ComputeDistance(reference As Integer(), hypotheses As String, Optional cost As Double = 0.7) As DistResult
            If hypotheses Is Nothing Then hypotheses = ""
            If reference Is Nothing Then reference = New Integer() {}

            Dim distTable#(,) = createMatrix(reference,
                                              hypotheses.Select(Function(ch) Asc(ch)).ToArray,
                                              cost)
            Dim i As Integer = reference.Length
            Dim j As Integer = hypotheses.Length
            Dim result As New DistResult With {
                .hypotheses = hypotheses,
                .reference = Nothing
            }
            Return computeRouteImpl(hypotheses, result, i, j, distTable)
        End Function

        Const a As Integer = AscW("a"c)

        Public Function Similarity(Of T)(query As T(), subject As T(), Optional penalty As Double = 0.75) As Double
            If query.IsNullOrEmpty OrElse subject.IsNullOrEmpty Then
                Return 0
            End If

            Dim distinct As T() =
                (New [Set](query) + New [Set](subject)) _
                .ToArray _
                .Select(Function(x) DirectCast(x, T)) _
                .ToArray
            Dim dict = (From index As Integer
                        In distinct.Sequence(offSet:=a)
                        Select ch = ChrW(index),
                            obj = distinct(index - a)) _
                            .ToDictionary(Function(x) x.obj,
                                          Function(x)
                                              Return x.ch
                                          End Function)
            Dim ref As String = New String(query.Select(Function(x) dict(x)).ToArray)
            Dim sbj As String = New String(subject.Select(Function(x) dict(x)).ToArray)

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
        Private Function computeRouteImpl(hypotheses$,
                                        result As DistResult,
                                        i%, j%,
                                        distTable#(,)) As DistResult

            Dim css As New List(Of Coordinate)
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
                        .Select(Function(vec)
                                    Return New ArrayRow With {
                                        .data = vec
                                    }
                                End Function) _
                        .ToArray
                    result.DistEdits = New String(evolveRoute)
                    result.Path = css.ToArray
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
        <ExportAPI("ComputeDistance")>
        Public Function ComputeDistance(reference$, hypotheses$, Optional cost# = 0.7) As DistResult

            If hypotheses Is Nothing Then hypotheses = ""
            If reference Is Nothing Then reference = ""

            Dim vectorRef = reference.Select(Function(ch) AscW(ch)).ToArray
            Dim vectorHypo = hypotheses.Select(Function(ch) AscW(ch)).ToArray
            Dim distTable As Double(,) = createMatrix(vectorRef, vectorHypo, cost)
            Dim i As Integer = reference.Length
            Dim j As Integer = hypotheses.Length
            Dim result As New DistResult With {
                .hypotheses = hypotheses,
                .reference = reference
            }

            Return computeRouteImpl(hypotheses, result, i, j, distTable)
        End Function
    End Module
End Namespace
