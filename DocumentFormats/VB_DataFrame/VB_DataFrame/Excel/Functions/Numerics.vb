Imports System.Runtime.CompilerServices

Namespace Excel

    Public Module Numerics

        <Extension>
        Public Function SUM(data As DocumentStream.File, ParamArray cells As String()) As Double
            Return cells.Select(Function(c) data.CellValue(c).ParseDouble).Sum
        End Function

        <Extension>
        Public Function SUM(data As DocumentStream.File, range As String) As Double
            Throw New NotImplementedException
        End Function
    End Module
End Namespace