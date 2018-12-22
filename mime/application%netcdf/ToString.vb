#Region "Microsoft.VisualBasic::ca0c93cca11f4ddfe93a497fa0957972, mime\application%netcdf\ToString.vb"

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
Imports Microsoft.VisualBasic.MIME.application.netCDF.Components

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

            result.AppendLine($"  {variable.name.PadEnd(30)} = {stringify}")
        Next

        Return result.ToString
    End Function
End Module
