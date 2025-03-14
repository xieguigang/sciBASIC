#Region "Microsoft.VisualBasic::246cdc21d631273a2f90adf6edbf5ee3, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\MDS\SMACOF.vb"

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

    '   Total Lines: 429
    '    Code Lines: 278 (64.80%)
    ' Comment Lines: 112 (26.11%)
    '    - Xml Docs: 84.82%
    ' 
    '   Blank Lines: 39 (9.09%)
    '     File Size: 17.29 KB


    '     Class MDSMethod
    ' 
    '         Properties: Dissimilarities, Positions, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: iterate, weightMatrix
    ' 
    '     Class SMACOF
    ' 
    '         Properties: NormalizedStressProp, StressProp
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) iterate, (+2 Overloads) majorize, (+2 Overloads) normalizedStress, (+2 Overloads) stress
    ' 
    '         Sub: majorize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdf = System.Math

' 
'  Copyright (C) 2014. Daniel Asarnow
' 
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' 
'  This program is distributed in the hope that it will be useful,
'  but WITHOUT ANY WARRANTY; without even the implied warranty of
'  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'  GNU General Public License for more details.
' 
'  You should have received a copy of the GNU General Public License
'  along with this program.  If not, see <http://www.gnu.org/licenses/>.
' 

Namespace LinearAlgebra.Matrix.MDSScale

    Public MustInherit Class MDSMethod

        Protected x As Double()()
        Protected d As Double()()
        Protected w As Double()()

        Public Overridable Property Dissimilarities As Double()()
            Get
                Return d
            End Get
            Set(value As Double()())
                d = value
            End Set
        End Property

        Public Overridable Property Weights As Double()()
            Get
                Return w
            End Get
            Set(value As Double()())
                w = value
            End Set
        End Property

        Public Overridable Property Positions As Double()()
            Get
                Return x
            End Get
            Set(value As Double()())
                x = value
            End Set
        End Property

        Public Sub New(d As Double()(), x As Double()(), w As Double()())
            Me.x = x
            Me.d = d
            Me.w = w
        End Sub

        ''' <summary>
        ''' Perform 1 majorization iteration using this SMACOF instance. </summary>
        ''' <returns> report </returns>
        Public Function iterate() As String
            Return iterate(1)
        End Function

        ''' <summary>
        ''' Perform n majorization iterations using this SMACOF instance. </summary>
        ''' <param name="n"> number of iterations </param>
        ''' <returns> report </returns>
        Public MustOverride Function iterate(n As Integer) As String

        ''' <summary>
        ''' Perform majorization iterations until the maximum number of iterations
        ''' is reached, the maximum runtime has elapsed, or the change in
        ''' normalized stress falls below the threshold, whichever comes first. </summary>
        ''' <param name="iter"> maximum number of iterations </param>
        ''' <param name="threshold"> threshold for change in normalized stress </param>
        ''' <returns> report </returns>
        Public MustOverride Function iterate(iter As Integer, threshold As Integer) As String

        ''' <summary>
        ''' Element-wise matrix exponentiation for self-weighting of distances. </summary>
        ''' <param name="D"> distance matrix or initial weights </param>
        ''' <param name="exponent"> power to raise each element the matrix </param>
        ''' <returns> exponentiated weights </returns>
        Public Shared Function weightMatrix(D As Double()(), exponent As Double) As Double()()
            Dim n = D(0).Length
            Dim k = D.Length
            Dim result = RectangularArray.Matrix(Of Double)(k, n)

            For i = 0 To k - 1
                For j = 0 To n - 1
                    If D(i)(j) > 0.0R Then
                        result(i)(j) = stdf.Pow(D(i)(j), exponent)
                    End If
                Next
            Next
            Return result
        End Function
    End Class

    ''' <summary>
    ''' A class implementing Stress Minimization by Majorizing a Complex Function (SMACOF).
    ''' 
    ''' Created by IntelliJ IDEA.
    ''' User: da
    ''' Date: 10/8/11
    ''' Time: 2:43 AM
    ''' </summary>

    Public Class SMACOF : Inherits MDSMethod

        ''' <summary>
        ''' Construct a new SMACOF instance. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="x"> initial coordinate matrix </param>
        ''' <param name="w"> weights matrix </param>
        Public Sub New(d As Double()(), x As Double()(), w As Double()())
            Call MyBase.New(d, x, w)
        End Sub

        ''' <summary>
        ''' Construct a SMACOF instance without weights. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="x"> initial coordinate matrix </param>
        Public Sub New(d As Double()(), x As Double()())
            Call MyBase.New(d, x, w:=Nothing)
        End Sub

        ''' <summary>
        ''' Perform n majorization iterations using this SMACOF instance. </summary>
        ''' <param name="n"> number of iterations </param>
        ''' <returns> report </returns>
        Public Overrides Function iterate(n As Integer) As String
            If w IsNot Nothing Then
                Return majorize(x, d, w, n, 0)
            End If
            Return majorize(x, d, n, 0)
        End Function

        ''' <summary>
        ''' Perform majorization iterations until the maximum number of iterations
        ''' is reached, the maximum runtime has elapsed, or the change in
        ''' normalized stress falls below the threshold, whichever comes first. </summary>
        ''' <param name="iter"> maximum number of iterations </param>
        ''' <param name="threshold"> threshold for change in normalized stress </param>
        ''' <returns> report </returns>
        Public Overrides Function iterate(iter As Integer, threshold As Integer) As String
            If w IsNot Nothing Then
                Return majorize(x, d, w, iter, threshold)
            End If
            Return majorize(x, d, iter, threshold)
        End Function

        ''' <summary>
        ''' Compute the absolute stress for this SMACOF instance. </summary>
        ''' <returns> stress </returns>
        Public Overridable ReadOnly Property StressProp As Double
            Get
                If w IsNot Nothing Then
                    Return stress(d, w, x)
                End If
                Return stress(d, x)
            End Get
        End Property

        ''' <summary>
        ''' Compute the normalized stress for this SMACOF instance. </summary>
        ''' <returns> normalized stress </returns>
        Public Overridable ReadOnly Property NormalizedStressProp As Double
            Get
                If w IsNot Nothing Then
                    Return normalizedStress(d, w, x)
                End If
                Return normalizedStress(d, x)
            End Get
        End Property

        ''' <summary>
        ''' SMACOF algorithm (weighted). </summary>
        ''' <param name="x"> coordinates matrix </param>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="w"> weights matrix </param>
        ''' <param name="iter"> maximum iterations </param>
        ''' <param name="threshold"> halting threshold for change in normalized stress </param>
        ''' <returns> report </returns>
        Public Shared Function majorize(x As Double()(), d As Double()(), w As Double()(), iter As Integer, threshold As Integer) As String
            Dim report = ""
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            Dim wSum = New Double(n - 1) {}
            For i = 0 To n - 1
                For j = 0 To k - 1
                    wSum(i) += w(j)(i)
                Next
            Next
            Dim eps = stdf.Pow(10.0R, -threshold)

            If iter = 0 Then
                iter = 10000000
            End If
            For c = 0 To iter - 1
                Dim change = 0.0R
                Dim magnitude = 0.0R
                For i = 0 To n - 1
                    Dim xnew = New Double([dim] - 1) {}
                    For j = 0 To k - 1
                        Dim inv = 0.0R
                        For m = 0 To [dim] - 1
                            inv += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                        Next
                        If inv <> 0.0R Then
                            inv = stdf.Pow(inv, -0.5R)
                        End If
                        For m = 0 To [dim] - 1
                            xnew(m) += w(j)(i) * (x(m)(j) + d(j)(i) * (x(m)(i) - x(m)(j)) * inv)
                        Next
                    Next
                    If wSum(i) <> 0.0R Then
                        For m = 0 To [dim] - 1
                            change += stdf.Pow(xnew(m) / wSum(i) - x(m)(i), 2.0R)
                            magnitude += stdf.Pow(x(m)(i), 2.0R)
                            x(m)(i) = xnew(m) / wSum(i)
                        Next
                    End If
                Next
                change = stdf.Sqrt(change / magnitude)

                If iter > 0 AndAlso c >= iter - 1 Then
                    report = c + 1.ToString() & " iterations, " & change.ToString() & " relative change"
                End If
            Next
            Return report
        End Function

        ''' <summary>
        ''' SMACOF algorithm (unweighted). </summary>
        ''' <param name="x"> coordinates matrix </param>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="iter"> maximum iterations </param>
        ''' <param name="threshold"> halting threshold for change in normalized stress </param>
        ''' <returns> report </returns>
        Public Shared Function majorize(x As Double()(), d As Double()(), iter As Integer, threshold As Integer) As String
            Dim report = ""
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            Dim eps = stdf.Pow(10.0R, -threshold)

            If iter = 0 Then
                iter = 10000000
            End If
            For c = 0 To iter - 1
                Dim change = 0.0R
                Dim magnitude = 0.0R
                For i = 0 To n - 1
                    Dim xnew = New Double([dim] - 1) {}
                    For j = 0 To k - 1
                        Dim inv = 0.0R
                        For m = 0 To [dim] - 1
                            inv += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                        Next
                        If inv <> 0.0R Then
                            inv = stdf.Pow(inv, -0.5R)
                        End If
                        For m = 0 To [dim] - 1
                            xnew(m) += x(m)(j) + d(j)(i) * (x(m)(i) - x(m)(j)) * inv
                        Next
                    Next
                    For m = 0 To [dim] - 1
                        change += stdf.Pow(xnew(m) / n - x(m)(i), 2.0R)
                        magnitude += stdf.Pow(x(m)(i), 2.0R)
                        x(m)(i) = xnew(m) / n
                    Next
                Next
                change = stdf.Sqrt(change / magnitude)

                If iter > 0 AndAlso c >= iter - 1 Then
                    report = c + 1.ToString() & " iterations, " & change.ToString() & " relative change"
                End If
            Next
            Return report
        End Function

        ''' <summary>
        ''' Bare SMACOF algorithm. Convenient for reading the algorithm. </summary>
        ''' <param name="x"> coordinates matrix </param>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="w"> weights matrix </param>
        ''' <param name="iter"> number of iterations </param>
        Public Shared Sub majorize(x As Double()(), d As Double()(), w As Double()(), iter As Integer)
            Dim n = x(0).Length
            Dim [dim] = x.Length
            Dim wSum = New Double(n - 1) {}
            For i = 0 To n - 1
                For j = 0 To n - 1
                    wSum(i) += w(i)(j)
                Next
            Next
            For c = 0 To iter - 1
                For i = 0 To n - 1
                    Dim xnew = New Double([dim] - 1) {}
                    For j = 0 To n - 1
                        Dim inv = 0.0R
                        For k = 0 To [dim] - 1
                            inv += stdf.Pow(x(k)(i) - x(k)(j), 2.0R)
                        Next
                        If inv <> 0.0R Then
                            inv = stdf.Pow(inv, -0.5R)
                        End If
                        For k = 0 To [dim] - 1
                            xnew(k) += w(i)(j) * (x(k)(j) + d(i)(j) * (x(k)(i) - x(k)(j)) * inv)
                        Next
                    Next
                    If wSum(i) <> 0.0R Then
                        For k = 0 To [dim] - 1
                            x(k)(i) = xnew(k) / wSum(i)
                        Next
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' Compute the absolute stress between a weighted distance matrix and a configuration of coordinates. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="w"> weights matrix </param>
        ''' <param name="x"> coordinates matrix </param>
        ''' <returns> stress </returns>
        Public Shared Function stress(d As Double()(), w As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            For i = 0 To k - 1
                For j = i + 1 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                    Next
                    result += w(i)(j) * stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                Next
            Next
            Return result
        End Function

        ''' <summary>
        ''' Compute the absolute stress between a distance matrix and a configuration of coordinates. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="x"> coordinates matrix </param>
        ''' <returns> stress </returns>
        Public Shared Function stress(d As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            For i = 0 To k - 1
                For j = i + 1 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                    Next
                    result += stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                Next
            Next
            Return result
        End Function

        ''' <summary>
        ''' Compute the normlized stress between a weighted distance matrix and a configuration of coordinates. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="w"> weights matrix </param>
        ''' <param name="x"> coordinate matrix </param>
        ''' <returns> normalized stress </returns>
        Public Shared Function normalizedStress(d As Double()(), w As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            Dim sum = 0.0R
            For i = 0 To k - 1
                For j = i + 1 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                    Next
                    result += w(i)(j) * stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                    sum += w(i)(j) * stdf.Pow(d(i)(j), 2.0R)
                Next
            Next
            Return result / sum
        End Function

        ''' <summary>
        ''' Return the normlized stress between a distance matrix and a configuration of coordinates. </summary>
        ''' <param name="d"> distance matrix </param>
        ''' <param name="x"> coordinate matrix </param>
        ''' <returns> normalized stress </returns>
        Public Shared Function normalizedStress(d As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length

            Dim sum = 0.0R
            For i = 0 To k - 1
                For j = i + 1 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(i) - x(m)(j), 2.0R)
                    Next
                    result += stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                    sum += stdf.Pow(d(i)(j), 2.0R)
                Next
            Next
            Return result / sum
        End Function
    End Class

End Namespace
