#Region "Microsoft.VisualBasic::1116a469efcc1ad425bfb282067f5e45, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\ClassMapper.vb"

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

    '   Total Lines: 147
    '    Code Lines: 96
    ' Comment Lines: 28
    '   Blank Lines: 23
    '     File Size: 5.50 KB


    '     Module ClassMapper
    ' 
    '         Function: (+2 Overloads) ClassWriter, LoadIni, (+2 Overloads) MapParser, WriteClass
    ' 
    '         Sub: (+2 Overloads) ClassDumper
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace ComponentModel.Settings.Inf

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

            Dim source = DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True)
            Dim binds = source.Values.ToArray

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
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ClassWriter(Of T As {New, Class})(ini As IniFile) As T
            Return DirectCast(ClassWriter(ini, GetType(T)), T)
        End Function

        Public Function ClassWriter(ini As IniFile, type As Type) As Object
            Dim obj As Object = Activator.CreateInstance(type)
            Dim maps = MapParser(type)
            Dim o As Object

            For Each map As BindProperty(Of DataFrameColumnAttribute) In maps.Value
                Dim key As String = map.field.Name

                If Not DataFramework.IsPrimitive(map.Type) Then
                    o = ClassWriter(ini, map.Type)
                Else
                    o = any.CTypeDynamic(ini.ReadValue(maps.Name, key), map.Type)
                End If

                Call map.SetValue(obj, o)
            Next

            Return obj
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub ClassDumper(Of T As Class)(x As T, ini As IniFile)
            Call GetType(T).ClassDumper(x, ini)
        End Sub

        <Extension>
        Public Sub ClassDumper(type As Type, x As Object, ini As IniFile)
            Dim maps = MapParser(type)

            For Each map As BindProperty(Of DataFrameColumnAttribute) In maps.Value
                Dim key As String = map.field.Name
                Dim value As Object = map.GetValue(x)

                If value IsNot Nothing AndAlso Not DataFramework.IsPrimitive(value.GetType) Then
                    Call ClassDumper(value.GetType, value, ini)
                    Continue For
                End If

                Dim val_str As String = any.ToString(value)
                Dim comment As String = map.member.Description

                If val_str.StringEmpty Then
                    Call ini.WriteComment(maps.Name, $"{key}=<{map.Type.FullName}>", key)
                Else
                    Call ini.WriteValue(maps.Name, key, val_str, comment)
                End If
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
        Public Function WriteClass(Of T As Class)(x As T, ini As String, Optional clean As Boolean = False) As Boolean
            If clean Then
                Call "".SaveTo(ini)
            End If

            Try
                Using inf As New IniFile(ini)
                    Call x.ClassDumper(inf)
                End Using
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
