#Region "Microsoft.VisualBasic::3b3f96c2fa69bfd98555996ed6740259, Data\BinaryData\netCDF\ToString.vb"

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


    ' Code Statistics:

    '   Total Lines: 54
    '    Code Lines: 34
    ' Comment Lines: 8
    '   Blank Lines: 12
    '     File Size: 1.69 KB


    ' Module ToStringHelper
    ' 
    '     Sub: toString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' CDF file summary
''' </summary>
Module ToStringHelper

    ''' <summary>
    ''' Summary netCDF data
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="dev"></param>
    <Extension>
    Public Sub toString(file As netCDFReader, dev As TextWriter)
        Dim [dim] As Dimension
        Dim summary$
        Dim record = file.recordDimension

        Call dev.WriteLine("DIMENSIONS")

        For Each dimension As SeqValue(Of Dimension) In file.dimensions.SeqIterator
            [dim] = dimension
            summary = $"  [{dimension.i.ToString.PadLeft(2)}] {[dim].name.PadEnd(25)} = size: {[dim].size}"

            If [dim].name = record.name Then
                summary &= $" [recordDimension, size={record.length}x{record.recordStep}]"
            End If

            Call dev.WriteLine(summary)
        Next

        Call dev.WriteLine()
        Call dev.WriteLine("GLOBAL ATTRIBUTES")

        For Each attribute As attribute In file.globalAttributes
            Call dev.WriteLine($"  {attribute.name.PadEnd(30)} = {attribute.value}")
        Next

        Call dev.WriteLine()
        Call dev.WriteLine("VARIABLES:")

        For Each variable As variable In file.variables
            Dim value As ICDFDataVector = file.getDataVariable(variable)
            Dim stringify = value.ToString

            Call dev.WriteLine($"  {variable.name.PadEnd(30)} = {stringify}")
        Next

        Call dev.Flush()
    End Sub
End Module
