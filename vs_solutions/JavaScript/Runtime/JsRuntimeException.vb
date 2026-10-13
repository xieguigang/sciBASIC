
Namespace Runtime

    ''' <summary>
    ''' Error object visible to JS try/catch; <see cref="Payload"/> is the thrown value.
    ''' <para>
    ''' Engine-raised errors (TypeError / ReferenceError / SyntaxError / RangeError)
    ''' carry a <see cref="JsError"/> payload so that script handlers can read
    ''' <c>e.name</c> and <c>e.message</c>, exactly like JavaScript. Values thrown by
    ''' the script itself (<c>throw x</c>) keep whatever payload the script threw.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsRuntimeException
        Inherits Exception

        Public ReadOnly Payload As Object

        Public Sub New(payload As Object)
            MyBase.New(Describe(payload))
            Me.Payload = payload
        End Sub

        Private Shared Function Describe(payload As Object) As String
            If TypeOf payload Is String Then Return CStr(payload)
            If TypeOf payload Is JsError Then Return DirectCast(payload, JsError).ToString()
            Return "JS runtime error"
        End Function

        ''' <summary>Build an engine error of the given name, carrying a JsError payload.</summary>
        Public Shared Function Error_(name As String, message As String) As JsRuntimeException
            Return New JsRuntimeException(New JsError(name, message))
        End Function

        Public Shared Function TypeError(message As String) As JsRuntimeException
            Return Error_("TypeError", message)
        End Function

        Public Shared Function ReferenceError(name As String) As JsRuntimeException
            Return Error_("ReferenceError", name & " is not defined")
        End Function

        Public Shared Function SyntaxError(message As String) As JsRuntimeException
            Return Error_("SyntaxError", message)
        End Function

        Public Shared Function RangeError(message As String) As JsRuntimeException
            Return Error_("RangeError", message)
        End Function
    End Class

End Namespace
