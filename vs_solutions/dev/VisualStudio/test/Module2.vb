Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Module Module2

    Sub Main()
        Dim method = GetType(aaaa).GetMethod(NameOf(aaaa.bbbb))
        Dim result = New MethodAnalyzer().AnalyzeMethod(method)

        Call Console.WriteLine(result.ToString)
        Call Pause()
    End Sub
End Module

Public Class aaaa

    Public Function bbbb(a As String, b As Double) As Object
        Dim rand As New Random
        Dim d As Long = rand.NextDouble * b
        Return New NamedValue(Of Single)(a & Now.ToShortDateString, CDbl(d))
    End Function

End Class