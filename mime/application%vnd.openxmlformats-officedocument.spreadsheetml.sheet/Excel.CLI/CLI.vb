#Region "Microsoft.VisualBasic::cb04da09883b0b077439ad778cfa6bf8, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI.vb"

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
    '     Function: Association, cbind, rbind, rbindGroup, Union
    '               Unique
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports Microsoft.VisualBasic.Scripting
Imports Contract = Microsoft.VisualBasic.Data.csv.DATA.DataFrame
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

<CLI> Module CLI

    ''' <summary>
    ''' 为ID编号添加一个tag来让重复出现的ID编号变成唯一的编号
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/Unique")>
    <Usage("/Unique /in <dataset.csv> [/out <out.csv>]")>
    <Description("Helper tools for make the ID column value uniques.")>
    <Group(Program.CsvTools)>
    Public Function Unique(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ID_unique.csv"
        Dim file As csv = csv.Load(Path:=[in])
        Dim idIndex As New Dictionary(Of String, String)

        For Each row As RowObject In file
            If idIndex.ContainsKey(row.First) Then
                For i As Integer = 1 To Integer.MaxValue
                    If Not idIndex.ContainsKey(row.First & "_" & i) Then
                        row(Scan0) = row.First & "_" & i
                        idIndex.Add(row.First, "+")
                        Exit For
                    End If
                Next
            Else
                idIndex.Add(row.First, "")
            End If
        Next

        Return file.Save(out).CLICode
    End Function

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
    <Group(Program.CsvTools)>
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
    <Group(Program.CsvTools)>
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

    <ExportAPI("/union")>
    <Description("")>
    <Usage("/union /in <*.csv.DIR> [/tag.field <null> /out <export.csv>]")>
    <Group(Program.CsvTools)>
    Public Function Union(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out$ = args("/out") Or ([in].Split("*"c).First.TrimDIR & ".union.csv")
        Dim source$()
        Dim tagField As String = args("/tag.field")

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

        Dim unionData As DATA.DataFrame = DATA.DataFrame.Load(source(Scan0))

        If Not tagField.StringEmpty Then
            unionData.TagFieldName(source(Scan0).BaseName, tagField)
        End If

        For Each file As String In source.Skip(1)
            If tagField.StringEmpty Then
                unionData += EntityObject.LoadDataSet(file)
            Else
                unionData += MappingsHelper.TagFieldName(EntityObject.LoadDataSet(file), file.BaseName, tagField)
            End If
        Next

        Return unionData.SaveTable(out).CLICode
    End Function

    ''' <summary>
    ''' 指定文件夹之中的csv文件按照文件名中第一个小数点前面的单词作为分组的key，进行分组合并
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/rbind.group")>
    <Usage("/rbind.group /in <*.csv.DIR> [/out <out.directory>]")>
    <Group(Program.CsvTools)>
    Public Function rbindGroup(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimDIR}.rbind.groups/"
        Dim files = (ls - l - r - "*.csv" <= [in]).GroupBy(Function(path) path.BaseName.Split("."c).First).ToArray

        For Each group In files
            Call group.DirectAppends(EXPORT:=$"{out}/{group.Key}.csv")
        Next

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
