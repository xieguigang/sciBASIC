#Region "Microsoft.VisualBasic::c3a7c4395b77a39a38cfa13e2767bcd5, Data\BinaryData\SQLite3\Objects\Enums\SqliteDataType.vb"

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

    '   Total Lines: 72
    '    Code Lines: 37 (51.39%)
    ' Comment Lines: 32 (44.44%)
    '    - Xml Docs: 90.62%
    ' 
    '   Blank Lines: 3 (4.17%)
    '     File Size: 3.95 KB


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

Namespace ManagedSqlite.Core.Objects.Enums

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
            Select Case Strings.LCase(type).Trim
                Case "integer", "int", "int64", "[int]", "[bigint]", "bigint"
                    Return SqliteDataType.Integer
                Case "float", "double", "[float]", "real"
                    Return SqliteDataType.Float
                Case "text", "blob_text", "[text]", "varchar"
                    Return SqliteDataType.Text
                Case "blob", "[blob]"
                    Return SqliteDataType.Blob
                Case "null"
                    Return SqliteDataType.Null
                Case "[bool]", "bool", "bit", "[bit]"
                    Return SqliteDataType.Boolean1
                Case "datetime", "[datetime]"
                    Return SqliteDataType.Integer
                Case Else
                    If type.IsPattern("varchar\(\d+\)") OrElse type.IsPattern("\[varchar\]\(\d+\)") Then
                        Return SqliteDataType.Text
                    Else
                        Throw New NotImplementedException(type)
                    End If
            End Select
        End Function
    End Module
End Namespace
