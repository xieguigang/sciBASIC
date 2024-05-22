#Region "Microsoft.VisualBasic::446ad01da79a5dc6acc415c04290a232, Data_science\MachineLearning\DeepLearning\NeuralNetwork\Trainings\Helpers.vb"

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

    '   Total Lines: 67
    '    Code Lines: 44 (65.67%)
    ' Comment Lines: 13 (19.40%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (14.93%)
    '     File Size: 2.48 KB


    '     Module Helpers
    ' 
    '         Properties: MaxEpochs, MinimumError
    ' 
    '         Function: GetRandom, (+2 Overloads) PopulateAllSynapses, RandomWeightInitializer, UnifyWeightInitializer
    ' 
    '     Enum TrainingType
    ' 
    '         Epoch, MinimumError
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.Math

Namespace NeuralNetwork

    Public Module Helpers

        Public Property MaxEpochs As Integer = 10000
        Public Property MinimumError As Double = 0.01

        ''' <summary>
        ''' <see cref="Sigmoid"/> as default
        ''' </summary>
        Friend ReadOnly defaultActivation As [Default](Of IActivationFunction) = New Sigmoid
        Friend ReadOnly randomWeight As New [Default](Of Func(Of Double))(AddressOf GetRandom)

        ''' <summary>
        ''' 通过这个帮助函数生成``[-1, 1]``之间的随机数
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function GetRandom() As Double
            SyncLock seeds
                Return 2 * seeds.NextDouble() - 1
            End SyncLock
        End Function

        Public Function RandomWeightInitializer() As Func(Of Double)
            Return AddressOf GetRandom
        End Function

        Public Function UnifyWeightInitializer(unify As Double) As Func(Of Double)
            Return Function() unify
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function PopulateAllSynapses(neuron As Neuron) As IEnumerable(Of Synapse)
            Return neuron.InputSynapses.AsEnumerable + neuron.OutputSynapses.AsList
        End Function

        <Extension>
        Friend Iterator Function PopulateAllSynapses(network As Network) As IEnumerable(Of Synapse)
            For Each layer In network.HiddenLayer.AsList + {network.InputLayer, network.OutputLayer}
                For Each neuron In layer
                    For Each s In neuron.PopulateAllSynapses
                        Yield s
                    Next
                Next
            Next
        End Function
    End Module

    Public Enum TrainingType
        ''' <summary>
        ''' 以给定的迭代次数的方式进行训练. <see cref="Helpers.MaxEpochs"/>
        ''' </summary>
        Epoch
        ''' <summary>
        ''' 以小于目标误差的方式进行训练. <see cref="Helpers.MinimumError"/>
        ''' </summary>
        MinimumError
    End Enum
End Namespace
