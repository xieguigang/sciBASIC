#Region "Microsoft.VisualBasic::cb55c3f9622f833b75d651f1a1558e46, Data_science\Mathematica\Plot\Chart\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: Scatter
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Module CLI

    <ExportAPI("/Scatter")>
    <Usage("/Scatter /in <data.csv> /x <fieldX> /y <fieldY> [/label.X <labelX> /label.Y <labelY> /color <default=black> /out <out.png>]")>
    <Description("")>
    Public Function Scatter(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim fx$ = args <= "/x"
        Dim fy$ = args <= "/y"
        Dim labelX$ = args("/label.X") Or fx
        Dim labelY$ = args("/label.Y") Or fy
        Dim color$ = args("/color") Or "black"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}_[{fx.NormalizePathString},{fy.NormalizePathString}].png"
        Dim csv = DataSet.LoadDataSet([in]).ToArray

    End Function
End Module

