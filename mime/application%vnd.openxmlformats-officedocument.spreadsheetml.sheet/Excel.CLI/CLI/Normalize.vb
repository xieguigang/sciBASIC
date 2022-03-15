#Region "Microsoft.VisualBasic::639eff1cc46dc6adeae52d7a6b82763c, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI\Normalize.vb"

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

    '   Total Lines: 19
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 632.00 B


    ' Module CLI
    ' 
    '     Function: FillZero
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv

Partial Module CLI

    <ExportAPI("/fill.zero")>
    <Usage("/fill.zero /in <dataset.csv> [/out <out.csv>]")>
    Public Function FillZero(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.fillZero.csv"
        Dim dataset = Microsoft.VisualBasic.Data.csv.IO _
            .DataSet _
            .LoadDataSet([in]) _
            .ToArray

        Return dataset.SaveTo(out).CLICode
    End Function
End Module
