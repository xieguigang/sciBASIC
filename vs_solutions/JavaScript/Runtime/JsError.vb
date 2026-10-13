
Namespace Runtime

    ''' <summary>
    ''' JavaScript Error object: <c>name</c> plus <c>message</c>.
    ''' <para>
    ''' Engine-raised runtime errors carry one of these as their payload, so script
    ''' <c>catch</c> handlers can read <c>e.message</c> / <c>e.name</c> exactly like
    ''' in JavaScript; <c>String(e)</c> renders as <c>"Name: message"</c>.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsError

        Public ReadOnly Name As String
        Public ReadOnly Message As String

        ''' <summary>
        ''' The .NET exception this error was raised from (CLR bridge calls), or Nothing
        ''' for engine-raised errors. <see cref="ClrInterop.ToHostException"/> hands it
        ''' back to host code, so a script rethrow keeps the original exception type.
        ''' </summary>
        Public ReadOnly Original As Exception

        Public Sub New(name As String, Optional message As String = "", Optional original As Exception = Nothing)
            Me.Name = name
            Me.Message = If(message, "")
            Me.Original = original
        End Sub

        Public Overrides Function ToString() As String
            If Message Is Nothing OrElse Message.Length = 0 Then Return Name
            Return Name & ": " & Message
        End Function

    End Class

End Namespace
