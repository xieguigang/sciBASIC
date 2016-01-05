Imports System.Collections.Generic

Namespace AprioriAlgorithm.Entities

    Public Class Output

#Region "Public Properties"

        Public Property StrongRules() As IList(Of Rule)
        Public Property MaximalItemSets() As IList(Of String)
        Public Property ClosedItemSets() As Dictionary(Of String, Dictionary(Of String, Double))
        Public Property FrequentItems() As Dictionary(Of String, TransactionTokensItem)
#End Region

    End Class
End Namespace