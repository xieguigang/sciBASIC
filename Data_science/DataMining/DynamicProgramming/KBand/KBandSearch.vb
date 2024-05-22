#Region "Microsoft.VisualBasic::969abd21470b422ac98d3c2763e066ad, Data_science\DataMining\DynamicProgramming\KBand\KBandSearch.vb"

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

    '   Total Lines: 134
    '    Code Lines: 90 (67.16%)
    ' Comment Lines: 20 (14.93%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 24 (17.91%)
    '     File Size: 3.69 KB


    ' Class KBandSearch
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CalculateEditDistance, calculateMinimum
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Public Class KBandSearch

    ReadOnly globalAlign$()

    Sub New(ByRef globalAlign$())
        Me.globalAlign = globalAlign
    End Sub

    ''' <summary>
    ''' Global alignment and function to calculate the edit distances
    ''' 
    ''' + 0   diagonal
    ''' + 1   left
    ''' + 2   up
    ''' 
    ''' </summary>
    ''' <param name="seq1$"></param>
    ''' <param name="seq2$"></param>
    ''' <returns></returns>
    Public Function CalculateEditDistance(seq1$, seq2$) As Integer
        Dim l1 = seq1.Length
        Dim l2 = seq2.Length
        Dim direction%

        If (seq1 = seq2) Then
            Return 0
        End If

        Dim i, j, k As Integer
        Dim score()() = RectangularArray.Matrix(Of Integer)(l1 + 1, l2 + 1)
        Dim trace()() = RectangularArray.Matrix(Of Integer)(l1 + 1, l2 + 1)
        Dim match = 0

        score(0)(0) = 0
        trace(0)(0) = 0

        For i = 1 To l2 - 1
            score(0)(i) = i
            trace(0)(i) = 1
        Next
        For j = 1 To l1 - 1
            score(j)(0) = j
            trace(j)(0) = 2
        Next

        ' Filling the remaining cells in the matrix
        For i = 1 To l1 - 1
            For j = 1 To l2 - 1
                If (seq1(i - 1) = seq2(j - 1)) Then
                    match = 0
                Else
                    match = 1
                End If
                score(i)(j) = calculateMinimum(score(i - 1)(j - 1) + match, score(i)(j - 1) + 1, score(i - 1)(j) + 1, direction)
                trace(i)(j) = direction
            Next
        Next

        ' Creating the global alignment by the trace found
        i = l1
        j = l2
        k = 0

        Dim pairAlignment As Char()() = RectangularArray.Matrix(Of Char)(2, l1 + l2)

        Do While i <> 0 OrElse j <> 0
            If (trace(i)(j) = 0) Then
                pairAlignment(0)(k) = seq1(i - 1)
                pairAlignment(1)(k) = seq2(j - 1)
                i -= 1
                j -= 1
                k += 1

            ElseIf (trace(i)(j) = 1) Then
                pairAlignment(0)(k) = "-"c
                pairAlignment(1)(k) = seq2(j - 1)
                j -= 1
                k += 1

            Else
                pairAlignment(0)(k) = seq1(i - 1)
                pairAlignment(1)(k) = "-"c
                i -= 1
                k += 1
            End If
        Loop

        Dim input$
        Dim stringReverse = RectangularArray.Matrix(Of Char)(2, k)

        i = 0

        Do While (k > 0)
            stringReverse(0)(i) = pairAlignment(0)(k - 1)
            stringReverse(1)(i) = pairAlignment(1)(k - 1)
            i += 1
            k -= 1
        Loop

        input = New String(stringReverse(0))
        globalAlign(0) = input
        input = New String(stringReverse(1))
        globalAlign(1) = input

        Return score(l1)(l2)
    End Function

    ''' <summary>
    ''' This Function calculates the minimum choice of three choices in the next move
    ''' </summary>
    ''' <param name="diagonal%"></param>
    ''' <param name="left%"></param>
    ''' <param name="up%"></param>
    ''' <returns></returns>
    Public Function calculateMinimum(diagonal%, left%, up%, ByRef direction%) As Integer
        Dim temp = diagonal

        direction = 0

        If (temp > left) Then
            temp = left
            direction = 1
        End If

        If (temp > up) Then
            temp = up
            direction = 2
        End If

        Return temp
    End Function
End Class
