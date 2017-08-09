#Region "Microsoft.VisualBasic::5a573b1b0f47f05096b70d881e63ef7f, ..\sciBASIC#\Data\DataFrame\Mappings.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Field = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.StorageProvider

''' <summary>
''' 在写csv的时候生成列域名的映射的一些快捷函数
''' </summary>
Public Class MappingsHelper

    ''' <summary>
    ''' 这个函数只适用于只需要解析一个或者少数属性的列名称，假若需要解析的列数量很多，则出于性能方面的考虑不推荐使用这个函数来进行
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="propertyName$">
    ''' 推荐使用``NameOf``操作符来获取属性的名称
    ''' </param>
    ''' <returns>这个函数返回空值表名没有这个属性</returns>
    Public Shared Function ColumnName(schema As Type, propertyName$) As String
        Dim schemaTable As SchemaProvider = SchemaProvider.CreateObject(schema)

        For Each field As Field In schemaTable
            If field.BindProperty.Name = propertyName Then
                Return field.Name
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets property name to column name mapping table.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Shared Function PropertyNames(Of T)() As Dictionary(Of String, String)
        Dim schemaTable = SchemaProvider.CreateObject(GetType(T))
        Dim table = schemaTable _
            .ToDictionary(Function(prop)
                              Return prop.BindProperty.Name
                          End Function,
                          Function(field) field.Name)
        Return table
    End Function

    ''' <summary>
    ''' 使用这个函数来判断当前的Class对象的定义之下，能否将csv文件之中的所有的列的数据都读取完全
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="csv$"></param>
    ''' <returns></returns>
    Public Shared Function CheckFieldConsistent(Of T As Class)(csv$) As String
        Dim headers As New RowObject(Tokenizer.CharsParser(csv.ReadFirstLine))
        Dim schema As SchemaProvider = SchemaProvider _
            .CreateObject(Of T)(strict:=False) _
            .CopyWriteDataToObject  ' 因为这里是判断读文件的时候是否能够把csv文件之中的所有的列数据都读取完全了，所以在这里是获取所有的可写属性
        Dim result$ = schema.CheckFieldConsistent(headers)
        Return result
    End Function

    ''' <summary>
    ''' <see cref="NamedValue(Of T)"/>
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value$"></param>
    ''' <returns></returns>
    Public Shared Function NamedValueMapsWrite(name$, value$, Optional description$ = NameOf(NamedValue(Of Object).Description)) As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {NameOf(NamedValue(Of Object).Name), name},
            {NameOf(NamedValue(Of Object).Value), value},
            {NameOf(NamedValue(Of Object).Description), description}
        }
    End Function
End Class
