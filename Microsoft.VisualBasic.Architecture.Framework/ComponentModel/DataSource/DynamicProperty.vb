Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataSourceModel

    Public Interface IDynamicMeta(Of T)

        Property Properties As Dictionary(Of String, T)
    End Interface

    ''' <summary>
    ''' Has a dictionary as a dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class DynamicPropertyBase(Of T) : Inherits ClassObject
        Implements IDynamicMeta(Of T)

        Public Overridable Property Properties As Dictionary(Of String, T) Implements IDynamicMeta(Of T).Properties
            Get
                If _propHash Is Nothing Then
                    _propHash = New Dictionary(Of String, T)
                End If
                Return _propHash
            End Get
            Set(value As Dictionary(Of String, T))
                _propHash = value
            End Set
        End Property

        Dim _propHash As Dictionary(Of String, T)

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function
    End Class
End Namespace