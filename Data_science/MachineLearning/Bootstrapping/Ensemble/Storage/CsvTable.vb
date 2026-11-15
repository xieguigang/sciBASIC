Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 极简 RFC4180 CSV 读写器，专供模型包的数值附表使用。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 为什么自带而不是复用 <c>DataFrame</c>：<c>Microsoft.VisualBasic.Data.Framework</c>
    ''' 是独立项目，它的 <c>File</c> 类型带有大量与本任务无关的元数据与类型推断逻辑，
    ''' 而模型附表只需要「表头 + 字符串矩阵」，自带实现可以做到：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>写入：<see cref="StreamWriter"/> 缓冲 + 逐行 <see cref="StringBuilder"/>，
    '''       单次顺序遍历，额外内存为一行；</item>
    ''' <item>读取：<see cref="StreamReader"/> 单遍状态机解析，支持引号包裹、
    '''       <c>""</c> 转义、CRLF/LF 混排与 UTF-8 BOM；</item>
    ''' <item>不做任何类型推断：数值解析集中在 <see cref="Doubles"/> / <see cref="Integers"/>，
    '''       由调用方给出出错上下文。</item>
    ''' </list>
    ''' <para>
    ''' 全流程使用 <see cref="CultureInfo.InvariantCulture"/>，参见 <see cref="ModelFormat"/>。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class CsvTable

        Private ReadOnly _header As String()
        Private ReadOnly _rows As New List(Of String())()

        Private Sub New(header As String(), rows As List(Of String()))
            Me._header = header
            Me._rows = rows
        End Sub

        ''' <summary>表头（列名）</summary>
        Public ReadOnly Property Header As String()
            Get
                Return _header
            End Get
        End Property

        ''' <summary>数据行；每行的长度可能小于表头（缺失列按空字符串处理）</summary>
        Public ReadOnly Property Rows As IReadOnlyList(Of String())
            Get
                Return _rows
            End Get
        End Property

        ''' <summary>数据行数（不含表头）</summary>
        Public ReadOnly Property RowCount As Integer
            Get
                Return _rows.Count
            End Get
        End Property

        ''' <summary>列数（等于表头长度）</summary>
        Public ReadOnly Property ColumnCount As Integer
            Get
                Return If(_header Is Nothing, 0, _header.Length)
            End Get
        End Property

        ''' <summary>按列名查找列下标，缺失时抛出 <see cref="KeyNotFoundException"/></summary>
        ''' <param name="name">列名（大小写不敏感）</param>
        Public Function IndexOf(name As String) As Integer
            If _header IsNot Nothing Then
                For i As Integer = 0 To _header.Length - 1
                    If String.Equals(_header(i), name, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                Next
            End If

            Throw New KeyNotFoundException(
                $"the CSV column '{name}' is missing; available columns: {If(_header Is Nothing, "", String.Join(", ", _header))}")
        End Function

        ''' <summary>读取一列原始文本（缺失的单元格返回空字符串）</summary>
        ''' <param name="name">列名</param>
        Public Function Texts(name As String) As String()
            Dim index As Integer = IndexOf(name)
            Dim values As String() = New String(_rows.Count - 1) {}

            For i As Integer = 0 To _rows.Count - 1
                Dim row As String() = _rows(i)

                values(i) = If(row Is Nothing OrElse index >= row.Length, "", If(row(index), ""))
            Next

            Return values
        End Function

        ''' <summary>读取一列浮点数</summary>
        ''' <param name="name">列名</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function Doubles(name As String, Optional context As String = Nothing) As Double()
            Dim cells As String() = Texts(name)
            Dim values As Double() = New Double(cells.Length - 1) {}
            Dim label As String = If(context, name)

            For i As Integer = 0 To cells.Length - 1
                values(i) = ModelFormat.ParseNumber(cells(i), $"{label} row[{i}]")
            Next

            Return values
        End Function

        ''' <summary>读取一列整数</summary>
        ''' <param name="name">列名</param>
        ''' <param name="context">出错时用于定位的调用上下文</param>
        Public Function Integers(name As String, Optional context As String = Nothing) As Integer()
            Dim cells As String() = Texts(name)
            Dim values As Integer() = New Integer(cells.Length - 1) {}
            Dim label As String = If(context, name)

            For i As Integer = 0 To cells.Length - 1
                values(i) = ModelFormat.ParseInteger(cells(i), $"{label} row[{i}]")
            Next

            Return values
        End Function

        ''' <summary>
        ''' 读取 CSV 文件。文件缺失或者表头为空时抛出异常。
        ''' </summary>
        ''' <param name="path">CSV 文件的绝对路径</param>
        Public Shared Function Load(path As String) As CsvTable
            If Not File.Exists(path) Then
                Throw New FileNotFoundException($"the model data table '{path}' does not exist!", path)
            End If

            Dim rows As New List(Of String())()
            Dim header As String() = Nothing

            Using reader As New StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks:=True)
                ' StreamReader.ReadLine 对「最后一行没有换行符」同样会返回内容，
                ' 因此这里逐行提交即可；不要在 EOF 处重复提交最后一条记录。
                Do While True
                    Dim line As String = reader.ReadLine()

                    If line Is Nothing Then
                        Exit Do
                    End If

                    Dim record As List(Of String) = ParseRecord(line)

                    If record Is Nothing Then
                        Continue Do
                    End If

                    If header Is Nothing Then
                        header = record.ToArray()
                    Else
                        Call rows.Add(record.ToArray())
                    End If
                Loop
            End Using

            If header Is Nothing OrElse header.Length = 0 Then
                Throw New InvalidDataException($"the model data table '{path}' has no header row!")
            End If

            Return New CsvTable(header, rows)
        End Function

        ''' <summary>把表格写入 CSV 文件（覆盖同名文件）</summary>
        ''' <param name="path">目标文件路径</param>
        ''' <param name="header">表头</param>
        ''' <param name="rows">数据行</param>
        Public Shared Sub Save(path As String, header As String(), rows As IEnumerable(Of String()))
            If header Is Nothing OrElse header.Length = 0 Then
                Throw New ArgumentException("the CSV header can not be empty!", NameOf(header))
            End If
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If

            Dim directory As String = System.IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(directory) AndAlso Not System.IO.Directory.Exists(directory) Then
                Call System.IO.Directory.CreateDirectory(directory)
            End If

            Using writer As New StreamWriter(path, append:=False, encoding:=New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
                Call writer.WriteLine(FormatRecord(header))

                Dim buffer As New StringBuilder()

                For Each row As String() In rows
                    Call buffer.Clear()
                    Call AppendRecord(buffer, row)
                    Call writer.WriteLine(buffer.ToString())
                Next
            End Using
        End Sub

        ''' <summary>
        ''' 解析单行 CSV 记录；空行或纯空白行返回 <c>Nothing</c>。
        ''' </summary>
        ''' <param name="line">去掉换行符的单行文本</param>
        Public Shared Function ParseRecord(line As String) As List(Of String)
            If line Is Nothing Then
                Return Nothing
            End If
            If line.Trim().Length = 0 Then
                Return Nothing
            End If

            Dim cells As New List(Of String)()
            Dim buffer As New StringBuilder()
            Dim quoted As Boolean = False
            Dim i As Integer = 0

            While i < line.Length
                Dim c As Char = line(i)

                If quoted Then
                    If c = """"c Then
                        If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                            Call buffer.Append(""""c)
                            i += 2
                            Continue While
                        End If

                        quoted = False
                        i += 1
                        Continue While
                    End If

                    Call buffer.Append(c)
                    i += 1
                    Continue While
                End If

                Select Case c
                    Case """"c
                        quoted = True
                        i += 1
                    Case ","c
                        Call cells.Add(buffer.ToString())
                        Call buffer.Clear()
                        i += 1
                    Case Else
                        Call buffer.Append(c)
                        i += 1
                End Select
            End While

            If quoted Then
                Throw New InvalidDataException($"unterminated quoted field in CSV line: {line}")
            End If

            Call cells.Add(buffer.ToString())

            Return cells
        End Function

        Private Shared Function FormatRecord(cells As String()) As String
            Dim buffer As New StringBuilder()

            Call AppendRecord(buffer, cells)

            Return buffer.ToString()
        End Function

        Private Shared Sub AppendRecord(buffer As StringBuilder, cells As String())
            If cells Is Nothing Then
                Return
            End If

            For i As Integer = 0 To cells.Length - 1
                If i > 0 Then
                    Call buffer.Append(","c)
                End If

                Call AppendCell(buffer, cells(i))
            Next
        End Sub

        Private Shared Sub AppendCell(buffer As StringBuilder, value As String)
            If String.IsNullOrEmpty(value) Then
                Return
            End If

            Dim needQuote As Boolean =
                value.IndexOfAny(New Char() {","c, """"c, ControlChars.Cr, ControlChars.Lf}) >= 0 OrElse
                value <> value.Trim()

            If Not needQuote Then
                Call buffer.Append(value)
                Return
            End If

            Call buffer.Append(""""c)

            For i As Integer = 0 To value.Length - 1
                If value(i) = """"c Then
                    Call buffer.Append(""""c)
                End If

                Call buffer.Append(value(i))
            Next

            Call buffer.Append(""""c)
        End Sub

    End Class

End Namespace
