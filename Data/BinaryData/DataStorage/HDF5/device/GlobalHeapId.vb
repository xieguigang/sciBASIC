#Region "Microsoft.VisualBasic::33f0a735700d68653ccd145ccd850861, Data\BinaryData\DataStorage\HDF5\device\GlobalHeapId.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class GlobalHeapId
    ' 
    '         Properties: heapAddress, index, isNull
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
