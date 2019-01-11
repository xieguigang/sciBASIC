#Region "Microsoft.VisualBasic::f9a057b63804083ccc54625fd9846ca9, Data_science\MachineLearning\NeuralNetwork\Helpers.vb"

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

    '     Module Helpers
    ' 
    '         Function: GetRandom, NormalizeSamples, PopulateAllSynapses
    ' 
    '         Sub: normalizeMatrix, Train
    ' 
    '     Enum TrainingType
    ' 
    '         Epoch, MinimumError
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Encoder
    ' 
    '         Function: Decode, Encode
    ' 
    '         Sub: AddMap
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace NeuralNetwork

    Public Module Helpers

        Public Property MaxEpochs As Integer = 10000
        Public Property MinimumError As Double = 0.01

        ''' <summary>
        ''' <see cref="Sigmoid"/> as default
        ''' </summary>
        Friend ReadOnly defaultActivation As DefaultValue(Of IActivationFunction) = New Sigmoid

        ReadOnly rand As New Random()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function GetRandom() As Double
            SyncLock rand
                Return 2 * rand.NextDouble() - 1
            End SyncLock
        End Function

        <Extension>
        Public Sub Train(ByRef neuron As Network, data As Sample(),
                         Optional trainingType As TrainingType = TrainingType.Epoch,
                         Optional minErr As Double = 0.01,
                         Optional parallel As Boolean = False)

            If trainingType = TrainingType.Epoch Then
                Call neuron.Train(data, Helpers.MaxEpochs, parallel)
            Else
                Call neuron.Train(data, minimumError:=minErr, parallel:=parallel)
            End If
        End Sub

        <Extension>
        Friend Function PopulateAllSynapses(neuron As Neuron) As IEnumerable(Of Synapse)
            Return neuron.InputSynapses + neuron.OutputSynapses.AsList
        End Function

        ''' <summary>
        ''' 将所有的属性结果都归一化为相同等级的``[0,1]``区间内的数
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NormalizeSamples(samples As Sample()) As Sample()
            ' 每一行数据不可以直接比较
            ' 但是每一列数据是可以直接做比较的
            Call samples.Select(Function(s) s.status).ToArray.normalizeMatrix
            Call samples.Select(Function(s) s.target).ToArray.normalizeMatrix

            Return samples
        End Function

        <Extension>
        Private Sub normalizeMatrix(ByRef matrix As Double()())
            Dim m As Integer = matrix(Scan0).Length
            Dim index%
            Dim v As Vector
            Dim val As Double
            Dim avg As Double

            For i As Integer = 0 To m - 1
                index = i
                v = matrix.Select(Function(x) x(index)).AsVector
                v = v / v.Max
                avg = v.Average

                If avg.IsNaNImaginary Then
                    avg = 0
                End If

                For j As Integer = 0 To matrix.Length - 1
                    val = v.Item(j)

                    If val.IsNaNImaginary Then
                        val = avg
                    End If

                    matrix(j)(index) = val
                Next
            Next
        End Sub
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

    ''' <summary>
    ''' 可以尝试使用这个对象将非数值的离散数据对象映射为连续的数值，从而能够被应用于ANN分析之中
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Encoder(Of T)

        Dim maps As New Dictionary(Of T, Double)

        Default Public Property item(x As T) As Double
            Get
                If maps.ContainsKey(x) Then
                    Return maps(x)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As Double)
                maps(x) = value
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddMap(x As T, value As Double)
            Call maps.Add(x, value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Encode(x As T) As Double
            Return maps(x)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="out">神经网络的输出值</param>
        ''' <returns></returns>
        Public Function Decode(out As Double) As T
            Dim minX As T, minD As Double = 9999

            For Each x In maps
                Dim d As Double = Math.Abs(x.Value - out)

                If d < minD Then
                    minD = d
                    minX = x.Key
                End If
            Next

            Return minX
        End Function
    End Class
End Namespace
