#Region "Microsoft.VisualBasic::14b25fa7b7c72842c27891176396af38, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\SchemaMaps\SQL.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
