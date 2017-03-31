#Region "Microsoft.VisualBasic::1cdf09addd8505d3fd1f3d78a1d5996e, ..\sciBASIC#\Data_science\Mathematical\Plots\csv\BarData.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.Excel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.OfficeColorThemes
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot

Namespace csv

    ''' <summary>
    ''' 
    ''' ###### csv format
    ''' 
    ''' ```
    ''' "    ",serial1,serial2,...
    ''' group1,
    ''' group2,
    ''' group3,
    ''' ...
    ''' ```
    ''' </summary>
    Public Module BarData

        ''' <summary>
        ''' Loading bar plot data table from specific excel sheet.
        ''' </summary>
        ''' <param name="xlsx$">
        ''' (*.xlsx) required of excel format version at least office 2010
        ''' </param>
        ''' <param name="sheet$">
        ''' The table sheet name in the excel file.
        ''' </param>
        ''' <param name="theme$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadBarDataExcel(xlsx$, sheet$, Optional theme$ = "PRGn:c6") As BarDataGroup
            Dim csv As DataFrame = xlsx.ReadXlsx(sheet)
            Dim model As BarDataGroup = csv.LoadBarData(Designer.GetColors(theme))
            Return model
        End Function

        <Extension>
        Public Function LoadBarData(csv$, Optional theme$ = NameOf(Office2016)) As BarDataGroup
            Return csv.LoadBarData(Designer.FromSchema(theme, 50))
        End Function

        <Extension>
        Public Function LoadBarData(csv$, colors$()) As BarDataGroup
            Return csv.LoadBarData(colors.ToArray(AddressOf ToColor))
        End Function

        <Extension>
        Public Function LoadBarData(csv$, colors As Color()) As BarDataGroup
            Dim file As DataFrame = DataFrame.CreateObject(New File(csv))
            Dim model As BarDataGroup = file.LoadBarData(colors)
            Return model
        End Function

        <Extension>
        Public Function LoadBarData(csv As DataFrame, colors As Color()) As BarDataGroup
            Dim header As RowObject = csv.Headers
            Dim names$() = header.Skip(1).ToArray
            Dim clData As Color() = If(
                colors.IsNullOrEmpty,
                Designer.FromSchema(NameOf(Office2016), names.Length),
                colors)

            Return New BarDataGroup With {
                .Serials = names _
                    .SeqIterator _
                    .ToArray(Function(x) New NamedValue(Of Color) With {
                        .Name = x.value,
                        .Value = clData(x.i)
                    }),
                .Samples = csv.Rows _
                    .ToArray(Function(x) New BarDataSample With {
                        .Tag = x.First,
                        .data = x _
                            .Skip(1) _
                            .ToArray(AddressOf Val)
                    })
            }
        End Function
    End Module
End Namespace
