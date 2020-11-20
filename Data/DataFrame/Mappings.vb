#Region "Microsoft.VisualBasic::4f3beff9b1935ec5c39357aaa0c1b439, Data\DataFrame\Mappings.vb"

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

    ' Class MappingsHelper
    ' 
    '     Function: [Typeof], CheckFieldConsistent, ColumnName, NamedValueMapsWrite, PropertyNames
    '               TagFieldName
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Field = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.StorageProvider

''' <summary>
''' 在写csv的时候生成列域名的映射的一些快捷函数
''' </summary>
Public Class MappingsHelper

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="types"></param>
    ''' <returns>这个匹配函数是安全的函数, 如果一个结果都没有被匹配上,则这个函数会返回<see cref="System.Void"/>类型</returns>
    Public Shared Function [Typeof](file$, ParamArray types As Type()) As Type
        Dim headers As New RowObject(Tokenizer.CharsParser(file.ReadFirstLine))
        Dim match As Type = StreamIO.TypeOf(headers, types)

        If match Is Nothing Then
            Return GetType(Void)
        Else
            Return match
        End If
    End Function

    ''' <summary>
    ''' 这个函数只适用于只需要解析一个或者少数属性的列名称，假若需要解析的列数量很多，则出于性能方面的考虑不推荐使用这个函数来进行
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="propertyName$">
    ''' 推荐使用``NameOf``操作符来获取属性的名称
    ''' </param>
    ''' <returns>这个函数返回空值表名没有这个属性</returns>
    Public Shared Function ColumnName(schema As Type, propertyName$) As String
        Dim schemaTable As SchemaProvider = SchemaProvider.CreateObjectInternal(schema)

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
        Dim schemaTable = SchemaProvider.CreateObjectInternal(GetType(T))
        Dim table As Dictionary(Of String, String) = schemaTable _
            .ToDictionary(Function(prop)
                              Return prop.BindProperty.Name
                          End Function,
                          Function(field)
                              Return field.Name
                          End Function)
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
        ' 因为这里是判断读文件的时候是否能够把csv文件之中的
        ' 所有的列数据都读取完全了， 所以在这里是获取所有的
        ' 可写属性
        Dim schema As SchemaProvider = SchemaProvider _
            .CreateObject(Of T)(strict:=False) _
            .CopyWriteDataToObject
        Dim result$ = schema.CheckFieldConsistent(headers)

        Return result
    End Function

    ''' <summary>
    ''' <see cref="NamedValue(Of T)"/>
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <param name="value$"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function NamedValueMapsWrite(name$, value$, Optional description$ = NameOf(NamedValue(Of Object).Description)) As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {
            {NameOf(NamedValue(Of Object).Name), name},
            {NameOf(NamedValue(Of Object).Value), value},
            {NameOf(NamedValue(Of Object).Description), description}
        }
    End Function

    Public Shared Iterator Function TagFieldName(data As IEnumerable(Of EntityObject), tagName As String, fieldName$) As IEnumerable(Of EntityObject)
        For Each obj As EntityObject In data
            Dim val As String = obj.Properties(fieldName)

            obj.Properties.Remove(fieldName)
            obj($"{tagName}.{fieldName}") = val

            Yield obj
        Next
    End Function
End Class
