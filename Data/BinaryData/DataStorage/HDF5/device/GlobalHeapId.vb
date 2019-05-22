Namespace HDF5.device

    Public Class GlobalHeapId

        Public ReadOnly Property heapAddress As Long
        Public ReadOnly Property index As Integer

        Sub New(address&, index As Integer)
            Me.index = index
            Me.heapAddress = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] &{heapAddress}"
        End Function
    End Class

End Namespace
