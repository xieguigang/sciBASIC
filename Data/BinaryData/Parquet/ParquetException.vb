Option Strict On
Option Explicit On
Option Infer On

Imports System.IO

''' <summary>
''' Parquet 文件格式错误（文件截断、magic 不匹配、元数据非法、编码不支持等）。
''' </summary>
Public Class ParquetException : Inherits IOException

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub

End Class

''' <summary>
''' Parquet 文件格式相关的常量。
''' </summary>
Public Module ParquetConstants

    ''' <summary>
    ''' 文件头尾的魔数标识 <c>PAR1</c>。
    ''' </summary>
    Public ReadOnly Magic As Byte() = {&H50, &H41, &H52, &H31}

    ''' <summary>
    ''' 嵌套结构的最大深度，防止恶意文件导致的递归爆栈。
    ''' </summary>
    Public Const MaxNestedDepth As Integer = 100

    ''' <summary>
    ''' 写入 DataFrame 行名所使用的保留列名。
    ''' </summary>
    Public Const IndexColumnName As String = "__index__"

End Module
