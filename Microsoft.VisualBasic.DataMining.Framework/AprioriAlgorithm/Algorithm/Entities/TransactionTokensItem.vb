Namespace AprioriAlgorithm.Entities

    Public Class TransactionTokensItem : Implements IComparable(Of TransactionTokensItem)

#Region "Public Properties"

        Public Property Name() As String
        Public Property Support() As Double

#End Region

        Public Overrides Function ToString() As String
            Return String.Format("(support={0})  {1}", Support, Name)
        End Function

#Region "IComparable"

        Public Function CompareTo(other As TransactionTokensItem) As Integer Implements IComparable(Of TransactionTokensItem).CompareTo
            Return Name.CompareTo(other.Name)
        End Function
#End Region

    End Class
End Namespace