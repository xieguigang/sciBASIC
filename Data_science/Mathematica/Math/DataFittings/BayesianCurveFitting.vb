#Region "Microsoft.VisualBasic::d0f8edf93580fed05125c995a219daa2, Data_science\Mathematica\Math\DataFittings\BayesianCurveFitting.vb"

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

    '   Total Lines: 82
    '    Code Lines: 62
    ' Comment Lines: 10
    '   Blank Lines: 10
    '     File Size: 2.74 KB


    ' Class BayesianCurveFitting
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: getMx, getPhiX, getS2X
    ' 
    '     Sub: computeS
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Public Class BayesianCurveFitting
    Private ReadOnly M As Integer ' order of curve fitting
    Private ReadOnly N As Integer ' size of data
    Private x As Double()
    Private t As Double()
    Private ReadOnly alpha As Double = 0.005
    Private ReadOnly beta As Double = 11.1
    Private S As NumericMatrix
    Private I As NumericMatrix

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(t As Double(), m As Integer)
        Call Me.New(t.Sequence(offSet:=1).Select(Function(i) CDbl(i)).ToArray, t, m)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="t"></param>
    ''' <param name="m">order of curve fitting</param>
    Public Sub New(x As Double(), t As Double(), m As Integer)
        Me.M = m
        N = x.Length
        Me.x = x
        Me.t = t
        I = CType(NumericMatrix.Identity(Me.M + 1, Me.M + 1), NumericMatrix)
        computeS()
    End Sub

    ' compute matrix S
    Private Sub computeS()
        Dim sum As NumericMatrix = New NumericMatrix(M + 1, M + 1)
        For i As Integer = 0 To N - 1
            Dim phi = getPhiX(x(i))
            Dim t_phi = phi.Transpose()
            Dim a = phi.DotProduct(t_phi)

            sum = sum + a
        Next
        sum = CType(sum * beta, NumericMatrix)
        S = CType(CType(I * alpha, NumericMatrix) + sum, NumericMatrix)
        S = CType(S.Inverse(), NumericMatrix)
    End Sub

    ' compute matrix phi(x)
    Private Function getPhiX(x As Double) As NumericMatrix
        Dim phiVal = New Double(M + 1 - 1) {}
        For i As Integer = 0 To M
            phiVal(i) = std.Pow(x, i)
        Next
        Dim phi As NumericMatrix = New NumericMatrix(phiVal, M + 1)
        Return phi
    End Function

    ' compute s2(x)
    Public Overridable Function getS2X(x As Double) As Double
        Dim s As NumericMatrix = CType(getPhiX(x).Transpose(), NumericMatrix) * Me.S * getPhiX(x)
        Dim sVal = s(0, 0)
        Dim s2x = 1 / beta + sVal
        Return s2x
    End Function

    ' compute m(x)
    Public Overridable Function getMx(x As Double) As Double
        Dim sum As NumericMatrix = New NumericMatrix(M + 1, 1)
        For i As Integer = 0 To N - 1
            Dim phi = getPhiX(Me.x(i))
            sum = CType(sum + phi * t(i), NumericMatrix)
        Next
        Dim mx As NumericMatrix = CType((CType(getPhiX(x).Transpose(), NumericMatrix) * beta), NumericMatrix)
        mx = mx * S
        mx = mx * sum

        Return mx(0, 0)
    End Function

End Class
