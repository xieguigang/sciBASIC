#Region "Microsoft.VisualBasic::d81a74c352036e75044a38eb2a027291, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\ConsoleTableBuilderExtensions.vb"

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

    '   Total Lines: 639
    '    Code Lines: 484 (75.74%)
    ' Comment Lines: 46 (7.20%)
    '    - Xml Docs: 91.30%
    ' 
    '   Blank Lines: 109 (17.06%)
    '     File Size: 28.91 KB


    '     Module ConsoleTableBuilderExtensions
    ' 
    '         Function: (+3 Overloads) AddColumn, (+4 Overloads) AddRow, BuildMetaRowsFormat, CreateTableForCustomFormat, Export
    '                   FillCharMap, FillHeaderCharMap, RealLength, TrimColumn, (+3 Overloads) WithCharMapDefinition
    '                   (+2 Overloads) WithColumn, WithColumnFormatter, WithFormat, WithFormatter, WithHeaderCharMapDefinition
    '                   WithHeaderTextAlignment, WithMetadataRow, WithMinLength, WithPaddingLeft, WithPaddingRight
    '                   WithTextAlignment, (+3 Overloads) WithTitle
    ' 
    '         Sub: ExportAndWrite, ExportAndWriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Terminal.TablePrinter

    <HideModuleName>
    Public Module ConsoleTableBuilderExtensions

        <Extension()>
        Public Function AddColumn(builder As ConsoleTableBuilder, columnName As String) As ConsoleTableBuilder
            builder.Column.Add(columnName)
            Return builder
        End Function

        <Extension()>
        Public Function AddColumn(builder As ConsoleTableBuilder, columnNames As List(Of String)) As ConsoleTableBuilder
            builder.Column.AddRange(columnNames)
            Return builder
        End Function

        <Extension()>
        Public Function AddColumn(builder As ConsoleTableBuilder, ParamArray columnNames As String()) As ConsoleTableBuilder
            builder.Column.AddRange(New List(Of Object)(columnNames))
            Return builder
        End Function

        <Extension()>
        Public Function WithColumn(builder As ConsoleTableBuilder, columnNames As List(Of String)) As ConsoleTableBuilder
            builder.Column = New List(Of Object)()
            builder.Column.AddRange(columnNames)
            Return builder
        End Function

        <Extension()>
        Public Function WithColumn(builder As ConsoleTableBuilder, ParamArray columnNames As String()) As ConsoleTableBuilder
            builder.Column = New List(Of Object)()
            builder.Column.AddRange(New List(Of Object)(columnNames))
            Return builder
        End Function

        <Extension()>
        Public Function AddRow(builder As ConsoleTableBuilder, ParamArray rowValues As Object()) As ConsoleTableBuilder
            If rowValues Is Nothing Then Return builder
            builder.Rows.Add(rowValues)
            Return builder
        End Function

        <Extension()>
        Public Function WithMetadataRow(builder As ConsoleTableBuilder, position As MetaRowPositions, contentGenerator As Func(Of ConsoleTableBuilder, String)) As ConsoleTableBuilder
            Select Case position
                Case MetaRowPositions.Top

                    If builder.TopMetadataRows Is Nothing Then
                        builder.TopMetadataRows = New List(Of KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String)))()
                    End If

                    builder.TopMetadataRows.Add(New KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String))(position, contentGenerator))
                Case MetaRowPositions.Bottom

                    If builder.BottomMetadataRows Is Nothing Then
                        builder.BottomMetadataRows = New List(Of KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String)))()
                    End If

                    builder.BottomMetadataRows.Add(New KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String))(position, contentGenerator))
                Case Else
            End Select

            Return builder
        End Function

        ''' <summary>
        ''' Add title row on top of table
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="title"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function WithTitle(builder As ConsoleTableBuilder, title As String, Optional titleAligntment As TextAligntment = TextAligntment.Center) As ConsoleTableBuilder
            builder.TableTitle = title
            builder.TableTitleTextAlignment = titleAligntment
            Return builder
        End Function

        ''' <summary>
        ''' Add title row on top of table
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="title"></param>
        ''' <param name="foregroundColor">text color</param>
        ''' <returns></returns>
        <Extension()>
        Public Function WithTitle(builder As ConsoleTableBuilder, title As String, foregroundColor As ConsoleColor, Optional titleAligntment As TextAligntment = TextAligntment.Center) As ConsoleTableBuilder
            builder.TableTitle = title
            builder.TableTitleColor = New ConsoleColorNullable(foregroundColor)
            builder.TableTitleTextAlignment = titleAligntment
            Return builder
        End Function

        ''' <summary>
        ''' Add title row on top of table
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="title"></param>
        ''' <param name="foregroundColor">text color</param>
        ''' <param name="backgroundColor">background color</param>
        ''' <returns></returns>
        <Extension()>
        Public Function WithTitle(builder As ConsoleTableBuilder, title As String, foregroundColor As ConsoleColor, backgroundColor As ConsoleColor, Optional titleAligntment As TextAligntment = TextAligntment.Center) As ConsoleTableBuilder
            builder.TableTitle = title
            builder.TableTitleColor = New ConsoleColorNullable(foregroundColor, backgroundColor)
            builder.TableTitleTextAlignment = titleAligntment
            Return builder
        End Function

        <Extension()>
        Public Function WithPaddingLeft(builder As ConsoleTableBuilder, paddingLeft As String) As ConsoleTableBuilder
            builder.PaddingLeft = If(paddingLeft, String.Empty)
            Return builder
        End Function

        <Extension()>
        Public Function WithPaddingRight(builder As ConsoleTableBuilder, paddingRight As String) As ConsoleTableBuilder
            builder.PaddingRight = If(paddingRight, String.Empty)
            Return builder
        End Function

        <Extension()>
        Public Function WithFormatter(builder As ConsoleTableBuilder, columnIndex As Integer, formatter As Func(Of String, String)) As ConsoleTableBuilder
            If Not builder.FormatterStore.ContainsKey(columnIndex) Then
                builder.FormatterStore.Add(columnIndex, formatter)
            Else
                builder.FormatterStore(columnIndex) = formatter
            End If

            Return builder
        End Function

        <Extension()>
        Public Function WithColumnFormatter(builder As ConsoleTableBuilder, columnIndex As Integer, formatter As Func(Of String, String)) As ConsoleTableBuilder
            If Not builder.ColumnFormatterStore.ContainsKey(columnIndex) Then
                builder.ColumnFormatterStore.Add(columnIndex, formatter)
            Else
                builder.ColumnFormatterStore(columnIndex) = formatter
            End If

            Return builder
        End Function

        ''' <summary>
        ''' Text alignment definition
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="alignmentData"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function WithTextAlignment(builder As ConsoleTableBuilder, alignmentData As Dictionary(Of Integer, TextAligntment)) As ConsoleTableBuilder
            If alignmentData IsNot Nothing Then
                builder.TextAligmentData = alignmentData
            End If

            Return builder
        End Function

        <Extension()>
        Public Function WithHeaderTextAlignment(builder As ConsoleTableBuilder, alignmentData As Dictionary(Of Integer, TextAligntment)) As ConsoleTableBuilder
            If alignmentData IsNot Nothing Then
                builder.HeaderTextAligmentData = alignmentData
            End If

            Return builder
        End Function

        <Extension()>
        Public Function WithMinLength(builder As ConsoleTableBuilder, minLengthData As Dictionary(Of Integer, Integer)) As ConsoleTableBuilder
            If minLengthData IsNot Nothing Then
                builder.MinLengthData = minLengthData
            End If

            Return builder
        End Function

        <Extension()>
        Public Function TrimColumn(builder As ConsoleTableBuilder, Optional canTrimColumn As Boolean = True) As ConsoleTableBuilder
            builder.CanTrimColumn = canTrimColumn
            Return builder
        End Function

        <Extension()>
        Public Function AddRow(builder As ConsoleTableBuilder, row As List(Of Object)) As ConsoleTableBuilder
            If row Is Nothing Then Return builder
            builder.Rows.Add(row.ToArray)
            Return builder
        End Function

        <Extension()>
        Public Function AddRow(builder As ConsoleTableBuilder, rows As List(Of List(Of Object))) As ConsoleTableBuilder
            If rows Is Nothing Then Return builder
            builder.Rows.AddRange(rows)
            Return builder
        End Function

        <Extension()>
        Public Function AddRow(builder As ConsoleTableBuilder, row As DataRow) As ConsoleTableBuilder
            If row Is Nothing Then Return builder
            builder.Rows.Add(row.ItemArray)
            Return builder
        End Function

        <Extension()>
        Public Function WithFormat(builder As ConsoleTableBuilder, format As ConsoleTableBuilderFormat) As ConsoleTableBuilder
            ' reset CharMapPositions
            builder.CharMapPositionStore = Nothing
            builder.TableFormat = format

            Select Case builder.TableFormat
                Case ConsoleTableBuilderFormat.Default
                    builder.CharMapPositionStore = New Dictionary(Of CharMapPositions, Char) From {
                        {CharMapPositions.TopLeft, "-"c},
                        {CharMapPositions.TopCenter, "-"c},
                        {CharMapPositions.TopRight, "-"c},
                        {CharMapPositions.MiddleLeft, "-"c},
                        {CharMapPositions.MiddleCenter, "-"c},
                        {CharMapPositions.MiddleRight, "-"c},
                        {CharMapPositions.BottomLeft, "-"c},
                        {CharMapPositions.BottomCenter, "-"c},
                        {CharMapPositions.BottomRight, "-"c},
                        {CharMapPositions.BorderTop, "-"c},
                        {CharMapPositions.BorderLeft, "|"c},
                        {CharMapPositions.BorderRight, "|"c},
                        {CharMapPositions.BorderBottom, "-"c},
                        {CharMapPositions.DividerX, "-"c},
                        {CharMapPositions.DividerY, "|"c}
                    }
                Case ConsoleTableBuilderFormat.MarkDown
                    builder.CharMapPositionStore = New Dictionary(Of CharMapPositions, Char) From {
                        {CharMapPositions.DividerY, "|"c},
                        {CharMapPositions.BorderLeft, "|"c},
                        {CharMapPositions.BorderRight, "|"c}
                    }
                    builder.HeaderCharMapPositionStore = New Dictionary(Of HeaderCharMapPositions, Char) From {
                        {HeaderCharMapPositions.BorderBottom, "-"c},
                        {HeaderCharMapPositions.BottomLeft, "|"c},
                        {HeaderCharMapPositions.BottomCenter, "|"c},
                        {HeaderCharMapPositions.BottomRight, "|"c},
                        {HeaderCharMapPositions.BorderLeft, "|"c},
                        {HeaderCharMapPositions.BorderRight, "|"c},
                        {HeaderCharMapPositions.Divider, "|"c}
                    }
                Case ConsoleTableBuilderFormat.Alternative
                    builder.CharMapPositionStore = New Dictionary(Of CharMapPositions, Char) From {
                        {CharMapPositions.TopLeft, "+"c},
                        {CharMapPositions.TopCenter, "+"c},
                        {CharMapPositions.TopRight, "+"c},
                        {CharMapPositions.MiddleLeft, "+"c},
                        {CharMapPositions.MiddleCenter, "+"c},
                        {CharMapPositions.MiddleRight, "+"c},
                        {CharMapPositions.BottomLeft, "+"c},
                        {CharMapPositions.BottomCenter, "+"c},
                        {CharMapPositions.BottomRight, "+"c},
                        {CharMapPositions.BorderTop, "-"c},
                        {CharMapPositions.BorderRight, "|"c},
                        {CharMapPositions.BorderBottom, "-"c},
                        {CharMapPositions.BorderLeft, "|"c},
                        {CharMapPositions.DividerX, "-"c},
                        {CharMapPositions.DividerY, "|"c}
                    }
                Case ConsoleTableBuilderFormat.Minimal
                    builder.CharMapPositionStore = New Dictionary(Of CharMapPositions, Char) From {
                    }
                    builder.HeaderCharMapPositionStore = New Dictionary(Of HeaderCharMapPositions, Char) From {
                        {HeaderCharMapPositions.BorderBottom, "-"c}
                    }
                    builder.PaddingLeft = String.Empty
                    builder.PaddingRight = " "
                Case Else
            End Select

            Return builder
        End Function

        <Extension()>
        Public Function WithCharMapDefinition(builder As ConsoleTableBuilder) As ConsoleTableBuilder
            Return builder.WithCharMapDefinition(New Dictionary(Of CharMapPositions, Char) From {
            })
        End Function

        <Extension()>
        Public Function WithCharMapDefinition(builder As ConsoleTableBuilder, charMapPositions As Dictionary(Of CharMapPositions, Char)) As ConsoleTableBuilder
            builder.CharMapPositionStore = charMapPositions
            Return builder
        End Function

        <Extension()>
        Public Function WithCharMapDefinition(builder As ConsoleTableBuilder, charMapPositions As Dictionary(Of CharMapPositions, Char), Optional headerCharMapPositions As Dictionary(Of HeaderCharMapPositions, Char) = Nothing) As ConsoleTableBuilder
            builder.CharMapPositionStore = charMapPositions
            builder.HeaderCharMapPositionStore = headerCharMapPositions
            Return builder
        End Function

        <Extension()>
        Public Function WithHeaderCharMapDefinition(builder As ConsoleTableBuilder, Optional headerCharMapPositions As Dictionary(Of HeaderCharMapPositions, Char) = Nothing) As ConsoleTableBuilder
            builder.HeaderCharMapPositionStore = headerCharMapPositions
            Return builder
        End Function

        ''' <summary>
        ''' export the table print content string
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function Export(builder As ConsoleTableBuilder) As StringBuilder
            Dim numberOfColumns As Integer = 0

            If builder.Rows.Any() Then
                numberOfColumns = builder.Rows.Max(Function(x) x.Length)
            Else

                If builder.Column IsNot Nothing Then
                    numberOfColumns = builder.Column.Count()
                End If
            End If

            If numberOfColumns = 0 Then
                Return New StringBuilder()
            End If

            If builder.Column Is Nothing Then
                numberOfColumns = 0
            Else

                If numberOfColumns < builder.Column.Count Then
                    numberOfColumns = builder.Column.Count
                End If
            End If

            For i As Integer = 0 To 1 - 1

                If builder.Column IsNot Nothing AndAlso builder.Column.Count < numberOfColumns Then
                    Dim missCount = numberOfColumns - builder.Column.Count

                    For j = 0 To missCount - 1
                        builder.Column.Add(Nothing)
                    Next
                End If
            Next

            For i As Integer = 0 To builder.Rows.Count - 1

                If builder.Rows(i).Length < numberOfColumns Then
                    Dim missCount = numberOfColumns - builder.Rows(i).Length
                    Dim miss As Object() = New Object(missCount - 1) {}

                    builder.Rows(i) = builder.Rows(i).JoinIterates(miss).ToArray
                End If
            Next

            Return CreateTableForCustomFormat(builder)
        End Function

        ''' <summary>
        ''' export the table content as string and do console write
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="alignment"></param>
        <Extension()>
        Public Sub ExportAndWrite(builder As ConsoleTableBuilder, Optional alignment As TableAligntment = TableAligntment.Left)
            Dim strBuilder = builder.Export()
            Dim lines = strBuilder.ToString.LineTokens
            Dim linesCount = lines.Length
            Dim pad As Integer

            For i As Integer = 0 To linesCount - 1
                Dim row = String.Empty

                Select Case alignment
                    Case TableAligntment.Left
                        row = lines(i)
                    Case TableAligntment.Center
                        pad = Console.WindowWidth / 2 + lines(i).RealLength(True) / 2 - (lines(i).RealLength(True) - lines(i).Length)
                        row = String.Format("{0," & pad & "}", lines(i))
                    Case TableAligntment.Right
                        row = New String(" "c, Console.WindowWidth - lines(i).RealLength(True)) & lines(i)
                End Select

                If i = 0 AndAlso
                    Not String.IsNullOrEmpty(builder.TableTitle) AndAlso
                    builder.TableTitle.Trim().Length <> 0 AndAlso
                    builder.TableTitleColor.ForegroundColor IsNot Nothing AndAlso
                    builder.TitlePositionStartAt > 0 AndAlso
                    builder.TitlePositionLength > 0 Then

                    Dim newTitlePositionStartAt = builder.TitlePositionStartAt + (row.Length - lines(i).Length)
                    Console.Write(row.Substring(0, newTitlePositionStartAt))
                    Console.ForegroundColor = builder.TableTitleColor.ForegroundColor

                    If Not builder.TableTitleColor.BackgroundColor Is Nothing Then
                        Console.BackgroundColor = builder.TableTitleColor.BackgroundColor
                    End If

                    Console.Write(row.Substring(newTitlePositionStartAt, builder.TitlePositionLength))
                    Console.ResetColor()
                    Console.Write(row.Substring(newTitlePositionStartAt + builder.TitlePositionLength, row.Length - (newTitlePositionStartAt + builder.TitlePositionLength)))
                    Console.Write(Microsoft.VisualBasic.Strings.ChrW(10))
                Else

                    If i = linesCount - 2 Then
                        If row.EndsWith(Microsoft.VisualBasic.Strings.ChrW(13).ToString()) Then
                            Console.Write(row.Substring(0, row.Length - 1))
                        Else
                            Console.Write(row)
                        End If
                    Else

                        If i = linesCount - 1 Then ' is last line
                            Console.Write(row)
                        Else
                            Console.WriteLine(row)
                        End If
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' console write line of the table data
        ''' </summary>
        ''' <param name="builder"></param>
        ''' <param name="alignment"></param>
        <Extension()>
        Public Sub ExportAndWriteLine(builder As ConsoleTableBuilder, Optional alignment As TableAligntment = TableAligntment.Left)
            builder.ExportAndWrite(alignment)
            Console.Write(Microsoft.VisualBasic.Strings.ChrW(10))
        End Sub

        Private Function CreateTableForCustomFormat(builder As ConsoleTableBuilder) As StringBuilder
            If builder.CharMapPositionStore Is Nothing Then
                builder.WithFormat(ConsoleTableBuilderFormat.Default)
            End If

            builder.PopulateFormattedColumnsRows()
            Dim columnLengths = builder.GetCadidateColumnLengths()
            Dim columnNoUtf8CharasLengths = builder.GetCadidateColumnLengths(False)
            builder.CenterRowContent(columnLengths)
            Dim filledMap = FillCharMap(builder.CharMapPositionStore)
            Dim filledHeaderMap = FillHeaderCharMap(builder.HeaderCharMapPositionStore)
            Dim strBuilder = New StringBuilder()
            Dim topMetadataStringBuilder = BuildMetaRowsFormat(builder, MetaRowPositions.Top)

            For i = 0 To topMetadataStringBuilder.Count - 1
                strBuilder.AppendLine(topMetadataStringBuilder(i))
            Next

            Dim tableTopLine = builder.CreateTableTopLine(columnLengths, filledMap)
            Dim tableRowContentFormat = builder.CreateTableContentLineFormat(columnLengths, filledMap)
            Dim tableMiddleLine = builder.CreateTableMiddleLine(columnLengths, filledMap)
            Dim tableBottomLine = builder.CreateTableBottomLine(columnLengths, filledMap)
            Dim headerTopLine = String.Empty
            Dim headerRowContentFormat = String.Empty
            Dim headerBottomLine = String.Empty

            If filledHeaderMap IsNot Nothing Then
                headerTopLine = builder.CreateHeaderTopLine(columnLengths, filledMap, filledHeaderMap)
                headerRowContentFormat = builder.CreateHeaderContentLineFormat(columnLengths, filledMap, filledHeaderMap)
                headerBottomLine = builder.CreateHeaderBottomLine(columnLengths, filledMap, filledHeaderMap)
            End If

            ' find the longest formatted line
            Dim hasHeader = builder.FormattedColumns IsNot Nothing AndAlso builder.FormattedColumns.Any() AndAlso builder.FormattedColumns.Max(Function(x) If(x, String.Empty).ToString().Length) > 0

            ' header
            If hasHeader Then
                If Not Equals(headerTopLine, Nothing) AndAlso headerTopLine.Trim().Length > 0 Then
                    strBuilder.AppendLine(headerTopLine)
                Else

                    If Not Equals(tableTopLine, Nothing) AndAlso tableTopLine.Trim().Length > 0 Then
                        strBuilder.AppendLine(tableTopLine)
                    End If
                End If

                Dim headerSlices = builder.FormattedColumns.ToArray()
                Dim formattedHeaderSlice = builder.CenterColumnContent(headerSlices, columnLengths)

                If Not Equals(headerRowContentFormat, Nothing) AndAlso headerRowContentFormat.Trim().Length > 0 Then
                    strBuilder.AppendLine(String.Format(headerRowContentFormat, formattedHeaderSlice))
                Else
                    strBuilder.AppendLine(String.Format(tableRowContentFormat, formattedHeaderSlice))
                End If
            End If

            Dim results = builder.FormattedRows.[Select](Function(row)
                                                             Dim rowFormate = builder.CreateRawLineFormat(columnLengths, filledMap, row.ToArray())
                                                             Return String.Format(rowFormate, row.ToArray())
                                                         End Function).ToList()
            Dim isFirstRow = True

            For Each row In results

                If isFirstRow Then
                    If hasHeader Then
                        If (String.IsNullOrEmpty(headerBottomLine) OrElse headerBottomLine.Length = 0) AndAlso tableMiddleLine.Length > 0 Then
                            strBuilder.AppendLine(tableMiddleLine)
                        Else

                            If headerBottomLine.Length > 0 Then
                                strBuilder.AppendLine(headerBottomLine)
                            End If
                        End If
                    Else

                        If tableTopLine.Length > 0 Then
                            strBuilder.AppendLine(tableTopLine)
                        End If
                    End If

                    isFirstRow = False
                Else

                    If tableMiddleLine.Length > 0 Then
                        strBuilder.AppendLine(tableMiddleLine)
                    End If
                End If

                strBuilder.AppendLine(row)
            Next

            If results.Any() Then
                If tableBottomLine.Length > 0 Then
                    strBuilder.AppendLine(tableBottomLine)
                End If
            Else

                If (String.IsNullOrEmpty(headerBottomLine) OrElse headerBottomLine.Length = 0) AndAlso tableBottomLine.Length > 0 Then
                    strBuilder.AppendLine(tableBottomLine)
                Else

                    If headerBottomLine.Length > 0 Then
                        strBuilder.AppendLine(headerBottomLine)
                    End If
                End If
            End If

            Dim bottomMetadataStringBuilder = BuildMetaRowsFormat(builder, MetaRowPositions.Bottom)

            For i = 0 To bottomMetadataStringBuilder.Count - 1
                strBuilder.AppendLine(bottomMetadataStringBuilder(i))
            Next

            Return strBuilder
        End Function

        Private Function BuildMetaRowsFormat(builder As ConsoleTableBuilder, position As MetaRowPositions) As List(Of String)
            Dim result = New List(Of String)()

            Select Case position
                Case MetaRowPositions.Top

                    If builder.TopMetadataRows.Any() Then
                        For Each item In builder.TopMetadataRows

                            If item.Value IsNot Nothing Then
                                result.Add(item.Value.Invoke(builder))
                            End If
                        Next
                    End If

                Case MetaRowPositions.Bottom

                    If builder.BottomMetadataRows.Any() Then
                        For Each item In builder.BottomMetadataRows

                            If item.Value IsNot Nothing Then
                                result.Add(item.Value.Invoke(builder))
                            End If
                        Next
                    End If

                Case Else
            End Select

            Return result
        End Function

        Private Function FillCharMap(definition As Dictionary(Of CharMapPositions, Char)) As Dictionary(Of CharMapPositions, Char)
            If definition Is Nothing Then
                Return New Dictionary(Of CharMapPositions, Char)()
            End If

            Dim filledMap = definition

            For Each c In CType([Enum].GetValues(GetType(CharMapPositions)), CharMapPositions())

                If Not filledMap.ContainsKey(c) Then
                    filledMap.Add(c, Microsoft.VisualBasic.Strings.ChrW(0))
                End If
            Next

            Return filledMap
        End Function

        Private Function FillHeaderCharMap(definition As Dictionary(Of HeaderCharMapPositions, Char)) As Dictionary(Of HeaderCharMapPositions, Char)
            If definition Is Nothing Then
                Return Nothing
            End If

            Dim filledMap = definition

            For Each c In CType([Enum].GetValues(GetType(HeaderCharMapPositions)), HeaderCharMapPositions())

                If Not filledMap.ContainsKey(c) Then
                    filledMap.Add(c, Microsoft.VisualBasic.Strings.ChrW(0))
                End If
            Next

            Return filledMap
        End Function

        <Extension()>
        Public Function RealLength(value As String, withUtf8Characters As Boolean) As Integer
            If String.IsNullOrEmpty(value) Then Return 0
            If Not withUtf8Characters Then Return value.Length
            Dim i = 0 'count

            For Each newChar In value.Select(AddressOf AscW)
                If newChar >= &H4E00 AndAlso newChar <= &H9FBB Then
                    'utf-8 characters
                    i += 2
                Else
                    i += 1
                End If
            Next

            Return i
        End Function
    End Module
End Namespace
