Imports System.Text

Namespace CNN

    Public Class Size

        Public ReadOnly x As Integer
        Public ReadOnly y As Integer

        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
        End Sub

        Public Overrides Function ToString() As String
            Dim s As StringBuilder = (New StringBuilder("Size(")).Append(" x = ").Append(x).Append(" y= ").Append(y).Append(")")
            Return s.ToString()
        End Function

        Public Overridable Function divide(scaleSize As Size) As Size
            Dim x As Integer = Me.x / scaleSize.x
            Dim y As Integer = Me.y / scaleSize.y
            If x * scaleSize.x <> Me.x OrElse y * scaleSize.y <> Me.y Then
                Throw New Exception(ToString() & "��������" & scaleSize.ToString())
            End If
            Return New Size(x, y)
        End Function

        Public Overridable Function subtract(size As Size, append As Integer) As Size
            Dim x = Me.x - size.x + append
            Dim y = Me.y - size.y + append
            Return New Size(x, y)
        End Function
    End Class
End Namespace