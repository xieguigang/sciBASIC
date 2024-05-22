#Region "Microsoft.VisualBasic::5da60dbd3e007a0efc48dec6e821a958, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Settings.vb"

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

    '   Total Lines: 85
    '    Code Lines: 52 (61.18%)
    ' Comment Lines: 21 (24.71%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 12 (14.12%)
    '     File Size: 3.18 KB


    '     Class Settings
    ' 
    '         Properties: SettingsData
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: __createSave, CreateEmpty, Load, LoadFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

#If NET_40 = 0 Then

Namespace ComponentModel.Settings

    Public Class Settings(Of T As {New, IProfile}) : Inherits ConfigEngine
        Implements IDisposable

        ''' <summary>
        ''' The target object instance that provides the data source for this config engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SettingsData As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return DirectCast(MyBase.profilesData, T)
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 从配置数据的实例对象创建配置映射
        ''' </summary>
        ''' <param name="config"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(config As T)
            Call MyBase.New(config)
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="XmlFile">目标配置文件的Xml文件的文件名</param>
        ''' <returns>可以调用的配置项的数目，解析失败则返回0</returns>
        ''' <remarks></remarks>
        Public Shared Function LoadFile(XmlFile As String, Optional CreateSave As Action(Of T, String) = Nothing) As Settings(Of T)
            If Not XmlFile.FileExists Then
                Return __createSave(XmlFile, CreateSave)
            Else
                Dim File As T = XmlFile.LoadXml(Of T)(throwEx:=False)

                If File Is Nothing Then
                    Return __createSave(XmlFile, CreateSave)
                Else
                    Return Load(Linq.SetValue(Of T).InvokeSet(File, NameOf(File.FilePath), XmlFile))
                End If
            End If
        End Function

        Private Shared Function __createSave(xml As String, createSave As Action(Of T, String)) As Settings(Of T)
            Dim FileObject As T = DirectCast(Activator.CreateInstance(Of T)(), T)
            FileObject.FilePath = xml
            If Not createSave Is Nothing Then
                Call createSave(FileObject, xml)
            End If
            Return Load(Data:=FileObject)
        End Function

        ''' <summary>
        ''' 使用<see cref="ProfileItem"/>来标记想要作为变量的属性
        ''' </summary>
        ''' <param name="Data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function Load(Data As T) As Settings(Of T)
            Return New Settings(Of T)(Data)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateEmpty() As Settings(Of T)
            Return New Settings(Of T)(Activator.CreateInstance(Of T))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(Settings As Settings(Of T)) As T
            Return Settings.SettingsData
        End Operator
    End Class
End Namespace
#End If
