#Region "Microsoft.VisualBasic::f81bf5833d1e0f73e4d84ce55be4ef41, mime\application%netcdf\ToString.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module ToStringHelper
    ' 
    '     Function: toString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components

''' <summary>
''' CDF file summary
''' </summary>
Module ToStringHelper

    <Extension>
    Public Function toString(file As netCDFReader) As String
        Dim result As New StringBuilder
        Dim [dim] As Dimension
        Dim summary$
        Dim record = file.recordDimension

        result.AppendLine("DIMENSIONS")
        For Each dimension As SeqValue(Of Dimension) In file.dimensions.SeqIterator
            [dim] = dimension
            summary = $"  [{dimension.i.ToString.PadLeft(2)}] {[dim].name.PadEnd(25)} = size: {[dim].size}"

            If [dim].name = record.name Then
                summary &= $" [recordDimension, size={record.length}x{record.recordStep}]"
            End If

            result.AppendLine(summary)
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

            result.AppendLine($"  {variable.name.PadEnd(30)} = {stringify}")
        Next

        Return result.ToString
    End Function
End Module
