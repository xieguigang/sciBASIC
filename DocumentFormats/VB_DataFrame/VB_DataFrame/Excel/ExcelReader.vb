#Region "Microsoft.VisualBasic::85e3c24f6d7dc9e2c025688a11bb31b7, ..\VB_DataFrame\Excel\ExcelReader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections.Generic
Imports System.Text

Imports System.Data
Imports System.Data.OleDb

Public Class ExcelReader

    Dim _Path, _StrConnection As String

    Public Sub New(path As String, hasHeaders As Boolean, hasMixedData As Boolean)
        Dim strBuilder As OleDbConnectionStringBuilder = New OleDbConnectionStringBuilder()
        strBuilder.Provider = "Microsoft.Jet.OLEDB.4.0"
        strBuilder.DataSource = path
        strBuilder.Add("Extended Properties", $"Excel 8.0;HDR={If(hasHeaders, "Yes", "No")};Imex={If(hasMixedData, "2", "0")};")

        Me._Path = path
        Me._StrConnection = strBuilder.ToString()
    End Sub

    Public Function GetWorksheetList() As String()
        Dim Connection As New OleDbConnection(_StrConnection)
        Call Connection.Open()

        Dim TableWorksheets As DataTable = Connection.GetSchema("Tables")
        Call Connection.Close()

        Dim Worksheets As String() = New String(TableWorksheets.Rows.Count - 1) {}

        For i As Integer = 0 To Worksheets.Length - 1
            Worksheets(i) = DirectCast(TableWorksheets.Rows(i)("TABLE_NAME"), String)
            Worksheets(i) = Worksheets(i).Remove(Worksheets(i).Length - 1).Trim(""""c, "'"c)
            ' removes the trailing $ and other characters appended in the table name
            While Worksheets(i).EndsWith("$")
                Worksheets(i) = Worksheets(i).Remove(Worksheets(i).Length - 1).Trim(""""c, "'"c)
            End While
        Next

        Return Worksheets
    End Function

    Public Function GetColumnsList(worksheet As String) As String()
        Dim connection As New OleDbConnection(_StrConnection)
        Call connection.Open()

        Dim tableColumns As DataTable = connection.GetSchema("Columns", New String() {Nothing, Nothing, worksheet & "$"c, Nothing})
        Call connection.Close()

        Dim columns As String() = New String(tableColumns.Rows.Count - 1) {}

        For i As Integer = 0 To columns.Length - 1
            columns(i) = DirectCast(tableColumns.Rows(i)("COLUMN_NAME"), String)
        Next

        Return columns
    End Function

    Public Function GetWorksheet(worksheet As String) As DataTable
        Dim connection As New OleDbConnection(_StrConnection)
        Dim adaptor As New OleDbDataAdapter($"SELECT * FROM [{worksheet}$]", connection)
        Dim ws As DataTable = New DataTable(worksheet)
        adaptor.FillSchema(ws, SchemaType.Source)
        adaptor.Fill(ws)

        adaptor.Dispose()
        connection.Close()

        Return ws
    End Function

    Public Function GetWorkplace() As DataSet
        Dim workplace As DataSet

        Dim connection As New OleDbConnection(_StrConnection)
        Dim adaptor As New OleDbDataAdapter("SELECT * FROM *", connection)
        workplace = New DataSet()
        adaptor.FillSchema(workplace, SchemaType.Source)
        adaptor.Fill(workplace)

        adaptor.Dispose()
        connection.Close()

        Return workplace
    End Function
End Class
