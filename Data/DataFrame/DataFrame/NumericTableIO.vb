#Region "Microsoft.VisualBasic::NumericTableIO, Data\DataFrame\DataFrame\NumericTableIO.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Text

''' <summary>
''' <see cref="NumericTable"/> 对象的 csv/tsv 文本文件读写。
''' 
''' 文本布局（第一行为表头）：
''' 
''' ```
''' ,f1,f2,f3,label:cluster,label:score
''' s1,1.5,-2.25,0,1,0.1
''' s2,3.75,4.5,-5.125,1,0.2
''' ```
''' 
''' 即「首列行名 + 全部特征列 + 末尾带 <see cref="DefaultLabelPrefix"/> 前缀的标签列」；
''' 读入的时候会依据该前缀自动把标签列还原到标签矩阵之中（前缀会被去掉），
''' 并可以再通过 <c>labels</c> 参数叠加指定其它的标签列。
''' 
''' 文本格式是一种简单格式：单元格的内容不应该包含分隔符、双引号或者换行符，
''' 否则无法保证读写的往返一致性。
''' </summary>
Public Module NumericTableIO

    ''' <summary>
    ''' 标签列在文本文件之中的缺省列名前缀
    ''' </summary>
    Public Const DefaultLabelPrefix As String = "label:"

#Region "write"

    ''' <summary>
    ''' 将二维表保存为分隔符文本文件（csv / tsv）。
    ''' 
    ''' 这个方法不会抛出IO异常：文件写入失败的时候会返回 False
    ''' （与 <c>FileWriter.WriteCsv(String)</c> 的约定一致）。
    ''' </summary>
    ''' <param name="table">需要保存的二维表对象</param>
    ''' <param name="file">目标文件的路径</param>
    ''' <param name="delimiter">列分隔符，``","c`` 为 csv、``vbTab`` 为 tsv</param>
    ''' <param name="labelPrefix">标签列的列名前缀</param>
    ''' <param name="naValue">缺失值（<see cref="Double.NaN"/>）在文件之中的文本表示</param>
    ''' <returns>是否保存成功</returns>
    <Extension>
    Public Function WriteDelimited(table As NumericTable,
                                   file As String,
                                   delimiter As Char,
                                   Optional labelPrefix As String = DefaultLabelPrefix,
                                   Optional naValue As String = "") As Boolean

        Try
            Using s As Stream = New FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None)
                Call table.WriteDelimited(s, delimiter, labelPrefix, naValue)
            End Using

            Return True
        Catch ex As Exception
            Call System.Diagnostics.Debug.WriteLine($"failed to save the NumericTable object as a delimited text file: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 将二维表写入到指定的流之中（**不会关闭调用方传入的流**）。
    ''' </summary>
    ''' <param name="table">需要保存的二维表对象</param>
    ''' <param name="stream">目标数据流</param>
    ''' <param name="delimiter">列分隔符</param>
    ''' <param name="labelPrefix">标签列的列名前缀</param>
    ''' <param name="naValue">缺失值的文本表示</param>
    <Extension>
    Public Sub WriteDelimited(table As NumericTable,
                              stream As Stream,
                              delimiter As Char,
                              Optional labelPrefix As String = DefaultLabelPrefix,
                              Optional naValue As String = "")

        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If
        If stream Is Nothing Then
            Throw New ArgumentNullException(NameOf(stream))
        End If

        Dim prefix As String = If(labelPrefix, DefaultLabelPrefix)
        Dim sep As String = delimiter.ToString
        Dim featureNames As String() = FeatureNameList(table)
        Dim labelNames As String() = If(table.labels Is Nothing, New String() {}, If(table.labelNames, New String() {}))
        Dim text As New StringBuilder
        Dim header As New List(Of String)

        Call header.Add("")
        Call header.AddRange(featureNames)

        For Each label As String In labelNames
            Call header.Add(prefix & label)
        Next

        Call text.AppendLine(RowObject.ToString(header, sep))

        For i As Integer = 0 To table.nsamples - 1
            Dim cells As New List(Of String)

            Call cells.Add(table.GetRowName(i))

            For j As Integer = 0 To featureNames.Length - 1
                Call cells.Add(FormatValue(table.features(i)(j), naValue))
            Next
            For j As Integer = 0 To labelNames.Length - 1
                Call cells.Add(FormatValue(table.labels(i)(j), naValue))
            Next

            Call text.AppendLine(RowObject.ToString(cells, sep))
        Next

        Dim data As Byte() = New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False).GetBytes(text.ToString())

        Call stream.Write(data, 0, data.Length)
    End Sub

    ''' <summary>
    ''' 将二维表保存为逗号分隔的 csv 文件
    ''' </summary>
    <Extension>
    Public Function WriteCsv(table As NumericTable,
                             file As String,
                             Optional labelPrefix As String = DefaultLabelPrefix,
                             Optional naValue As String = "") As Boolean

        Return table.WriteDelimited(file, ","c, labelPrefix, naValue)
    End Function

    ''' <summary>
    ''' 将二维表写入到指定的流之中，逗号分隔的 csv 格式
    ''' </summary>
    <Extension>
    Public Sub WriteCsv(table As NumericTable,
                        stream As Stream,
                        Optional labelPrefix As String = DefaultLabelPrefix,
                        Optional naValue As String = "")

        Call table.WriteDelimited(stream, ","c, labelPrefix, naValue)
    End Sub

    ''' <summary>
    ''' 将二维表保存为制表符分隔的 tsv 文件
    ''' </summary>
    <Extension>
    Public Function WriteTsv(table As NumericTable,
                             file As String,
                             Optional labelPrefix As String = DefaultLabelPrefix,
                             Optional naValue As String = "") As Boolean

        Return table.WriteDelimited(file, vbTab, labelPrefix, naValue)
    End Function

    ''' <summary>
    ''' 将二维表写入到指定的流之中，制表符分隔的 tsv 格式
    ''' </summary>
    <Extension>
    Public Sub WriteTsv(table As NumericTable,
                        stream As Stream,
                        Optional labelPrefix As String = DefaultLabelPrefix,
                        Optional naValue As String = "")

        Call table.WriteDelimited(stream, vbTab, labelPrefix, naValue)
    End Sub

    Private Function FormatValue(value As Double, naValue As String) As String
        If Double.IsNaN(value) Then
            Return naValue
        Else
            ' 使用往返格式保证数值可以无损地还原
            Return value.ToString("R", CultureInfo.InvariantCulture)
        End If
    End Function

#End Region

#Region "read"

    ''' <summary>
    ''' 从逗号分隔的 csv 文件之中加载一个二维表对象
    ''' </summary>
    Public Function ReadCsv(file As String,
                            Optional labels As String() = Nothing,
                            Optional labelPrefix As String = DefaultLabelPrefix,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional strict As Boolean = True,
                            Optional columns As String() = Nothing) As NumericTable

        Using s As Stream = New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)
            Return ReadDelimited(s, ","c, labels, labelPrefix, rowHeader, encoding, strict, columns)
        End Using
    End Function

    ''' <summary>
    ''' 从逗号分隔的 csv 数据流之中加载一个二维表对象（**不会关闭调用方传入的流**）
    ''' </summary>
    Public Function ReadCsv(stream As Stream,
                            Optional labels As String() = Nothing,
                            Optional labelPrefix As String = DefaultLabelPrefix,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional strict As Boolean = True,
                            Optional columns As String() = Nothing) As NumericTable

        Return ReadDelimited(stream, ","c, labels, labelPrefix, rowHeader, encoding, strict, columns)
    End Function

    ''' <summary>
    ''' 从制表符分隔的 tsv 文件之中加载一个二维表对象
    ''' </summary>
    Public Function ReadTsv(file As String,
                            Optional labels As String() = Nothing,
                            Optional labelPrefix As String = DefaultLabelPrefix,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional strict As Boolean = True,
                            Optional columns As String() = Nothing) As NumericTable

        Using s As Stream = New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)
            Return ReadDelimited(s, vbTab, labels, labelPrefix, rowHeader, encoding, strict, columns)
        End Using
    End Function

    ''' <summary>
    ''' 从制表符分隔的 tsv 数据流之中加载一个二维表对象（**不会关闭调用方传入的流**）
    ''' </summary>
    Public Function ReadTsv(stream As Stream,
                            Optional labels As String() = Nothing,
                            Optional labelPrefix As String = DefaultLabelPrefix,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8,
                            Optional strict As Boolean = True,
                            Optional columns As String() = Nothing) As NumericTable

        Return ReadDelimited(stream, vbTab, labels, labelPrefix, rowHeader, encoding, strict, columns)
    End Function

    ''' <summary>
    ''' 从分隔符文本数据流之中加载一个二维表对象。
    ''' 
    ''' 列归属规则：
    ''' 
    ''' 1. 列名以 <paramref name="labelPrefix"/> 开头的列归入**标签矩阵**（列名会被去掉前缀）
    ''' 2. 列名出现在 <paramref name="labels"/> 参数之中的列同样归入标签矩阵
    ''' 3. 其余的列归入**特征矩阵**
    ''' </summary>
    ''' <param name="stream">文本数据流（**不会关闭调用方传入的流**）</param>
    ''' <param name="delimiter">列分隔符</param>
    ''' <param name="labels">
    ''' 需要额外作为标签列的列名集合；可以写成带前缀的原始列名，也可以写成去掉前缀之后的标签名
    ''' </param>
    ''' <param name="labelPrefix">标签列的列名前缀</param>
    ''' <param name="rowHeader">是否第一列是行名</param>
    ''' <param name="encoding">文本编码</param>
    ''' <param name="strict">
    ''' 无法解析为数值的单元格的处理策略：
    ''' 
    ''' + True（缺省）：直接抛出 <see cref="System.IO.InvalidDataException"/>
    ''' + False：宽容模式下将该单元格置为 <see cref="Double.NaN"/>
    ''' </param>
    ''' <param name="columns">
    ''' 需要加载的列名白名单（缺省加载全部列）；不在白名单之中的列会被完全忽略，
    ''' 因此可以借助这个参数跳过 csv 文件之中的ID列、文本类别列等非数值列
    ''' </param>
    ''' <returns></returns>
    Public Function ReadDelimited(stream As Stream,
                                  delimiter As Char,
                                  Optional labels As String() = Nothing,
                                  Optional labelPrefix As String = DefaultLabelPrefix,
                                  Optional rowHeader As Boolean = True,
                                  Optional encoding As Encodings = Encodings.UTF8,
                                  Optional strict As Boolean = True,
                                  Optional columns As String() = Nothing) As NumericTable

        If stream Is Nothing Then
            Throw New ArgumentNullException(NameOf(stream))
        End If

        Dim lines As List(Of String) = ReadLines(stream, encoding)

        If lines.Count = 0 Then
            Throw New InvalidDataException("the given delimited text document contains no data table!")
        End If

        Dim prefix As String = If(labelPrefix, DefaultLabelPrefix)
        Dim header As String() = lines(0).Split(delimiter)
        Dim offset As Integer = If(rowHeader, 1, 0)

        If header.Length <= offset Then
            Throw New InvalidDataException($"the delimited text document has no data column: '{lines(0)}'")
        End If

        Dim headerColumns As String() = header.Skip(offset).ToArray
        Dim rowNames As New List(Of String)
        Dim cells As New List(Of String())

        For i As Integer = 1 To lines.Count - 1
            Dim tokens As String() = lines(i).Split(delimiter)

            If tokens.Length <> header.Length Then
                Throw New InvalidDataException(
                    $"the column number {tokens.Length} at line {i + 1} is not equals to the header column number {header.Length}: '{lines(i)}'"
                )
            End If

            Call rowNames.Add(If(rowHeader, tokens(0), CStr(i)))
            Call cells.Add(tokens.Skip(offset).ToArray)
        Next

        Dim featureCols As New List(Of String)
        Dim featureIndex As New List(Of Integer)
        Dim labelCols As New List(Of String)
        Dim labelIndex As New List(Of Integer)

        For j As Integer = 0 To headerColumns.Length - 1
            If Not IsSelected(headerColumns(j), columns) Then
                Continue For
            End If

            Dim stripped As String = StripPrefix(headerColumns(j), prefix)

            If stripped <> headerColumns(j) OrElse IsDeclaredLabel(headerColumns(j), stripped, labels) Then
                Call labelCols.Add(stripped)
                Call labelIndex.Add(j)
            Else
                Call featureCols.Add(headerColumns(j))
                Call featureIndex.Add(j)
            End If
        Next

        If columns IsNot Nothing AndAlso columns.Length > 0 AndAlso featureCols.Count = 0 AndAlso labelCols.Count = 0 Then
            Throw New InvalidDataException(
                $"none of the required columns {String.Join(", ", columns)} was found in the delimited text document!"
            )
        End If

        Dim hasLabels As Boolean = labelCols.Count > 0

        Return New NumericTable(BuildMatrix(cells, featureIndex, featureCols, strict),
                                rowNames.ToArray,
                                featureCols.ToArray) With {
            .labels = If(hasLabels, BuildMatrix(cells, labelIndex, labelCols, strict), Nothing),
            .labelNames = If(hasLabels, labelCols.ToArray, Nothing)
        }
    End Function

    ''' <summary>
    ''' 读取文本之中的全部非空行
    ''' </summary>
    Private Function ReadLines(stream As Stream, encoding As Encodings) As List(Of String)
        Dim lines As New List(Of String)
        Dim reader As New StreamReader(stream, encoding.CodePage)
        Dim line As String = reader.ReadLine()

        While line IsNot Nothing
            If line.Trim().Length > 0 Then
                Call lines.Add(line)
            End If

            line = reader.ReadLine()
        End While

        Return lines
    End Function

    ''' <summary>
    ''' 依据列下标从单元格文本之中构建行主序的数值矩阵
    ''' </summary>
    Private Function BuildMatrix(cells As List(Of String()),
                                 indexes As List(Of Integer),
                                 names As List(Of String),
                                 strict As Boolean) As Double()()

        Dim matrix As Double()() = New Double(cells.Count - 1)() {}

        For i As Integer = 0 To cells.Count - 1
            Dim row As Double() = New Double(indexes.Count - 1) {}

            For j As Integer = 0 To indexes.Count - 1
                row(j) = ParseText(cells(i)(indexes(j)), names(j), i, strict)
            Next

            matrix(i) = row
        Next

        Return matrix
    End Function

#End Region

#Region "column helpers"

    ''' <summary>
    ''' 依据 ``label:`` 前缀与 <c>labels</c> 参数把列名拆分为特征列与标签列。
    ''' 
    ''' 返回值之中的 <c>labels</c> 是**去掉前缀之后**的标签列名（写入标签矩阵的名称），
    ''' <c>labelColumns</c> 则是标签列在源表格之中的**原始列名**（用于取值）。
    ''' </summary>
    Friend Function SplitColumns(names As String(), labels As String(), prefix As String) As (features As String(), labels As String(), labelColumns As String())
        Dim featureCols As New List(Of String)
        Dim labelCols As New List(Of String)
        Dim labelSource As New List(Of String)

        For Each name As String In If(names, New String() {})
            Dim stripped As String = StripPrefix(name, prefix)

            If stripped <> name OrElse IsDeclaredLabel(name, stripped, labels) Then
                Call labelCols.Add(stripped)
                Call labelSource.Add(name)
            Else
                Call featureCols.Add(name)
            End If
        Next

        Return (featureCols.ToArray, labelCols.ToArray, labelSource.ToArray)
    End Function

    ''' <summary>
    ''' 指定的列是否在加载白名单之中（白名单为空的时候表示加载全部列）
    ''' </summary>
    Private Function IsSelected(name As String, columns As String()) As Boolean
        If columns Is Nothing OrElse columns.Length = 0 Then
            Return True
        End If

        For Each col As String In columns
            If String.Equals(col, name, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function StripPrefix(name As String, prefix As String) As String
        If Not String.IsNullOrEmpty(prefix) AndAlso name.StartsWith(prefix, StringComparison.Ordinal) Then
            Return name.Substring(prefix.Length)
        Else
            Return name
        End If
    End Function

    Private Function IsDeclaredLabel(name As String, stripped As String, labels As String()) As Boolean
        If labels Is Nothing Then
            Return False
        End If

        For Each declared As String In labels
            If String.Equals(declared, name, StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(declared, stripped, StringComparison.OrdinalIgnoreCase) Then

                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' 按照指定的列名顺序把数据帧转换为行主序的数值矩阵
    ''' </summary>
    Friend Function ToMatrix(df As DataFrame, columns As String(), n As Integer, strict As Boolean) As Double()()
        Dim getters As Func(Of Integer, Object)() = columns.Select(Function(c) df(c).Getter()).ToArray
        Dim matrix As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            Dim row As Double() = New Double(columns.Length - 1) {}

            For j As Integer = 0 To columns.Length - 1
                row(j) = ParseValue(getters(j)(i), columns(j), i, strict)
            Next

            matrix(i) = row
        Next

        Return matrix
    End Function

    ''' <summary>
    ''' 把数据帧之中的单元格转换为数值（单元格可能已经是数值类型）
    ''' </summary>
    Private Function ParseValue(value As Object, column As String, row As Integer, strict As Boolean) As Double
        If value Is Nothing Then
            Return Double.NaN
        ElseIf TypeOf value Is Double Then
            Return DirectCast(value, Double)
        ElseIf TypeOf value Is Single Then
            Return CDbl(value)
        ElseIf TypeOf value Is Integer OrElse TypeOf value Is Long OrElse TypeOf value Is Short OrElse TypeOf value Is Byte Then
            Return CDbl(value)
        ElseIf TypeOf value Is Boolean Then
            Return If(DirectCast(value, Boolean), 1.0, 0.0)
        ElseIf TypeOf value Is Date Then
            Return CDbl(DirectCast(value, Date).ToOADate())
        ElseIf TypeOf value Is TimeSpan Then
            Return DirectCast(value, TimeSpan).TotalSeconds
        Else
            Return ParseText(value.ToString(), column, row, strict)
        End If
    End Function

    ''' <summary>
    ''' 把单元格的文本转换为数值。
    ''' 
    ''' 空串以及 ``NA``/``N/A``/``NaN``/``null``/``none``/``nil``/``missing``/``?``
    ''' 等缺失值标记会被转换为 <see cref="Double.NaN"/>。
    ''' </summary>
    Friend Function ParseText(text As String, column As String, row As Integer, strict As Boolean) As Double
        If text Is Nothing Then
            Return Double.NaN
        End If

        Dim value As String = text.Trim()

        If IsNaNToken(value) Then
            Return Double.NaN
        End If

        Dim d As Double

        If Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then
            Return d
        ElseIf strict Then
            Throw New InvalidDataException(
                $"the cell value '{value}' at row {row + 1} of the column '{column}' cannot be parsed as a numeric value!"
            )
        Else
            Return Double.NaN
        End If
    End Function

    ''' <summary>
    ''' 是否为缺失值的文本标记
    ''' </summary>
    Friend Function IsNaNToken(text As String) As Boolean
        Select Case text.ToLowerInvariant
            Case "", "na", "n/a", "n.a.", "nan", "null", "none", "nil", "missing", "?"
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' 获取特征矩阵的列名（缺失的时候自动生成 ``1..n`` 序号）
    ''' </summary>
    Friend Function FeatureNameList(table As NumericTable) As String()
        Dim m As Integer = table.nfeatures

        If table.featureNames IsNot Nothing AndAlso table.featureNames.Length = m Then
            Return table.featureNames
        End If

        Return Enumerable.Range(1, m).Select(Function(i) CStr(i)).ToArray
    End Function

#End Region
End Module
