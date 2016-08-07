Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Excel

    Public Module Coordinates

        <Extension>
        Public Function CellValue(data As DocumentStream.File, c As String) As String
            Dim y As Integer = CInt(Regex.Match(c, "\d+").Value)
            Dim x As String = Mid(c, 1, c.Length - CStr(y).Length)
            Return data.Cell(XValue(x), y)
        End Function

        Public Function XValue(x As String) As Integer
            Throw New NotImplementedException
        End Function

        <Extension>
        Public Function RangeSelects(data As DocumentStream.File, range As String) As String()
            Throw New NotImplementedException
        End Function
    End Module
End Namespace