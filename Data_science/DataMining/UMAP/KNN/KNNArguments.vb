Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure KNNArguments

    ''' <summary>
    ''' nNeighbors
    ''' </summary>
    ''' <returns></returns>
    Public Property k As Integer
    Public Property localConnectivity As Double
    Public Property nIter As Integer
    Public Property bandwidth As Double

    Sub New(k As Integer, Optional localConnectivity As Double = 1, Optional nIter As Integer = 64, Optional bandwidth As Double = 1)
        Me.k = k
        Me.localConnectivity = localConnectivity
        Me.nIter = nIter
        Me.bandwidth = bandwidth
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Structure
