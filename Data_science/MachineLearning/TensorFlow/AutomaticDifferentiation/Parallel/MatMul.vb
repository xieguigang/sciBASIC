#Region "Microsoft.VisualBasic::955a0763bd81097adf5739773c3b3db8, Data_science\MachineLearning\TensorFlow\Parallel\MatMul.vb"

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

    '   Total Lines: 84
    '    Code Lines: 61 (72.62%)
    ' Comment Lines: 6 (7.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (20.24%)
    '     File Size: 3.10 KB


    '     Class SIMDMatMul
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Solve
    ' 
    '         Sub: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel

Namespace Parallel

    Public Class SIMDMatMul : Inherits VectorTask

        ReadOnly A, B, C As Tensor
        ReadOnly imax, jmax, kmax As Integer
        ReadOnly blockSizeA, blockSizeB, blockSizeC As Integer

        Public Sub New(A As Tensor, B As Tensor, C As Tensor, nrblocks As Integer,
                       Optional verbose As Boolean = False,
                       Optional workers As Integer? = Nothing)

            MyBase.New(nrblocks, verbose, workers)

            Me.A = A
            Me.B = B
            Me.C = C

            imax = A.sizes(A.Dimension - 2)
            jmax = B.sizes(B.Dimension - 1)
            kmax = A.sizes(A.Dimension - 1)

            blockSizeA = imax * kmax
            blockSizeB = jmax * kmax
            blockSizeC = jmax * imax

            If B.Dimension > 2 AndAlso A.nrValues / blockSizeA <> B.nrValues / blockSizeB Then
                Throw New ArgumentException("Wrong dimensions for matrix multiplication")
            End If

            If B.Dimension = 2 Then blockSizeB = 0
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For block As Integer = start To ends
                Dim offsetA = block * blockSizeA
                Dim offsetB = block * blockSizeB
                Dim offsetC = block * blockSizeC

                For i = 0 To imax - 1
                    For j = 0 To jmax - 1
                        For k = 0 To kmax - 1
                            C.values(offsetC + i * jmax + j) += A.values(offsetA + i * kmax + k) * B.values(offsetB + k * jmax + j)
                        Next
                    Next
                Next
            Next
        End Sub

        ''' <summary>
        ''' Matrix multiplication over the last two dimensions of two tensors
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="B"></param>
        ''' <returns></returns>
        Public Overloads Shared Function Solve(A As Tensor, B As Tensor) As Tensor
            If A.Dimension < 2 OrElse B.Dimension < 2 Then
                Throw New ArgumentException("Tensors must have >= 2 dimensions")
            End If
            If A.sizes(A.Dimension - 1) <> B.sizes(B.Dimension - 2) Then
                Throw New ArgumentException("Wrong dimensions for matrix multiplication")
            End If

            Dim imax = A.sizes(A.Dimension - 2)
            Dim jmax = B.sizes(B.Dimension - 1)
            Dim sizes = New Integer(A.Dimension - 1) {}
            A.sizes.CopyTo(sizes, 0)
            sizes(A.Dimension - 2) = imax
            sizes(A.Dimension - 1) = jmax
            Dim C As New Tensor(sizes)
            Dim nrblocks As Integer = C.nrValues / (imax * jmax)
            Dim task As New SIMDMatMul(A, B, C, nrblocks)

            Call task.Run()

            C = task.C
            C = Checkpoints.Instance.AddCheckpoint(C)

            Return C
        End Function
    End Class
End Namespace
