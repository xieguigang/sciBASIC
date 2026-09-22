#Region "Microsoft.VisualBasic::7c4c077bbbd2f24d74c59d4db7f194d0, Data_science\MachineLearning\DeepLearning\CNN\trainers\WindowGradTrainer.vb"

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

    '   Total Lines: 49
    '    Code Lines: 18 (36.73%)
    ' Comment Lines: 23 (46.94%)
    '    - Xml Docs: 82.61%
    ' 
    '   Blank Lines: 8 (16.33%)
    '     File Size: 2.20 KB


    '     Class WindowGradTrainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' This is AdaGrad but with a moving window weighted average
    ''' so the gradient is not accumulated over the entire history of the run.
    ''' it's also referred to as Idea #1 in Zeiler paper on AdaDelta.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class WindowGradTrainer : Inherits TrainerAlgorithm

        Private ReadOnly ro As Double = 0.95

        ''' <summary>
        ''' Creates a windowed gradient trainer.
        ''' </summary>
        ''' <param name="batch_size">Number of samples accumulated before the weights are updated.</param>
        ''' <param name="l2_decay">L2 regularization strength.</param>
        ''' <param name="ro">Decay factor of the moving window average, normally 0.95.</param>
        Public Sub New(batch_size As Integer, l2_decay As Single, Optional ro As Double = 0.95)
            MyBase.New(batch_size, l2_decay)
            Me.ro = ro
        End Sub

        ''' <summary>
        ''' Applies one windowed average update step to a single parameter.
        ''' </summary>
        ''' <param name="i">Index of the parameter block inside the network.</param>
        ''' <param name="j">Index of the parameter inside the block.</param>
        ''' <param name="gij">The raw batch gradient of that parameter.</param>
        ''' <param name="p">The parameter vector that is updated in place.</param>
        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)

            ' this is adagrad but with a moving window weighted average
            ' so the gradient is not accumulated over the entire history of the run.
            ' it's also referred to as Idea #1 in Zeiler paper on Adadelta. Seems reasonable to me!
            gsumi(j) = ro * gsumi(j) + (1 - ro) * gij * gij
            Dim dx As Double
            dx = gsumi(j) + eps
            dx = -learning_rate / std.Sqrt(If(dx < 0, 0, dx)) * gij ' eps added for better conditioning
            p(j) += dx
        End Sub
    End Class

End Namespace
