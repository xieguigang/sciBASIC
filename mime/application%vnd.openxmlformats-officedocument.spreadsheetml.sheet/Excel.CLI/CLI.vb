#Region "Microsoft.VisualBasic::ba2130569694a72405dd4d00ab9f6676, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: Association, cbind, Extract, newEmpty, Print
    '               PushTable, rbind, rbindGroup
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Text
Imports Contract = Microsoft.VisualBasic.Data.csv.DATA.DataFrame
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports Xlsx = Microsoft.VisualBasic.MIME.Office.Excel.File

<CLI> Module CLI

    ''' <summary>
    ''' /nothing.as.empty 可以允许将nothing使用空字符串进行替代，这样子就可以不抛出错误了
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/Cbind")>
    <Usage("/cbind /in <a.csv> /append <b.csv> [/ID.a <default=ID> /ID.b <default=ID> /grep.ID <grep_script, default=""token <SPACE> first""> /nothing.as.empty /out <ALL.csv>]")>
    <Description("Join of two table by a unique ID.")>
    <Argument("/in", False, CLITypes.File,
              Description:="The table for append by column, its row ID can be duplicated.")>
    <Argument("/append", False, CLITypes.File,
              Description:="The target table that will be append into the table ``a``, the row ID must be unique!")>
    <Argument("/grep.ID", True, CLITypes.String, PipelineTypes.undefined, AcceptTypes:={GetType(String)},
              Description:="This argument parameter describ how to parse the ID in file ``a.csv``")>
    Public Function cbind(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim append$ = args <= "/append"
        Dim out$ = args("/out") Or ([in].TrimSuffix & "+" & append.BaseName & ".csv")
        Dim IDa$ = args("/ID.a")
        Dim IDb$ = args("/ID.b")
        Dim nothingAsEmpty As Boolean = args("/nothing.as.empty")
        Dim a = EntityObject.LoadDataSet([in], uidMap:=IDa)
        Dim b As Contract = Contract.Load(append, uidMap:=IDb)

        With TextGrepScriptEngine.Compile(args("/grep.ID") Or "tokens ' ' first")
            If Not .IsDoNothing Then
                Call .Explains.JoinBy(" -> ").__DEBUG_ECHO

                For Each obj As EntityObject In a
                    obj.ID = .Grep(obj.ID)
                Next
            End If
        End With

        Return Contract.Append(a, b, allowNothing:=nothingAsEmpty) _
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
        Dim out$ = args("/out") Or ([in].Split("*"c).First.TrimDIR & ".rbind.csv")
        Dim source$()

        If InStr([in], "*") > 0 Then
            Dim t$() = [in].Split("*"c)
            Dim dir$ = t(Scan0)
            Dim file$ = t(1)

            source = dir.ListDirectory() _
                .Select(Function(folder) $"{folder}/{file}") _
                .ToArray
        Else
            source = (ls - l - r - "*.csv" <= [in]).ToArray
        End If

        Return source _
            .DirectAppends(EXPORT:=out) _
            .CLICode
    End Function

    ''' <summary>
    ''' 指定文件夹之中的csv文件按照文件名中第一个小数点前面的单词作为分组的key，进行分组合并
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/rbind.group")>
    <Usage("/rbind.group /in <*.csv.DIR> [/out <out.directory>]")>
    Public Function rbindGroup(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimDIR}.rbind.groups/"
        Dim files = (ls - l - r - "*.csv" <= [in]).GroupBy(Function(path) path.BaseName.Split("."c).First).ToArray

        For Each group In files
            Call group.DirectAppends(EXPORT:=$"{out}/{group.Key}.csv")
        Next

        Return 0
    End Function

    <ExportAPI("/push")>
    <Usage("/push /write <*.xlsx> /table <*.csv> [/sheetName <name_string> /saveAs <*.xlsx>]")>
    <Description("Write target csv table its content data as a worksheet into the target Excel package.")>
    <Argument("/sheetName", True, CLITypes.String, PipelineTypes.std_in,
              Description:="The New sheet table name, if this argument Is Not presented, then the program will using the file basename as the sheet table name. If the sheet table name Is exists in current xlsx file, then the exists table value will be updated, otherwise will add New table.")>
    Public Function PushTable(args As CommandLine) As Integer
        With args <= "/write"

            Dim Excel As Xlsx = Xlsx.Open(.ByRef)
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
    <Argument("/Create", False, CLITypes.File,
              Description:="The file path for save this New created Excel xlsx package.")>
    Public Function newEmpty(args As CommandLine) As Integer
        Return "" _
            .SaveTo(args <= "/target", Encodings.ASCII) _
            .CLICode
    End Function

    <ExportAPI("/Extract")>
    <Usage("/Extract /open <xlsx> [/sheetName <name_string, default=*> /out <out.csv/directory>]")>
    <Description("Open target excel file And get target table And save into a csv file.")>
    <Argument("/open", False, CLITypes.File,
              Description:="File path of the Excel ``*.xlsx`` file for open And read.")>
    <Argument("/sheetName", True, CLITypes.String,
              Description:="The worksheet table name for read data And save as csv file. 
              If this argument value is equals to ``*``, then all of the tables in the target xlsx excel file will be extract.")>
    <Argument("/out", True, CLITypes.File,
              Description:="The csv output file path or a directory path value when the ``/sheetName`` parameter is value ``*``.")>
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
                Dim excel = Xlsx.Open(args <= "/open")

                For Each sheet As NamedValue(Of csv) In excel.EnumerateTables
                    Dim save$ = $"{ .ByRef}/{sheet.Name.NormalizePathString(False)}.csv"
                    Call sheet.Value.Save(save, encoding:=Encodings.UTF8)
                Next

                Return 0
            Else
                Return Xlsx.Open(args <= "/open") _
                    .GetTable(sheetName) _
                    .Save(.ByRef, encoding:=Encodings.UTF8) _
                    .CLICode
            End If
        End With
    End Function

    <ExportAPI("/Print")>
    <Usage("/Print /in <table.csv/xlsx> [/sheet <sheetName> /out <device/txt>]")>
    <Description("Print the csv/xlsx file content onto the console screen or text file in table layout.")>
    Public Function Print(args As CommandLine) As Integer
        Dim table As (header As String(), rows As String()())
        Dim csv As csv

        With args <= "/in"
            If .ExtensionSuffix.TextEquals("csv") Then
#Disable Warning
                csv = csv.Load(.ByRef)
#Enable Warning
            Else
                csv = Xlsx.Open(.ByRef).GetTable(sheetName:=args("/sheet") Or "Sheet1")
            End If
        End With

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

    <ExportAPI("/Association")>
    <Usage("/Association /a <a.csv> /b <dataset.csv> [/column.A <scan0> /out <out.csv>]")>
    Public Function Association(args As CommandLine) As Integer
        Dim a$ = args <= "/a"
        Dim b$ = args <= "/b"
        Dim columnNameA$ = args("/column.A")
        Dim bName$ = b.BaseName
        Dim aData = EntityObject.LoadDataSet(a, uidMap:=columnNameA)
        Dim bData = EntityObject.LoadDataSet(b) _
            .GroupBy(Function(bb) bb.ID) _
            .ToDictionary(Function(g) g.Key,
                          Function(g)
                              Dim values = g _
                                  .Select(Function(bb) bb.Properties) _
                                  .IteratesALL _
                                  .GroupBy(Function(p) p.Key) _
                                  .ToDictionary(Function(p) p.Key,
                                                Function(v)
                                                    Return v.Values _
                                                        .Select(Function(s) s.Split(";"c)) _
                                                        .IteratesALL _
                                                        .Distinct _
                                                        .JoinBy(";")
                                                End Function)

                              Return New EntityObject With {
                                  .ID = g.Key,
                                  .Properties = values
                              }
                          End Function)
        Dim out$ = args("/out") Or $"{a.TrimSuffix}_AND_{bName}.csv"
        Dim associates As New List(Of EntityObject)

        For Each x As EntityObject In aData
            If bData.ContainsKey(x.ID) Then
                Dim copy As EntityObject = x.Copy
                Dim y = bData(x.ID)

                For Each [property] In y.Properties
                    Dim key = bName & "." & [property].Key
                    copy.Properties.Add(key, [property].Value)
                Next

                associates += copy
            Else
                associates += x
            End If
        Next

        Return associates _
            .SaveDataSet(out, KeyMap:=columnNameA) _
            .CLICode
    End Function
End Module
