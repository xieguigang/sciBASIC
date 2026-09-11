#Region "Microsoft.VisualBasic::24e88b0065b9708285ca0d1952deea90, Data\BinaryData\SQLite3\Objects\Enums\SqliteDataType.vb"

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

    '   Total Lines: 100
    '    Code Lines: 57 (57.00%)
    ' Comment Lines: 37 (37.00%)
    '    - Xml Docs: 78.38%
    ' 
    '   Blank Lines: 6 (6.00%)
    '     File Size: 5.44 KB


    '     Enum SqliteDataType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module DataTypeParser
    ' 
    '         Function: TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Core.Objects.Enums

    Public Enum SqliteDataType As Byte
        ''' <summary>
        ''' The value is a NULL value.
        ''' </summary>
        Null = 0
        ''' <summary>
        ''' The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
        ''' </summary>
        [Integer] = 1
        Float = 7
        Boolean0 = 8
        Boolean1 = 9
        Blob = 12
        Text = 13
    End Enum

    Module DataTypeParser

        ''' <summary>
        ''' ### Affinity Name
        ''' 
        ''' The following table shows how many common datatype names from more 
        ''' traditional SQL implementations are converted into affinities by the 
        ''' five rules of the previous section. This table shows only a small 
        ''' subset of the datatype names that SQLite will accept. Note that numeric 
        ''' arguments in parentheses that following the type name 
        ''' (ex: "VARCHAR(255)") are ignored by SQLite - SQLite does not impose any 
        ''' length restrictions (other than the large global SQLITE_MAX_LENGTH limit)
        ''' on the length of strings, BLOBs or numeric values.
        ''' 
        ''' |Typenames                                                                                               |Affinity|Rule Used To Determine Affinity|
        ''' |--------------------------------------------------------------------------------------------------------|--------|-------------------------------|
        ''' |INT, Integer, TINYINT, SMALLINT, MEDIUMINT, BIGINT, UNSIGNED BIG INT, INT2, INT8                        |INTEGER | 1                             |
        ''' |CHARACTER(20),VARCHAR(255),VARYING CHARACTER(255),NCHAR(55),NATIVE CHARACTER(70),NVARCHAR(100),TEXT,CLOB|TEXT    | 2                             |
        ''' |BLOB(no datatype specified)                                                                             |BLOB    | 3                             |
        ''' |REAL, Double, Double PRECISION,FLOAT                                                                    |REAL    | 4                             |
        ''' |NUMERIC,Decimal(10, 5),Boolean,Date,DATETIME                                                            |NUMERIC | 5                             |
        ''' 
        ''' > Note that a declared type of "FLOATING POINT" would give INTEGER 
        ''' affinity, not REAL affinity, due to the "INT" at the end of "POINT". 
        ''' And the declared type of "STRING" has an affinity of NUMERIC, not TEXT.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function TryParse(type As String) As SqliteDataType
            Dim t As String = If(type, "").Trim.ToLower()

            ' 去除 [] 转义, 例如 [varchar]
            If t.Length > 1 AndAlso t.StartsWith("[") AndAlso t.EndsWith("]") Then
                t = t.Substring(1, t.Length - 2).Trim()
            End If

            ' 去除长度限定, 例如 varchar(255)
            Dim p As Integer = t.IndexOf("("c)
            If p >= 0 Then
                t = t.Substring(0, p).Trim()
            End If

            Select Case t
                Case "integer", "int", "int64", "int32", "int16", "int8", "int2",
                     "tinyint", "smallint", "mediumint", "bigint", "unsigned big int"
                    Return SqliteDataType.Integer
                Case "float", "double", "double precision", "real", "numeric", "decimal"
                    Return SqliteDataType.Float
                Case "text", "blob_text", "varchar", "char", "character", "character varying",
                     "varying character", "nchar", "native character", "nvarchar", "clob"
                    Return SqliteDataType.Text
                Case "blob"
                    Return SqliteDataType.Blob
                Case "null"
                    Return SqliteDataType.Null
                Case "bool", "boolean", "bit"
                    Return SqliteDataType.Boolean1
                Case "datetime", "date", "time", "timestamp"
                    Return SqliteDataType.Integer
                Case Else
                    ' 未在列表之中声明的类型, 按照 SQLite 的亲和性规则进行回退, 不再抛出异常
                    If t.Length = 0 Then
                        ' 未声明类型 = BLOB 亲和性
                        Return SqliteDataType.Blob
                    ElseIf t.Contains("char") OrElse t.Contains("clob") OrElse t.Contains("text") Then
                        Return SqliteDataType.Text
                    ElseIf t.Contains("blob") Then
                        Return SqliteDataType.Blob
                    ElseIf t.Contains("int") Then
                        Return SqliteDataType.Integer
                    ElseIf t.Contains("bool") Then
                        Return SqliteDataType.Boolean1
                    ElseIf t.Contains("real") OrElse t.Contains("floa") OrElse t.Contains("doub") Then
                        Return SqliteDataType.Float
                    Else
                        ' 其余未识别类型按 NUMERIC 亲和性处理
                        Return SqliteDataType.Float
                    End If
            End Select
        End Function
    End Module
End Namespace
