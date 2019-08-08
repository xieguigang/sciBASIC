#Region "Microsoft.VisualBasic::5454f2d3e543162035ddfb17af315a80, Data_science\Darwinism\NonlinearGrid\TopologyInference\GA\FitnessMethods.vb"

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

    ' Enum FitnessMethods
    ' 
    '     LabelGroupAverage, NaiveAverage, R2
    ' 
    '  
    ' 
    ' 
    ' 
    ' Delegate Function
    ' 
    ' 
    ' Module FitnessMethodExtensions
    ' 
    '     Function: GetMethod, LabelGroupAverage, NaiveAverage, (+2 Overloads) R2
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Enum FitnessMethods
    NaiveAverage
    LabelGroupAverage
    R2
End Enum

Public Delegate Function EvaluateFitness(target As IGridFitness, trainingSet As TrainingSet(), parallel As Boolean) As Double

<HideModuleName>
<Extension>
Public Module FitnessMethodExtensions

    <Extension>
    Public Function GetMethod(method As FitnessMethods) As EvaluateFitness
        Select Case method
            Case FitnessMethods.LabelGroupAverage
                Return AddressOf LabelGroupAverage
            Case FitnessMethods.R2
                Return AddressOf R2
            Case Else
                Return AddressOf NaiveAverage
        End Select
    End Function

    <Extension>
    Public Function NaiveAverage(target As IGridFitness, trainingSet As TrainingSet(), parallel As Boolean) As Double
        Return trainingSet _
            .AsParallel _
            .Select(Function(sample)
                        SyncLock sample
                            Return target.CalculateError(sample.X, sample.Y)
                        End SyncLock
                    End Function) _
            .AverageError
    End Function

    <Extension>
    Public Function LabelGroupAverage(target As IGridFitness, trainingSet As TrainingSet(), parallel As Boolean) As Double
        ' 理论上是应该使用MAX err来作为fitness的
        ' 但是在最开始的时候,因为整个系统的大部分样本的计算结果误差都是Inf
        ' 所以使用MAX来作为fitness的话,会因为结果都是Inf而导致前期没有办法收敛
        ' 在这里应该是使用平均值来避免这个问题
        Return trainingSet _
            .AsParallel _
            .Select(Function(sample)
                        SyncLock sample
                            Dim err = target.CalculateError(sample.X, sample.Y)

                            ' 降低零结果的误差权重
                            If sample.X = 0R Then
                                err *= 2
                            End If

                            Return (errors:=err, id:=sample.targetID)
                        End SyncLock
                    End Function) _
            .GroupBy(Function(g) g.id) _
            .Select(Function(g)
                        Return g.Select(Function(s) s.errors).AverageError
                    End Function) _
            .Average
    End Function

    ''' <summary>
    ''' R2 is in range [0, 1], 1 is better, so we needs result ``1 - R2`` as fitness
    ''' </summary>
    ''' <param name="target"></param>
    ''' <param name="trainingSet"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function R2(target As IGridFitness, trainingSet As TrainingSet(), parallel As Boolean) As Double
        Return target.R2(trainingSet, parallel)
    End Function

    ''' <summary>
    ''' R2 is in range [0, 1], 1 is better, so we needs result ``1 - R2`` as fitness
    ''' </summary>
    ''' <param name="target"></param>
    ''' <param name="trainingSet"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function R2(Of T As IDynamicsComponent(Of T))(target As T, trainingSet As TrainingSet(), parallel As Boolean) As Double
        Dim R2Group = Iterator Function() As IEnumerable(Of Double)
                          For Each type As IGrouping(Of String, TrainingSet) In trainingSet.GroupBy(Function(d) d.targetID)
                              Dim sampleArray = type.ToArray
                              Dim X As Vector() = sampleArray.Select(Function(d) d.X).ToArray
                              Dim Y As Double() = sampleArray.Select(Function(d) d.Y).ToArray
                              Dim yfit As Func(Of Vector, Double) =
                                  Function(xi)
                                      Dim fx As Double = target.Evaluate(xi)

                                      If fx.IsNaNImaginary Then
                                          Return 10 ^ 200
                                      Else
                                          Return fx
                                      End If
                                  End Function
                              Dim evaluate = Evaluation.Calculate(X, Y, yfit, parallel)
                              Dim R2result As Double = evaluate.R_square

                              ' 在进行比较的时候
                              ' NaN值是会被判定为比其他的double值都要小的?
                              If R2result.IsNaNImaginary Then
                                  Yield 10 ^ 200
                              Else
                                  Yield 1 - R2result
                              End If
                          Next
                      End Function

        Return R2Group().Average
    End Function
End Module
