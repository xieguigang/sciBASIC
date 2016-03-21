Imports System.Collections.Generic
Imports System.Linq
Imports System.Collections.ObjectModel
Imports System.Windows.Input
Imports System.ComponentModel.DataAnnotations
Imports Microsoft.VisualBasic.DataMining.Framework.AprioriAlgorithm.Entities
Imports Microsoft.VisualBasic.Linq

Namespace AprioriAlgorithm

    Public Class AlgorithmInvoker

        Private ReadOnly _aprioriWorkerAnalysis As Apriori.AprioriPredictions

        Public Sub New(apriori As Apriori.AprioriPredictions)
            Me._aprioriWorkerAnalysis = apriori
        End Sub

        Public Function AnalysisTransactions(Transactions As IEnumerable(Of String),
                                             Items As Char(),
                                             Optional MinSupport As Double = 1,
                                             Optional MinConfidence As Double = 1) As Output
            Dim items__1 As IEnumerable(Of String) = (From ch In Items Select str = ch.ToString).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Function AnalysisTransactions(Transactions As IEnumerable(Of String),
                                             Optional MinSupport As Double = 1,
                                             Optional MinConfidence As Double = 1) As Output
            Dim source = (From Transaction As String In Transactions Select Transaction.ToArray).MatrixAsIterator
            Dim items__1 As IEnumerable(Of String) = (From ch In source Select str = ch.ToString Distinct).ToArray
            Dim output As Output = _aprioriWorkerAnalysis(MinSupport / 100, MinConfidence / 100, items__1, Transactions.ToArray())
            Return output
        End Function

        Public Shared Function CreateObject() As AlgorithmInvoker
            Return New AlgorithmInvoker(apriori:=AddressOf Apriori.InvokeAnalysis)
        End Function

        Public Overrides Function ToString() As String
            Return _aprioriWorkerAnalysis.ToString
        End Function
    End Class
End Namespace