#Region "Microsoft.VisualBasic::79e87570261f95460d4bf73f9a3fd983, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\MDS\LandmarkMDS.vb"

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

    '   Total Lines: 182
    '    Code Lines: 144 (79.12%)
    ' Comment Lines: 22 (12.09%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 16 (8.79%)
    '     File Size: 6.94 KB


    '     Class LandmarkMDS
    ' 
    '         Properties: NormalizedStressProp, StressProp
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) iterate, majorize, normalizedStress, stress
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

    ''' <summary>
    ''' Created by IntelliJ IDEA.
    ''' User: da
    ''' Date: 10/8/11
    ''' Time: 2:43 AM
    ''' </summary>
    Public Class LandmarkMDS : Inherits MDSMethod

        Public Sub New(d As Double()(), x As Double()(), w As Double()())
            Call MyBase.New(d, x, w)
        End Sub

        Public Sub New(d As Double()(), x As Double()())
            Call MyBase.New(d, x, w:=weightMatrix(d, 0.0R))
        End Sub

        Public Overrides Function iterate(n As Integer) As String
            Return majorize(x, d, w, n, 0)
        End Function

        Public Overrides Function iterate(iter As Integer, threshold As Integer) As String
            Return majorize(x, d, w, iter, threshold)
        End Function

        Public Overridable ReadOnly Property StressProp As Double
            Get
                Return stress(d, w, x)
            End Get
        End Property

        Public Overridable ReadOnly Property NormalizedStressProp As Double
            Get
                Return normalizedStress(d, w, x)
            End Get
        End Property

        Public Shared Function majorize(x As Double()(), d As Double()(), w As Double()(), iter As Integer, threshold As Integer) As String
            Dim report = ""
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length
            Dim index = Data.landmarkIndices(d)
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
                            inv += stdf.Pow(x(m)(i) - x(m)(index(j)), 2.0R)
                        Next
                        If inv <> 0.0R Then
                            inv = stdf.Pow(inv, -0.5R)
                        End If
                        For m = 0 To [dim] - 1
                            xnew(m) += w(j)(i) * (x(m)(index(j)) + d(j)(i) * (x(m)(i) - x(m)(index(j))) * inv)
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

        Public Shared Function stress(d As Double()(), w As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length
            Dim index = Data.landmarkIndices(d)
            For i = 0 To k - 1
                For j = 0 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(index(i)) - x(m)(j), 2.0R)
                    Next
                    result += w(i)(j) * stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                Next
            Next
            Return result
        End Function

        Public Shared Function normalizedStress(d As Double()(), w As Double()(), x As Double()()) As Double
            Dim result = 0.0R
            Dim n = x(0).Length
            Dim k = d.Length
            Dim [dim] = x.Length
            Dim index = Data.landmarkIndices(d)
            Dim sum = 0.0R
            For i = 0 To k - 1
                For j = 0 To n - 1
                    Dim dist = 0.0R
                    For m = 0 To [dim] - 1
                        dist += stdf.Pow(x(m)(index(i)) - x(m)(j), 2.0R)
                    Next
                    result += w(i)(j) * stdf.Pow(d(i)(j) - stdf.Sqrt(dist), 2.0R)
                    sum += w(i)(j) * stdf.Pow(d(i)(j), 2.0R)
                Next
            Next
            Return result / sum
        End Function
    End Class

End Namespace
