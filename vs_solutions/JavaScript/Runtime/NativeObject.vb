Namespace Runtime

    ''' <summary>Builtin container object (Math, console, JSON, Object, Array, global fns).</summary>
    Public NotInheritable Class NativeObject
        Public ReadOnly Name As String
        Public Sub New(name As String)
            Me.Name = name
        End Sub
    End Class

End Namespace