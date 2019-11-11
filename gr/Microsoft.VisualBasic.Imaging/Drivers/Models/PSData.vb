Imports System.Drawing
Imports System.IO

Namespace Driver

    Public Class PSData : Inherits GraphicsData

        Public Sub New(img As Object, size As Size)
            MyBase.New(img, size)
        End Sub

        Public Overrides ReadOnly Property Driver As Drivers

        Public Overrides Function Save(path As String) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace