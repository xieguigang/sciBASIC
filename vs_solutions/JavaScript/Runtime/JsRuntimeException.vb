Namespace Runtime

    ''' <summary>Error object visible to JS try/catch; Payload is the thrown value.</summary>
    Public NotInheritable Class JsRuntimeException
        Inherits Exception

        Public ReadOnly Payload As Object

        Public Sub New(payload As Object)
            MyBase.New(If(TypeOf payload Is String, CStr(payload), "JS runtime error"))
            Me.Payload = payload
        End Sub

        Public Shared Function TypeError(message As String) As JsRuntimeException
            Return New JsRuntimeException("TypeError: " & message)
        End Function

        Public Shared Function ReferenceError(name As String) As JsRuntimeException
            Return New JsRuntimeException("ReferenceError: " & name & " is not defined")
        End Function
    End Class

End Namespace