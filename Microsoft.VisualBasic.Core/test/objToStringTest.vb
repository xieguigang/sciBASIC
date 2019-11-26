Module objToStringTest

    Sub Main()
        Dim m1 = GetType(overridesToString).GetMethods.Where(Function(m) m.Name = "ToString" AndAlso m.GetParameters.IsNullOrEmpty AndAlso m.GetGenericArguments.IsNullOrEmpty).First
        Dim m2 = GetType(notOverridesToString).GetMethods.Where(Function(m) m.Name = "ToString" AndAlso m.GetParameters.IsNullOrEmpty AndAlso m.GetGenericArguments.IsNullOrEmpty).First
        Dim objToString = GetType(Object).GetMethod("ToString")

        Console.WriteLine(m1.DeclaringType Is objToString.DeclaringType)
        Console.WriteLine(m2.DeclaringType Is objToString.DeclaringType)

        Pause()
    End Sub
End Module

Public Class overridesToString
    Public Overrides Function ToString() As String
        Return "1"
    End Function
End Class

Public Class notOverridesToString

End Class

