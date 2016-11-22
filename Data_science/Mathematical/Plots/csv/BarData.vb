#Region "Microsoft.VisualBasic::c26260b39a473641cdad95730cab4e82, ..\sciBASIC#\Data_science\Mathematical\Plots\csv\BarData.vb"

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
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

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

        <Extension>
        Public Function LoadBarData(csv$, Optional colors$() = Nothing) As BarDataGroup
            Dim file As New File(csv)
            Dim header As RowObject = file.First
            Dim names$() = header.Skip(1).ToArray
            Dim clData As Color() = If(
                colors.IsNullOrEmpty,
                ChartColors.Shuffles,
                colors.ToArray(AddressOf ToColor))

            Return New BarDataGroup With {
                .Serials = names _
                    .SeqIterator _
                    .ToArray(Function(x) New NamedValue(Of Color) With {
                        .Name = x.obj,
                        .Value = clData(x.i)
                    }),
                .Samples = file _
                    .Skip(1) _
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
