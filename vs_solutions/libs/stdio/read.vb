Imports System.IO
Imports System.Text.Json
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.MachineLearning.DataStorage
Imports Microsoft.VisualBasic.Text
Imports xlsxFile = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File

''' <summary>
''' 标准数据文件读取模块。
''' <para>
''' 所有的表格类数据文件都统一读取为 <see cref="NumericTable"/>（行名 + 特征列 + ``label:`` 标签列），
''' 这样读进来的表可以直接用于 <c>stdlib</c> 的分析与绘图函数。
''' </para>
''' <para>
''' 这些函数同时被 VBS 与 NJS 两个脚本引擎调用，因此参数与返回值只使用脚本桥接可以转换的类型
''' （数组、字符串、数值、<see cref="NumericTable"/> 等），不使用 <c>IEnumerable(Of T)</c> 或泛型。
''' </para>
''' </summary>
Public Module read

    ''' <summary>
    ''' 读取逗号分隔的表格文件。
    ''' </summary>
    ''' <param name="file">文件路径</param>
    ''' <param name="columns">可选的列白名单；为空时读取除行名列以外的全部列</param>
    ''' <param name="rowHeader">第一列是否为行名</param>
    ''' <param name="strict">遇到非数值单元格时是否抛异常；False 时写入 Double.NaN</param>
    Public Function csv(file As String,
                        Optional columns As String() = Nothing,
                        Optional rowHeader As Boolean = True,
                        Optional strict As Boolean = True) As NumericTable

        Return NumericTableIO.ReadCsv(Require(file), rowHeader:=rowHeader, strict:=strict, columns:=columns)
    End Function

    ''' <summary>读取制表符分隔的表格文件。</summary>
    Public Function tsv(file As String,
                        Optional columns As String() = Nothing,
                        Optional rowHeader As Boolean = True,
                        Optional strict As Boolean = True) As NumericTable

        Return NumericTableIO.ReadTsv(Require(file), rowHeader:=rowHeader, strict:=strict, columns:=columns)
    End Function

    ''' <summary>
    ''' 读取任意分隔符的文本表格。
    ''' </summary>
    ''' <param name="delimiter">分隔符，例如 "," / vbTab / ";" / "\s+"</param>
    Public Function table(file As String,
                          delimiter As String,
                          Optional columns As String() = Nothing,
                          Optional rowHeader As Boolean = True,
                          Optional strict As Boolean = True) As NumericTable

        Dim ch As Char = If(String.IsNullOrEmpty(delimiter), ","c, delimiter(0))

        Using s As Stream = System.IO.File.OpenRead(Require(file))
            Return NumericTableIO.ReadDelimited(s, ch, rowHeader:=rowHeader, strict:=strict, columns:=columns)
        End Using
    End Function

    ''' <summary>读取 ARFF 数据文件（Weka 格式），返回原始的 <see cref="DataFrame"/>。</summary>
    Public Function arff(file As String) As DataFrame
        Return DataFrame.read_arff(Require(file))
    End Function

    ''' <summary>读取 excel 工作簿中的指定名称的工作表。</summary>
    Public Function xlsx(file As String, sheet As String) As NumericTable
        Return ReadSheet(Require(file), sheetName:=sheet)
    End Function

    ''' <summary>读取 excel 工作簿中的指定序号的工作表（序号从 0 开始）。</summary>
    Public Function xlsx(file As String, index As Integer) As NumericTable
        Return ReadSheet(Require(file), index:=index)
    End Function

    ''' <summary>列出 excel 工作簿中的所有工作表名称。</summary>
    Public Function sheetNames(file As String) As String()
        Using pkg As xlsxFile = xlsxFile.Open(Require(file))
            Return pkg.SheetNames.ToArray
        End Using
    End Function

    ''' <summary>
    ''' 把整个文本文件读取为一个字符串（自动识别 BOM 与 UTF-8）。
    ''' </summary>
    Public Function text(file As String) As String
        Return System.IO.File.ReadAllText(Require(file), System.Text.Encoding.UTF8)
    End Function

    ''' <summary>把文本文件按行读取为字符串数组。</summary>
    Public Function lines(file As String) As String()
        Return System.IO.File.ReadAllLines(Require(file), System.Text.Encoding.UTF8)
    End Function

    ''' <summary>
    ''' 读取 JSON 文件并解析为脚本对象树：
    ''' 对象 → <c>Dictionary(Of String, Object)</c>，数组 → <c>List(Of Object)</c>，其余为字符串/数值/布尔/Nothing。
    ''' </summary>
    Public Function json(file As String) As Object
        Dim doc As JsonDocument = JsonDocument.Parse(text(file))

        Return JsonToObject(doc.RootElement)
    End Function

    ''' <summary>
    ''' 读取 MNIST 手写数字数据集（idx 格式的 images + labels 两个文件），
    ''' 返回行名为 ``sample_i``、特征列为 ``p_1 .. p_784``、标签列 ``class`` 为数字类别的表格。
    ''' </summary>
    ''' <param name="images">train-images-idx3-ubyte 文件路径</param>
    ''' <param name="labels">train-labels-idx1-ubyte 文件路径</param>
    ''' <param name="limit">最多读取的样本数；小于等于 0 表示读取全部</param>
    Public Function mnist(images As String, labels As String, Optional limit As Integer = 0) As NumericTable
        Dim pixels As New List(Of Double())
        Dim classes As New List(Of Double)
        Dim names As New List(Of String)

        Using source As New MNIST(Require(images), Require(labels))
            For Each digit In source.ExtractVectors()
                If limit > 0 AndAlso pixels.Count >= limit Then
                    Exit For
                End If

                ' NamedCollection(Of Byte) 只暴露 value As Byte()，逐个转换为 double 作为像素灰度
                pixels.Add(digit.value.Select(Function(b) CDbl(b)).ToArray)
                classes.Add(Val(digit.description))
                names.Add($"sample_{pixels.Count + 1}")
            Next
        End Using

        If pixels.Count = 0 Then
            Throw New InvalidDataException($"the mnist dataset at '{images}' contains no sample!")
        End If

        Dim table As NumericTable = NumericTable.FromRows(
            names.ToArray,
            pixels.ToArray,
            fieldNames("p", pixels(0).Length))
        Dim classLabels As Double() = classes.ToArray

        Return table.SetLabel("class", classLabels)
    End Function

    ''' <summary>
    ''' 生成 ``prefix + 序号`` 形式的名称列表，例如 <c>fieldNames("p", 3)</c> 得到 <c>p_1, p_2, p_3</c>。
    ''' 这是 VBS 脚本魔法函数 ``fieldname`` 的标准库等价物。
    ''' </summary>
    Public Function fieldNames(prefix As String, n As Integer, Optional start As Integer = 1, Optional sep As String = "_") As String()
        Dim out(n - 1) As String

        For i As Integer = 0 To n - 1
            out(i) = prefix & sep & (start + i)
        Next

        Return out
    End Function

    Private Function ReadSheet(file As String, Optional sheetName As String = Nothing, Optional index As Integer = 0) As NumericTable
        Using pkg As xlsxFile = xlsxFile.Open(file)
            Dim tbl = If(sheetName Is Nothing, pkg.GetTable(index:=index), pkg.GetTable(sheetName:=sheetName))
            Dim df As DataFrame = tbl.as_dataframe

            Return df.AsNumericTable
        End Using
    End Function

    ''' <summary>路径检查：文件不存在时抛出带路径信息的异常，而不是让下层抛 NullReference。</summary>
    Private Function Require(file As String) As String
        If String.IsNullOrEmpty(file) Then
            Throw New ArgumentNullException(NameOf(file), "a data file path is required")
        End If

        Dim path As String = System.IO.Path.GetFullPath(file)

        If Not System.IO.File.Exists(path) Then
            Throw New FileNotFoundException($"data file not found: {path}", path)
        End If

        Return path
    End Function

    Private Function JsonToObject(el As JsonElement) As Object
        Select Case el.ValueKind
            Case JsonValueKind.Object
                Dim obj As New Dictionary(Of String, Object)
                For Each p As JsonProperty In el.EnumerateObject()
                    obj(p.Name) = JsonToObject(p.Value)
                Next
                Return obj
            Case JsonValueKind.Array
                Dim list As New List(Of Object)
                For Each item As JsonElement In el.EnumerateArray()
                    list.Add(JsonToObject(item))
                Next
                Return list
            Case JsonValueKind.String
                Return el.GetString()
            Case JsonValueKind.Number
                Return el.GetDouble()
            Case JsonValueKind.True
                Return True
            Case JsonValueKind.False
                Return False
            Case Else
                Return Nothing
        End Select
    End Function
End Module
