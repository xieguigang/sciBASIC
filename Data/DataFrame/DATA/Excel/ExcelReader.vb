#Region "Microsoft.VisualBasic::2557ed6e864d08d936fc865c1343761e, Data\DataFrame\DATA\Excel\ExcelReader.vb"

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
