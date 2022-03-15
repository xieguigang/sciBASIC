#Region "Microsoft.VisualBasic::95f5ece1c2736512c366d2b4e99c6052, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\CLI\CLI.vb"

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

    '   Total Lines: 419
    '    Code Lines: 346
    ' Comment Lines: 24
    '   Blank Lines: 49
    '     File Size: 17.69 KB


    ' Module CLI
    ' 
    '     Function: Association, cbind, NameValues, rbind, rbindGroup
    '               Removes, SubsetByColumns, Subtract, Takes, Transpose
    '               Union, Unique
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.IO.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Office.Excel
Imports Microsoft.VisualBasic.Scripting
Imports Contract = Microsoft.VisualBasic.Data.csv.DATA.DataFrame
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

<CLI> Module CLI

    <ExportAPI("/name.values")>
    <Usage("/name.values /in <table.csv> /name <fieldName> /value <fieldName> [/describ <descriptionInfo.fieldName, default=Description> /out <values.csv>]")>
    <Description("Subset of the input table file by columns, produce a <name,value,description> dataset.")>
    Public Function NameValues(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim name$ = args <= "/name"
        Dim value$ = args <= "/value"
        Dim describ$ = args("/describ") Or "Description"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.[{name.NormalizePathString}={value.NormalizePathString}].csv"
        Dim dataset = DataFrame.Load([in])
        Dim getName = dataset.GetValueLambda(name)
        Dim getValue = dataset.GetValueLambda(value)
        Dim getDescribInfo = dataset.GetValueLambda(describ)
        Dim maps = dataset.Rows _
            .Select(Function(r)
                        Return New NamedValue(Of String) With {
                            .Name = getName(r),
                            .Value = getValue(r),
                            .Description = getDescribInfo(r)
                        }
                    End Function) _
            .ToArray

        Return maps.SaveTo(out).CLICode
    End Function

    <ExportAPI("/subset")>
    <Description("Subset of the table file by a given specific column labels")>
    <Usage("/subset /in <table.csv> /columns <column.list> [/out <subset.csv>]")>
    Public Function SubsetByColumns(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim columns$() = Tokenizer.CharsParser(args <= "/columns")
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.projects={columns.JoinBy(",").NormalizePathString(False)}.csv"

        Using output As StreamWriter = out.OpenWriter
            Call DATA.ProjectLargeDataFrame([in], columns, output)
        End Using

        Return 0
    End Function

    ''' <summary>
    ''' 为ID编号添加一个tag来让重复出现的ID编号变成唯一的编号
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/unique")>
    <Usage("/unique /in <dataset.csv> [/out <out.csv>]")>
    <Description("Helper tools for make the ID column value uniques.")>
    <Group(Program.CsvTools)>
    Public Function Unique(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.ID_unique.csv"
        Dim file As csv = csv.Load(path:=[in])
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
    <ExportAPI("/cbind")>
    <Usage("/cbind /in <a.csv> /append <b.csv> [/ID.a <default=ID> /ID.b <default=ID> /grep.ID <grep_script, default=""token <SPACE> first""> /unique /nothing.as.empty /out <ALL.csv>]")>
    <Description("Join of two table by a unique ID.")>
    <ArgumentAttribute("/in", False, CLITypes.File,
              Description:="The table for append by column, its row ID can be duplicated.")>
    <ArgumentAttribute("/append", False, CLITypes.File,
              Description:="The target table that will be append into the table ``a``, the row ID must be unique!")>
    <ArgumentAttribute("/grep.ID", True, CLITypes.String, PipelineTypes.undefined, AcceptTypes:={GetType(String)},
              Description:="This argument parameter describ how to parse the ID in file ``a.csv``")>
    <ArgumentAttribute("/unique", True, CLITypes.Boolean,
              Description:="Make the id of file ``append`` be unique?")>
    <Group(Program.CsvTools)>
    Public Function cbind(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim append$ = args <= "/append"
        Dim out$ = args("/out") Or ([in].TrimSuffix & "+" & append.BaseName & ".csv")
        Dim IDa$ = args("/ID.a")
        Dim IDb$ = args("/ID.b")
        Dim nothingAsEmpty As Boolean = args("/nothing.as.empty")
        Dim a = EntityObject.LoadDataSet([in], uidMap:=IDa)
        Dim b As Contract = Contract.Load(append, uidMap:=IDb, doUnique:=args("/unique"))

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
    <Usage("/rbind /in <*.csv.DIR> [/order_by <column_name> /out <EXPORT.csv>]")>
    <ArgumentAttribute("/in", False, CLITypes.File, PipelineTypes.std_in,
              Description:="A directory path that contains csv files that will be merge into one file directly.")>
    <Group(Program.CsvTools)>
    Public Function rbind(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim out$ = args("/out") Or ([in].Split("*"c).First.TrimDIR & ".rbind.csv")
        Dim source$()
        Dim order_by$ = args("/order_by")
        Dim orderMethod As Func(Of Dictionary(Of String, String), Double) = Nothing

        If Not order_by.StringEmpty Then
            orderMethod = Function(propData)
                              Return Val(propData(order_by).Select(AddressOf AscW).JoinBy(""))
                          End Function
        End If

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
            .DirectAppends(EXPORT:=out, orderBy:=orderMethod) _
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

    <ExportAPI("/association")>
    <Usage("/association /a <a.csv> /b <dataset.csv> [/column.A <scan0> /column.B <scan0> /ignore.blank.index /out <out.csv>]")>
    <Description("Append part of data of table ``b`` to table ``a``")>
    Public Function Association(args As CommandLine) As Integer
        Dim a$ = args <= "/a"
        Dim b$ = args <= "/b"
        Dim columnNameA$ = args("/column.A")
        Dim BcolumnIndex$ = args("/column.B")
        Dim bName$ = b.BaseName
        Dim ignoreBlankIndex As Boolean = args("/ignore.blank.index")
        Dim aData = EntityObject.LoadDataSet(a, uidMap:=columnNameA) _
            .GroupBy(Function(n) If(n.ID.StringEmpty, "", n.ID)) _
            .ToDictionary(Function(n) n.Key,
                          Function(g)
                              Return g.ToArray
                          End Function)
        Dim mapsB As New Dictionary(Of String, String) From {
            {BcolumnIndex, NameOf(EntityObject.ID)}
        }
        Dim out$ = args("/out") Or $"{a.TrimSuffix}_AND_{bName}.csv"
        Dim associates As New List(Of EntityObject)
        Dim key$

        If ignoreBlankIndex Then
            If aData.ContainsKey("") Then
                associates += aData.Popout("")
            End If
        End If

        ' 假设B的数据非常大的话
        For Each rowB As EntityObject In b.OpenHandle(maps:=mapsB).AsLinq(Of EntityObject)
            If ignoreBlankIndex AndAlso rowB.ID.StringEmpty Then
                Continue For
            End If

            If aData.ContainsKey(rowB.ID) Then
                Dim rowsA As EntityObject() = aData.Popout(rowB.ID)

                For Each x As EntityObject In rowsA
                    For Each [property] In rowB.Properties
                        key = bName & "." & [property].Key
                        x.Properties.Add(key, [property].Value)
                    Next

                    associates += x
                Next

                If aData.Count = 0 Then
                    ' 已经关联完了，不需要在遍历整个B文件的数据
                    Exit For
                End If
            End If
        Next

        If aData.Count > 0 Then
            ' 还有一部分的数据是没有在B中存在关联的
            associates += aData.Values.IteratesALL
        End If

        Return associates _
            .SaveDataSet(out, KeyMap:=columnNameA) _
            .CLICode
    End Function

    ''' <summary>
    ''' a - b
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("/subtract")>
    <Description("Performing ``a - b`` subtract by row unique id.")>
    <Usage("/subtract /a <data.csv> /b <data.csv> [/out <subtract.csv>]")>
    Public Function Subtract(args As CommandLine) As Integer
        Dim a$ = args <= "/a"
        Dim b$ = args <= "/b"
        Dim out$ = args("/out") Or $"{a.TrimSuffix}_subtract_{b.BaseName}.csv"
        Dim dataA = EntityObject.LoadDataSet(a).ToArray
        Dim indexB As Index(Of String) = EntityObject _
            .LoadDataSet(b) _
            .Select(Function(r) r.ID) _
            .Indexing

        Return dataA _
            .Where(Function(r) Not r.ID Like indexB) _
            .SaveTo(out) _
            .CLICode
    End Function

    <ExportAPI("/removes")>
    <Usage("/removes /in <dataset.csv> /pattern <regexp_pattern> [/by_row /out <out.csv>]")>
    <Description("Removes row or column data by given regular expression pattern.")>
    <ArgumentAttribute("/by_row", True, CLITypes.Boolean, AcceptTypes:={GetType(Boolean)},
              Description:="This argument specific that removes data by row or by column, by default is by column.")>
    Public Function Removes(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim pattern$ = args <= "/pattern"
        Dim by_row As Boolean = args("/by_row")
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.removes[{pattern.NormalizePathString(False)}].csv"
        Dim removedOut = out.TrimSuffix & "_removedParts.csv"
        Dim data = EntityObject.LoadDataSet([in])
        Dim regexp As New Regex(pattern, RegexICSng)
        Dim result As New List(Of EntityObject)
        Dim removedParts As New List(Of EntityObject)

        If by_row Then
            For Each row As EntityObject In data
                If regexp.Match(row.ID).Success Then
                    removedParts += row
                Else
                    result += row
                End If
            Next
        Else
            Dim columnNames As StringVector = data(Scan0).Properties.Keys.ToArray
            Dim deletes As Index(Of String) = columnNames _
                .Where(Function(name) regexp.Match(name).Success) _
                .Indexing
            Dim subsetKeys As String() = columnNames(Not columnNames Like deletes)

            result = data _
                .Select(Function(row)
                            Return New EntityObject With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(subsetKeys)
                            }
                        End Function) _
                .AsList
            subsetKeys = deletes.Objects
            removedParts = data _
                .Select(Function(row)
                            Return New EntityObject With {
                                .ID = row.ID,
                                .Properties = row.Properties.Subset(subsetKeys)
                            }
                        End Function) _
                .AsList
        End If

        Call removedParts.SaveTo(removedOut)

        Return result.SaveTo(out).CLICode
    End Function

    <ExportAPI("/takes")>
    <Description("Takes specific rows by a given row id list.")>
    <Usage("/takes /in <data.csv> /id <id.list> [/reverse /out <takes.csv>]")>
    <ArgumentAttribute("/reverse", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="If this argument is presents in the cli inputs, then all of the rows that not in input list will be output as result.")>
    Public Function Takes(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim id$ = args <= "/id"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.{id.BaseName}.csv"
        Dim idlist As Index(Of String) = id.ReadAllLines.Distinct.ToLower.ToArray
        Dim data = EntityObject.LoadDataSet([in]).ToArray
        Dim isReverse As Boolean = args("/reverse")
        Dim result As EntityObject()

        If Not isReverse Then
            result = data _
                .Where(Function(row)
                           Return LCase(row.ID) Like idlist
                       End Function) _
                .ToArray
        Else
            result = data _
                .Where(Function(row)
                           Return Not LCase(row.ID) Like idlist
                       End Function) _
                .ToArray
        End If

        Return result.SaveTo(out).CLICode
    End Function

    <ExportAPI("/transpose")>
    <Usage("/transpose /in <data.csv> [/out <data.transpose.csv>]")>
    Public Function Transpose(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.transpose.csv"
        Dim matrix = csv.Load([in]).Transpose

        Return matrix.Save(out).CLICode
    End Function
End Module
