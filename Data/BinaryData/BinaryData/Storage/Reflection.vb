#Region "Microsoft.VisualBasic::a9b14135004a7ea01f518ba84a3636e2, Data\BinaryData\BinaryData\Storage\Reflection.vb"

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

' Module Reflection
' 
'     Function: CreateReader, CreateWriter, ReadObject, WriteObject
' 
' Class SchemaTree
' 
'     Properties: BaseType, Schema, Tree
' 
'     Function: BuildTree, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Array = Microsoft.VisualBasic.Language.List(Of Microsoft.VisualBasic.Data.IO.SchemaTree)

''' <summary>
''' Binary serialization
''' </summary>
Public Module Reflection

    ''' <summary>
    ''' Compile write method
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="writer"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateWriter(Of T)(writer As StringWriter) As Func(Of T, Integer)
        Dim schema As SchemaTree = SchemaTree.BuildTree(GetType(T))
        Return Function(obj As T) As Integer
                   Return writer.WriteObject(schema, obj)
               End Function
    End Function

    ''' <summary>
    ''' suitable for array reader
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="reader"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateReader(Of T As {New, Class})(reader As StringReader) As Func(Of Long, T)
        Dim schema As SchemaTree = SchemaTree.BuildTree(GetType(T))
        Return Function(offset As Long) As T
                   Call reader.Seek(offset)
                   Return reader.ReadObject(schema, GetType(T))
               End Function
    End Function

    <Extension>
    Private Function ReadObject(reader As StringReader, tree As SchemaTree, target As Type) As Object
        Dim s$
        Dim value As Object
        Dim obj As Object = Activator.CreateInstance(target)

        For Each read As SchemaTree In tree.Tree
            If Not read.BaseType Is Nothing Then
                Dim n% = reader.ReadString
                Dim array As System.Array = System.Array.CreateInstance(read.BaseType, n)

                If DataFramework.IsPrimitive(read.BaseType) Then
                    ' 字符串或者数值数组
                    ' 则读取出字符串，再做类型转换
                    For i As Integer = 0 To n - 1
                        s = reader.ReadString
                        value = Scripting.CTypeDynamic(s, read.BaseType)
                        array.SetValue(value, i)
                    Next
                Else
                    For i As Integer = 0 To n - 1
                        value = reader.ReadObject(read, read.BaseType)
                        array.SetValue(value, i)
                    Next
                End If

                value = array
            ElseIf read.Tree Is Nothing Then
                ' 读取字符串类型，再做转换
                s = reader.ReadString
                value = Scripting.CTypeDynamic(s, read.Schema.PropertyType)
            Else
                value = reader.ReadObject(read, target:=read.Schema.PropertyType)
            End If

            Call read.Schema.SetValue(obj, value)
        Next

        Return obj
    End Function

    <Extension>
    Private Function WriteObject(writer As StringWriter, schema As SchemaTree, obj As Object) As Integer
        Dim s$
        Dim n%
        Dim value As Object

        For Each read As SchemaTree In schema.Tree
            value = read.Schema.GetValue(obj)

            If Not read.BaseType Is Nothing Then
                Dim array() = DirectCast(value, IEnumerable).ToVector

                n += writer.Append(array.Length)

                If DataFramework.IsPrimitive(read.BaseType) Then
                    For Each item As Object In array
                        n += writer.Append(item)
                    Next
                Else
                    For Each item As Object In array
                        n += writer.WriteObject(read, item)
                    Next
                End If
            ElseIf Not read.Tree Is Nothing Then
                ' is complexe type 
                n += writer.WriteObject(read, value)
            Else
                ' is primitive type
                s = Scripting.ToString(value)
                n += writer.Append(s)
            End If
        Next

        Return n
    End Function
End Module

''' <summary>
''' 只允许数组类型的集合
''' </summary>
Public Class SchemaTree

    Public Property Schema As PropertyInfo
    Public Property Tree As SchemaTree()
    ''' <summary>
    ''' Base type of the array collection
    ''' </summary>
    ''' <returns></returns>
    Public Property BaseType As Type

    Public Overrides Function ToString() As String
        If Schema Is Nothing Then
            Return "/" ' root
        ElseIf Tree Is Nothing Then
            Return Schema.Name
        Else
            Return Schema.Name & ": " & Tree.Select(Function(t) t.ToString).GetJson
        End If
    End Function

    Public Shared Function BuildTree(type As Type) As SchemaTree
        Dim readers = type _
            .Schema(PropertyAccess.ReadWrite, PublicProperty, nonIndex:=True) _
            .OrderBy(Function(name) name.Key) _
            .Values
        Dim tree As New Array
        Dim arrayType As Type

        For Each read As PropertyInfo In readers
            Dim target As Type = read.PropertyType

            If DataFramework.IsPrimitive(target) Then
                tree += New SchemaTree With {.Schema = read}
            ElseIf target.IsInheritsFrom(GetType(System.Array)) Then
                arrayType = target.GetElementType

                If DataFramework.IsPrimitive(arrayType) Then
                    tree += New SchemaTree With {
                        .Schema = read,
                        .BaseType = arrayType,
                        .Tree = Nothing
                    }
                Else
                    tree += New SchemaTree With {
                        .Schema = read,
                        .BaseType = arrayType,
                        .Tree = BuildTree(arrayType).Tree
                    }
                End If
            Else
                tree += New SchemaTree With {
                    .Schema = read,
                    .Tree = BuildTree(target).Tree
                }
            End If
        Next

        Return New SchemaTree With {
            .Tree = tree,
            .Schema = Nothing
        }
    End Function
End Class
