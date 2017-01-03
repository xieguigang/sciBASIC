#Region "Microsoft.VisualBasic::5de7ec5f384c143877ea3241f3088e93, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Settings\Settings.vb"

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

Imports System.Text
Imports System.Reflection
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Linq

#If NET_40 = 0 Then

Namespace ComponentModel.Settings

    Public Class Settings(Of T As IProfile) : Inherits ConfigEngine
        Implements System.IDisposable

        ''' <summary>
        ''' The target object instance that provides the data source for this config engine.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SettingsData As T
            Get
                Return _SettingsData.As(Of T)
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 从配置数据的实例对象创建配置映射
        ''' </summary>
        ''' <param name="config"></param>
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
                Dim File As T = XmlFile.LoadXml(Of T)(ThrowEx:=False)
                If File Is Nothing Then
                    Return __createSave(XmlFile, CreateSave)
                Else
                    Return Load(SetValue(Of T).InvokeSet(File, NameOf(File.FilePath), XmlFile))
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
        Public Overloads Shared Function Load(Data As T) As Settings(Of T)
            Return New ComponentModel.Settings.Settings(Of T)(Data)
        End Function

        Public Shared Function CreateEmpty() As Settings(Of T)
            Dim x As T = Activator.CreateInstance(Of T)
            Return New Settings(Of T)(x)
        End Function

        Public Overloads Shared Narrowing Operator CType(Settings As Settings.Settings(Of T)) As T
            Return Settings._SettingsData.As(Of T)
        End Operator
    End Class
End Namespace
#End If
