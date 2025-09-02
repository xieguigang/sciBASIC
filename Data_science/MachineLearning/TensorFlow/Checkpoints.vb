
''' <summary>
''' Increases performence by saving states that the backtracking 
''' derivative calculations can be performed between.
''' </summary>
Public NotInheritable Class Checkpoints
    Private Shared ReadOnly instanceField As Checkpoints = New Checkpoints()
    Public Shared ReadOnly Property Instance As Checkpoints
        Get
            Return instanceField
        End Get
    End Property

    ' Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
    Shared Sub New()
    End Sub

    Private Sub New()
    End Sub

    Private nr As Integer = 1
    Private checkpoints_in As SortedDictionary(Of Integer, Tensor) = New SortedDictionary(Of Integer, Tensor)()
    Private checkpoints_out As SortedDictionary(Of Integer, Tensor) = New SortedDictionary(Of Integer, Tensor)()

    Public Function AddCheckpoint(data As Tensor) As Tensor
        checkpoints_in(nr) = data
        Dim data_copy = New Tensor(data)
        checkpoints_out(nr) = data_copy
        nr += 1

        Return data_copy
    End Function

    Public Sub ClearCheckpoints()
        checkpoints_in.Clear()
        checkpoints_out.Clear()
        nr = 1
    End Sub

    ''' <summary>
    ''' Perform derivative calculations for all elements in a tensor with input values from 
    ''' the derivatives of another tensor with the same shape
    ''' </summary>
    Public Sub CalculateCheckpointGradients()
        Dim checkpoint_numbers As List(Of Integer) = New List(Of Integer)(checkpoints_in.Keys)
        checkpoint_numbers.Sort()
        checkpoint_numbers.Reverse()
        For Each nr In checkpoint_numbers
            checkpoints_in(nr).TransferDerivatives(checkpoints_out(nr))
        Next
    End Sub


End Class
