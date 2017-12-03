#Region "Microsoft.VisualBasic::8cb6ca66283671e62502b813d2454fff, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI.vb"

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

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Text
Imports Contract = Microsoft.VisualBasic.Data.csv.DATA.DataFrame
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File

<CLI> Module CLI

    <ExportAPI("/Cbind")>
    <Usage("/cbind /in <a.csv> /append <b.csv> [/token0.ID <deli, default=<SPACE> /out <ALL.csv>]")>
    <Description("Join of two table by a unique ID.")>
    <Argument("/in", False, CLITypes.File,
              Description:="The table for append by column, its row ID can be duplicated.")>
    <Argument("/append", False, CLITypes.File,
              Description:="The target table that will be append into the table ``a``, the row ID must be unique!")>
    Public Function cbind(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim append$ = args <= "/append"
        Dim out$ = (args <= "/out") Or ([in].TrimSuffix & "+" & append.BaseName & ".csv").AsDefault
        Dim a = EntityObject.LoadDataSet([in])
        Dim b = Contract.Load(append)

        With args <= "/token0.ID"
            If Not String.IsNullOrEmpty(.ref) Then
                For Each obj As EntityObject In a
                    obj.ID = Strings.Split(obj.ID, Delimiter:= .ref)(0)
                Next
            End If
        End With

        Return Contract.Append(a, b) _
            .SaveTo(out) _
            .CLICode
    End Function

    <ExportAPI("/rbind")>
    <Description("Row bind(merge tables directly) of the csv tables")>
    <Usage("/rbind /in <*.csv.DIR> [/out <EXPORT.csv>]")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in,
              Description:="A directory path that contains csv files that will be merge into one file directly.")>
    Public Function rbind(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out$ = (args <= "/out") Or ([in].TrimSuffix & ".rbind.csv").AsDefault

        Return (ls - l - r - "*.csv" <= [in]) _
            .DirectAppends(EXPORT:=out) _
            .CLICode
    End Function

    <ExportAPI("/push")>
    <Usage("/push /write <*.xlsx> /table <*.csv> [/sheetName <name_string> /saveAs <*.xlsx>]")>
    <Description("Write target csv table its content data as a worksheet into the target Excel package.")>
    <Argument("/sheetName", True, CLITypes.String, PipelineTypes.std_in,
              Description:="The new sheet table name, if this argument is not presented, then the program will using the file basename as the sheet table name. If the sheet table name is exists in current xlsx file, then the exists table value will be updated, otherwise will add new table.")>
    Public Function PushTable(args As CommandLine) As Integer
        With args <= "/write"

            Dim Excel As Xlsx = Xlsx.Open(.ref)
            Dim table As csv = args <= "/table"
            Dim sheetName$ = (args <= "/sheetName") Or .ref.BaseName.AsDefault

            Call Excel.WriteSheetTable(table, sheetName)
            Call Excel.WriteXlsx(
                (args <= "/saveAs") Or .ref.AsDefault)

            Return 0
        End With
    End Function

    <ExportAPI("/Create")>
    <Usage("/Create /target <xlsx>")>
    <Description("Create an empty Excel xlsx package file on a specific file path")>
    <Argument("/Create", False, CLITypes.File,
              Description:="The file path for save this new created Excel xlsx package.")>
    Public Function newEmpty(args As CommandLine) As Integer
        Return "" _
            .SaveTo(args <= "/target", Encodings.ASCII) _
            .CLICode
    End Function

    <ExportAPI("/Extract")>
    <Usage("/Extract /open <xlsx> /sheetName <name_string> [/out <out.csv>]")>
    <Description("Open target excel file and get target table and save into a csv file.")>
    <Argument("/open", False, CLITypes.File,
              Description:="File path of the Excel ``*.xlsx`` file for open and read.")>
    <Argument("/sheetName", False, CLITypes.String,
              Description:="The worksheet table name for read data and save as csv file.")>
    <Argument("/out", True, CLITypes.File,
              Description:="The csv output file path.")>
    Public Function Extract(args As CommandLine) As Integer
        Dim sheet$ = args <= "/sheetName"
        Dim defaultOut As DefaultValue(Of String) =
            (args <= "/open").TrimSuffix & $"-{sheet}.csv"

        With (args <= "/out") Or defaultOut

            Return Xlsx.Open(args <= "/open") _
                .GetTable(sheet) _
                .Save(.ref, encoding:=Encodings.UTF8) _
                .CLICode
        End With
    End Function
End Module
