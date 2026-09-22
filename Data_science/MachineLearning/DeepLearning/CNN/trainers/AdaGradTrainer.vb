#Region "Microsoft.VisualBasic::80984e9213c8d04f63180e720b91907d, Data_science\MachineLearning\DeepLearning\CNN\trainers\AdaGradTrainer.vb"

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

    '   Total Lines: 31
    '    Code Lines: 19 (61.29%)
    ' Comment Lines: 7 (22.58%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 5 (16.13%)
    '     File Size: 1.06 KB


    '     Class AdaGradTrainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' The adaptive gradient trainer will over time sum up the square of
    ''' the gradient and use it to change the weights.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class AdaGradTrainer : Inherits TrainerAlgorithm

        ''' <summary>
        ''' Creates an AdaGrad trainer.
        ''' </summary>
        ''' <param name="batch_size">Number of samples accumulated before the weights are updated.</param>
        ''' <param name="l2_decay">L2 regularization strength.</param>
        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        ''' <summary>
        ''' Applies one AdaGrad update step, scaling the step size by the accumulated squared gradient.
        ''' </summary>
        ''' <param name="i">Index of the parameter block inside the network.</param>
        ''' <param name="j">Index of the parameter inside the block.</param>
        ''' <param name="gij">The raw batch gradient of that parameter.</param>
        ''' <param name="p">The parameter vector that is updated in place.</param>
        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi As Double() = gsum(i)
            Dim dx As Double
            ' adagrad update
            gsumi(j) = gsumi(j) + gij * gij
            dx = gsumi(j) + eps
            dx = -learning_rate / std.Sqrt(If(dx < 0, 0, dx)) * gij
            p(j) += dx
        End Sub

        ''' <summary>Returns a short description of this update rule.</summary>
        ''' <returns>A text that reports the batch size, the L2 decay and the learning rate.</returns>
        Public Overrides Function ToString() As String
            Return $"ada_grad(batch_size:{batch_size}, l2_decay:{l2_decay})"
        End Function
    End Class
End Namespace
