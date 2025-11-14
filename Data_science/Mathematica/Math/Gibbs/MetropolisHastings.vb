#Region "Microsoft.VisualBasic::8a213793d892e2bab4ae22fd3490fdb6, Data_science\Mathematica\Math\Gibbs\MetropolisHastings.vb"

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

    '   Total Lines: 162
    '    Code Lines: 102 (62.96%)
    ' Comment Lines: 26 (16.05%)
    '    - Xml Docs: 42.31%
    ' 
    '   Blank Lines: 34 (20.99%)
    '     File Size: 4.74 KB


    ' Class MetropolisHastings
    ' 
    '     Function: calcChiSquare, calcTransProb
    ' 
    '     Sub: Main, RunSampling
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Public Class MetropolisHastings

    Const NUM_ITERATIONS = 1000000

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dim1"></param>
    ''' <param name="dim2"></param>
    ''' <param name="observed">
    ''' should be has element size equals to ``<paramref name="dim1"/>*<paramref name="dim2"/>``.
    ''' </param>
    ''' <param name="markovBasis">
    ''' store moves of markov basis
    ''' 
    ''' matrix in size of: RectangularArray.Matrix(Of Integer)(dim3, dim4)
    ''' </param>
    Public Shared Sub Main(dim1 As Integer, dim2 As Integer, observed As Integer(), dim3 As Integer, dim4 As Integer, markovBasis As Integer()())
        Dim chiSquareValues As New Dictionary(Of Double?, Integer?)()

        If observed.Length <> dim1 * dim2 Then
            Throw New InvalidDataException
        End If

        Dim columnSums = New Integer(dim1 - 1) {}
        Dim total = 0

        'add up columns
        For i = 0 To dim1 - 1
            For j = 0 To dim2 - 1
                total += observed(i * dim2 + j)
            Next

            columnSums(i) = total
            total = 0
        Next

        Dim rowSums = New Integer(dim2 - 1) {}

        'add up rows
        For i = 0 To dim2 - 1
            For j = 0 To dim1 - 1
                total += observed(j * dim2 + i)
            Next

            rowSums(i) = total
            total = 0
        Next

        Dim n = 0

        'get total
        For i = 0 To dim2 - 1
            n += columnSums(i)
        Next

        'null hypothesis: expected
        Dim expected As Double() = New Double(dim1 * dim2 - 1) {}

        'calculate expected independence values
        For i = 0 To dim1 - 1
            For j = 0 To dim2 - 1
                expected(i * dim1 + j) = columnSums(i) * rowSums(j) / n
            Next
        Next

        'algorithm
        Call RunSampling(dim3, dim4, observed, expected, markovBasis)
    End Sub

    Public Shared Sub RunSampling(dim3 As Integer, dim4 As Integer, observed As Integer(), expected As Double(), markovBasis As Integer()())
        'calc Chi-Square for observed compared to expected
        Dim obs As Double = calcChiSquare(observed, expected)
        Dim rand As Random = New Random()
        Dim markovPick = 0
        Dim tempMarkov = New Integer(dim4 - 1) {}
        Dim epsPick = 0
        Dim inFiber = True
        Dim u As Double = 0
        Dim sig = 0

        'copy observed to x
        Dim x = New Integer(observed.Length - 1) {}

        For i = 0 To observed.Length - 1
            x(i) = observed(i)
        Next

        For i = 0 To NUM_ITERATIONS - 1
            'initialize
            inFiber = True

            'step 2
            markovPick = rand.Next(dim3)
            epsPick = rand.Next(2)

            For j = 0 To dim4 - 1
                tempMarkov(j) = CInt((-1 ^ epsPick) * markovBasis(markovPick)(j))
            Next

            'step 3: test for any negative values
            For j = 0 To x.Length - 1
                If x(j) + tempMarkov(j) < 0 Then
                    inFiber = False
                End If
            Next

            'step 4
            If inFiber Then
                u = rand.NextDouble()

                If u < calcTransProb(x, tempMarkov) Then
                    For k = 0 To x.Length - 1
                        x(k) = x(k) + tempMarkov(k)
                    Next
                End If
            End If

            Dim chiSquare = calcChiSquare(x, expected)

            'step 5
            If chiSquare >= obs Then
                sig += 1
            End If
        Next

        Console.WriteLine("p-value: " & sig / NUM_ITERATIONS.ToString())
    End Sub

    Public Shared Function calcTransProb(x As Integer(), markov As Integer()) As Double
        Dim prob As Double = 1

        For i = 0 To x.Length - 1
            If markov(i) > 0 Then
                For j = 1 To markov(i)
                    prob = prob / (x(i) + j)
                Next
            End If

            If markov(i) < 0 Then
                For j = 0 To markov(i) * -1 - 1
                    prob = prob * (x(i) - j)
                Next
            End If
        Next

        Return prob
    End Function

    Public Shared Function calcChiSquare(observed As Integer(), expected As Double()) As Double
        Dim total As Double = 0

        For i = 0 To observed.Length - 1
            total += ((observed(i) - expected(i)) ^ 2) / expected(i)
        Next

        Return total
    End Function
End Class
