#Region "Microsoft.VisualBasic::76afe325706bd5635c04e86eb971ac3c, Data_science\Mathematica\Math\Math\Numerics\Optimization\SteepestDescentFit.vb"

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

    '   Total Lines: 117
    '    Code Lines: 88 (75.21%)
    ' Comment Lines: 1 (0.85%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 28 (23.93%)
    '     File Size: 4.36 KB


    '     Class OptimizationObject
    ' 
    ' 
    ' 
    '     Class SteepestDescentFit
    ' 
    '         Function: SteepestDescent
    ' 
    '         Sub: ComputeGradients
    '         Class SDTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Parallel

Namespace Framework.Optimization

    Public MustInherit Class OptimizationObject

        Public MustOverride ReadOnly Property GradientDims As Integer

        Public MustOverride Function PartialDerivative(x As Double(), err As Double) As Double()
        Public MustOverride Function Predict(x As Double()) As Double
        Public MustOverride Sub Update(gradient As Double())
        Public MustOverride Sub AddLoss(errors As Double())

    End Class

    Public Class SteepestDescentFit(Of T As {New, OptimizationObject})

        Dim learningRate As Double
        Dim xValues As Double()()
        Dim yValues As Double()
        Dim errors As Double() = Nothing
        Dim maxNorm As Double
        Dim makeL2Clamp As Boolean = False

        Private Sub ComputeGradients(fit As T, ByRef grad As Double())
            Dim task As New SDTask(Me, fit)

            grad = New Double(fit.GradientDims - 1) {}
            task.Run()

            For Each batch As Double() In task.batches
                If batch Is Nothing Then
                    Exit For
                End If

                grad = SIMD.Add.f64_op_add_f64(grad, batch)
            Next

            If makeL2Clamp Then
                Dim L2Norm As Double = grad.EuclideanDistance

                If L2Norm > maxNorm Then
                    Dim scaleFactor As Double = maxNorm / L2Norm

                    ' 缩放梯度
                    For i As Integer = 0 To grad.Length - 1
                        grad(i) *= scaleFactor
                    Next
                End If
            End If

            grad = SIMD.Multiply.f64_scalar_op_multiply_f64(learningRate, grad)
        End Sub

        Private Class SDTask : Inherits VectorTask

            Dim errors As Double()
            Dim fit As T
            Dim sd As SteepestDescentFit(Of T)

            Friend ReadOnly batches As Double()()

            Public Sub New(sdf As SteepestDescentFit(Of T), obj As T,
                           Optional verbose As Boolean = False,
                           Optional workers As Integer? = Nothing)

                MyBase.New(sdf.xValues.Length, verbose, workers)

                fit = obj
                errors = sdf.errors
                sd = sdf
                batches = Allocate(Of Double())(all:=False)
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim grad As Double() = New Double(fit.GradientDims - 1) {}

                For i As Integer = start To ends
                    Dim [error] As Double = sd.yValues(i) - fit.Predict(sd.xValues(i))
                    Dim pdx As Double() = fit.PartialDerivative(sd.xValues(i), [error])

                    grad = SIMD.Add.f64_op_add_f64(pdx, grad)
                    errors(i) = [error]
                Next

                batches(cpu_id) = grad
            End Sub
        End Class

        Public Shared Function SteepestDescent(xValues As Double()(), yValues As Double(),
                                               Optional iterations As Integer = 1000,
                                               Optional learningRate As Double = 0.05,
                                               Optional maxNorm As Double = 0,
                                               Optional progress As Boolean = False) As T
            Dim t As T = New T()
            Dim grad As Double() = Nothing
            Dim sdf As New SteepestDescentFit(Of T) With {
                .learningRate = learningRate,
                .xValues = xValues,
                .yValues = yValues,
                .errors = New Double(xValues.Length - 1) {},
                .maxNorm = maxNorm,
                .makeL2Clamp = maxNorm > 0
            }

            For Each iter As Integer In TqdmWrapper.Range(0, iterations, wrap_console:=progress)
                Call sdf.ComputeGradients(t, grad)
                Call t.Update(grad)
                Call t.AddLoss(sdf.errors)
            Next

            Return t
        End Function
    End Class
End Namespace
