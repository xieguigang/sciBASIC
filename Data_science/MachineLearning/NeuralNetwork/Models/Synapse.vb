#Region "Microsoft.VisualBasic::f6b79d43d1de6c6c2b7163d855ca0280, Data_science\MachineLearning\NeuralNetwork\Models\Synapse.vb"

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

    '     Class Synapse
    ' 
    '         Properties: InputNeuron, OutputNeuron, Weight, WeightDelta
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace NeuralNetwork

    ''' <summary>
    ''' （神经元的）突触 a connection between two nerve cells
    ''' </summary>
    ''' <remarks>
    ''' 可以将这个对象看作为网络节点之间的边链接
    ''' </remarks>
    Public Class Synapse

#Region "-- Properties --"
        Public Property InputNeuron As Neuron
        Public Property OutputNeuron As Neuron
        ''' <summary>
        ''' 两个神经元之间的连接强度
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight As Double
        Public Property WeightDelta As Double
#End Region

        Public Sub New(inputNeuron As Neuron, outputNeuron As Neuron)
            Me.InputNeuron = inputNeuron
            Me.OutputNeuron = outputNeuron

            ' 权重初始
            Weight = Helpers.GetRandom()
            WeightDelta = Helpers.GetRandom
        End Sub
    End Class
End Namespace
