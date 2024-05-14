#Region "Microsoft.VisualBasic::5ff4d89124a64b1c77ac8a0e343c9c05, Data\BinaryData\DataStorage\test\demoCDFWrite.vb"

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

    '   Total Lines: 34
    '    Code Lines: 26
    ' Comment Lines: 1
    '   Blank Lines: 7
    '     File Size: 1.67 KB


    ' Module demoCDFWrite
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module demoCDFWrite

    Sub Main()
        Using cdf As New CDFWriter("E:\GCModeller\src\R-sharp\tutorials\io\CDF\dataframe.netcdf")
            Dim range1 As DoubleRange = {-99, 99}

            ' create random data vectors as demo data
            Dim a As integers = {2, 2, 3, 4, 5, 1, 1, 1, 1, 1}
            Dim b As doubles = a.Select(Function(any) randf.GetRandomValue(range1)).ToArray
            Dim flags As flags = a.Select(Function(any) randf.NextBoolean).ToArray
            Dim id As integers = a.Select(Function(any, i) i).ToArray

            Dim data_size As New Dimension With {
                .name = "nrow",
                .size = a.Length
            }

            Call cdf.GlobalAttributes(New attribute("time", Now.ToString, CDFDataTypes.CHAR)) _
                    .GlobalAttributes(New attribute("num_of_variables", 4, CDFDataTypes.INT)) _
                    .GlobalAttributes(New attribute("github", "https://github.com/xieguigang/sciBASIC", CDFDataTypes.CHAR))

            Call cdf.AddVariable("a", a, data_size, {New attribute("note", "this is an integer vector", CDFDataTypes.CHAR)})
            Call cdf.AddVariable("b", b, data_size)
            Call cdf.AddVariable("flags", flags, data_size)
            Call cdf.AddVariable("id", id, data_size, {New attribute("note", "this is a unique id vector in asc order", CDFDataTypes.CHAR)})
        End Using

    End Sub
End Module
