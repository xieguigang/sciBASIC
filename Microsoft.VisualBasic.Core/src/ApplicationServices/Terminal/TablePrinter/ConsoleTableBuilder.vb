#Region "Microsoft.VisualBasic::5122faa1c50741a39d7b302b4d946f26, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\ConsoleTableBuilder.vb"

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

    '   Total Lines: 669
    '    Code Lines: 500 (74.74%)
    ' Comment Lines: 43 (6.43%)
    '    - Xml Docs: 25.58%
    ' 
    '   Blank Lines: 126 (18.83%)
    '     File Size: 38.40 KB


    '     Class ConsoleTableBuilder
    ' 
    '         Properties: CharMapPositionStore, Column, FormattedColumns, FormattedRows, HeaderCharMapPositionStore
    '                     NumberOfColumns, NumberOfRows, Rows, TableFormat, TitlePositionLength
    '                     TitlePositionStartAt
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CanRemoveBorderLeft, CanRemoveBorderRight, CanRemoveDividerY, CenterColumnContent, CenteredString
    '                   CreateHeaderBottomLine, CreateHeaderContentLineFormat, CreateHeaderTopLine, CreateRawLineFormat, CreateTableBottomLine
    '                   CreateTableContentLineFormat, CreateTableMiddleLine, CreateTableTopLine, EmbedTitle, (+9 Overloads) From
    '                   GetCadidateColumnLengths
    ' 
    '         Sub: CenterRowContent, PopulateFormattedColumnsRows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Namespace ApplicationServices.Terminal.TablePrinter

    ''' <summary>
    ''' A fluent library to print out a nicely formatted table in a console application
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/minhhungit/ConsoleTableExt
    ''' </remarks>
    Public Class ConsoleTableBuilder
        Friend Property Column As List(Of Object)
        Friend Property FormattedColumns As List(Of String)
        Friend Property Rows As List(Of Object())
        Friend Property FormattedRows As List(Of List(Of Object))
        Friend Property TableFormat As ConsoleTableBuilderFormat
        Friend Property CharMapPositionStore As Dictionary(Of CharMapPositions, Char) = Nothing
        Friend Property HeaderCharMapPositionStore As Dictionary(Of HeaderCharMapPositions, Char) = Nothing
        Friend TopMetadataRows As New List(Of KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String)))()
        Friend BottomMetadataRows As New List(Of KeyValuePair(Of MetaRowPositions, Func(Of ConsoleTableBuilder, String)))()
        Friend TextAligmentData As Dictionary(Of Integer, TextAligntment) = New Dictionary(Of Integer, TextAligntment)()
        Friend HeaderTextAligmentData As Dictionary(Of Integer, TextAligntment) = New Dictionary(Of Integer, TextAligntment)()
        Friend MinLengthData As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)()
        Friend CanTrimColumn As Boolean = False
        Friend TableTitle As String = String.Empty
        Friend TableTitleTextAlignment As TextAligntment = TextAligntment.Center
        Friend TableTitleColor As ConsoleColorNullable = New ConsoleColorNullable()
        Friend PaddingLeft As String = " "
        Friend PaddingRight As String = " "
        Friend Property TitlePositionStartAt As Integer
        Friend Property TitlePositionLength As Integer
        Friend FormatterStore As Dictionary(Of Integer, Func(Of String, String)) = New Dictionary(Of Integer, Func(Of String, String))()
        Friend ColumnFormatterStore As Dictionary(Of Integer, Func(Of String, String)) = New Dictionary(Of Integer, Func(Of String, String))()

        Private Sub New()
            Column = New List(Of Object)()
            Rows = New List(Of Object())()
            TableFormat = ConsoleTableBuilderFormat.Default
        End Sub

        ''' <summary>
        ''' This function allow developer implement theme self data-source
        ''' </summary>
        ''' <param name="func"></param>
        ''' <returns></returns>
        Public Shared Function From(func As Func(Of ConsoleTableBaseData)) As ConsoleTableBuilder
            If func IsNot Nothing Then
                Dim baseData = func.Invoke()
                Return From(baseData)
            Else
                Throw New Exception("invaild function")
            End If
        End Function

        Public Shared Function From(baseData As ConsoleTableBaseData) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            If baseData IsNot Nothing Then
                If baseData.Rows IsNot Nothing Then
                    builder.Rows = baseData.Rows
                End If

                If baseData.Column IsNot Nothing Then
                    builder.Column = baseData.Column
                End If
            End If

            Return builder
        End Function

        Public Shared Function From(list As List(Of Integer)) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            For Each value In list
                builder.Rows.Add({value})
            Next

            Return builder
        End Function

        Public Shared Function From(list As List(Of String)) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            For Each value In list
                builder.Rows.Add({value})
            Next

            Return builder
        End Function

        Public Shared Function From(list As List(Of Object)) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            For Each value In list
                builder.Rows.Add({value})
            Next

            Return builder
        End Function

        Public Shared Function From(dt As DataTable) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            If dt Is Nothing Then
                Return builder
            End If

            Dim columnNames = dt.Columns.Cast(Of DataColumn)().[Select](Function(x) x.ColumnName).ToList()
            builder.Column = New List(Of Object)(columnNames)

            For Each row As DataRow In dt.Rows
                builder.Rows.Add(row.ItemArray)
            Next

            Return builder
        End Function

        Public Shared Function From(Of T As Class)(list As List(Of T)) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            If list Is Nothing Then
                Return builder
            End If

            Dim isClass = GetType(T).IsClass
            Dim props = New List(Of PropertyInfo)()

            If list.Any() Then
                props = Enumerable.First(list).GetType().GetProperties().ToList()
            End If

            Dim columnNames As List(Of Object)

            If isClass Then
                columnNames = If(props.[Select](Function(p)
                                                    Dim attrs = p.GetCustomAttributes(True)

                                                    For Each attr In attrs

                                                        If TypeOf attr Is DescriptionAttribute Then
                                                            Return CType(attr, DescriptionAttribute).Description
                                                        End If
                                                    Next

                                                    Return TryCast(p.Name, Object)
                                                End Function).ToList(), New List(Of Object)())
            Else
                columnNames = New List(Of Object) From {
                    "Value"
                }
            End If

            builder.Column = columnNames

            For Each item In list

                If isClass = True Then
                    Dim itemPropValues = New List(Of Object)()

                    For Each prop In props
                        Dim objValue = prop.GetValue(item, Nothing)
                        itemPropValues.Add(objValue)
                    Next

                    builder.Rows.Add(itemPropValues.ToArray)
                Else
                    builder.Rows.Add({item})
                End If
            Next

            Return builder
        End Function

        Public Shared Function From(rows As List(Of Object())) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            If rows Is Nothing Then
                Return builder
            End If

            For Each row In rows
                builder.Rows.Add(row)
            Next

            Return builder
        End Function

        Public Shared Function From(rows As List(Of List(Of Object))) As ConsoleTableBuilder
            Dim builder = New ConsoleTableBuilder()

            If rows Is Nothing Then
                Return builder
            End If

            For Each row In rows
                builder.Rows.Add(row.ToArray)
            Next

            Return builder
        End Function

        Friend Sub PopulateFormattedColumnsRows()
            FormattedColumns = Enumerable.Range(0, Column.Count).[Select](Function(idx)
                                                                              If ColumnFormatterStore.ContainsKey(idx) Then
                                                                                  Return ColumnFormatterStore(idx)(If(Column(idx) Is Nothing, String.Empty, Column(idx).ToString()))
                                                                              Else
                                                                                  Return If(Column(idx) Is Nothing, String.Empty, Column(idx).ToString())
                                                                              End If
                                                                          End Function).ToList()
            FormattedRows = New List(Of List(Of Object))()

            For i = 0 To Rows.Count - 1
                Dim index = i

                FormattedRows.Add(Enumerable.Range(0, Rows(i).Length).[Select](Function(idx)
                                                                                   If FormatterStore.ContainsKey(idx) Then
                                                                                       Return FormatterStore(idx)(If(Rows(index)(idx) Is Nothing, String.Empty, Rows(index)(idx).ToString()))
                                                                                   Else
                                                                                       Return Rows(index)(idx)
                                                                                   End If
                                                                               End Function).ToList())
            Next
        End Sub

        Friend Sub CenterRowContent(columnLengths As List(Of Integer))
            For i = 0 To FormattedRows.Count - 1

                For j = 0 To FormattedRows(i).Count - 1

                    If TextAligmentData.ContainsKey(j) AndAlso TextAligmentData(j) = TextAligntment.Center Then
                        FormattedRows(i)(j) = CenteredString(FormattedRows(i)(j), columnLengths(j))
                    End If
                Next
            Next
        End Sub

        Friend Function CenterColumnContent(columnSlices As String(), columnLengths As List(Of Integer)) As String()
            For i = 0 To columnSlices.Length - 1

                If HeaderTextAligmentData.ContainsKey(i) Then
                    If HeaderTextAligmentData(i) = TextAligntment.Center Then
                        columnSlices(i) = CenteredString(columnSlices(i), columnLengths(i))
                    End If
                Else

                    If TextAligmentData.ContainsKey(i) AndAlso TextAligmentData(i) = TextAligntment.Center Then
                        columnSlices(i) = CenteredString(columnSlices(i), columnLengths(i))
                    End If
                End If
            Next

            Return columnSlices
        End Function

        Private Function CenteredString(s As Object, width As Integer) As String
            If s Is Nothing Then
                Return Nothing
            End If

            If s.ToString().Length >= width Then
                Return s.ToString()
            End If

            Dim leftPadding As Integer = (width - s.ToString().Length) / 2
            Dim rightPadding As Integer = width - s.ToString().Length - leftPadding
            Return New String(" "c, leftPadding) & s.ToString & New String(" "c, rightPadding)
        End Function

        Friend Function GetCadidateColumnLengths(Optional withUtf8Characters As Boolean = True) As List(Of Integer)
            Dim columnLengths = New List(Of Integer)()
            Dim numberOfColumns = 0

            If FormattedRows.Any() Then
                numberOfColumns = FormattedRows.Max(Function(x) x.Count)
            Else

                If FormattedColumns IsNot Nothing Then
                    numberOfColumns = FormattedColumns.Count
                End If
            End If

            If numberOfColumns = 0 Then
                Return New List(Of Integer)()
            End If

            If numberOfColumns < FormattedColumns.Count Then
                numberOfColumns = FormattedColumns.Count
            End If

            For i = 0 To numberOfColumns - 1
                Dim maxRow = 0
                Dim index = i

                If FormattedRows.Any() Then
                    maxRow = FormattedRows.Where(Function(x) index < x.Count).[Select](Function(x) x(index)).Max(Function(x) If(x Is Nothing, 0, x.ToString().RealLength(withUtf8Characters))) ' list cells of column i
                End If

                If FormattedColumns.ToArray().Length > i AndAlso If(FormattedColumns(i), String.Empty).ToString().RealLength(withUtf8Characters) > maxRow Then
                    maxRow = FormattedColumns(i).ToString().RealLength(withUtf8Characters)
                End If

                If MinLengthData IsNot Nothing AndAlso MinLengthData.ContainsKey(i) Then
                    columnLengths.Add(If(maxRow > MinLengthData(i), maxRow, MinLengthData(i)))
                Else
                    columnLengths.Add(maxRow)
                End If
            Next

            'if (!columnLengths.Any())
            '{
            '    throw new Exception("Table has no columns");
            '}

            If CanTrimColumn Then
                If columnLengths.Any() Then
                    Dim temp = columnLengths

                    'for (int i = 0; i < temp.Count; i++)
                    '{
                    '    if (temp[i] == 0)
                    '    {
                    '        columnLengths.RemoveAt(0);
                    '    }
                    '    else
                    '    {
                    '        break;
                    '    }
                    '}

                    For i = temp.Count - 1 To 0 Step -1

                        If temp(i) = 0 Then
                            columnLengths.RemoveAt(i)
                        Else
                            Exit For
                        End If
                    Next
                End If
            End If

            Return columnLengths
        End Function

        Public ReadOnly Property NumberOfColumns As Integer
            Get
                Return GetCadidateColumnLengths().Count
            End Get
        End Property

        Public ReadOnly Property NumberOfRows As Integer
            Get
                Return Rows.Count
            End Get
        End Property

        'internal string Format(char delimiter)
        '{
        '    string delim = delimiter == '\0' ? string.Empty : delimiter.ToString();

        '    var columnLengths = GetCadidateColumnLengths();

        '    // | {0,-14} | {1,-29} | {2,-13} | {3,-3} | {4,-22} |
        '    if (columnLengths.Count > 0)
        '    {
        '        var format = Enumerable.Range(0, columnLengths.Count)
        '                    .Select(i => PaddingLeft + "{" + i + "," + (TextAligmentData == null ? "-" : (TextAligmentData.ContainsKey(i) ? TextAligmentData[i].ToString() : "-")) + columnLengths[i] + "}" + PaddingRight)
        '                    .Aggregate((s, a) => s + delim + a);

        '        return delim + format + delim;
        '    }
        '    else
        '    {
        '        return string.Empty;
        '    }
        '}

        Private Function EmbedTitle(line As String) As String
            Dim originalTitleLength = TableTitle.Length

            If Not String.IsNullOrEmpty(TableTitle) AndAlso TableTitle.Trim().Length > 0 Then ' !IsNullOrWhiteSpace
                If TableTitle.Length > line.Length - 4 Then
                    TableTitle = TableTitle.Substring(0, line.Length - 4)

                    If originalTitleLength <> TableTitle.Length AndAlso TableTitle.Length > 3 Then
                        TableTitle = TableTitle.Substring(0, TableTitle.Length - 3) & "..."
                    End If
                End If

                TableTitle = TableTitle.Trim()
                TableTitle = " " & TableTitle & " "
                Dim startPoint = 0

                Select Case TableTitleTextAlignment
                    Case TextAligntment.Left
                        startPoint = 1
                    Case TextAligntment.Right
                        startPoint = line.Length - 1 - TableTitle.RealLength(True)
                    Case TextAligntment.Center
                        startPoint = (line.Length - TableTitle.RealLength(True)) / 2
                    Case Else
                End Select

                TitlePositionStartAt = startPoint
                Dim newBeginTableFormat = line.Substring(0, startPoint)
                newBeginTableFormat += TableTitle
                TitlePositionLength = TableTitle.Length
                Dim reallength = newBeginTableFormat.RealLength(True)
                newBeginTableFormat += line.Substring(reallength, line.Length - reallength)
                line = newBeginTableFormat
                line = line.Replace(vbNullChar, " ")
            End If

            Return line
        End Function
#Region "Table lines"

        Friend Function CreateTableTopLine(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char)) As String
            Dim borderTop = definition(CharMapPositions.BorderTop)
            Dim topLeft = definition(CharMapPositions.TopLeft)
            Dim topCenter = definition(CharMapPositions.TopCenter)
            Dim topRight = definition(CharMapPositions.TopRight)

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i) New String(borderTop, columnLengths(i) + (PaddingLeft & PaddingRight).Length)).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, topCenter.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, topLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, topRight.ToString()))
                line = EmbedTitle(line)

                If line.Trim(CChar(Microsoft.VisualBasic.Strings.ChrW(0))).Length = 0 Then
                    line = String.Empty
                End If

                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateTableContentLineFormat(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char)) As String
            Dim borderLeft = definition(CharMapPositions.BorderLeft)
            Dim divider = definition(CharMapPositions.DividerY)
            Dim borderRight = definition(CharMapPositions.BorderRight)

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i)
                                                                                   Dim alignmentChar = String.Empty

                                                                                   If TextAligmentData Is Nothing OrElse Not TextAligmentData.ContainsKey(i) OrElse TextAligmentData(i) = TextAligntment.Left Then
                                                                                       alignmentChar = "-"
                                                                                   End If

                                                                                   Return PaddingLeft & "{" & i & "," & alignmentChar & columnLengths(i) & "}" & PaddingRight
                                                                               End Function).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, divider.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, borderLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, borderRight.ToString()))
                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateRawLineFormat(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char), ParamArray args As Object()) As String
            Dim borderLeft = definition(CharMapPositions.BorderLeft)
            Dim divider = definition(CharMapPositions.DividerY)
            Dim borderRight = definition(CharMapPositions.BorderRight)

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i)
                                                                                   Dim alignmentChar = String.Empty

                                                                                   If TextAligmentData Is Nothing OrElse Not TextAligmentData.ContainsKey(i) OrElse TextAligmentData(i) = TextAligntment.Left Then
                                                                                       alignmentChar = "-"
                                                                                   End If

                                                                                   If args.Length > i Then
                                                                                       Dim value As String = If(args(i)?.ToString(), "")
                                                                                       Return PaddingLeft & "{" & i & "," & alignmentChar & columnLengths(i) - (value.RealLength(True) - value.Length) & "}" & PaddingRight
                                                                                   Else
                                                                                       Return PaddingLeft & "{" & i & "," & alignmentChar & columnLengths(i) & "}" & PaddingRight
                                                                                   End If
                                                                               End Function).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, divider.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, borderLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, borderRight.ToString()))
                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateTableMiddleLine(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char)) As String
            Dim dividerX = definition(CharMapPositions.DividerX)
            Dim middleLeft = definition(CharMapPositions.MiddleLeft)
            Dim middleCenter = definition(CharMapPositions.MiddleCenter)
            Dim middleRight = definition(CharMapPositions.MiddleRight)

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i) New String(dividerX, columnLengths(i) + (PaddingLeft & PaddingRight).Length)).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, middleCenter.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, middleLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, middleRight.ToString()))

                If line.Trim(CChar(Microsoft.VisualBasic.Strings.ChrW(0))).Length = 0 Then
                    line = String.Empty
                End If

                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateTableBottomLine(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char)) As String
            Dim borderBottom = definition(CharMapPositions.BorderBottom)
            Dim bottomLeft = definition(CharMapPositions.BottomLeft)
            Dim bottomCenter = definition(CharMapPositions.BottomCenter)
            Dim bottomRight = definition(CharMapPositions.BottomRight)

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i) New String(borderBottom, columnLengths(i) + (PaddingLeft & PaddingRight).Length)).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, bottomCenter.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, bottomLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, bottomRight.ToString()))

                If line.Trim(CChar(Microsoft.VisualBasic.Strings.ChrW(0))).Length = 0 Then
                    line = String.Empty
                End If

                Return line
            Else
                Return String.Empty
            End If
        End Function

#End Region


#Region "Header lines"

        Friend Function CreateHeaderTopLine(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char), headerDefinition As Dictionary(Of HeaderCharMapPositions, Char)) As String
            Dim borderTop = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BorderTop), headerDefinition(HeaderCharMapPositions.BorderTop), definition(CharMapPositions.BorderTop))
            Dim topLeft = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.TopLeft), headerDefinition(HeaderCharMapPositions.TopLeft), definition(CharMapPositions.TopLeft))
            Dim topCenter = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.TopCenter), headerDefinition(HeaderCharMapPositions.TopCenter), definition(CharMapPositions.TopCenter))
            Dim topRight = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.TopRight), headerDefinition(HeaderCharMapPositions.TopRight), definition(CharMapPositions.TopRight))

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i) New String(borderTop, columnLengths(i) + (PaddingLeft & PaddingRight).Length)).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, topCenter.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, topLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, topRight.ToString()))
                line = EmbedTitle(line)

                If line.Trim(CChar(Microsoft.VisualBasic.Strings.ChrW(0))).Length = 0 Then
                    line = String.Empty
                End If

                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateHeaderContentLineFormat(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char), headerDefinition As Dictionary(Of HeaderCharMapPositions, Char)) As String
            Dim borderLeft = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BorderLeft), headerDefinition(HeaderCharMapPositions.BorderLeft), definition(CharMapPositions.BorderLeft))
            Dim divider = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.Divider), headerDefinition(HeaderCharMapPositions.Divider), definition(CharMapPositions.DividerY))
            Dim borderRight = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BorderRight), headerDefinition(HeaderCharMapPositions.BorderRight), definition(CharMapPositions.BorderRight))

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i)
                                                                                   Dim alignmentChar = String.Empty

                                                                                   If HeaderTextAligmentData.ContainsKey(i) Then
                                                                                       If HeaderTextAligmentData(i) = TextAligntment.Left Then
                                                                                           alignmentChar = "-"
                                                                                       End If
                                                                                   Else

                                                                                       If TextAligmentData Is Nothing OrElse Not TextAligmentData.ContainsKey(i) OrElse TextAligmentData(i) = TextAligntment.Left Then
                                                                                           alignmentChar = "-"
                                                                                       End If
                                                                                   End If

                                                                                   Return PaddingLeft & "{" & i & "," & alignmentChar & columnLengths(i) & "}" & PaddingRight
                                                                               End Function).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, divider.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, borderLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, borderRight.ToString()))
                Return line
            Else
                Return String.Empty
            End If
        End Function

        Friend Function CreateHeaderBottomLine(columnLengths As List(Of Integer), definition As Dictionary(Of CharMapPositions, Char), headerDefinition As Dictionary(Of HeaderCharMapPositions, Char)) As String
            Dim borderBottom = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BorderBottom), headerDefinition(HeaderCharMapPositions.BorderBottom), definition(CharMapPositions.DividerX))
            Dim bottomLeft = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BottomLeft), headerDefinition(HeaderCharMapPositions.BottomLeft), definition(CharMapPositions.MiddleLeft))
            Dim bottomCenter = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BottomCenter), headerDefinition(HeaderCharMapPositions.BottomCenter), definition(CharMapPositions.MiddleRight))
            Dim bottomRight = If(headerDefinition IsNot Nothing AndAlso headerDefinition.ContainsKey(HeaderCharMapPositions.BottomRight), headerDefinition(HeaderCharMapPositions.BottomRight), definition(CharMapPositions.MiddleCenter))

            If columnLengths.Count > 0 Then
                Dim result = Enumerable.Range(0, columnLengths.Count).[Select](Function(i) New String(borderBottom, columnLengths(i) + (PaddingLeft & PaddingRight).Length)).Aggregate(Function(s, a) s & (If(CanRemoveDividerY(), String.Empty, bottomCenter.ToString())) & a)
                Dim line = (If(CanRemoveBorderLeft(), String.Empty, bottomLeft.ToString())) & result & (If(CanRemoveBorderRight(), String.Empty, bottomRight.ToString()))

                If line.Trim(CChar(Microsoft.VisualBasic.Strings.ChrW(0))).Length = 0 Then
                    line = String.Empty
                End If

                Return line
            Else
                Return String.Empty
            End If
        End Function

#End Region

        Friend Function CanRemoveBorderLeft() As Boolean
            If HeaderCharMapPositionStore Is Nothing Then
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(New List(Of Char) From {
                    CharMapPositionStore(CType(CharMapPositions.TopLeft, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.MiddleLeft, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.BottomLeft, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.BorderLeft, CharMapPositions))
                }, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            Else
                Dim data = New List(Of Char) From {
                }
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.TopLeft), HeaderCharMapPositionStore(HeaderCharMapPositions.TopLeft), CharMapPositionStore(CharMapPositions.TopLeft)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.BorderLeft), HeaderCharMapPositionStore(HeaderCharMapPositions.BorderLeft), CharMapPositionStore(CharMapPositions.BorderLeft)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.BottomLeft), HeaderCharMapPositionStore(HeaderCharMapPositions.BottomLeft), CharMapPositionStore(CharMapPositions.MiddleLeft)))
                data.Add(CharMapPositionStore(CharMapPositions.MiddleLeft))
                data.Add(CharMapPositionStore(CharMapPositions.BorderLeft))
                data.Add(CharMapPositionStore(CharMapPositions.BottomLeft))
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(data, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            End If
        End Function

        Friend Function CanRemoveBorderRight() As Boolean
            If HeaderCharMapPositionStore Is Nothing Then
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(New List(Of Char) From {
                    CharMapPositionStore(CType(CharMapPositions.TopRight, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.MiddleRight, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.BottomRight, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.BorderRight, CharMapPositions))
                }, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            Else
                Dim data = New List(Of Char) From {
                }
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.TopRight), HeaderCharMapPositionStore(HeaderCharMapPositions.TopRight), CharMapPositionStore(CharMapPositions.TopRight)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.BorderRight), HeaderCharMapPositionStore(HeaderCharMapPositions.BorderRight), CharMapPositionStore(CharMapPositions.BorderRight)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.BottomRight), HeaderCharMapPositionStore(HeaderCharMapPositions.BottomRight), CharMapPositionStore(CharMapPositions.MiddleRight)))
                data.Add(CharMapPositionStore(CharMapPositions.MiddleRight))
                data.Add(CharMapPositionStore(CharMapPositions.BorderRight))
                data.Add(CharMapPositionStore(CharMapPositions.BottomRight))
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(data, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            End If
        End Function

        Friend Function CanRemoveDividerY() As Boolean
            If HeaderCharMapPositionStore Is Nothing Then
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(New List(Of Char) From {
                    CharMapPositionStore(CType(CharMapPositions.TopCenter, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.MiddleCenter, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.BottomCenter, CharMapPositions)),
                    CharMapPositionStore(CType(CharMapPositions.DividerY, CharMapPositions))
                }, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            Else
                Dim data = New List(Of Char) From {
                }
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.TopCenter), HeaderCharMapPositionStore(HeaderCharMapPositions.TopCenter), CharMapPositionStore(CharMapPositions.TopCenter)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.Divider), HeaderCharMapPositionStore(HeaderCharMapPositions.Divider), CharMapPositionStore(CharMapPositions.DividerY)))
                data.Add(If(HeaderCharMapPositionStore.ContainsKey(HeaderCharMapPositions.BottomCenter), HeaderCharMapPositionStore(HeaderCharMapPositions.BottomCenter), CharMapPositionStore(CharMapPositions.MiddleCenter)))
                data.Add(CharMapPositionStore(CharMapPositions.MiddleCenter))
                data.Add(CharMapPositionStore(CharMapPositions.DividerY))
                data.Add(CharMapPositionStore(CharMapPositions.BottomCenter))
                Return Enumerable.Aggregate(Of String)(Enumerable.Select(Of Char, Global.System.[String])(data, CType(Function(x) CStr(x.ToString()), Func(Of Char, String))), CType(Function(s, a) CStr(s & a), Func(Of String, String, String))).Replace(CStr(vbNullChar), CStr(String.Empty)).Trim().Length = 0
            End If
        End Function
    End Class
End Namespace
