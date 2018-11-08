Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' CDF file summary
''' </summary>
Module ToStringHelper

    <Extension>
    Public Function toString(file As netCDFReader) As String
        Dim result As New StringBuilder

        result.AppendLine("DIMENSIONS")
        For Each dimension In file.dimensions
            result.AppendLine($"  {dimension.name.PadEnd(30)} = size: {dimension.size}")
        Next

        result.AppendLine()
        result.AppendLine("GLOBAL ATTRIBUTES")
        For Each attribute As attribute In file.globalAttributes
            result.AppendLine($"  {attribute.name.PadEnd(30)} = {attribute.value}")
        Next

        result.AppendLine()
        result.AppendLine("VARIABLES:")
        For Each variable As variable In file.variables
            Dim value As CDFData = file.getDataVariable(variable)
            Dim stringify = value.ToString

            If (stringify.Length > 50) Then
                stringify = stringify.Substring(0, 50)
            End If
            If (Not value Is Nothing) Then
                stringify &= $" (length: ${value.Length})"
            End If

            result.AppendLine($"  {variable.name.PadEnd(30)} = {stringify}")
        Next

        Return result.ToString
    End Function
End Module
