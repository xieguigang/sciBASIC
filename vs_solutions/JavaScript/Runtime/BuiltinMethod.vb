Namespace Runtime

    ''' <summary>A builtin method bound to its receiver; callable through <see cref="JsRuntime.JsInvoke"/>.</summary>
    Public NotInheritable Class BuiltinMethod
        Public ReadOnly Target As Object
        Public ReadOnly Name As String
        Public Sub New(target As Object, name As String)
            Me.Target = target
            Me.Name = name
        End Sub
    End Class

End Namespace