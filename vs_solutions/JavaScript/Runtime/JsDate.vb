Namespace Runtime

    ''' <summary>
    ''' JavaScript Date — wraps a .NET <see cref="Date"/>.
    ''' <para>
    ''' The value is stored in UTC (so <c>getTime()</c> round-trips the epoch
    ''' milliseconds exactly), while the calendar accessors read the LOCAL time
    ''' component, matching JavaScript. <c>getMonth</c> is 0-based, like JS.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsDate

        ''' <summary>The instant, stored as UTC.</summary>
        Public ReadOnly Value As Date

        Public Sub New(value As Date)
            Me.Value = value.ToUniversalTime()
        End Sub

        ''' <summary>new Date() — the current time.</summary>
        Public Shared Function Now() As JsDate
            Return New JsDate(Date.Now)
        End Function

        ''' <summary>new Date(ms) — milliseconds since the unix epoch.</summary>
        Public Shared Function FromEpochMs(ms As Double) As JsDate
            If Double.IsNaN(ms) Then Return New JsDate(Date.MaxValue.ToUniversalTime())
            Return New JsDate(EpochUtc.AddMilliseconds(ms))
        End Function

        ''' <summary>new Date(string) — a parseable date string, or Nothing (invalid date).</summary>
        Public Shared Function Parse(text As String) As JsDate
            Dim d As Date
            If Date.TryParse(text, d) Then Return New JsDate(d)
            Return Nothing
        End Function

        Private Shared ReadOnly Property EpochUtc As Date
            Get
                Return New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            End Get
        End Property

        ''' <summary>The local-time view used by the calendar accessors.</summary>
        Public ReadOnly Property Local As Date
            Get
                Return Value.ToLocalTime()
            End Get
        End Property

        ''' <summary>Milliseconds since the unix epoch (the JS time value).</summary>
        Public ReadOnly Property EpochMs As Double
            Get
                Return (Value - EpochUtc).TotalMilliseconds
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Local.ToString("ddd MMM dd yyyy HH:mm:ss")
        End Function

    End Class

End Namespace
