#Region "Microsoft.VisualBasic::5b06e4396b4bda66d00a83bb75c598bd, Microsoft.VisualBasic.Core\ComponentModel\DataSource\SchemaMaps\SQL.vb"

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

    '     Class SQLTable
    ' 
    '         Function: Clone, Copy, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' A generic abstract model of a SQL table.
    ''' (SQL之中的一个数据表的抽象描述接口)
    ''' </summary>
    Public MustInherit Class SQLTable
        Implements ICloneable

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return GetInsertSQL()
        End Function

        ''' <summary>
        ''' Clones the property values.
        ''' (由于这个<see cref="SQLTable"/>对象的所有属性都是简单的基本类型，所以能够很容易
        ''' 的复制这些属性，从而很容易的复制数据表之中的一个行对象)
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Clone() As Object Implements ICloneable.Clone
            Return MyClass.MemberwiseClone
        End Function

        ''' <summary>
        ''' MemberwiseClone
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Copy() As SQLTable
            Return DirectCast(MyClass.MemberwiseClone, SQLTable)
        End Function
    End Class
End Namespace
