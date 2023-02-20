Imports System.Collections.Concurrent
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.SplitStrategies
Imports stdNum = System.Math
Imports TasksParallel = System.Threading.Tasks.Parallel

Public Class StrategyRunner
    Private ReadOnly m_splitStrategy As ISplitStrategy

    Private Const MinGroupSize As Integer = 1

    Public Sub New(ByVal strategy As ISplitStrategy)
        m_splitStrategy = strategy
    End Sub

    Public Function Run(ByVal genes As List(Of IEnumerable(Of GeneExpression))) As IOrderedEnumerable(Of GeneResult)
        Dim results As ConcurrentBag(Of GeneResult) = New ConcurrentBag(Of GeneResult)()
        Dim execGroup =
            Sub(geneGroup As IEnumerable(Of GeneExpression))
                Dim groupA As IEnumerable(Of Patient) = Nothing
                Dim groupB As IEnumerable(Of Patient) = Nothing

                m_splitStrategy.DoSplit(geneGroup, groupA, groupB)

                If groupA.Count() < MinGroupSize OrElse groupB.Count() < MinGroupSize Then Return

                Dim kmEstimate As KaplanMeierEstimate = New KaplanMeierEstimate(groupA, groupB)
                kmEstimate.RunEstimate()

                results.Add(New GeneResult With {
                                                                   .GeneId = Enumerable.First(geneGroup).GeneId,
                                                                   .Estimate = kmEstimate,
                                                                   .GroupSize = stdNum.Min(groupA.Count(), groupB.Count())
                                                               })
            End Sub

        Call TasksParallel.ForEach(genes, Sub(geneGroup)

                                          End Sub)

        Dim sortedResults = results.OrderBy(Function(result) result.Estimate.PValue)
        Return sortedResults
    End Function
End Class
