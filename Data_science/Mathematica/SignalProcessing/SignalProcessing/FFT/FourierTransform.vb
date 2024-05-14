#Region "Microsoft.VisualBasic::b6405285d4f8fa76564909c9678e07e5, Data_science\Mathematica\SignalProcessing\SignalProcessing\FFT\FourierTransform.vb"

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

    '   Total Lines: 351
    '    Code Lines: 191
    ' Comment Lines: 109
    '   Blank Lines: 51
    '     File Size: 12.68 KB


    '     Module FourierTransform
    ' 
    ' 
    '         Enum Direction
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: GetComplexRotation, GetReversedBits
    ' 
    '     Sub: DFT, DFT2, FFT, FFT2, ReorderData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' AForge Math Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © Andrew Kirillov, 2005-2009
' andrew.kirillov@aforgenet.com
'
' FFT idea from Exocortex.DSP library
' http://www.exocortex.org/dsp/
'

Imports System.Numerics

Namespace FFT

    ''' <summary>
    ''' Fourier transformation.
    ''' </summary>
    ''' 
    ''' <remarks>The class implements one dimensional and two dimensional
    ''' Discrete and Fast Fourier Transformation.</remarks>
    ''' 
    Public Module FourierTransform

        ''' <summary>
        ''' Fourier transformation direction.
        ''' </summary>
        Public Enum Direction
            ''' <summary>
            ''' Forward direction of Fourier transformation.
            ''' </summary>
            Forward = 1

            ''' <summary>
            ''' Backward direction of Fourier transformation.
            ''' </summary>
            Backward = -1
        End Enum

        ''' <summary>
        ''' One dimensional Discrete Fourier Transform.
        ''' </summary>
        ''' 
        ''' <param name="data">Data to transform.</param>
        ''' <param name="direction">Transformation direction.</param>
        ''' 
        Public Sub DFT(data As Complex(), direction As Direction)
            Dim n As Integer = data.Length
            Dim arg As Double
            Dim dst As Complex() = New Complex(n - 1) {}

            ' for each destination element
            For i As Integer = 0 To n - 1
                dst(i) = Complex.Zero

                arg = -CInt(direction) * 2.0 * System.Math.PI * CDbl(i) / CDbl(n)

                ' sum source elements
                For j As Integer = 0 To n - 1
                    dst(i) += data(j) * Complex.FromPolarCoordinates(1.0, j * arg)
                Next
            Next

            ' copy elements
            If direction = Direction.Forward Then
                ' devide also for forward transform
                For i As Integer = 0 To n - 1
                    data(i) = dst(i) / n
                Next
            Else
                For i As Integer = 0 To n - 1
                    data(i) = dst(i)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Two dimensional Discrete Fourier Transform.
        ''' </summary>
        ''' 
        ''' <param name="data">Data to transform.</param>
        ''' <param name="direction">Transformation direction.</param>
        ''' 
        Public Sub DFT2(data As Complex(,), direction As Direction)
            Dim n As Integer = data.GetLength(0)
            ' rows
            Dim m As Integer = data.GetLength(1)
            ' columns
            Dim arg As Double
            Dim dst As Complex() = New Complex(System.Math.Max(n, m) - 1) {}

            ' process rows
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    dst(j) = Complex.Zero

                    arg = -CInt(direction) * 2.0 * System.Math.PI * CDbl(j) / CDbl(m)

                    ' sum source elements
                    For k As Integer = 0 To m - 1
                        dst(j) += data(i, k) * Complex.FromPolarCoordinates(1.0, k * arg)
                    Next
                Next

                ' copy elements
                If direction = Direction.Forward Then
                    ' devide also for forward transform
                    For j As Integer = 0 To m - 1
                        data(i, j) = dst(j) / m
                    Next
                Else
                    For j As Integer = 0 To m - 1
                        data(i, j) = dst(j)
                    Next
                End If
            Next

            ' process columns
            For j As Integer = 0 To m - 1
                For i As Integer = 0 To n - 1
                    dst(i) = Complex.Zero

                    arg = -CInt(direction) * 2.0 * System.Math.PI * CDbl(i) / CDbl(n)

                    ' sum source elements
                    For k As Integer = 0 To n - 1
                        dst(i) += data(k, j) * Complex.FromPolarCoordinates(1.0, k * arg)
                    Next
                Next

                ' copy elements
                If direction = Direction.Forward Then
                    ' devide also for forward transform
                    For i As Integer = 0 To n - 1
                        data(i, j) = dst(i) / n
                    Next
                Else
                    For i As Integer = 0 To n - 1
                        data(i, j) = dst(i)
                    Next
                End If
            Next
        End Sub


        ''' <summary>
        ''' One dimensional Fast Fourier Transform.
        ''' </summary>
        ''' 
        ''' <param name="data">Data to transform.</param>
        ''' <param name="direction">Transformation direction.</param>
        ''' 
        ''' <remarks><para><note>The method accepts <paramref name="data"/> array of 2<sup>n</sup> size
        ''' only, where <b>n</b> may vary in the [1, 14] range.</note></para></remarks>
        ''' 
        ''' <exception cref="ArgumentException">Incorrect data length.</exception>
        ''' 
        Public Sub FFT(data As Complex(), direction As Direction)
            Dim n As Integer = data.Length
            Dim m As Integer = Log2(n)

            ' reorder data first
            ReorderData(data)

            ' compute FFT
            Dim tn As Integer = 1, tm As Integer

            For k As Integer = 1 To m
                Dim rotation As Complex() = FourierTransform.GetComplexRotation(k, direction)

                tm = tn
                tn <<= 1

                For i As Integer = 0 To tm - 1
                    Dim t = rotation(i)

                    Dim even As Integer = i
                    While even < n
                        Dim odd As Integer = even + tm
                        Dim ce = data(even)
                        Dim cot = data(odd) * t

                        data(even) += cot
                        data(odd) = ce - cot
                        even += tn
                    End While
                Next
            Next

            If direction = Direction.Forward Then
                For i As Integer = 0 To n - 1
                    data(i) /= CDbl(n)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Two dimensional Fast Fourier Transform.
        ''' </summary>
        ''' 
        ''' <param name="data">Data to transform.</param>
        ''' <param name="direction">Transformation direction.</param>
        ''' 
        ''' <remarks><para><note>The method accepts <paramref name="data"/> array of 2<sup>n</sup> size
        ''' only in each dimension, where <b>n</b> may vary in the [1, 14] range. For example, 16x16 array
        ''' is valid, but 15x15 is not.</note></para></remarks>
        ''' 
        ''' <exception cref="ArgumentException">Incorrect data length.</exception>
        ''' 
        Public Sub FFT2(data As Complex(,), direction As Direction)
            Dim k As Integer = data.GetLength(0)
            Dim n As Integer = data.GetLength(1)

            ' check data size
            If (Not IsPowerOf2(k)) OrElse (Not IsPowerOf2(n)) OrElse (k < minLength) OrElse (k > maxLength) OrElse (n < minLength) OrElse (n > maxLength) Then
                Throw New ArgumentException("Incorrect data length.")
            End If

            ' process rows
            Dim row As Complex() = New Complex(n - 1) {}

            For i As Integer = 0 To k - 1
                ' copy row
                For j As Integer = 0 To n - 1
                    row(j) = data(i, j)
                Next
                ' transform it
                FourierTransform.FFT(row, direction)
                ' copy back
                For j As Integer = 0 To n - 1
                    data(i, j) = row(j)
                Next
            Next

            ' process columns
            Dim col As Complex() = New Complex(k - 1) {}

            For j As Integer = 0 To n - 1
                ' copy column
                For i As Integer = 0 To k - 1
                    col(i) = data(i, j)
                Next
                ' transform it
                FourierTransform.FFT(col, direction)
                ' copy back
                For i As Integer = 0 To k - 1
                    data(i, j) = col(i)
                Next
            Next
        End Sub

#Region "Private Region"

        Private Const minLength As Integer = 2
        Private Const maxLength As Integer = 16384
        Private Const minBits As Integer = 1
        Private Const maxBits As Integer = 14

        Private reversedBits As Integer()() = New Integer(maxBits - 1)() {}
        Private complexRotation As Complex(,)() = New Complex(maxBits - 1, 1)() {}

        ''' <summary>
        ''' Get array, indicating which data members should be swapped before FFT
        ''' </summary>
        ''' <param name="numberOfBits"></param>
        ''' <returns></returns>
        Private Function GetReversedBits(numberOfBits As Integer) As Integer()
            If (numberOfBits < minBits) OrElse (numberOfBits > maxBits) Then
                Throw New ArgumentOutOfRangeException()
            End If

            ' check if the array is already calculated
            If reversedBits(numberOfBits - 1) Is Nothing Then
                Dim n As Integer = Pow2(numberOfBits)
                Dim rBits As Integer() = New Integer(n - 1) {}

                ' calculate the array
                For i As Integer = 0 To n - 1
                    Dim oldBits As Integer = i
                    Dim newBits As Integer = 0

                    For j As Integer = 0 To numberOfBits - 1
                        newBits = (newBits << 1) Or (oldBits And 1)
                        oldBits = (oldBits >> 1)
                    Next
                    rBits(i) = newBits
                Next
                reversedBits(numberOfBits - 1) = rBits
            End If
            Return reversedBits(numberOfBits - 1)
        End Function

        ''' <summary>
        ''' Get rotation of complex number
        ''' </summary>
        ''' <param name="numberOfBits"></param>
        ''' <param name="direction__1"></param>
        ''' <returns></returns>
        Private Function GetComplexRotation(numberOfBits As Integer, direction__1 As Direction) As Complex()
            Dim directionIndex As Integer = If((direction__1 = Direction.Forward), 0, 1)

            ' check if the array is already calculated
            If complexRotation(numberOfBits - 1, directionIndex) Is Nothing Then
                Dim n As Integer = 1 << (numberOfBits - 1)
                Dim uR As Double = 1.0
                Dim uI As Double = 0.0
                Dim angle As Double = System.Math.PI / n * CInt(direction__1)
                Dim wR As Double = System.Math.Cos(angle)
                Dim wI As Double = System.Math.Sin(angle)
                Dim t As Double
                Dim rotation As Complex() = New Complex(n - 1) {}

                For i As Integer = 0 To n - 1
                    rotation(i) = New Complex(uR, uI)
                    t = uR * wI + uI * wR
                    uR = uR * wR - uI * wI
                    uI = t
                Next

                complexRotation(numberOfBits - 1, directionIndex) = rotation
            End If
            Return complexRotation(numberOfBits - 1, directionIndex)
        End Function

        ''' <summary>
        ''' Reorder data for FFT using
        ''' </summary>
        ''' <param name="data"></param>
        Private Sub ReorderData(data As Complex())
            Dim len As Integer = data.Length

            ' check data length
            If (len < minLength) OrElse (len > maxLength) OrElse (Not IsPowerOf2(len)) Then
                Throw New ArgumentException("Incorrect data length.")
            End If

            Dim rBits As Integer() = GetReversedBits(Log2(len))

            For i As Integer = 0 To len - 1
                Dim s As Integer = rBits(i)

                If s > i Then
                    Dim t As Complex = data(i)
                    data(i) = data(s)
                    data(s) = t
                End If
            Next
        End Sub
#End Region
    End Module
End Namespace
