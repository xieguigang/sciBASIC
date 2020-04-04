#Region "Microsoft.VisualBasic::481aa80c3ab3ee9dc450dd07ace36248, Data_science\MachineLearning\CellularAutomaton\Debugger\WriteCDF.vb"

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

    ' Module WriteCDF
    ' 
    '     Sub: Flush
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language

Module WriteCDF

    Public Sub Flush(path As String, cache As Integer()()(), type As Type)
        Using file As New CDFWriter(path)
            Dim w = cache(Scan0).Length
            Dim h = cache.Length
            Dim data As CDFData
            Dim id As i32 = Scan0
            Dim dims As Dimension()
            Dim attrs As attribute()

            file.GlobalAttributes(
                New attribute With {.name = "schema", .type = CDFDataTypes.CHAR, .value = type.FullName},
                New attribute With {.name = "size\width", .type = CDFDataTypes.INT, .value = w},
                New attribute With {.name = "size\height", .type = CDFDataTypes.INT, .value = h}
            )
            file.Dimensions(Dimension.Integer)

            For j As Integer = 0 To w - 1
                For i As Integer = 0 To h - 1
                    data = cache(i)(j).ToArray
                    dims = {Dimension.Integer}
                    attrs = {
                        New attribute With {.name = "i", .type = CDFDataTypes.INT, .value = i},
                        New attribute With {.name = "j", .type = CDFDataTypes.INT, .value = j}
                    }
                    file.AddVariable((++id).ToHexString, data, dims, attrs)
                Next
            Next
        End Using
    End Sub
End Module

