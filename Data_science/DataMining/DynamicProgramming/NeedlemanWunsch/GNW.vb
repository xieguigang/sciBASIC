#Region "Microsoft.VisualBasic::f9a2dac2c06ac9bd3cdefc00815698e3, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\GNW.vb"

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

    '   Total Lines: 199
    '    Code Lines: 105 (52.76%)
    ' Comment Lines: 66 (33.17%)
    '    - Xml Docs: 56.06%
    ' 
    '   Blank Lines: 28 (14.07%)
    '     File Size: 8.43 KB


    '     Class NeedlemanWunsch
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Compute, MaximizingCell
    ' 
    '         Sub: traceback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace NeedlemanWunsch

    ''' <summary>
    ''' ## Needleman-Wunsch Algorithm
    ''' 
    ''' Bioinformatics 1, WS 15/16
    ''' Dr. Kay Nieselt and Alexander Seitz
    ''' </summary>
    Public Class NeedlemanWunsch(Of T) : Inherits Workspace(Of T)

        Dim matrix!()() = Nothing
        Dim tracebackMatrix%()() = Nothing

        ReadOnly symbol As GenericSymbol(Of T)

        Sub New(q As IEnumerable(Of T), s As IEnumerable(Of T), score As ScoreMatrix(Of T), symbol As GenericSymbol(Of T))
            Call Me.New(score, symbol)

            Sequence1 = q.ToArray
            Sequence2 = s.ToArray
        End Sub

        Sub New(score As ScoreMatrix(Of T), symbol As GenericSymbol(Of T))
            Call MyBase.New(score, symbol.m_viewChar)

            Me.symbol = symbol
        End Sub

        ''' <summary>
        '''	this function is called for the first time with two empty stacks
        '''	and the end indices of the matrix
        '''	
        '''	the function computes a traceback over the matrix, it calls itself recursively
        '''	for each sequence, it pushes the aligned character (a,c,g,t or -)
        '''	on a stack (use java.util.Stack with the function push()) 
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        Protected Sub traceback(s1 As Stack(Of T), s2 As Stack(Of T), i As Integer, j As Integer)
            Dim direction As Integer = tracebackMatrix(i - 1)(j - 1)

            Select Case direction
                Case 0
                    ' end of the global alignment
                    '			
                    '			 * TODO format the aligned sequences, such that they are in the fasta-format!
                    '			 * i.e. remove any unsupported fasta characters, such as "," " " or "[" ...
                    '			 * think of a clever object oriented way to do so!
                    '			 

                    Dim aligned1 As T() = s1.ToArray
                    Dim aligned2 As T() = s2.ToArray

                    ' Call Array.Reverse(aligned1)
                    ' Call Array.Reverse(aligned2)

                    Me.AddAligned1(aligned1)
                    Me.AddAligned2(aligned2)
                Case 1 ' upper cell
                    s1.Push(symbol.getEmpty)
                    s2.Push(Me.Sequence2(i - 2))
                    Me.traceback(s1, s2, i - 1, j)
                Case 2 ' upperLeft cell
                    s1.Push(Me.Sequence1(j - 2))
                    s2.Push(Me.Sequence2(i - 2))
                    Me.traceback(s1, s2, i - 1, j - 1)
                Case 3 ' upper + upperLeft cell
                    s1.Push(symbol.getEmpty)
                    s2.Push(Me.Sequence2(i - 2))
                    Me.traceback(s1, s2, i - 1, j)
                Case 4 'left cell
                    s1.Push(Me.Sequence1(j - 2))
                    s2.Push(symbol.getEmpty)
                    Me.traceback(s1, s2, i, j - 1)
                Case 5 ' left + upper cell
                    s1.Push(Me.Sequence1(j - 2))
                    s2.Push(symbol.getEmpty)
                    Me.traceback(s1, s2, i, j - 1)
                Case 6 ' left + upperLeft cell
                    s1.Push(Me.Sequence1(j - 2))
                    s2.Push(symbol.getEmpty)
                    Me.traceback(s1, s2, i, j - 1)
                Case 7 ' all 3 cells
                    s1.Push(symbol.getEmpty)
                    s2.Push(Me.Sequence2(i - 2))
                    Me.traceback(s1, s2, i - 1, j)
            End Select
        End Sub

        ''' <summary>
        ''' computes the matrix for the Needleman-Wunsch Algorithm
        ''' </summary>
        ''' <remarks>	
        ''' this function computes the NW-algorithm with linear gap-costs
        ''' 
        '''  - first make yourself familiar with this function and the functions used to compute the resulting alignment!
        '''  
        '''  - modify the functions used in this class such that the NW algorithm is modular
        '''    i.e. the following criteria should be fulfilled: 
        '''        - it should be easy to replace the linear gap cost function with an affine gap cost function
        '''        - the initialization step, fill and traceback should be modular, to allow
        '''          to switch between different algorithms later (NW, SW, OverlapAlignment etc.)
        '''    
        '''  - you are allowed to change the class structure, if you think that it is necessary!
        '''    (make sure to use object oriented programming concepts, i.e. use objects to abstract your code 
        '''   	-> don't do everything in a single class)    	 
        ''' </remarks>
        Public Function Compute() As NeedlemanWunsch(Of T)

            ' Set the number of rows and columns
            Dim rows As Integer = Me.Sequence2.Length + 1 ' number of rows
            Dim columns As Integer = Me.Sequence1.Length + 1 ' number of columns

            ' Strings, which will be aligned
            Dim seq1 As T() = Me.Sequence1
            Dim seq2 As T() = Me.Sequence2

            ' Set up the score- and the traceback-matrix
            matrix = RectangularArray.Matrix(Of Single)(rows, columns)
            tracebackMatrix = RectangularArray.Matrix(Of Integer)(rows, columns)

            ' fill the first row and first column of matrix and tracebackMatrix
            For i As Integer = 0 To rows - 1
                matrix(i)(0) = -i * scoreMatrix.GapPenalty
                tracebackMatrix(i)(0) = 1 ' see explanation below
            Next

            For j As Integer = 0 To columns - 1
                matrix(0)(j) = -j * scoreMatrix.GapPenalty
                tracebackMatrix(0)(j) = 4 ' see explanation below
            Next

            tracebackMatrix(0)(0) = 0

            Dim a, b, c As Double
            Dim max As Double

            ' Fill matrix and traceback matrix
            For i As Integer = 1 To rows - 1
                For j As Integer = 1 To columns - 1
                    a = matrix(i - 1)(j - 1) + scoreMatrix.getMatchScore(seq1(j - 1), seq2(i - 1))
                    b = matrix(i)(j - 1) - scoreMatrix.GapPenalty
                    c = matrix(i - 1)(j) - scoreMatrix.GapPenalty
                    max = Math.Max(a, b, c)

                    ' fill cell of the scoring matrix
                    matrix(i)(j) = max
                    ' fill cell of the traceback matrix
                    tracebackMatrix(i)(j) = MaximizingCell(a, b, c)
                Next
            Next

            ' set the alignment score
            Me.Score = matrix(rows - 1)(columns - 1)

            ' call the traceback function
            Me.traceback(New Stack(Of T), New Stack(Of T), rows, columns)

            Return Me
        End Function

        ''' <summary>
        ''' return the maximizing cell(s)
        ''' 
        ''' 1, if the maximizing cell is the upper cell
        ''' 2, if the maximizing cell is the left-upper cell
        ''' 4, if the maximizing cell is the left cell
        ''' 
        ''' if there are more than one maximizing cells, the values are summed up
        ''' </summary>
        ''' <param name="upperLeft"> </param>
        ''' <param name="left"> </param>
        ''' <param name="upper"> </param>
        ''' <returns> code for the maximizing cell(s) </returns>
        Private Function MaximizingCell(upperLeft As Integer, left As Integer, upper As Integer) As Integer
            Dim max As Integer = Math.Max(upperLeft, left, upper)

            If upperLeft = left AndAlso left = upper Then
                Return 7
            ElseIf upper = upperLeft AndAlso upperLeft = max Then
                Return 3
            ElseIf upper = left AndAlso left = max Then
                Return 5
            ElseIf upperLeft = left AndAlso left = max Then
                Return 6
            ElseIf upper = max Then
                Return 1
            ElseIf upperLeft = max Then
                Return 2
            Else
                Return 4
            End If
        End Function
    End Class
End Namespace
