#Region "Microsoft.VisualBasic::f719d8d2d778d891f41996af8c770eb1, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Protocols\Dropout.vb"

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

    '   Total Lines: 30
    '    Code Lines: 17
    ' Comment Lines: 10
    '   Blank Lines: 3
    '     File Size: 1.18 KB


    '     Module Dropout
    ' 
    '         Sub: DoDropOut
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace NeuralNetwork.Protocols

    Module Dropout

        ''' <summary>
        ''' 只针对隐藏层进行随机删除操作
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="percentage">
        ''' [0,1]之间,建议设置一个[0.3,0.6]之间的值, 这个参数表示被随机删除的节点的数量百分比,值越高,则剩下的神经元节点越少
        ''' </param>
        <Extension>
        Public Sub DoDropOut(model As Network, Optional percentage As Double = 0.5)
            For Each layer As Layer In model.HiddenLayer
                ' reset the status of all nodes
                For Each node As Neuron In layer
                    node.isDroppedOut = False
                Next
                ' drop out parts of the neuron nodes 
                ' in current network layer
                For Each node As Neuron In layer.TakeRandomly(layer.Neurons.Length * percentage)
                    node.isDroppedOut = True
                Next
            Next
        End Sub
    End Module
End Namespace
