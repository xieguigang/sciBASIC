Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCMeans

    Public Class Entity : Inherits KMeans.Entity

        Public Property Memberships As Dictionary(Of String, Double)

        Public Overrides Function ToString() As String
            Return $"{uid} --> {Memberships.GetJson}"
        End Function
    End Class
End Namespace