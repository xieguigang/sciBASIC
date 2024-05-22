#Region "Microsoft.VisualBasic::5b2e294a7bab02f989a148184763ed48, Data\BinaryData\SQLite3\Extensions.vb"

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

    '   Total Lines: 69
    '    Code Lines: 50 (72.46%)
    ' Comment Lines: 10 (14.49%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (13.04%)
    '     File Size: 3.02 KB


    '     Module Extensions
    ' 
    '         Function: (+3 Overloads) ExportTable
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ManagedSqlite

    <HideModuleName> Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ExportTable(table As Sqlite3Table) As IEnumerable(Of [Property](Of String))
            Return table.ExportTable(
                activator:=Function(fields, i)
                               Dim rowObject As New [Property](Of String)

                               For Each field In fields
                                   Call rowObject.Add(field.Name, Scripting.ToString(field.Value))
                               Next

                               Return rowObject
                           End Function)
        End Function

        ''' <summary>
        ''' 可以将这个函数和csv反序列化功能联用从而可以直接反序列化导出对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="activator"></param>
        ''' <param name="trimNameEscape">
        ''' 将表之中的字段名称之中可能出现的``[]``转义删除
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ExportTable(Of T)(table As Sqlite3Table,
                                                   activator As Func(Of IEnumerable(Of NamedValue(Of Object)), Integer, T),
                                                   Optional trimNameEscape As Boolean = False) As IEnumerable(Of T)

            Dim schema As SeqValue(Of NamedValue(Of String))() = table.SchemaDefinition _
                .ParseSchema(removeNameEscape:=trimNameEscape) _
                .columns _
                .SeqIterator _
                .ToArray
            Dim populateValues =
                Iterator Function(row As Sqlite3Row) As IEnumerable(Of NamedValue(Of Object))
                    For Each field In schema
                        Yield New NamedValue(Of Object) With {
                            .Name = field.value.Name,
                            .Value = row(field)
                        }
                    Next
                End Function
            Dim i As i32 = Scan0

            For Each row As Sqlite3Row In table.EnumerateRows
                Yield activator(populateValues(row), ++i)
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ExportTable(database As Sqlite3Database, tableName$) As IEnumerable(Of [Property](Of String))
            Return database.GetTable(tableName).ExportTable
        End Function
    End Module
End Namespace
