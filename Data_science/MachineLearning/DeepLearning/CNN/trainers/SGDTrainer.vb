#Region "Microsoft.VisualBasic::b1ff2c2041da66ee8061460dfc0e7115, Data_science\MachineLearning\DeepLearning\CNN\trainers\SGDTrainer.vb"

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

    '   Total Lines: 33
    '    Code Lines: 17 (51.52%)
    ' Comment Lines: 11 (33.33%)
    '    - Xml Docs: 63.64%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 1.29 KB


    '     Class SGDTrainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CNN.trainers

    ''' <summary>
    ''' Stochastic gradient descent (often shortened in SGD), also known as incremental gradient descent, is a
    ''' stochastic approximation of the gradient descent optimization method for minimizing an objective function
    ''' that is written as a sum of differentiable functions. In other words, SGD tries to find minimums or
    ''' maximums by iteration.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class SGDTrainer : Inherits TrainerAlgorithm

        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            ' assume SGD
            If momentum > 0.0 Then
                ' momentum update
                Dim dx = momentum * gsumi(j) - learning_rate * gij ' step
                gsumi(j) = dx ' back this up for next iteration of momentum
                p(j) += dx ' apply corrected gradient
            Else
                ' vanilla sgd
                p(j) += -learning_rate * gij
            End If
        End Sub
    End Class

End Namespace
