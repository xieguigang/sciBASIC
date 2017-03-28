#Region "Microsoft.VisualBasic::1cce6dcb2d405598edb42927dcdbccb8, ..\sciBASIC#\Data\DataFrame.Extensions\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text

Public Module SchemasAPI

    ''' <summary>
    ''' 这个函数是将某一个复杂的对象类型分别拆分，分别保存在一个文件夹之中的不同的csv数据文件
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="DIR"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveData(Of T As Class)(source As IEnumerable(Of T), DIR As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim schema As Schema = Schema.GetSchema(Of T)
        Dim type As Type = GetType(T)
        Dim IO As [Class] = [Class].GetSchema(type)
        Dim i As New Uid

        Using writer As New Writer(IO, DIR, encoding)
            For Each x As T In source
                Call writer.WriteRow(x, +i)
            Next
        End Using

        Return JSON.GetJson(schema, True).SaveTo(DIR & "/" & Schema.DefaultName)
    End Function

    ''' <summary>
    ''' 这个函数不像<see cref="SaveData"/>函数是完整的将对象Dump在一个文件夹之中的，
    ''' 这个函数的用途和R语言之中的``summary``函数的作用类似，就是将复杂的对象类型展开为一个二维表
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="primary$">
    ''' 如果没有指定主键域的话，会默认用元素在集合之中的index编号来作为<see cref="EntityObject.ID"/>的属性值
    ''' </param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Summary(Of T As Class)(source As IEnumerable(Of T), Optional primary$ = Nothing) As EntityObject()
        Return Summary(source, GetType(T), primary)
    End Function

    Public Function Summary(source As IEnumerable, type As Type, Optional primary$ = Nothing) As EntityObject()
        Dim schema As SchemaProvider = SchemaProvider.CreateObject(type).CopyReadDataFromObject
        Dim getID As Func(Of SeqValue(Of Object), String)
        Dim out As New List(Of EntityObject)

        If primary.StringEmpty Then
            Dim field = schema.GetField(primary)

            schema.Remove(primary)
            getID = Function(o)
                        Dim value As Object = field _
                            .BindProperty _
                            .GetValue(+o)
                        Return Scripting.CStrSafe(value)
                    End Function
        Else
            getID = Function(o) CStr(o.i)
        End If

        For Each i As SeqValue(Of Object) In source.SeqIterator
            Dim ID$ = getID(i)
            Dim table As New Dictionary(Of String, String)

            For Each field In schema
                If DataFramework.IsPrimitive(field.BindProperty.PropertyType) Then
                    table.Add(field.Name, field.GetValue(+i))
                Else
                    ' 递归进行下一级的展开

                End If
            Next
        Next

        Return out
    End Function
End Module
