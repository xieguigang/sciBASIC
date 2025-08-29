#Region "Microsoft.VisualBasic::8c9d000bee10c5ae43ddbe35549b0995, Data_science\Mathematica\SignalProcessing\MachineVision\HoughCircles\HoughSpace.vb"

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

    '   Total Lines: 55
    '    Code Lines: 43 (78.18%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (21.82%)
    '     File Size: 1.98 KB


    '     Class HoughSpace
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getHoughSpace
    ' 
    '         Sub: Solve, UpdateHoughMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace HoughCircles

    Public Class HoughSpace : Inherits VectorTask

        ReadOnly edges As Boolean(,)
        ReadOnly resultCube As Short(,,)

        ReadOnly binarHeight As Integer
        ReadOnly binarWidth As Integer
        ReadOnly radius As Integer

        Public Sub New(edges As Boolean(,), Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
            MyBase.New(edges.GetLength(0), verbose, workers)

            Me.edges = edges

            binarHeight = edges.GetLength(0)
            binarWidth = edges.GetLength(1)
            radius = If(binarHeight < binarWidth, binarHeight, binarWidth)
            resultCube = New Short(radius - 1, binarHeight - 1, binarWidth - 1) {}
        End Sub

        Public Function getHoughSpace() As Short(,,)
            Return resultCube
        End Function

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For Y As Integer = start To ends
                For X As Integer = 0 To binarWidth - 1
                    If edges(Y, X) Then
                        UpdateHoughMatrix(resultCube, X, Y, radius)
                    End If
                Next
            Next
        End Sub

        Private Sub UpdateHoughMatrix(ByRef cube As Short(,,), x As Integer, y As Integer, maxRadius As Integer)
            For radius As Integer = 1 To maxRadius - 1
                For teta = 0 To 359
                    Dim a = CInt(x + radius * std.Cos(teta))
                    Dim b = CInt(y + radius * std.Sin(teta))

                    If a < 0 OrElse b < 0 OrElse a >= cube.GetLength(0) OrElse b >= cube.GetLength(1) Then
                        Continue For
                    End If

                    cube(radius, b, a) += 1
                Next
            Next
        End Sub
    End Class
End Namespace
