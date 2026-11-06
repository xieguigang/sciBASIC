Option Strict On
Option Explicit On
Option Infer On

Imports System.IO
Imports DataFrameModel = Microsoft.VisualBasic.Data.Framework.DataFrame

''' <summary>
''' Parquet 读写的对外门面，提供与 pyarrow 用法类似的两个基础 API：
''' <code>
''' Call pq.write_table(df, "data.parquet")   ' 写入
''' Dim df2 = pq.read_table("data.parquet")   ' 读取
''' </code>
''' <para>
''' VB.NET 标识符大小写不敏感，因此 <c>write_table</c> / <c>WriteTable</c> 是同一个方法。
''' </para>
''' </summary>
Public Module pq

    ''' <summary>
    ''' 将 DataFrame 写入 Parquet 文件（覆盖同名文件）。
    ''' </summary>
    Public Sub write_table(df As DataFrameModel, fileName As String)
        Call write_table(df, fileName, ParquetOptions.Defaults)
    End Sub

    ''' <summary>
    ''' 将 DataFrame 写入 Parquet 文件，并指定写入选项。
    ''' </summary>
    Public Sub write_table(df As DataFrameModel, fileName As String, options As ParquetOptions)
        If String.IsNullOrWhiteSpace(fileName) Then
            Throw New ArgumentException("the output parquet file path can not be empty!", NameOf(fileName))
        End If

        Dim fullPath As String = IO.Path.GetFullPath(fileName)
        Dim directory As String = IO.Path.GetDirectoryName(fullPath)

        If Not String.IsNullOrEmpty(directory) AndAlso Not IO.Directory.Exists(directory) Then
            Call IO.Directory.CreateDirectory(directory)
        End If

        Try
            Using output As New FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None)
                Call write_table(df, output, options)
            End Using
        Catch
            ' 写入失败时删除半成品文件
            Try
                If IO.File.Exists(fullPath) Then
                    Call IO.File.Delete(fullPath)
                End If
            Catch
                ' ignore
            End Try

            Throw
        End Try
    End Sub

    ''' <summary>
    ''' 将 DataFrame 写入一个可写的数据流。
    ''' </summary>
    Public Sub write_table(df As DataFrameModel, output As Stream, Optional options As ParquetOptions = Nothing)
        Dim plan As WritePlan = DataFrameMapping.CreateWritePlan(df, options)

        Call ParquetWriter.Write(plan, output, options)
    End Sub

    ''' <summary>
    ''' 从 Parquet 文件读取 DataFrame。
    ''' </summary>
    Public Function read_table(fileName As String) As DataFrameModel
        Using input As Stream = New FileStream(IO.Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read)
            Return read_table(input)
        End Using
    End Function

    ''' <summary>
    ''' 从数据流读取 DataFrame（不会关闭调用方的数据流）。
    ''' </summary>
    Public Function read_table(input As Stream) As DataFrameModel
        Dim table As ParquetTable = ParquetReader.ReadTable(input)

        Return DataFrameMapping.ToDataFrame(table.Schema, table.Columns, CInt(table.NumRows))
    End Function

    ''' <summary>
    ''' 从 Parquet 文件读取完整的列式数据表（包含定义级别 / 重复级别等原始信息）。
    ''' </summary>
    Public Function read_parquet_table(fileName As String) As ParquetTable
        Using input As Stream = New FileStream(IO.Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read)
            Return ParquetReader.ReadTable(input)
        End Using
    End Function

    ''' <summary>
    ''' 读取 Parquet 文件的 schema。
    ''' </summary>
    Public Function read_schema(fileName As String) As Schema.ParquetSchema
        Dim meta As Format.FileMetaData = read_metadata(fileName)

        Return Schema.ParquetSchema.FromMetadata(meta.Schema)
    End Function

    ''' <summary>
    ''' 读取 Parquet 文件的 footer 元数据。
    ''' </summary>
    Public Function read_metadata(fileName As String) As Format.FileMetaData
        Using input As Stream = New FileStream(IO.Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read, FileShare.Read)
            Return ParquetReader.ReadMetadata(input)
        End Using
    End Function

End Module
