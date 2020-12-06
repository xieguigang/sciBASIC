Public Class UmapProject

    Public Property projection As Umap
    Public Property labels As String()

    Public ReadOnly Property dimension As Integer
        Get
            Return projection.dimension
        End Get
    End Property

    Public Shared Function CreateProjection() As UmapProject
        Throw New NotImplementedException
    End Function

End Class
