#Region "Microsoft.VisualBasic::93e4af80684106fb91ef4edf332e73f2, Data\DataFrame.Extensions\Serialize\Extensions.vb"

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

    '   Total Lines: 141
    '    Code Lines: 96 (68.09%)
    ' Comment Lines: 26 (18.44%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 19 (13.48%)
    '     File Size: 5.33 KB


    ' Module SchemasAPI
    ' 
    '     Function: PrimaryField, SaveData, (+3 Overloads) Summary
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.Serialize.ObjectSchema
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Schema = Microsoft.VisualBasic.Data.csv.Serialize.ObjectSchema.Schema

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
    Public Function SaveData(Of T As Class)(source As IEnumerable(Of T), DIR$, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim schema As Schema = Schema.GetSchema(Of T)
        Dim type As Type = GetType(T)
        Dim IO As [Class] = [Class].GetSchema(type)
        Dim i As New Uid

        Using writer As New Writer(IO, DIR, encoding)
            For Each x As T In source
                Call writer.WriteRow(x, +i)
            Next
        End Using

        Return schema _
            .GetJson(indent:=True) _
            .SaveTo(DIR & "/" & Schema.DefaultName)
    End Function

    ''' <summary>
    ''' 这个函数不像<see cref="SaveData"/>函数是完整的将对象Dump在一个文件夹之中的，
    ''' 这个函数的用途和R语言之中的``summary``函数的作用类似，就是将复杂的对象类型展开为一个二维表
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="primary$">
    ''' 如果没有指定主键域的话，会默认用元素在集合之中的index编号来作为<see cref="EntityObject.ID"/>的属性值，
    ''' 假若目标类型也继承了<see cref="INamedValue"/>则会自动读取这个属性作为主键
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Summary(Of T As Class)(source As IEnumerable(Of T), Optional primary$ = Nothing) As EntityObject()
        Return Summary(source, GetType(T), primary)
    End Function

    Public Function Summary(source As IEnumerable, type As Type, Optional primary$ = Nothing) As EntityObject()
        Dim schema As [Class] = [Class].GetSchema(type)
        Dim getID As Func(Of SeqValue(Of Object), String)
        Dim out As New List(Of EntityObject)
        Dim primaryField As Field

        If Not primary.StringEmpty Then
            primaryField = schema.GetField(primary)
        Else
            primaryField = schema.PrimaryField
        End If

        If Not primaryField Is Nothing Then
            schema.Remove(primaryField.Name)
            getID = Function(o)
                        Dim value As Object = primaryField _
                            .BindProperty _
                            .GetValue(+o)
                        Return CStrSafe(value)
                    End Function
        Else
            getID = Function(o) CStr(o.i)
        End If

        For Each i As SeqValue(Of Object) In source.SeqIterator
            out += schema.Summary(
                (+i),
                stack:="",
                fill:=New EntityObject With {
                    .ID = getID(i),
                    .Properties = New Dictionary(Of String, String)
                })
        Next

        Return out
    End Function

    ''' <summary>
    ''' 获取Schema之中的主键域
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PrimaryField(schema As [Class]) As Field
        Dim ilist As Type() = schema.Type.GetInterfaces

        For Each [interface] As Type In {
            GetType(INamedValue),
            GetType(IReadOnlyId)
        }
            If ilist.IndexOf([interface]) > -1 Then
                Dim map = schema.Type.GetInterfaceMap([interface])
                Dim props = map.TargetMethods

                ' 不能够查找出实现接口的属性？？？

            End If
        Next

        Return Nothing
    End Function

    <Extension>
    Public Function Summary(schema As [Class], o As Object, ByRef fill As EntityObject, stack$) As EntityObject
        For Each prop As Field In schema
            Dim name$ = prop.Name

            If Not stack.StringEmpty Then
                name = $"{stack}${name}"
            End If

            If Scripting.IsPrimitive(prop.Type) Then
                Dim s$ = CStrSafe(prop.GetValue(o))
                fill.Properties.Add(name, s)
            Else
                ' 对于复杂类型，进行递归展开
                Dim [sub] As Object = prop.GetValue(o)
                prop.InnerClass.Summary([sub], fill, stack:=name)
            End If
        Next

        Return fill
    End Function
End Module
