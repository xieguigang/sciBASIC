﻿#Region "Microsoft.VisualBasic::193f67ffc0a6558739f3d7b3c9b00ecb, Data\DataFrame\Extensions\PipeStream.vb"

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

    '   Total Lines: 87
    '    Code Lines: 60 (68.97%)
    ' Comment Lines: 17 (19.54%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (11.49%)
    '     File Size: 3.32 KB


    ' Module PipeStream
    ' 
    '     Function: AsDataSet, AsMatrix, CreateFilter, DataFrame, LoadStream
    '               Vectors
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Table = Microsoft.VisualBasic.Data.csv.IO.File

Public Module PipeStream

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateFilter(filter As NamedValue(Of String)) As Func(Of EntityObject, Boolean)
        Return EntityObject.CreateFilter(filter)
    End Function

    ''' <summary>
    ''' 将一个任意的<see cref="RowObject"/>集合转换为一个csv表格文件模型对象
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DataFrame(data As IEnumerable(Of RowObject)) As Table
        Return New Table(data)
    End Function

    <Extension>
    Public Iterator Function LoadStream(Of T As Class)(input As StreamReader,
                                                       Optional strict As Boolean = False,
                                                       Optional maps As Dictionary(Of String, String) = Nothing) As IEnumerable(Of T)

    End Function

    ''' <summary>
    ''' 将目标字典集合转换为数据集集合
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsDataSet(source As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))) As IEnumerable(Of DataSet)
        Return source.Select(
            Function(x)
                Return New DataSet With {
                    .ID = x.Name,
                    .Properties = x.Value
                }
            End Function)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Vectors(data As IEnumerable(Of DataSet)) As NamedValue(Of Double())()
        Return data.Select(
            Function(d)
                Return New NamedValue(Of Double()) With {
                    .Name = d.ID,
                    .Value = d.Properties.Values.ToArray
                }
            End Function).ToArray
    End Function

    ''' <summary>
    ''' 将所读取出来的csv文件之中的数据转化为数值矩阵
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="header">csv文件之中是否含有行标题？TRUE的话则会跳过第一行</param>
    ''' <param name="rowNames">csv文件之中是否含有列标题？TRUE的话会跳过第一列</param>
    ''' <returns></returns>
    <Extension>
    Public Function AsMatrix(data As Table, Optional header As Boolean = False, Optional rowNames As Boolean = False) As Double()()
        Dim source As IO.File = data

        If header Then
            source = New IO.File(source.Skip(1))
        End If
        If rowNames Then
            source = source.Columns.Skip(1).JoinColumns
        End If

        Return source _
            .Select(Function(row)
                        Return row.Select(AddressOf Val).ToArray
                    End Function) _
            .ToArray
    End Function
End Module
