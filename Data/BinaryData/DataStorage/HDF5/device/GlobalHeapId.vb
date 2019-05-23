Namespace HDF5.device

    Public Class GlobalHeapId

        Public ReadOnly Property heapAddress As Long
        Public ReadOnly Property index As Integer

        Public ReadOnly Property isNull As Boolean
            Get
                Return heapAddress = 0 AndAlso index = 0
            End Get
        End Property

        Sub New(address&, index As Integer)
            Me.index = index
            Me.heapAddress = address
        End Sub

        Public Overrides Function ToString() As String
            If isNull Then
                Return "null"
            Else
                Return $"[{index}] &{heapAddress}"
            End If
        End Function
    End Class

End Namespace
