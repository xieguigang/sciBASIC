#Region "Microsoft.VisualBasic::c1b67bbbb8ab4483d181bc24f480bc35, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI\XlsxHelpers.vb"

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

    '   Total Lines: 143
    '    Code Lines: 123
    ' Comment Lines: 0
    '   Blank Lines: 20
    '     File Size: 6.50 KB


    ' Module CLI
    ' 
    '     Function: Extract, newEmpty, Print, PushTable
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports Microsoft.VisualBasic.Text
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Partial Module CLI

    <ExportAPI("/push")>
    <Usage("/push /write <*.xlsx> /table <*.csv> [/sheetName <name_string> /saveAs <*.xlsx>]")>
    <Description("Write target csv table its content data as a worksheet into the target Excel package.")>
    <ArgumentAttribute("/sheetName", True, CLITypes.String, PipelineTypes.std_in,
              Description:="The New sheet table name, if this argument Is Not presented, then the program will 
              using the file basename as the sheet table name. If the sheet table name Is exists in current xlsx file, 
              then the exists table value will be updated, otherwise will add New table.")>
    <Group(Program.XlsxTools)>
    Public Function PushTable(args As CommandLine) As Integer
        With args <= "/write"

            Dim Excel As MIME.Office.Excel.File = MIME.Office.Excel.File.Open(.ByRef)
            Dim table As csv = args <= "/table"
            Dim sheetName$ = args("/sheetName") Or .BaseName

            Call Excel.WriteSheetTable(table, sheetName)
            Call Excel.WriteXlsx(args("/saveAs") Or .ByRef)

            Return 0
        End With
    End Function

    <ExportAPI("/Create")>
    <Usage("/Create /target <xlsx>")>
    <Description("Create an empty Excel xlsx package file on a specific file path")>
    <ArgumentAttribute("/target", False, CLITypes.File,
              Description:="The file path for save this New created Excel xlsx package.")>
    <Group(Program.XlsxTools)>
    Public Function newEmpty(args As CommandLine) As Integer
        Return "" _
            .SaveTo(args <= "/target", Encodings.ASCII) _
            .CLICode
    End Function

    <ExportAPI("/Extract")>
    <Usage("/Extract /open <xlsx> [/sheetName <name_string, default=*> /out <out.csv/directory>]")>
    <Description("Open target excel file And get target table And save into a csv file.")>
    <ArgumentAttribute("/open", False, CLITypes.File,
              Description:="File path of the Excel ``*.xlsx`` file for open And read.")>
    <ArgumentAttribute("/sheetName", True, CLITypes.String,
              Description:="The worksheet table name for read data And save as csv file. 
              If this argument value is equals to ``*``, then all of the tables in the target xlsx excel file will be extract.")>
    <ArgumentAttribute("/out", True, CLITypes.File,
              Description:="The csv output file path or a directory path value when the ``/sheetName`` parameter is value ``*``.")>
    <Group(Program.XlsxTools)>
    Public Function Extract(args As CommandLine) As Integer
        Dim sheetName$ = args("/sheetName") Or "*"
        Dim defaultOut$

        If sheetName = "*" Then
            defaultOut = (args <= "/open").TrimSuffix
        Else
            defaultOut = (args <= "/open").TrimSuffix & $"-{sheetName}.csv"
        End If

        With args("/out") Or defaultOut

            If sheetName = "*" Then
                Dim excel = MIME.Office.Excel.File.Open(args <= "/open")

                For Each sheet As NamedValue(Of csv) In excel.EnumerateTables
                    Dim save$ = $"{ .ByRef}/{sheet.Name.NormalizePathString(False)}.csv"
                    Call sheet.Value.Save(save, encoding:=Encodings.UTF8)
                Next

                Return 0
            Else
                Return MIME.Office.Excel.File.Open(args <= "/open") _
                    .GetTable(sheetName) _
                    .Save(.ByRef, encoding:=Encodings.UTF8) _
                    .CLICode
            End If
        End With
    End Function

    <ExportAPI("/Print")>
    <Usage("/Print /in <table.csv/xlsx> [/fields <fieldNames> /sheet <sheetName> /out <device/txt>]")>
    <Description("Print the csv/xlsx file content onto the console screen or text file in table layout.")>
    <ArgumentAttribute("/sheet", True, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="The sheet name of table in xlsx file for display, this option only works when target file format is a xlsx file.")>
    <ArgumentAttribute("/fields", True, CLITypes.String,
              AcceptTypes:={GetType(String())},
              Description:="A list of selected field names for display, seperated with comma symbol. By default, is display all of the fields data.")>
    <ArgumentAttribute("/in", False, CLITypes.File, PipelineTypes.std_in,
              Extensions:="*.csv, *.xlsx, *.txt, *.tsv",
              Description:="Standard input pipeline device only works for csv/tsv file. Target table file for display on the console.")>
    Public Function Print(args As CommandLine) As Integer
        Dim table As (header As String(), rows As String()())
        Dim csv As csv

        With args <= "/in"
            Select Case .ExtensionSuffix _
                        .ToLower
#Disable Warning
                Case "csv"
                    csv = csv.Load(.ByRef)
                Case "txt", "tsv"
                    csv = csv.LoadTsv(.ByRef, Encodings.UTF8)
#Enable Warning
                Case "xlsx"
                    csv = MIME.Office.Excel.File.Open(.ByRef).GetTable(sheetName:=args("/sheet") Or "Sheet1")
                Case Else
                    Throw New NotSupportedException("Unknown file type: " & .FileName)
            End Select
        End With

        Dim fields$() = args("/fields").Split(",")

        If Not fields.IsNullOrEmpty Then
            csv = csv.Project(fields)
        End If

        With csv _
            .Select(Function(r) r.ToArray) _
            .ToArray

            table = (.First, .Skip(1).ToArray)
        End With

        Using out As StreamWriter = args.OpenStreamOutput("/out")
            Call PrintAsTable.PrintTable(table.rows, out,, table.header)
        End Using

        Return 0
    End Function
End Module
