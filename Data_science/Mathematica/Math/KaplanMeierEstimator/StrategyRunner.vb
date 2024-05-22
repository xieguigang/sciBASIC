#Region "Microsoft.VisualBasic::de618e67f1212fdd64cc4be7ace788a7, Data_science\Mathematica\Math\KaplanMeierEstimator\StrategyRunner.vb"

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

    '   Total Lines: 44
    '    Code Lines: 33 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (25.00%)
    '     File Size: 1.97 KB


    ' Class StrategyRunner
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Run
    ' 
    ' /********************************************************************************/

#End Region

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
