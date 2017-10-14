#Region "Microsoft.VisualBasic::3137a325e2d86e2b9283e59062116172, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Settings\Inf\ClassMapper.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 定义在Ini配置文件之中的Section的名称
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ClassName : Inherits Attribute

        Public ReadOnly Property Name As String

        ''' <summary>
        ''' Defines the section name in the ini profile data.(定义在Ini配置文件之中的Section的名称)
        ''' </summary>
        ''' <param name="name"></param>
        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' 使用这个属性来标记需要进行序列化的对象属性: <see cref="DataFrameColumnAttribute"/>
    ''' </summary>
    Public Module ClassMapper

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MapParser(Of T As Class)() As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)())
            Return GetType(T).MapParser
        End Function

        ''' <summary>
        ''' Get mapping data, includes section name and keys
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MapParser(type As Type) As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)())
            Dim nameCLS As ClassName = type.GetAttribute(Of ClassName)
            Dim name As String

            If nameCLS Is Nothing Then
                name = type.Name
            Else
                name = nameCLS.Name
            End If

            Dim source = DataFrameColumnAttribute.LoadMapping(type)
            Dim binds As BindProperty(Of DataFrameColumnAttribute)() =
                source.Values.ToArray

            Return New NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) With {
                .Name = name,
                .Value = binds
            }
        End Function

        ''' <summary>
        ''' Read data from ini file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="ini"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ClassWriter(Of T As {New, Class})(ini As IniFile) As T
            Return DirectCast(ClassWriter(ini, GetType(T)), T)
        End Function

        Public Function ClassWriter(ini As IniFile, type As Type) As Object
            Dim maps As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) =
                MapParser(type)
            Dim obj As Object = Activator.CreateInstance(type)

            For Each map In maps.Value
                Dim key As String = map.Field.Name
                Dim value As String = ini.ReadValue(maps.Name, key)
                Dim o As Object = Scripting.CTypeDynamic(value, map.Type)
                Call map.SetValue(obj, o)
            Next

            Return obj
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub ClassDumper(Of T As Class)(x As T, ini As IniFile)
            Call ClassDumper(x, GetType(T), ini)
        End Sub

        Public Sub ClassDumper(x As Object, type As Type, ini As IniFile)
            Dim maps As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) =
                MapParser(type)

            For Each map In maps.Value
                Dim key As String = map.Field.Name
                Dim value As String = Scripting.ToString(map.GetValue(x))
                Call ini.WriteValue(maps.Name, key, value)
            Next
        End Sub

        ''' <summary>
        ''' Load a ini section profile data from a ini file.
        ''' </summary>
        ''' <typeparam name="T">The section mapper</typeparam>
        ''' <param name="path">*.ini file</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadIni(Of T As {New, Class})(path As String) As T
            Return New IniFile(path).ClassWriter(Of T)
        End Function

        ''' <summary>
        ''' Write ini section into data file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">A section class in the ini profile file.</param>
        ''' <param name="ini"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteClass(Of T As Class)(x As T, ini As String) As Boolean
            Try
                Call x.ClassDumper(New IniFile(ini))
            Catch ex As Exception
                ex = New Exception(ini, ex)
                ex = New Exception(GetType(T).FullName, ex)
                Call ex.PrintException
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function
    End Module
End Namespace
