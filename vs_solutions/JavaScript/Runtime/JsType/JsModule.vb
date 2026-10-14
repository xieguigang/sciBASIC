Namespace Runtime

    ''' <summary>
    ''' A <see cref="NativeObject"/> that can also hold real, non-callable property
    ''' values (e.g. <c>fs.constants</c>, <c>path.sep</c>, <c>process.argv</c>).
    ''' <para>
    ''' Member access in <see cref="JsRuntime.JsGet"/> checks <see cref="PropertyTable"/>
    ''' first and returns the stored value; otherwise it falls back to a callable
    ''' <see cref="BuiltinMethod"/> (handled by the generic <c>NativeObject</c> branch).
    ''' This keeps console/Math behaviour unchanged while letting the new fs/path/process
    ''' modules expose both values and methods.
    ''' </para>
    ''' </summary>
    Public Class JsModule
        Inherits NativeObject

        Public ReadOnly PropertyTable As New Dictionary(Of String, Object)()

        Public Sub New(name As String, Optional props As Dictionary(Of String, Object) = Nothing)
            MyBase.New(name)
            If props IsNot Nothing Then
                For Each kv In props
                    PropertyTable(kv.Key) = kv.Value
                Next
            End If
        End Sub

    End Class

End Namespace
