Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.AprioriAlgorithm
Imports System.Windows.Input
Imports System.ComponentModel.DataAnnotations
Imports Microsoft.VisualBasic.DataVisualization.DataMining.Framework.AprioriAlgorithm.Entities

Namespace AprioriAlgorithm

    Public Class AlgorithmInvoker

        Private ReadOnly _aprioriWorkerAnalysis As Apriori.AprioriPredictions

        Public Sub New(apriori As Apriori.AprioriPredictions)
            Me._aprioriWorkerAnalysis = apriori
        End Sub

        Public Function AnalysisTransactions(Transactions As Generic.IEnumerable(Of String), Items As Char(), Optional MinSupport As Double = 1, Optional MinConfidence As Double = 1) As AprioriAlgorithm.Entities.Output
            Dim items__1 As IEnumerable(Of String) = (From ch In Items Select str = ch.ToString).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Function AnalysisTransactions(Transactions As Generic.IEnumerable(Of String), Optional MinSupport As Double = 1, Optional MinConfidence As Double = 1) As AprioriAlgorithm.Entities.Output
            Dim items__1 As IEnumerable(Of String) = (From ch In (From Transaction In Transactions Select Transaction.ToArray).ToArray.MatrixToVector Select str = ch.ToString Distinct).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Shared Function CreateObject() As AlgorithmInvoker
            Return New AlgorithmInvoker(apriori:=AddressOf AprioriAlgorithm.Apriori.InvokeAnalysis)
        End Function

        Public Overrides Function ToString() As String
            Return _aprioriWorkerAnalysis.ToString
        End Function
    End Class
End Namespace