#Region "Microsoft.VisualBasic::1d877fa753fd9ac7d00f202cb9d1e894, Data\BinaryData\BinaryData\SQLite3\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
Imports Microsoft.VisualBasic.Linq

Namespace ManagedSqlite

    <HideModuleName> Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ExportTable(table As Sqlite3Table) As IEnumerable(Of [Property](Of String))
            Return table.ExportTable(
                activator:=Function(fields)
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
                                                   activator As Func(Of IEnumerable(Of NamedValue(Of Object)), T),
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

            For Each row As Sqlite3Row In table.EnumerateRows
                Yield activator(populateValues(row))
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ExportTable(database As Sqlite3Database, tableName$) As IEnumerable(Of [Property](Of String))
            Return database.GetTable(tableName).ExportTable
        End Function
    End Module
End Namespace
