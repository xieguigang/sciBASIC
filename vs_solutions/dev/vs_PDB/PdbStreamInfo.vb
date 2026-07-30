''' <summary>
''' Header information decoded from the PDB stream (stream #1).
''' </summary>
Public Class PdbStreamInfo
    Public Property Version As Integer
    Public Property Signature As Integer
    Public Property Age As Integer
    Public Property Guid As Guid

    Public Overrides Function ToString() As String
        Return $"v{Version} sig={Signature} age={Age} {Guid:B}"
    End Function
End Class