#Region "Microsoft.VisualBasic::62392eae59a48ed7bab9e0706a540c46, Data\DataFrame.Extensions\Outlining\Schema.vb"

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

    '   Total Lines: 159
    '    Code Lines: 102
    ' Comment Lines: 30
    '   Blank Lines: 27
    '     File Size: 5.95 KB


    '     Class Builder
    ' 
    '         Properties: Builder, SubTableSchema, Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateBuilder, Flush, GetBuilder, GetNextIndentLevel, IsSubIndentColumn
    ' 
    '         Sub: CacheObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting

Namespace Outlining

    ''' <summary>
    ''' The row object builder, convert csv data row to .NET object in given <see cref="Type"/>
    ''' </summary>
    Public Class Builder

        ''' <summary>
        ''' 一个对象之中只允许出现一个复杂类型的数组属性
        ''' </summary>
        ''' <returns></returns>
        Public Property SubTableSchema As (writer As PropertyInfo, row As Builder)
        Public Property Builder As RowBuilder

        Public ReadOnly Property Type As Type

        Dim cache As New List(Of Object)

        Sub New(type As Type, headers As IEnumerable(Of String), strict As Boolean)
            Dim topLevelHeaders As String() = headers _
                .TakeWhile(Function(s) Not s.StringEmpty) _
                .ToArray

            Me.Type = type
            Me.Builder = type.createBuilderByHeaders(topLevelHeaders, strict)
            Me.SubTableSchema = GetNextIndentLevel(type)

            If SubTableSchema.writer Is Nothing Then
                Call $"We found that '{type.FullName}' is a normal 2D data table, consider using ``LoadCsv`` extension method for read data...".Warning
            End If
        End Sub

        Private Sub New(type As Type)
            Me.Type = type.GetElementType
            ' 暂时先不初始化
            Me.Builder = Nothing
            Me.SubTableSchema = GetNextIndentLevel(type)
        End Sub

        Public Sub CacheObject(row As RowObject, metaBlank$)
            Dim obj As Object = Activator.CreateInstance(Type)

            row = row _
                .SkipWhile(Function(s) s.StringEmpty(whitespaceAsEmpty:=False)) _
                .ToArray
            obj = Builder.FillData(row, obj, metaBlank)

            ' add to cache
            Call cache.Add(obj)
        End Sub

        Public Function Flush(parent As Object) As Object
            If Not SubTableSchema.writer Is Nothing Then
                Dim array As Object = SubTableSchema.row.cache _
                    .ToArray _
                    .DirectCast(SubTableSchema.row.Type)

                Call SubTableSchema.writer.SetValue(parent, array)
                Call SubTableSchema.row.cache.Clear()
            End If

            Return parent
        End Function

        Private Shared Function GetNextIndentLevel(type As Type) As (PropertyInfo, Builder)
            Dim subTable As PropertyInfo = type _
                .GetProperties(PublicProperty) _
                .FirstOrDefault(AddressOf IsSubIndentColumn)

            If Not subTable Is Nothing Then
                Return (subTable, New Builder(subTable.PropertyType))
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 数组类型，并且元素类型不是基础类型
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        Public Shared Function IsSubIndentColumn(p As PropertyInfo) As Boolean
            Dim type As Type = p.PropertyType

            If Not type.IsArray Then
                Return False
            ElseIf DataFramework.IsPrimitive(type.GetElementType) Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' Get element object builder by indent level
        ''' </summary>
        ''' <param name="indentLevel"></param>
        ''' <returns></returns>
        Public Function GetBuilder(indentLevel As Integer) As Builder
            If indentLevel = 0 Then
                Return Me
            Else
                Dim builder As Builder = Me

                For i As Integer = 0 To indentLevel - 1
                    builder = builder.SubTableSchema.row
                Next

                Return builder
            End If
        End Function

        ''' <summary>
        ''' 实际上是做初始化，因为对象已经创建了
        ''' </summary>
        ''' <param name="indent"></param>
        ''' <param name="headers"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Function CreateBuilder(indent As Integer, headers As IEnumerable(Of String), strict As Boolean) As Builder
            Dim builder As Builder = Me

            If indent = 0 Then
                ' 不可以初始化最顶层的对象构建模块对象
                Throw New InvalidExpressionException
            Else
                For i As Integer = 0 To indent - 1
                    builder = builder.SubTableSchema.row
                Next
            End If

            If builder.Builder Is Nothing Then
                builder.Builder = headers _
                    .Skip(indent) _
                    .TakeWhile(Function(header)
                                   ' 如果上一层的列数目多于当前的层的数量的话
                                   ' 当前的层的数量在Excel导出数据后会使用空格填充
                                   ' 需要在这里删除这些空格
                                   Return Not header.StringEmpty
                               End Function) _
                    .DoCall(Function(titles)
                                Return builder _
                                    .Type _
                                    .createBuilderByHeaders(titles, strict)
                            End Function)
            End If

            Return builder
        End Function

    End Class
End Namespace
