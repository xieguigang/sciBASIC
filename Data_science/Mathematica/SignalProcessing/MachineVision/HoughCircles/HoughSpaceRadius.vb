#Region "Microsoft.VisualBasic::ebd4809d60991f9ffef51c03ced07096, Data_science\Mathematica\SignalProcessing\MachineVision\HoughCircles\HoughSpaceRadius.vb"

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

    '   Total Lines: 58
    '    Code Lines: 43 (74.14%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (25.86%)
    '     File Size: 1.90 KB


    '     Class HoughSpaceRadius
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

    Public Class HoughSpaceRadius : Inherits VectorTask

        ReadOnly BinarEdgeMap As Boolean(,)
        ReadOnly radius As Integer

        ReadOnly binarHeight As Integer
        ReadOnly binarWidth As Integer

        ReadOnly resultMatrix As Short(,)

        Public Sub New(edges As Boolean(,), radius As Integer, Optional verbose As Boolean = False, Optional workers As Integer? = Nothing)
            MyBase.New(edges.GetLength(0), verbose, workers)

            Me.BinarEdgeMap = edges

            binarHeight = edges.GetLength(0)
            binarWidth = edges.GetLength(1)

            resultMatrix = New Short(binarHeight - 1, binarWidth - 1) {}
        End Sub

        Public Function getHoughSpace() As Short(,)
            Return resultMatrix
        End Function

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For Y As Integer = start To ends - 1
                For X As Integer = 0 To binarWidth - 1
                    If BinarEdgeMap(Y, X) Then
                        UpdateHoughMatrix(resultMatrix, X, Y, radius)
                    End If
                Next
            Next
        End Sub

        Private Sub UpdateHoughMatrix(ByRef matrix As Short(,), x As Integer, y As Integer, radius As Integer)
            For teta = 0 To 359
                Dim a = CInt(x + radius * std.Cos(teta))
                Dim b = CInt(y + radius * std.Sin(teta))

                If a < 0 OrElse
                    b < 0 OrElse
                    b >= matrix.GetLength(0) OrElse
                    a >= matrix.GetLength(1) Then

                    Continue For
                End If

                matrix(b, a) += 1
            Next
        End Sub
    End Class
End Namespace
