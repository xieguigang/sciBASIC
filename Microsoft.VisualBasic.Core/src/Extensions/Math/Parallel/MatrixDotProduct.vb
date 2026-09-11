#Region "Microsoft.VisualBasic::019197557ceab25216ec29d882a3720e, Microsoft.VisualBasic.Core\src\Extensions\Math\Parallel\MatrixDotProduct.vb"

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

    '   Total Lines: 51
    '    Code Lines: 37 (72.55%)
    ' Comment Lines: 5 (9.80%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 9 (17.65%)
    '     File Size: 1.85 KB


    '     Class MatrixDotProduct
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Resolve
    ' 
    '         Sub: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Parallel

Namespace Math.Parallel

    ''' <summary>
    ''' module for run matrix dot product in parallel
    ''' </summary>
    Public Class MatrixDotProduct : Inherits VectorTask

        ReadOnly a, b, c As Double()()
        ReadOnly n As Integer
        ReadOnly m As Integer

        Private Sub New(a As Double()(), b As Double()(), ByRef C As Double()(), ncolB As Integer)
            MyBase.New(ncolB)

            Me.a = a
            Me.b = b
            Me.c = C
            Me.n = a(0).Length
            Me.m = a.Length
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim Bcolj As Double() = New Double(n - 1) {}

            For j As Integer = start To ends
                For k As Integer = 0 To n - 1
                    Bcolj(k) = b(k)(j)
                Next
                For i As Integer = 0 To m - 1
                    Dim Arowi As Double() = a(i)

                    ' 行内使用 SIMD 点积（处理器支持 FMA 时会自动使用融合乘加）：
                    ' 单条指令处理 Vector(Of Double).Count 个元素，取代原先的标量累加循环
                    c(i)(j) = Math.SIMD.SimdReduce.Dot(Arowi, Bcolj)
                Next
            Next
        End Sub

        Public Shared Function Resolve(a As Double()(), b As Double()()) As Double()()
            Dim nrowA As Integer = a.Length
            Dim ncolB As Integer = b(0).Length
            Dim c As Double()() = RectangularArray.Matrix(Of Double)(nrowA, ncolB)
            Dim solver As New MatrixDotProduct(a, b, c, ncolB)
            Call solver.Run()
            Return c
        End Function
    End Class
End Namespace
