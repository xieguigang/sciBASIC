Namespace CNN

    ''' <summary>
    ''' The layer dimension data
    ''' </summary>
    ''' <remarks>
    ''' width and height
    ''' </remarks>
    Public Class Dimension

        ''' <summary>
        ''' the image dimension width
        ''' </summary>
        Public ReadOnly x As Integer
        ''' <summary>
        ''' the image dimension height
        ''' </summary>
        Public ReadOnly y As Integer

        Public Sub New(x As Integer, y As Integer)
            Me.x = x
            Me.y = y
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"size[x:{x}, y:{y}]"
        End Function

        Public Overridable Function divide(scaleSize As Dimension) As Dimension
            Dim x As Integer = Me.x / scaleSize.x
            Dim y As Integer = Me.y / scaleSize.y

            If x * scaleSize.x <> Me.x OrElse y * scaleSize.y <> Me.y Then
                Call $"{Me.ToString} is not matched with scale {scaleSize.ToString}?".Warning
            End If

            Return New Dimension(x, y)
        End Function

        Public Overridable Function subtract(size As Dimension, append As Integer) As Dimension
            Dim x = Me.x - size.x + append
            Dim y = Me.y - size.y + append

            Return New Dimension(x, y)
        End Function
    End Class
End Namespace