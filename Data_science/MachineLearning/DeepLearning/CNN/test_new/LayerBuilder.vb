Imports System.Runtime.CompilerServices

Namespace CNN

    Public Class LayerBuilder

        ReadOnly m_layers As New List(Of Layer)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(layer As Layer)
            m_layers = New List(Of Layer) From {layer}
        End Sub

        Sub New()
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{m_layers.Count} CNN layers: {m_layers.JoinBy(" -> ")}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(lb As LayerBuilder) As List(Of Layer)
            Return lb.m_layers.AsEnumerable.AsList
        End Operator
    End Class
End Namespace