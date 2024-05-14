#Region "Microsoft.VisualBasic::11b3127a85e844332b5155f9840a1f87, Data_science\MachineLearning\DeepLearning\CNN\trainers\NesterovTrainer.vb"

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

    '   Total Lines: 24
    '    Code Lines: 14
    ' Comment Lines: 5
    '   Blank Lines: 5
    '     File Size: 812 B


    '     Class NesterovTrainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CNN.trainers

    ''' <summary>
    ''' Another extension of gradient descent is due to Yurii Nesterov from 1983,[7] and has been subsequently generalized
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class NesterovTrainer : Inherits TrainerAlgorithm

        Public Sub New(batch_size As Integer, l2_decay As Single)
            MyBase.New(batch_size, l2_decay)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            Dim dx = gsumi(j)
            gsumi(j) = gsumi(j) * momentum + learning_rate * gij
            dx = momentum * dx - (1.0 + momentum) * gsumi(j)
            p(j) += dx
        End Sub
    End Class

End Namespace
