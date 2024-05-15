#Region "Microsoft.VisualBasic::0c44651776909e193cf7a4c80c5b91b9, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Models\Synapse.vb"

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

    '   Total Lines: 76
    '    Code Lines: 42
    ' Comment Lines: 25
    '   Blank Lines: 9
    '     File Size: 2.43 KB


    '     Class Synapse
    ' 
    '         Properties: Gradient, InputNeuron, OutputNeuron, Value, Weight
    '                     WeightDelta
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
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

        ''' <summary>
        ''' ``a.Weight * a.InputNeuron.Value``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As Double
            Get
                ' 在这里为了防止出现 0 * Inf = NaN 的情况出现
                If Weight = 0R OrElse InputNeuron.Value = 0R Then
                    Return 0
                Else
                    Return Weight * InputNeuron.Value
                End If
            End Get
        End Property

        ''' <summary>
        ''' ``a.OutputNeuron.Gradient * a.Weight``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Gradient As Double
            Get
                If OutputNeuron.Gradient = 0R OrElse Weight = 0R Then
                    Return 0
                Else
                    Return OutputNeuron.Gradient * Weight
                End If
            End Get
        End Property

        Friend Sub New()
        End Sub

        Public Sub New(inputNeuron As Neuron, outputNeuron As Neuron, weight As Func(Of Double))
            Call Me.New(inputNeuron, outputNeuron)

            ' 权重初始
            Me.Weight = weight()
            Me.WeightDelta = weight()
        End Sub

        ''' <summary>
        ''' Create from xml model
        ''' </summary>
        ''' <param name="inputNeuron"></param>
        ''' <param name="outputNeuron"></param>
        Sub New(inputNeuron As Neuron, outputNeuron As Neuron)
            Me.InputNeuron = inputNeuron
            Me.OutputNeuron = outputNeuron
        End Sub

        Public Overrides Function ToString() As String
            Return $"{InputNeuron.Guid}=>{OutputNeuron.Guid}"
        End Function
    End Class
End Namespace
