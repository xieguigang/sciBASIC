#Region "Microsoft.VisualBasic::efb2cd4822bc15ecff3dc2c9465589e8, Data_science\Visualization\Visualization\Tabular\csv.vb"

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

    '   Total Lines: 151
    '    Code Lines: 104 (68.87%)
    ' Comment Lines: 30 (19.87%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 17 (11.26%)
    '     File Size: 5.54 KB


    '     Module BarDataTableExtensions
    ' 
    '         Function: (+4 Overloads) LoadBarData, LoadDataSet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace TabularRender

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
    ''' 
    <HideModuleName>
    Public Module BarDataTableExtensions

        Public Function LoadDataSet(path$,
                                    Optional schema$ = "scibasic.category31()",
                                    Optional groupByColumn! = yes,
                                    Optional tsv! = no) As BarDataGroup

            Dim csv As File = path.LoadTable
            Dim samples As New List(Of BarDataSample)
            Dim serialsName$()
            Dim matrix As IEnumerable(Of String())

            If groupByColumn Then
                matrix = csv.Columns
            Else
                matrix = csv.Select(Function(row) row.ToArray)
            End If

            With matrix
                serialsName = .First _
                    .Skip(1) _
                    .ToArray

                For Each column As String() In .Skip(1)
                    Dim groupName$ = column(Scan0)
                    Dim data#() = column.Skip(1).AsDouble()
                    Dim sample As New BarDataSample With {
                        .tag = groupName,
                        .data = data
                    }

                    samples += sample
                Next
            End With

            Dim colors As Color() = Designer.GetColors(schema, serialsName.Length)
            Dim out As New BarDataGroup With {
                .Samples = samples,
                .Serials = serialsName _
                    .SeqIterator _
                    .Select(Function(i)
                                Return New NamedValue(Of Color) With {
                                    .Name = i.value,
                                    .Value = colors(i)
                                }
                            End Function) _
                    .ToArray
            }

            Return out
        End Function

        '''' <summary>
        '''' Loading bar plot data table from specific excel sheet.
        '''' </summary>
        '''' <param name="xlsx$">
        '''' (*.xlsx) required of excel format version at least office 2010
        '''' </param>
        '''' <param name="sheet$">
        '''' The table sheet name in the excel file.
        '''' </param>
        '''' <param name="theme$"></param>
        '''' <returns></returns>
        '<Extension>
        'Public Function LoadBarDataExcel(xlsx$, sheet$, Optional theme$ = "PRGn:c6") As BarDataGroup
        '    Dim csv As DataFrame = xlsx.ReadXlsx(sheet)
        '    Dim model As BarDataGroup = csv.LoadBarData(Designer.GetColors(theme))
        '    Return model
        'End Function

        <Extension>
        Public Function LoadBarData(csv$, Optional theme$ = NameOf(Office2016)) As BarDataGroup
            Return csv.LoadBarData(Designer.FromSchema(theme, 50))
        End Function

        <Extension>
        Public Function LoadBarData(csv$, colors$()) As BarDataGroup
            Return csv.LoadBarData(colors.Select(AddressOf ToColor).ToArray)
        End Function

        <Extension>
        Public Function LoadBarData(csv$, colors As Color()) As BarDataGroup
            Dim file As DataFrameResolver = DataFrameResolver.CreateObject(New File(csv))
            Dim model As BarDataGroup = file.LoadBarData(colors)
            Return model
        End Function

        <Extension>
        Public Function LoadBarData(csv As DataFrameResolver, colors As Color()) As BarDataGroup
            Dim header As RowObject = csv.Headers
            Dim names$() = header.Skip(1).ToArray
            Dim clData As Color()

            If colors.IsNullOrEmpty Then
                clData = Designer.FromSchema(NameOf(Office2016), names.Length)
            Else
                clData = colors
            End If

            Return New BarDataGroup With {
                .Serials = names _
                    .SeqIterator _
                    .Select(Function(x)
                                Return New NamedValue(Of Color) With {
                                    .Name = x.value,
                                    .Value = clData(x.i)
                                }
                            End Function) _
                    .ToArray,
                .Samples = csv.Rows _
                    .Select(Function(x)
                                Return New BarDataSample With {
                                    .tag = x.First,
                                    .data = x _
                                        .Skip(1) _
                                        .Select(AddressOf Val)
                                }
                            End Function) _
                    .ToArray
            }
        End Function
    End Module
End Namespace
