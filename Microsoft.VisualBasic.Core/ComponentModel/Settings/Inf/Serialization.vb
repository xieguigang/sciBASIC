#Region "Microsoft.VisualBasic::80558a737fc0f3b2b14508618eb8ae19, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\Serialization.vb"

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

    '     Class Serialization
    ' 
    '         Properties: Sections
    ' 
    '         Function: __getDefaultPath, Load, Save
    ' 
    '     Class IniMapIO
    ' 
    '         Properties: Path
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module IOProvider
    ' 
    '         Function: __getPath, __getSections, EmptySection, (+2 Overloads) LoadProfile, (+2 Overloads) WriteProfile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Settings.Inf

    Public Class Serialization : Inherits ITextFile

        <XmlElement> Public Property Sections As Section()

        Public Shared Function Load(path As String) As Serialization
            Throw New NotImplementedException
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class

    ''' <summary>
    ''' The path parameter can be shortcut by method <see cref="PathMapper.GetMapPath"/>.
    ''' additional, using ``@fileName`` for using <see cref="App.GetFile(String)"/> API.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class IniMapIO : Inherits Attribute

        Public ReadOnly Property Path As String

        ''' <summary>
        ''' The path parameter can be shortcut by method <see cref="PathMapper.GetMapPath"/>
        ''' </summary>
        ''' <param name="path"></param>
        Sub New(path As String)
            If path.First = "@"c Then
                Me.Path = App.GetFile(Mid(path, 2))
            Else
                Me.Path = PathMapper.GetMapPath(path)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

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
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T, path$) As Boolean
            Dim ini As New IniFile(path)
            Dim msg$

            For Each section As PropertyInfo In __getSections(Of T)()
                Dim obj As Object = section.GetValue(x, Nothing)
                Dim schema As Type = section.PropertyType

                If obj Is Nothing Then
                    msg = GetType(T).EmptySection(section)
                    obj = Activator.CreateInstance(schema)

                    Call msg.Warning
                    Call App.LogException(msg)
                End If

                Call ClassMapper.ClassDumper(
                    x:=obj,
                    type:=schema,
                    ini:=ini
                )
            Next

            Call $"Ini profile data was saved at location: {path.GetFullPath}".__INFO_ECHO

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

        Private Function __getSections(Of T As Class)() As PropertyInfo()
            Dim properties As PropertyInfo() =
                GetType(T).GetProperties(bindingAttr:=
                BindingFlags.Instance Or
                BindingFlags.Public
            )

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
        ''' 从指定的``*.ini``文件之中加载配置数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadProfile(Of T As Class)(path As String) As T
            Dim obj As Object = Activator.CreateInstance(Of T)
            Dim ini As New IniFile(path)

            For Each prop As PropertyInfo In __getSections(Of T)()
                Dim x As Object = ClassMapper.ClassWriter(ini, prop.PropertyType)
                Call prop.SetValue(obj, x, Nothing)
            Next

            Return DirectCast(obj, T)
        End Function

        Private Function __getPath(Of T As Class)() As String
            Dim path As IniMapIO = GetType(T).GetAttribute(Of IniMapIO)

            If path Is Nothing Then
                Throw New Exception("Could not found path mapping! @" & GetType(T).FullName)
            Else
                Return path.Path
            End If
        End Function

        Public Function LoadProfile(Of T As Class)(Optional ByRef fileExists As Boolean = False, Optional ByRef path$ = Nothing) As T
            path = __getPath(Of T)()
            fileExists = path.FileExists

            If Not fileExists Then  ' 文件不存在，则直接写文件了
                Dim obj As T = Activator.CreateInstance(Of T)
                Call obj.WriteProfile
                Return obj
            Else
                Return path.LoadProfile(Of T)
            End If
        End Function
    End Module
End Namespace
