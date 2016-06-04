Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' SQL之中的一个数据表的抽象描述接口
    ''' </summary>
    Public MustInherit Class SQLTable

        ''' <summary>
        ''' INSERT INTO table_name (field1, field2,...) VALUES (value1, value2,....)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>http://www.w3school.com.cn/sql/sql_insert.asp</remarks>
        Public MustOverride Function GetInsertSQL() As String
        ''' <summary>
        ''' UPDATE table_name SET field = &lt;new value> WHERE field = &lt;value>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>http://www.w3school.com.cn/sql/sql_update.asp</remarks>
        Public MustOverride Function GetUpdateSQL() As String
        ''' <summary>
        ''' DELETE FROM table_name WHERE field = value;
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetDeleteSQL() As String

        ''' <summary>
        ''' Display the INSERT INTO sql from function <see cref="GetInsertSQL"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return GetInsertSQL()
        End Function
    End Class
End Namespace
