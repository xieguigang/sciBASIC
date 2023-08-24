Namespace CNN

    Public Class LayerBuilder

        ReadOnly m_layers As New List(Of Layer)

        Public Sub New(layer As Layer)
            m_layers = New List(Of Layer) From {layer}
        End Sub

        ''' <summary>
        ''' add a new layer into current builder object
        ''' </summary>
        ''' <param name="layer"></param>
        ''' <returns></returns>
        Public Overridable Function addLayer(layer As Layer) As LayerBuilder
            m_layers.Add(layer)
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function

    End Class
End Namespace