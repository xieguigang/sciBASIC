
Namespace Runtime

    ''' <summary>
    ''' Marker wrapping the value of a spread element (<c>...xs</c>) while building
    ''' call argument lists, array literals and object literals —
    ''' consumed by <see cref="JsRuntime.JsBuildArgs"/> /
    ''' <see cref="JsRuntime.JsArrayFrom"/> / <see cref="JsRuntime.JsObjFrom"/>.
    ''' </summary>
    Public NotInheritable Class SpreadMarker

        Public ReadOnly Value As Object

        Public Sub New(value As Object)
            Me.Value = value
        End Sub

    End Class

End Namespace
