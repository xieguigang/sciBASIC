#Region "Microsoft.VisualBasic::428c0b7e7483866e25b8b7fe4284e7c3, Data_science\DataMining\DynamicProgramming\CenterStar\CenterStar.vb"

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

    '   Total Lines: 210
    '    Code Lines: 147 (70.00%)
    ' Comment Lines: 33 (15.71%)
    '    - Xml Docs: 78.79%
    ' 
    '   Blank Lines: 30 (14.29%)
    '     File Size: 7.08 KB


    ' Class CenterStar
    ' 
    '     Properties: NameList
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: calculateTotalCost, Compute, computeInternal
    ' 
    '     Sub: findStarIndex, multipleAlignmentImpl
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

''' <summary>
''' ##### Multiple-sequence-alignment
''' 
''' This program calculates the multiple sequence alignment of k>1 DNA sequences.
''' 
''' The program use the Matrix.txt file For the substitution matrix. The matrix 
''' can be changed, And it used With Default values As: 
''' 
''' + 0 - Match
''' + 1 - Missmatch
''' + 2 - Indel
''' 
''' Algorithm used For this purpose Is Center Star Algotrithm
''' 
''' > https://github.com/EranCohenSW/Multiple-sequence-alignment/blob/master/Project/src/CenterStar.java
''' </summary>
Public Class CenterStar

    Dim starIndex%
    Dim centerString$
    Dim globalAlign$() = New String(2) {}
    Dim multipleAlign$()
    Dim sequence$()
    Dim totalScore# = 0
    Dim names$()
    Dim kband As KBandSearch

    Public ReadOnly Property NameList As String()
        Get
            Return names
        End Get
    End Property

    Sub New(input As IEnumerable(Of NamedValue(Of String)))
        With input.ToArray
            sequence = .Select(Function(fa) fa.Value) _
                       .ToArray
            names = .Select(Function(fa) fa.Name) _
                    .ToArray
        End With

        kband = New KBandSearch(globalAlign)
    End Sub

    Sub New(input As IEnumerable(Of String))
        sequence = input.ToArray
        names = sequence.Select(Function(s, i) "seq" & i).ToArray
        kband = New KBandSearch(globalAlign)
    End Sub

    ''' <summary>
    ''' Main
    ''' </summary>
    ''' <param name="matrix">得分矩阵</param>
    ''' <returns></returns>
    Public Function Compute(matrix As IScore(Of Char), ByRef alignment As String()) As Double
        Dim totalCost#

        If sequence.All(Function(s) s = sequence(Scan0)) Then
            ' 所输入的序列全部都是一样的？？
            alignment = sequence.ToArray
            totalCost = 0
        Else
#If DEBUG Then
            totalCost = computeInternal(matrix) 
#Else
            Try
                totalCost = computeInternal(matrix)
            Catch ex As Exception
                Throw New Exception(sequence.JoinBy(vbCrLf), ex)
            End Try
#End If
            alignment = multipleAlign.ToArray
        End If

        Return totalCost
    End Function

    Private Function computeInternal(matrix As IScore(Of Char)) As Double
        Dim n As Integer = sequence.Length

        findStarIndex()
        centerString = sequence(starIndex)
        multipleAlign = New String(n - 1) {}
        multipleAlignmentImpl()

        Return calculateTotalCost(matrix, n)
    End Function

    ''' <summary>
    ''' this Function calculate() the total cost
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Private Function calculateTotalCost(matrix As IScore(Of Char), n%) As Double
        Dim length = multipleAlign(0).Length

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                If (j > i) Then
                    For k As Integer = 0 To length - 1
                        Dim ic As Char = multipleAlign(i)(k)
                        Dim jc As Char = multipleAlign(j)(k)

                        totalScore += matrix.GetSimilarityScore(ic, jc)
                    Next
                End If
            Next
        Next

        Return totalScore
    End Function

    ''' <summary>
    ''' The Function do the multiple alignment according to the center string 
    ''' </summary>
    Private Sub multipleAlignmentImpl()
        Dim centerString2$ = centerString
        Dim n = sequence.Length

        For i As Integer = 0 To n - 1
            If (i = starIndex) Then
                multipleAlign(i) = centerString2
                Continue For
            End If

            kband.CalculateEditDistance(centerString2, sequence(i))
            multipleAlign(i) = globalAlign(1)

            If (globalAlign(0).Length > centerString2.Length) Then
                Dim j2 = 0

                For j1 As Integer = 0 To globalAlign(0).Length - 1
                    If (centerString2.CharAtOrDefault(j2, "-"c) <> globalAlign(0)(j1)) Then
                        Dim a As StringBuilder

                        For k As Integer = 0 To i - 1
                            With multipleAlign(k)
                                If .Length > j1 Then
                                    a = New StringBuilder(multipleAlign(k))
                                    a.Insert(j1, "-"c)
                                    multipleAlign(k) = a.ToString
                                Else
                                    multipleAlign(k) = .ByRef & New String("-"c, j1 - .Length)
                                End If
                            End With
                        Next

                    Else
                        j2 += 1
                    End If
                Next
                centerString2 = globalAlign(0)
            ElseIf (globalAlign(0).Length < centerString2.Length) Then
                Dim j2 = 0
                Dim globalAlign0 = globalAlign(Scan0)

                For j1 As Integer = 0 To centerString2.Length - 1
                    If (centerString2(j1) <> globalAlign0.CharAtOrDefault(j2)) Then
                        With multipleAlign(i)
                            If .Length > j1 Then
                                Dim a As New StringBuilder(multipleAlign(i))
                                a.Insert(j1, "-"c)
                                multipleAlign(i) = a.ToString()
                            Else
                                multipleAlign(i) = .ByRef & New String("-"c, j1 - .Length)
                            End If
                        End With
                    Else
                        j2 += 1
                    End If
                Next
            End If
        Next

        Dim maxLen = Aggregate a In multipleAlign Into Max(a.Length)

        For i As Integer = 0 To multipleAlign.Length - 1
            multipleAlign(i) &= New String("-"c, maxLen - multipleAlign(i).Length)
        Next
    End Sub

    ''' <summary>
    ''' This Function finds the minimum star cost from all sequences
    ''' </summary>
    Private Sub findStarIndex()
        Dim editDist = 0
        Dim minEditDist = Integer.MaxValue
        Dim n = sequence.Length

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                editDist += kband.CalculateEditDistance(sequence(i), sequence(j))
            Next

            If (editDist < minEditDist) Then
                minEditDist = editDist
                starIndex = i
            End If

            editDist = 0
        Next
    End Sub
End Class
