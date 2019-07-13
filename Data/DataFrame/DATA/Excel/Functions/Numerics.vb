#Region "Microsoft.VisualBasic::450b3e6ad19d5d069b2311b2521082f0, Data\DataFrame\DATA\Excel\Functions\Numerics.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Numerics
    ' 
    '         Function: (+2 Overloads) SUM
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Excel

    Public Module Numerics

        <Extension>
        Public Function SUM(data As IO.File, ParamArray cells As String()) As Double
            Return cells.Select(Function(c) data.CellValue(c).ParseDouble).Sum
        End Function

        <Extension>
        Public Function SUM(data As IO.File, range As String) As Double
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
