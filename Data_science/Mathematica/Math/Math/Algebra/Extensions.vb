#Region "Microsoft.VisualBasic::74295104d9bc0b68efa8e65c3ecde59b, Data_science\Mathematica\Math\Math\Algebra\Extensions.vb"

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

'     Delegate Function
' 
' 
'     Module HelperExtensions
' 
'         Function: PrimitiveLinearEquation, Tangent
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace LinearAlgebra

    Public Delegate Function fx(x#) As Double

    Public Module HelperExtensions

        ''' <summary>
        ''' 返回一元一次方程
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="p2"></param>
        ''' <returns>``y=ax+b``</returns>
        Public Function PrimitiveLinearEquation(p1 As PointF, p2 As PointF) As fx
            Dim x1 = p1.X
            Dim x2 = p2.X
            Dim y1 = p1.Y
            Dim y2 = p2.Y
            Dim a# = (y2 - y1) / (x2 - x1)
            Dim b# = y1 - a * x1

            Return Function(x) a * x + b
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="slideWindows">Just contains two points.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Tangent(slideWindows As SlideWindow(Of PointF)) As fx
            Return PrimitiveLinearEquation(slideWindows.First, slideWindows.Last)
        End Function

        ''' <summary>
        ''' Compute jaccard coefficient between nearest-neighbor sets
        ''' 
        ''' Weights of both i->j and j->i are recorded if they have intersection. In this case
        ''' w(i->j) should be equal to w(j->i). In some case i->j has weights while j&lt;-i has no
        ''' intersections, only w(i->j) Is recorded. This Is determinded in code `if(u>0)`. 
        ''' In this way, the undirected graph Is symmetrized by halfing the weight 
        ''' in code `weights(r, 2) = u/(2.0*ncol - u)/2`.
        ''' </summary>
        ''' <param name="idx"></param>
        ''' <returns></returns>
        Public Function jaccard_coeff(idx As Integer()()) As GeneralMatrix
            Dim nrow As Integer = idx.Length
            Dim ncol As Integer = idx(Scan0).Length
            Dim weights As New NumericMatrix(nrow * ncol, 3)
            Dim r As Integer = 0

            For i As Integer = 0 To nrow - 1
                For j As Integer = 0 To ncol - 1
                    Dim k = idx(i)(j) - 1
                    Dim nodei As Integer() = idx(i)
                    Dim nodej As Integer() = idx(j)
                    Dim u As Integer = nodei.Intersect(nodej).Count

                    If u > 0 Then
                        ' symmetrize the graph
                        weights(r, 0) = i + 1
                        weights(r, 1) = k + 1
                        weights(r, 2) = u / (2.0 * ncol - u) / 2
                        r += 1
                    End If
                Next
            Next

            Return weights
        End Function
    End Module
End Namespace
