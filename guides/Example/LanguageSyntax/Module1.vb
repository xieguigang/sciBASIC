Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Module Module1

    Sub Main()
        Dim i As int = 0

        Call i.RangeDescription.__DEBUG_ECHO
        Call (++i).RangeDescription.__DEBUG_ECHO
        Call (i = 150).RangeDescription.__DEBUG_ECHO
        Call i.RangeDescription.__DEBUG_ECHO
        Call (i = 250).RangeDescription.__DEBUG_ECHO
        Call i.RangeDescription.__DEBUG_ECHO

        Pause()
    End Sub

    Private Iterator Function allLines() As IEnumerable(Of String)
        Yield "adasd"
        Yield "2342sdas"
        Yield "zdaasda"
        Yield Nothing
    End Function

    <Extension>
    Public Function RangeDescription(x As Integer) As String
        Return New int(x).RangeDescription
    End Function

    <Extension>
    Public Function RangeDescription(x As int) As String
        If 0 < x <= 100 Then
            Return "0-100"
        ElseIf 100 < x <= 200 Then
            Return "100-200"
        ElseIf 200 < x <= 300 Then
            Return "200-300"
        Else
            Return "undefined"
        End If
    End Function
End Module
