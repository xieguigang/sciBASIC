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

Public Delegate Function EvaluateFitness(target As Genome, trainingSet As TrainingSet(), parallel As Boolean) As Double

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
    Public Function NaiveAverage(target As Genome, trainingSet As TrainingSet(), parallel As Boolean) As Double
        Return trainingSet _
            .Populate(parallel) _
            .Select(Function(sample)
                        Return target.CalculateError(sample.X, sample.Y)
                    End Function) _
            .AverageError
    End Function

    <Extension>
    Public Function LabelGroupAverage(target As Genome, trainingSet As TrainingSet(), parallel As Boolean) As Double
        ' 理论上是应该使用MAX err来作为fitness的
        ' 但是在最开始的时候,因为整个系统的大部分样本的计算结果误差都是Inf
        ' 所以使用MAX来作为fitness的话,会因为结果都是Inf而导致前期没有办法收敛
        ' 在这里应该是使用平均值来避免这个问题
        Return trainingSet _
            .Populate(parallel) _
            .Select(Function(sample)
                        Dim err = target.CalculateError(sample.X, sample.Y)

                        Return (errors:=err, id:=sample.targetID)
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
    Public Function R2(target As Genome, trainingSet As TrainingSet(), parallel As Boolean) As Double
        Return target.chromosome.R2(trainingSet, parallel)
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
    Public Function R2(target As GridSystem, trainingSet As TrainingSet(), parallel As Boolean) As Double
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

        Return R2Group().Max
    End Function
End Module