#Region "Microsoft.VisualBasic::95d23915de8d20f4e4e6c2e0acd21c38, Data_science\Darwinism\NonlinearGrid\TopologyInference\GA\Environment.vb"

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

    ' Class Environment
    ' 
    '     Properties: Cacheable, sampleDist
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Calculate, GetTrainingSet
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Normalizer
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Environment(Of T As IDynamicsComponent(Of T), S As GridGenome(Of T)) : Implements Fitness(Of S)

    Dim matrix As TrainingSet()
    Dim fitness As EvaluateFitness
    Dim validates As TrainingSet()

    Public ReadOnly Property sampleDist As NormalizeMatrix

    Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of S).Cacheable
        Get
            Return True
        End Get
    End Property

    Sub New(trainingSet As DataSet, method As FitnessMethods, Optional validateSet As DataSet = Nothing)
        matrix = trainingSet.PopulateNormalizedSamples(Methods.NormalScaler) _
            .Select(Function(sample)
                        Return New TrainingSet With {
                            .X = sample.vector.AsVector,
                            .Y = sample.target(Scan0),
                            .targetID = sample.target(Scan0).ToString
                        }
                    End Function) _
            .ToArray
        sampleDist = trainingSet.NormalizeMatrix
        fitness = method.GetMethod

        If Not validateSet Is Nothing Then
            ' validateset should normalize use trainingset samples
            validates = validateSet.DataSamples _
                .Select(Function(sample, i)
                            Return New TrainingSet With {
                                .targetID = sample.target(Scan0),
                                .X = sampleDist.NormalizeInput(sample),
                                .Y = sample.target(Scan0)
                            }
                        End Function) _
                .ToArray

            Call $"Grid model is also constraint with {validates.Length} validate set!".__INFO_ECHO
        End If
    End Sub

    Public Iterator Function GetTrainingSet() As IEnumerable(Of NamedValue(Of Double()))
        For Each sample In matrix
            Yield New NamedValue(Of Double()) With {
                .Name = sample.targetID,
                .Description = sample.Y,
                .Value = sample.X.ToArray
            }
        Next
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Calculate(chromosome As S, parallel As Boolean) As Double Implements Fitness(Of S).Calculate
        Dim a As Double = fitness(chromosome, matrix, parallel)

        If validates.IsNullOrEmpty Then
            Return a
        Else
            ' 20190726 如果数据非常容易出现过拟合,可以尝试同时使用验证集进行约束
            Return (a + fitness(chromosome, validates, parallel)) / 2
        End If
    End Function
End Class
