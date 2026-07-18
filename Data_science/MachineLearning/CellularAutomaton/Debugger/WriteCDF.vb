#Region "Microsoft.VisualBasic::b0c09eacacf5368c7abbf550d3a38db1, Data_science\MachineLearning\CellularAutomaton\Debugger\WriteCDF.vb"

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

    '   Total Lines: 37
    '    Code Lines: 33 (89.19%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (10.81%)
    '     File Size: 1.63 KB


    ' Module WriteCDF
    ' 
    '     Sub: Flush
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Language

Module WriteCDF

    Public Sub Flush(path As String, cache As Integer()()(), type As Type)
        Using file As New CDFWriter(path)
            Dim w = cache(Scan0).Length
            Dim h = cache.Length
            Dim data As integers
            Dim id As i32 = Scan0
            Dim dims As Dimension()
            Dim attrs As attribute()

            file.GlobalAttributes(
                New attribute With {.name = "schema", .type = CDFDataTypes.NC_CHAR, .value = type.FullName},
                New attribute With {.name = "size\width", .type = CDFDataTypes.NC_INT, .value = w},
                New attribute With {.name = "size\height", .type = CDFDataTypes.NC_INT, .value = h}
            )

            For j As Integer = 0 To w - 1
                For i As Integer = 0 To h - 1
                    data = cache(i)(j).ToArray
                    dims = {New Dimension With {.name = $"sizeof", .size = data.Length}}
                    attrs = {
                        New attribute With {.name = "i", .type = CDFDataTypes.NC_INT, .value = i},
                        New attribute With {.name = "j", .type = CDFDataTypes.NC_INT, .value = j}
                    }
                    file.AddVariable((++id).ToHexString, data, dims, attrs)
                Next
            Next
        End Using
    End Sub
End Module
