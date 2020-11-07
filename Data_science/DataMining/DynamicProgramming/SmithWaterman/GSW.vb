#Region "Microsoft.VisualBasic::cc99f82c13756694c2f76b6186619f0f, Data_science\DataMining\DynamicProgramming\SmithWaterman\GSW.vb"

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

    '     Class GSW
    ' 
    '         Properties: AlignmentScore, Matches, MaxScore, query, subject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __similarity, GetDPMAT, GetMatches, GetTraceback, traceback
    ' 
    '         Sub: buildMatrix, getTrackback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein.LevenshteinDistance
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports stdNum = System.Math

Namespace SmithWaterman

    ''' <summary>
    ''' Generic Smith-Waterman computing kernel.(Smith-Waterman泛型化通用计算核心)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class GSW(Of T)

#Region "input data"

        ''' <summary>
        ''' The first input string
        ''' </summary>
        Public ReadOnly Property query As T()

        ''' <summary>
        ''' The second input String
        ''' </summary>
        Public ReadOnly Property subject As T()

        ''' <summary>
        ''' The lengths of the input strings
        ''' </summary>
        Public ReadOnly queryLength%, subjectLength%
#End Region

#Region "Matrix"

        ''' <summary>
        ''' The score matrix.
        ''' The true scores should be divided by the normalization factor.
        ''' </summary>
        Public score As Double()()
        ''' <summary>
        ''' The directions pointing to the cells that
        ''' give the maximum score at the current cell.
        ''' The first index is the column index.
        ''' The second index is the row index.
        ''' </summary>
        Public prevCells As Integer()()
#End Region

        ''' <summary>
        ''' The normalization factor.
        ''' To get the true score, divide the integer score used in computation
        ''' by the normalization factor.
        ''' </summary>
        Public Const NORM_FACTOR As Double = 1.0

        ''' <summary>
        ''' The similarity function constants.
        ''' They are amplified by the normalization factor to be integers.
        ''' </summary>
        Public Const MATCH_SCORE As Integer = 10
        Public Const MISMATCH_SCORE As Integer = -8
        Public Const INDEL_SCORE As Integer = -9

        ''' <summary>
        ''' Constants of directions.
        ''' Multiple directions are stored by bits.
        ''' The zero direction is the starting point.
        ''' </summary>
        Public Const DR_LEFT As Integer = 1
        ' 0001
        Public Const DR_UP As Integer = 2
        ' 0010
        Public Const DR_DIAG As Integer = 4
        ' 0100
        Public Const DR_ZERO As Integer = 8
        ' 1000

#Region "Interface"

        ''' <summary>
        ''' Compute the similarity score of substitution: use a substitution matrix if the cost model
        ''' The position of the first character is 1.
        ''' A position of 0 represents a gap. 
        ''' </summary>
        ReadOnly similarity As ISimilarity(Of T)
        Public ReadOnly ToChar As ToChar(Of T)
#End Region

        ''' <summary>
        ''' Get the maximum value in the score matrix.
        ''' </summary>
        Public ReadOnly Property MaxScore() As Double
            Get
                Dim max As Double = 0

                ' skip the first row and column
                For i As Integer = 1 To queryLength
                    For j As Integer = 1 To subjectLength
                        If score(i)(j) > max Then
                            max = score(i)(j)
                        End If
                    Next
                Next

                Return max
            End Get
        End Property

        ''' <summary>
        ''' Get the alignment score between the two input strings.
        ''' </summary>
        Public ReadOnly Property AlignmentScore() As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return MaxScore / NORM_FACTOR
            End Get
        End Property

        ''' <summary>
        ''' Return a set of Matches idenfied in Dynamic programming matrix. 
        ''' A match is a pair of subsequences whose score is higher than the 
        ''' preset scoreThreshold
        ''' 
        ''' </summary>
        Public ReadOnly Property Matches(Optional scoreThreshold As Double = 19.9) As Match()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetMatches(scoreThreshold) _
                    .OrderByDescending(Function(m) m.score) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' <paramref name="similarity"/>这个函数返回来的分数越高说明二者越相似
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="subject"></param>
        ''' <param name="similarity">Blosum matrix or motif similarity</param>
        ''' <param name="asChar">Display alignment</param>
        Sub New(query As T(), subject As T(), similarity As ISimilarity(Of T), asChar As ToChar(Of T))
            Me.queryLength = query.Length
            Me.subjectLength = subject.Length
            Me.query = query
            Me.subject = subject
            Me.score = MAT(Of Double)(queryLength + 1, subjectLength + 1)
            Me.prevCells = MAT(Of Integer)(queryLength + 1, subjectLength + 1)
            Me.similarity = similarity
            Me.ToChar = asChar

            Call buildMatrix()
        End Sub

        ''' <summary>
        ''' </summary>
        ''' <param name="i"> Position of the character in str1 </param>
        ''' <param name="j"> Position of the character in str2 </param>
        ''' <returns> Cost of substitution of the character in str1 by the one in str2 </returns>
        Private Function __similarity(i As Integer, j As Integer) As Double
            If i = 0 OrElse j = 0 Then
                ' it's a gap (indel)
                Return INDEL_SCORE
            End If

            'return (str1.charAt(i - 1) == str2.charAt(j  - 1)) ? MATCH_SCORE : MISMATCH_SCORE;
            Return similarity(query(i - 1), subject(j - 1))
        End Function

        ''' <summary>
        ''' Build the score matrix using dynamic programming.
        ''' Note: The indel scores must be negative. Otherwise, the
        ''' part handling the first row and column has to be
        ''' modified.
        ''' </summary>
        Private Sub buildMatrix()
            If INDEL_SCORE >= 0 Then
                Throw New Exception("Indel score must be negative")
            End If

            Dim i As Integer        ' length of prefix substring of str1
            Dim j As Integer        ' length of prefix substring of str2

            ' base case
            score(0)(0) = 0
            prevCells(0)(0) = DR_ZERO
            ' starting point
            ' the first row
            For i = 1 To queryLength
                score(i)(0) = 0
                prevCells(i)(0) = DR_ZERO
            Next

            ' the first column
            For j = 1 To subjectLength
                score(0)(j) = 0
                prevCells(0)(j) = DR_ZERO
            Next

            ' the rest of the matrix
            For i = 1 To queryLength
                For j = 1 To subjectLength
                    Dim diagScore As Double = score(i - 1)(j - 1) + __similarity(i, j)
                    Dim upScore As Double = score(i)(j - 1) + __similarity(0, j)
                    Dim leftScore As Double = score(i - 1)(j) + __similarity(i, 0)

                    score(i)(j) = stdNum.Max(diagScore, stdNum.Max(upScore, stdNum.Max(leftScore, 0)))
                    prevCells(i)(j) = 0

                    ' find the directions that give the maximum scores.
                    ' the bitwise OR operator is used to record multiple
                    ' directions.
                    If diagScore = score(i)(j) Then
                        prevCells(i)(j) = prevCells(i)(j) Or DR_DIAG
                    End If
                    If leftScore = score(i)(j) Then
                        prevCells(i)(j) = prevCells(i)(j) Or DR_LEFT
                    End If
                    If upScore = score(i)(j) Then
                        prevCells(i)(j) = prevCells(i)(j) Or DR_UP
                    End If
                    If 0 = score(i)(j) Then
                        prevCells(i)(j) = prevCells(i)(j) Or DR_ZERO
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' given the bottom right corner point trace back  the top left conrner.
        '''  at entry: i, j hold bottom right (end of Aligment coords)
        '''  at return:  hold top left (start of Alignment coords)
        ''' </summary>
        Private Function traceback(i As Integer, j As Integer) As Integer()

            ' find out which directions to backtrack
            While True
                If (prevCells(i)(j) And DR_LEFT) > 0 Then
                    If score(i - 1)(j) > 0 Then
                        i -= 1
                    Else
                        Exit While
                    End If
                End If
                If (prevCells(i)(j) And DR_UP) > 0 Then
                    '		    return traceback(i, j-1);
                    If score(i)(j - 1) > 0 Then
                        j -= 1
                    Else
                        Exit While
                    End If
                End If
                If (prevCells(i)(j) And DR_DIAG) > 0 Then
                    '		    return traceback(i-1, j-1);
                    If score(i - 1)(j - 1) > 0 Then
                        i -= 1
                        j -= 1
                    Else
                        Exit While
                    End If
                End If
            End While
            Dim m As Integer() = New Integer() {i, j}
            Return m
        End Function

        ''' <summary>
        ''' Gets the dynmaic programming matrix
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDPMAT() As Double()()
            Dim array As New List(Of Double())

            For i As Integer = 0 To queryLength
                Dim row As New List(Of Double)

                For j As Integer = 0 To subjectLength
                    Call row.Add(score(i)(j) / NORM_FACTOR)
                Next

                Call array.Add(row.ToArray)
            Next

            Return array.ToArray
        End Function

        ''' <summary>
        ''' See this overview on implementing Smith-Waterman, including trace back. At each location in the matrix you should check 4 things:
        ''' 
        ''' If the location value equals the gap penalty plus the location above, up Is a valid move.
        ''' If the location value equals the gap penalty plus the location left, left Is a valid move.
        ''' If the location value equals the match value plus the location up And left, diagonal Is a valid move.
        ''' If the location value Is 0, you're done.
        ''' 
        ''' The first And second options correlate To inserting a gap In one Of the strings, And the third correlates To aligning two characters. 
        ''' If multiple paths work, Then you have multiple possible alignments. 
        ''' 
        ''' As the article states, the decision at that point depends largely On context (you have several options).
        ''' </summary>
        ''' <returns></returns>
        Public Function GetTraceback() As Coordinate()
            Dim path As New List(Of Coordinate)
            Call path.Add(New Coordinate(queryLength, subjectLength))
            Call getTrackback(path, queryLength, subjectLength)
            Return path.ToArray
        End Function

        Private Sub getTrackback(ByRef path As List(Of Coordinate), i As Integer, j As Integer)
            If i = 0 OrElse j = 0 Then
                Call path.Add(New Coordinate(i, j))
                Return
            End If

            Dim s As Double = score(i)(j)
            Dim diagScore As Double = score(i - 1)(j - 1) + __similarity(i, j)
            Dim upScore As Double = score(i)(j - 1) + __similarity(0, j)
            Dim leftScore As Double = score(i - 1)(j) + __similarity(i, 0)

            If s = diagScore Then
                i -= 1
                j -= 1
                Call path.Add(New Coordinate(i, j))
            End If
            If s = upScore Then
                j -= 1
                Call path.Add(New Coordinate(i, j))
            End If
            If s = leftScore Then
                i -= 1
                Call path.Add(New Coordinate(i, j))
            End If

            If s = 0 Then
                Return
            Else
                Call getTrackback(path, i, j)
            End If
        End Sub

        ''' <summary>
        ''' 返回局部最优匹配的查找结果，请注意，匹配的位置都是``从下标1开始的``！
        ''' </summary>
        ''' <param name="scoreThreshold"></param>
        ''' <returns>could be empty if no HSP scores are > scoreThreshold</returns>
        Public Iterator Function GetMatches(Optional scoreThreshold As Double = 19.9) As IEnumerable(Of Match)
            Dim fA As Integer = 0
            Dim fB As Integer = 0

            ' skip the first row and column, find the next maxScore after prevmaxScore 
            For i As Integer = 1 To queryLength
                For j As Integer = 1 To subjectLength

                    If score(i)(j) > scoreThreshold AndAlso
                       score(i)(j) > score(i - 1)(j - 1) AndAlso
                       score(i)(j) > score(i - 1)(j) AndAlso
                       score(i)(j) > score(i)(j - 1) Then

                        If i = queryLength OrElse
                           j = subjectLength OrElse
                           score(i)(j) > score(i + 1)(j + 1) Then

                            ' should be lesser than prev maxScore					    	
                            fA = i
                            fB = j

                            With traceback(fA, fB)
                                ' sets the x, y to startAlignment coordinates
                                Yield New Match(.ByRef(0), i, .ByRef(1), j, score(i)(j) / NORM_FACTOR)
                            End With
                        End If
                    End If
                Next
            Next
        End Function
    End Class
End Namespace
