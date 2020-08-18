#Region "Microsoft.VisualBasic::3975243c6f0d47281f422fc907d6cdca, Data_science\MachineLearning\MachineLearning\NeuralNetwork\Trainings\Accelerator.vb"

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

    '     Module GAExtensions
    ' 
    '         Function: GetSynapseGroups
    ' 
    '         Sub: doPrint, RunGAAccelerator
    ' 
    '     Class WeightVector
    ' 
    '         Properties: MutationRate, UniqueHashKey
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone, Crossover, Mutate, ToString
    ' 
    '     Class Fitness
    ' 
    '         Properties: Cacheable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.SecurityString

Namespace NeuralNetwork.Accelerator

    ''' <summary>
    ''' 
    ''' </summary>
    Public Module GAExtensions

        <Extension>
        Public Function GetSynapseGroups(network As Network) As NamedCollection(Of Synapse)()
            Return network.PopulateAllSynapses _
                .GroupBy(Function(s) s.ToString) _
                .Select(Function(sg)
                            Return New NamedCollection(Of Synapse)(sg.Key, sg.ToArray)
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Public Sub RunGAAccelerator(network As Network, trainingSet As Sample(), Optional populationSize% = 1000, Optional iterations% = 10000)
            Dim synapses = network.GetSynapseGroups
            Dim population As Population(Of WeightVector) = New WeightVector(synapses).InitialPopulation(New Population(Of WeightVector)(New PopulationList(Of WeightVector), parallel:=True) With {.capacitySize = populationSize})
            Dim fitness As Fitness(Of WeightVector) = New Fitness(network, synapses, trainingSet)
            Dim ga As New GeneticAlgorithm(Of WeightVector)(population, fitness)
            Dim engine As New EnvironmentDriver(Of WeightVector)(ga, Sub(null, nullErr)
                                                                         ' do nothing
                                                                     End Sub) With {
                .Iterations = iterations,
                .Threshold = 0.005
            }

            Call "Run GA helper!".__DEBUG_ECHO
            Call engine.AttachReporter(AddressOf doPrint)
            Call engine.Train()
        End Sub

        Private Sub doPrint(i%, e#, g As GeneticAlgorithm(Of WeightVector))
            Call EnvironmentDriver(Of WeightVector).CreateReport(i, e, g).ToString.__DEBUG_ECHO
        End Sub
    End Module

    ''' <summary>
    ''' 在这里假设所有的<see cref="Neuron.Bias"/>偏差值都是零
    ''' 所以GA优化就被简化为只和突触链接的权重相关
    ''' </summary>
    Public Class WeightVector : Implements Chromosome(Of WeightVector), ICloneable

        ''' <summary>
        ''' 突触链接的权重
        ''' </summary>
        Friend weights#()

        Shared ReadOnly random As New Random
        ReadOnly keyCache As New Md5HashProvider
        Public Property MutationRate As Double Implements Chromosome(Of WeightVector).MutationRate
        Public ReadOnly Property UniqueHashKey As String Implements Chromosome(Of WeightVector).UniqueHashKey
            Get
                Return ToString()
            End Get
        End Property

        Sub New(Optional synapses As NamedCollection(Of Synapse)() = Nothing)
            If Not synapses Is Nothing Then
                Me.weights = New Double(synapses.Length - 1) {}

                For i As Integer = 0 To weights.Length - 1
                    weights(i) = synapses(i).First.Weight
                Next
            End If
        End Sub

        ''' <summary>
        ''' 需要这个方法重写来生成唯一的key
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 如果向量长度非常长的话,则会导致字符串非常长,这会导致缓存的键名称的内存占用非常高
        ''' 由于ANN网络之中的突触非常多,所以在这里会需要使用MD5来减少内存占用
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return keyCache.GetMd5Hash(String.Join(";", weights))
        End Function

        Public Function Crossover(another As WeightVector) As IEnumerable(Of WeightVector) Implements Chromosome(Of WeightVector).Crossover
            Dim thisClone As WeightVector = Me.Clone()
            Dim otherClone As WeightVector = another.Clone()
            Call random.Crossover(thisClone.weights, another.weights)
            Return {thisClone, otherClone}
        End Function

        Public Function Mutate() As WeightVector Implements Chromosome(Of WeightVector).Mutate
            Dim result As WeightVector = Me.Clone()
            Call result.weights.Mutate(random, rate:=MutationRate)
            Return result
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Dim weights#() = New Double(Me.weights.Length - 1) {}
            Call Array.Copy(Me.weights, Scan0, weights, Scan0, weights.Length)
            Return New WeightVector() With {
                .weights = weights,
                .MutationRate = MutationRate
            }
        End Function
    End Class

    Public Class Fitness : Implements Fitness(Of WeightVector)

        Dim network As Network
        Dim synapses As NamedCollection(Of Synapse)()
        Dim dataSets As TrainingSample()

        Sub New(network As Network, synapses As NamedCollection(Of Synapse)(), trainingSet As Sample())
            Me.dataSets = trainingSet.Select(Function(a) New TrainingSample(a)).toarray
            Me.network = network
            Me.synapses = synapses
        End Sub

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of WeightVector).Cacheable
            Get
                Return False
            End Get
        End Property

        Public Function Calculate(chromosome As WeightVector, parallel As Boolean) As Double Implements Fitness(Of WeightVector).Calculate
            For i As Integer = 0 To chromosome.weights.Length - 1
                For Each s In synapses(i)
                    s.Weight = chromosome.weights(i)
                Next
            Next

            Dim errors As New List(Of Double)

            For Each dataSet As TrainingSample In dataSets
                Call network.ForwardPropagate(dataSet.sample, False)
                ' 2019-1-14 因为在这里是计算误差，不是训练过程
                ' 所以在这里不需要进行反向传播修改权重和bias参数
                ' 否则会造成其他的解决方案的错误计算，因为反向传播将weights等参数更新了
                ' Call network.BackPropagate(dataSet.target, False)
                Call errors.Add(TrainingUtils.CalculateError(network, dataSet.classify).Average)
            Next

            Return errors.Average
        End Function
    End Class
End Namespace
