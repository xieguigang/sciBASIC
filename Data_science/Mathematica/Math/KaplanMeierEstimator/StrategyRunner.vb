Imports KaplanMeierEstimator.SplitStrategies
Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks

Namespace KaplanMeierEstimator.Common
    Public Class StrategyRunner
        Private ReadOnly m_splitStrategy As ISplitStrategy

        Private Const MinGroupSize As Integer = 1

        Public Sub New(ByVal strategy As ISplitStrategy)
            m_splitStrategy = strategy
        End Sub

        Public Function Run(ByVal genes As List(Of IEnumerable(Of GeneExpression))) As IOrderedEnumerable(Of GeneResult)
            Dim results As ConcurrentBag(Of GeneResult) = New ConcurrentBag(Of GeneResult)()

            Call Parallel.ForEach(genes, Sub(geneGroup)
                                             Dim groupA As IEnumerable(Of Patient)
                                             Dim groupB As IEnumerable(Of Patient)

                                             m_splitStrategy.DoSplit(geneGroup, groupA, groupB)

                                             If groupA.Count() < MinGroupSize OrElse groupB.Count() < MinGroupSize Then Return

                                             Dim kmEstimate As KaplanMeierEstimate = New KaplanMeierEstimate(groupA, groupB)
                                             kmEstimate.RunEstimate()

                                             results.Add(New GeneResult With {
                                                                                                     .GeneId = Enumerable.First(geneGroup).GeneId,
                                                                                                     .Estimate = kmEstimate,
                                                                                                     .GroupSize = Math.Min(groupA.Count(), groupB.Count())
                                                                                                 })
                                         End Sub)

            Dim sortedResults = results.OrderBy(Function(result) result.Estimate.PValue)
            Return sortedResults
        End Function
    End Class
End Namespace
