
Namespace ApplicationServices.Debugging.Diagnostics

    Public Class Method

        Public Property [Namespace] As String
        Public Property [Module] As String
        Public Property Method As String

        Sub New()
        End Sub

        Sub New(s As String)
            Dim t = s.Split("."c).AsList

            Method = t(-1)
            [Module] = t(-2)
            [Namespace] = t.Take(t.Count - 2).JoinBy(".")
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[Namespace]}.{[Module]}.{Method}"
        End Function
    End Class
End Namespace