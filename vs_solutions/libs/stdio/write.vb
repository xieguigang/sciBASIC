Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

''' <summary>
''' 标准数据文件写入模块。
''' <para>
''' 每个函数都会自动创建目标文件夹，并把绝对路径作为返回值返回，方便脚本直接打印或再次引用。
''' 参数与返回值只使用脚本桥接可以转换的类型，因此同一套 API 在 VBS 与 NJS 中都可用。
''' </para>
''' </summary>
Public Module write

    ''' <summary>把表格写成逗号分隔的 csv 文件。</summary>
    Public Function csv(table As NumericTable, file As String) As String
        Dim path As String = Target(file)

        If Not table.WriteCsv(path) Then
            Throw New System.IO.IOException($"failed to write the csv table: {path}")
        End If

        Return path
    End Function

    ''' <summary>把表格写成制表符分隔的 tsv 文件。</summary>
    Public Function tsv(table As NumericTable, file As String) As String
        Dim path As String = Target(file)

        If Not table.WriteTsv(path) Then
            Throw New System.IO.IOException($"failed to write the tsv table: {path}")
        End If

        Return path
    End Function

    ''' <summary>把表格写成任意分隔符的文本文件。</summary>
    Public Function table(source As NumericTable, file As String, delimiter As String) As String
        Dim path As String = Target(file)
        Dim ch As Char = If(String.IsNullOrEmpty(delimiter), ","c, delimiter(0))

        If Not source.WriteDelimited(path, ch) Then
            Throw New System.IO.IOException($"failed to write the delimited table: {path}")
        End If

        Return path
    End Function

    ''' <summary>把二维表写成 ARFF 数据文件。</summary>
    Public Function arff(source As NumericTable, file As String) As String
        Dim path As String = Target(file)
        Dim df As DataFrame = source.AsDataFrame

        Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Call DataFrame.write_arff(df, s)
        End Using

        Return path
    End Function

    ''' <summary>
    ''' 把表格写入 excel 工作簿的单个工作表；同名文件会被覆盖。
    ''' </summary>
    Public Function xlsx(source As NumericTable, file As String, sheetName As String) As String
        Dim path As String = Target(file)
        Dim df As DataFrame = source.AsDataFrame
        Dim wb As New Workbook(path, sheetName)

        Call wb.WriteSheetTable(df)
        Call wb.Save()

        Return path
    End Function

    ''' <summary>
    ''' 把多个表格写入 excel 工作簿的多个工作表（表数量与名称数量必须一致）。
    ''' </summary>
    Public Function xlsx(tables As NumericTable(), file As String, sheetNames As String()) As String
        If tables Is Nothing OrElse tables.Length = 0 Then
            Throw New ArgumentException("at least one table is required", NameOf(tables))
        End If
        If sheetNames Is Nothing OrElse sheetNames.Length <> tables.Length Then
            Throw New ArgumentException($"the sheet name count ({If(sheetNames Is Nothing, 0, sheetNames.Length)}) should equals the table count ({tables.Length})", NameOf(sheetNames))
        End If

        Dim path As String = Target(file)
        Dim wb As New Workbook(sheetNames(0))

        Call wb.WriteSheetTable(tables(0).AsDataFrame)

        For i As Integer = 1 To tables.Length - 1
            Call wb.AddWorksheet(sheetNames(i))
            Call wb.WriteSheetTable(tables(i).AsDataFrame)
        Next

        Call wb.SaveAs(path)

        Return path
    End Function

    ''' <summary>
    ''' 把脚本对象序列化为 JSON 文件。
    ''' </summary>
    ''' <param name="value">任意可序列化对象：表格、字典、数组、数值、字符串……</param>
    Public Function json(value As Object, file As String) As String
        Dim path As String = Target(file)
        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }

        System.IO.File.WriteAllText(path, JsonSerializer.Serialize(value, options), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

        Return path
    End Function

    ''' <summary>把文本写入文件（UTF-8 无 BOM）。</summary>
    Public Function text(value As String, file As String) As String
        Dim path As String = Target(file)

        System.IO.File.WriteAllText(path, If(value, ""), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

        Return path
    End Function

    ''' <summary>把字符串数组按行写入文件（UTF-8 无 BOM）。</summary>
    Public Function lines(values As String(), file As String) As String
        Dim path As String = Target(file)

        System.IO.File.WriteAllLines(path, If(values, New String() {}), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

        Return path
    End Function

    ''' <summary>解析目标路径并确保其所在目录存在。</summary>
    Private Function Target(file As String) As String
        If String.IsNullOrEmpty(file) Then
            Throw New ArgumentNullException(NameOf(file), "an output file path is required")
        End If

        Dim path As String = System.IO.Path.GetFullPath(file)
        Dim dir As String = System.IO.Path.GetDirectoryName(path)

        If Not String.IsNullOrEmpty(dir) AndAlso Not System.IO.Directory.Exists(dir) Then
            Call System.IO.Directory.CreateDirectory(dir)
        End If

        Return path
    End Function
End Module
