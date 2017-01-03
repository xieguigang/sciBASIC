#Region "Microsoft.VisualBasic::ba6e5120037f73fdef4618370b4f2d3d, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Href.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel

    ''' <summary>
    ''' Resource link data.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlType("href-text", Namespace:="Microsoft.VisualBasic/Href_Annotation-Text")>
    Public Class Href : Implements INamedValue

#Region "Public Property"

        ''' <summary>
        ''' 资源的名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("Resource.Id", Namespace:="Microsoft.VisualBasic/Href_Annotation-ResourceId")>
        Public Property ResourceId As String Implements INamedValue.Key
        ''' <summary>
        ''' The relative path of the target resource object in the file system.(资源对象在文件系统之中的相对路径)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlElement("href-text", Namespace:="Microsoft.VisualBasic/Href_Annotation-Text-Data")>
        Public Property Value As String
        ''' <summary>
        ''' 注释数据
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property Annotations As String
#End Region

        ''' <summary>
        ''' 获取<see cref="Value"></see>所指向的资源文件的完整路径
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFullPath(DIR As String) As String
            Dim previous As String = FileIO.FileSystem.CurrentDirectory

            FileIO.FileSystem.CurrentDirectory = DIR
            Dim url As String = System.IO.Path.GetFullPath(Me.Value)
            FileIO.FileSystem.CurrentDirectory = previous
            Return url
        End Function

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class
End Namespace
