#Region "Microsoft.VisualBasic::3d3eb3705667b82881bcc847fca2a15a, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\IOProvider.vb"

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

    '   Total Lines: 165
    '    Code Lines: 91 (55.15%)
    ' Comment Lines: 49 (29.70%)
    '    - Xml Docs: 93.88%
    ' 
    '   Blank Lines: 25 (15.15%)
    '     File Size: 6.50 KB


    '     Module IOProvider
    ' 
    '         Function: __getPath, __getSections, EmptySection, (+2 Overloads) LoadProfile, (+3 Overloads) WriteProfile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 在这个模块之中提供了.NET对象与``*.ini``配置文件之间的相互映射的序列化操作
    ''' </summary>
    Public Module IOProvider

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function EmptySection(x As Type, section As PropertyInfo) As String
            Return $"Property [{x.Name}\({section.PropertyType.Name}){section.Name}] for ini section is null."
        End Function

        ''' <summary>
        ''' 将目标对象写为``*.ini``文件
        ''' (目标对象之中的所有的简单属性都会被保存在一个对象名称的section中，)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T, path$) As Boolean
            Using ini As New IniFile(path)
                Call x.WriteProfile(ini)
                Call $"Ini profile data was saved at location: {path.GetFullPath}".__INFO_ECHO

                Return True
            End Using
        End Function

        ''' <summary>
        ''' 将目标对象写为``*.ini``文件
        ''' (目标对象之中的所有的简单属性都会被保存在一个对象名称的section中，)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T, ini As IniFile) As Boolean
            Dim sections = __getSections(Of T)()
            Dim msg$

            ' 首先写入global的配置数据
            Call GetType(T).ClassDumper(x, ini:=ini)

            For Each section As PropertyInfo In sections
                Dim obj = section.GetValue(x, Nothing)
                Dim schema As Type = section.PropertyType

                If obj Is Nothing Then
                    msg = GetType(T).EmptySection(section)
                    obj = Activator.CreateInstance(schema)

                    Call msg.Warning
                    Call App.LogException(msg)
                End If

                Call schema.ClassDumper(obj, ini:=ini)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 属性的类型需要定义<see cref="ClassName"/>，Section类型里面的属性还需要
        ''' 定义<see cref="DataFrameColumnAttribute"/>，否则将不会将对应的属性的值
        ''' 写入到ini文件之中。
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T) As Boolean
            Return x.WriteProfile(__getPath(Of T))
        End Function

        ''' <summary>
        ''' 查找出所有<see cref="ClassName"/>标记的属性
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Private Function __getSections(Of T As Class)() As PropertyInfo()
            Dim properties As PropertyInfo() = GetType(T).GetProperties(PublicProperty)

            properties = LinqAPI.Exec(Of PropertyInfo) _
 _
                () <= From p As PropertyInfo
                      In properties
                      Let type As Type = p.PropertyType
                      Let attr As ClassName = type.GetAttribute(Of ClassName)
                      Where Not attr Is Nothing
                      Select p

            Return properties
        End Function

        ''' <summary>
        ''' 从指定的``*.ini``文件之中加载配置数据，如果配置文件不存在，则这个函数会返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadProfile(Of T As Class)(path As String) As T
            Dim ini As New IniFile(path)

            If Not ini.FileExists Then
                Return Nothing
            End If

            Dim obj As Object = ClassMapper.ClassWriter(ini, GetType(T))
            Dim x As Object

            For Each prop As PropertyInfo In __getSections(Of T)()
                x = ClassMapper.ClassWriter(ini, prop.PropertyType)
                prop.SetValue(obj, x, Nothing)
            Next

            Return DirectCast(obj, T)
        End Function

        Private Function __getPath(Of T As Class)() As [Default](Of String)
            Dim path As IniMapIO = GetType(T).GetAttribute(Of IniMapIO)

            If path Is Nothing Then
                Throw New Exception("Could not found path mapping! @" & GetType(T).FullName)
            Else
                Return path.Path
            End If
        End Function

        ''' <summary>
        ''' 加载配置文件然后反序列化为一个指定类型的.NET对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="fileExists"></param>
        ''' <param name="path">
        ''' 如果这个参数是空值，则会需要在<typeparamref name="T"/>类型定义之中定义有一个<see cref="IniMapIO"/>属性来存储文件路径
        ''' </param>
        ''' <returns></returns>
        Public Function LoadProfile(Of T As Class)(Optional ByRef fileExists As Boolean = False, Optional ByRef path$ = Nothing) As T
            path = path Or __getPath(Of T)()
            fileExists = path.FileExists

            If Not fileExists Then
                ' 文件不存在，则直接写文件了
                Dim obj As T = Activator.CreateInstance(Of T)
                Call obj.WriteProfile
                Return obj
            Else
                Return path.LoadProfile(Of T)
            End If
        End Function
    End Module
End Namespace
