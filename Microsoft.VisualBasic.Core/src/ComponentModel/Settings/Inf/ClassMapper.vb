#Region "Microsoft.VisualBasic::c52953d3b5f2337a7cc033457a2f2fe8, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\ClassMapper.vb"

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
'    Code Lines: 96 (60.38%)
' Comment Lines: 40 (25.16%)
'    - Xml Docs: 97.50%
' 
'   Blank Lines: 23 (14.47%)
'     File Size: 5.98 KB


'     Module ClassMapper
' 
'         Function: (+2 Overloads) ClassWriter, LoadIni, (+2 Overloads) MapParser, WriteClass
' 
'         Sub: (+2 Overloads) ClassDumper
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript.ES6
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports any = Microsoft.VisualBasic.Scripting

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 使用这个属性来标记需要进行序列化的对象属性: <see cref="DataFrameColumnAttribute"/>
    ''' </summary>
    Public Module ClassMapper

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MapParser(Of T As Class)() As NamedCollection(Of BindProperty(Of DataFrameColumnAttribute))
            Return GetType(T).MapParser
        End Function

        ''' <summary>
        ''' Get mapping data, includes section name and keys
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MapParser(type As Type) As NamedCollection(Of BindProperty(Of DataFrameColumnAttribute))
            Dim nameCLS As ClassName = type.GetAttribute(Of ClassName)
            Dim name As String

            If nameCLS Is Nothing Then
                name = type.Name
            Else
                name = nameCLS.Name
            End If

            Dim source = DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True)
            Dim binds As BindProperty(Of DataFrameColumnAttribute)() = source.Values _
                .Where(Function(f)
                           If TypeOf f.member Is PropertyInfo Then
                               Return DirectCast(f.member, PropertyInfo) _
                                   .GetIndexParameters _
                                   .IsNullOrEmpty
                           Else
                               Return True
                           End If
                       End Function) _
                .ToArray

            Return New NamedCollection(Of BindProperty(Of DataFrameColumnAttribute)) With {
                .name = name,
                .value = binds
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

        ''' <summary>
        ''' Load data from ini file onto a clr object with specific type
        ''' </summary>
        ''' <param name="ini"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function ClassWriter(ini As IniFile, type As Type) As Object
            Dim obj As Object = Activator.CreateInstance(type)
            Dim maps = MapParser(type)
            Dim o As Object

            For Each map As BindProperty(Of DataFrameColumnAttribute) In maps.value
                Dim key As String = map.field.Name

                If Not DataFramework.IsPrimitive(map.Type) Then
                    o = ClassWriter(ini, map.Type)
                Else
                    o = any.CTypeDynamic(ini.ReadValue(maps.name, key), map.Type)
                End If

                Call map.SetValue(obj, o)
            Next

            Return obj
        End Function

        ''' <summary>
        ''' Extract the config data from a clr object to write ini file
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="ini"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub ClassDumper(Of T As Class)(x As T, ini As IniFile)
            Call GetType(T).ClassDumper(x, ini)
        End Sub

        <Extension>
        Public Sub ClassDumper(type As Type, x As Object, ini As IniFile)
            Dim maps As NamedCollection(Of BindProperty(Of DataFrameColumnAttribute)) = MapParser(type)
            Dim section As String = maps.name

            Call ClassDumper(section, maps, x, ini)
        End Sub

        <Extension>
        Public Sub ClassDumper(section As String, fields As IEnumerable(Of BindProperty(Of DataFrameColumnAttribute)), x As Object, ini As IniFile)
            For Each map As BindProperty(Of DataFrameColumnAttribute) In fields
                Dim value As Object = map.GetValue(x)
                Dim type As Type = Nothing

                If value Is Nothing Then
                    If DataFramework.IsPrimitive(map.Type) Then
                        Call ini.WritePrimitiveValue(Nothing, section, map)
                    End If

                    Continue For
                Else
                    type = value.GetType
                End If

                If type.IsArray Then
                    ' 20261105 required of the array string contains no comma symbol
                    If DataFramework.IsPrimitive(type.GetElementType) Then
                        type = type.GetElementType
                        value = DirectCast(value, Array).AsObjectEnumerator.AsCharacter.JoinBy(", ")
                    End If
                End If

                If Not DataFramework.IsPrimitive(type) Then
                    If type.IsArray Then
                        Dim vec As Array = DirectCast(value, Array)
                        Dim maps As NamedCollection(Of BindProperty(Of DataFrameColumnAttribute)) = MapParser(type.GetElementType)

                        For i As Integer = 0 To vec.Length - 1
                            ' [xxx #1]
                            Call ClassDumper(maps.name & " #" & (i + 1), maps, vec.GetValue(i), ini)
                        Next
                    ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                        Call ini.WritePrimitiveValue(JsonContract.GetObjectJson(value, indent:=False), section, map)
                    Else
                        Call ClassDumper(type, value, ini)
                    End If
                Else
                    Call ini.WritePrimitiveValue(value, section, map)
                End If
            Next
        End Sub

        <Extension>
        Private Sub WritePrimitiveValue(ini As IniFile, value As Object, section As String, map As BindProperty(Of DataFrameColumnAttribute))
            Dim key As String = map.field.Name
            ' nothing mapping as empty string
            Dim val_str As String = any.ToString(value, null:="")
            Dim comment As String = map.member.Description

            If val_str.StringEmpty Then
                Call ini.WriteComment(section, $"{key}=<{map.Type.FullName}>", key)
            Else
                Call ini.WriteValue(section, key, val_str, comment)
            End If
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
