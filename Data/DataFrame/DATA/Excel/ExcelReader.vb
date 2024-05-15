#Region "Microsoft.VisualBasic::86aebebcbcd403d3928aec072e4fb9a0, Data\DataFrame\DATA\Excel\ExcelReader.vb"

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

    '   Total Lines: 113
    '    Code Lines: 70
    ' Comment Lines: 19
    '   Blank Lines: 24
    '     File Size: 4.06 KB


    '     Class ExcelReader
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColumnsList, GetWorkplace, GetWorksheet, GetWorksheetList, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Data
Imports System.Data.OleDb
Imports System.Text

#If Not NETCOREAPP Then

Namespace Excel

    ''' <summary>
    ''' Excel reader by using ADO.NET
    ''' </summary>
    Public Class ExcelReader

        ''' <summary>
        ''' Excel file path
        ''' </summary>
        Dim _fileName As String
        ''' <summary>
        ''' ADO.NET connection string to the excel file <see cref="_fileName"/>
        ''' </summary>
        Dim _cnnExcel As String

        Public Sub New(path As String, hasHeaders As Boolean, hasMixedData As Boolean)
            Dim sb As New OleDbConnectionStringBuilder()
            sb.Provider = "Microsoft.Jet.OLEDB.4.0"
            sb.DataSource = path
            sb.Add("Extended Properties", $"Excel 8.0;HDR={If(hasHeaders, "Yes", "No")};Imex={If(hasMixedData, "2", "0")};")

            Me._fileName = path
            Me._cnnExcel = sb.ToString()
        End Sub

        Public Overrides Function ToString() As String
            Return _cnnExcel
        End Function

        ''' <summary>
        ''' Gets a list of work sheet name in the target excel file.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetWorksheetList() As String()
            Using Connection As New OleDbConnection(_cnnExcel)
                Call Connection.Open()

                Dim TableWorksheets As DataTable = Connection.GetSchema("Tables")
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
            End Using
        End Function

        Public Function GetColumnsList(worksheet As String) As String()
            Dim connection As New OleDbConnection(_cnnExcel)
            Call connection.Open()

            Dim tableColumns As DataTable = connection.GetSchema("Columns", New String() {Nothing, Nothing, worksheet & "$"c, Nothing})
            Call connection.Close()

            Dim columns As String() = New String(tableColumns.Rows.Count - 1) {}

            For i As Integer = 0 To columns.Length - 1
                columns(i) = DirectCast(tableColumns.Rows(i)("COLUMN_NAME"), String)
            Next

            Return columns
        End Function

        ''' <summary>
        ''' Read table data
        ''' </summary>
        ''' <param name="worksheet">table name</param>
        ''' <returns></returns>
        Public Function GetWorksheet(worksheet As String) As DataTable
            Dim connection As New OleDbConnection(_cnnExcel)
            Dim adaptor As New OleDbDataAdapter($"SELECT * FROM [{worksheet}$]", connection)
            Dim ws As New DataTable(worksheet)
            adaptor.FillSchema(ws, SchemaType.Source)
            adaptor.Fill(ws)

            adaptor.Dispose()
            connection.Close()

            Return ws
        End Function

        Public Function GetWorkplace() As DataSet
            Dim workplace As DataSet

            Dim connection As New OleDbConnection(_cnnExcel)
            Dim adaptor As New OleDbDataAdapter("SELECT * FROM *", connection)
            workplace = New DataSet()
            adaptor.FillSchema(workplace, SchemaType.Source)
            adaptor.Fill(workplace)

            adaptor.Dispose()
            connection.Close()

            Return workplace
        End Function
    End Class
End Namespace

#End If
