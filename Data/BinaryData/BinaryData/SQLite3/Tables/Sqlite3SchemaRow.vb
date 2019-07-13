#Region "Microsoft.VisualBasic::354602b18887368d56417b886441382a, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3SchemaRow.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Sqlite3SchemaRow
    ' 
    '         Properties: name, rootPage, Sql, tableName, type
    ' 
    '         Function: ParseSchema, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3SchemaRow

        Public Property type As String
        Public Property name As String
        Public Property tableName As String
        Public Property rootPage As UInteger
        Public Property Sql As String

        Public Overrides Function ToString() As String
            Return Sql
        End Function

        Public Function ParseSchema(Optional removeNameEscape As Boolean = False) As Schema
            Dim columns As String() = Sql _
                .GetStackValue("(", ")") _
                .StringSplit("\s*,\s*")

            ' 有一些字段的名称可能是包含有空格或者小数点之类的,
            ' 则这些字段名称会被[]转义
            ' 在下面的构造函数调用过程之中会根据参数值来自动处理这些转义
            Return New Schema(columns, removeNameEscape)
        End Function

    End Class
End Namespace
