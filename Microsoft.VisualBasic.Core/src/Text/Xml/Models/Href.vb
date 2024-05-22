#Region "Microsoft.VisualBasic::08c37d07e12bcd1b14d7abbe21a4729f, Microsoft.VisualBasic.Core\src\Text\Xml\Models\Href.vb"

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

    '   Total Lines: 56
    '    Code Lines: 25 (44.64%)
    ' Comment Lines: 24 (42.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (12.50%)
    '     File Size: 1.95 KB


    '     Class Href
    ' 
    '         Properties: Comment, ResourceId, Value
    ' 
    '         Function: GetFullPath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.FileIO

Namespace Text.Xml.Models

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
        <XmlText> Public Property Comment As String
#End Region

        ''' <summary>
        ''' 获取<see cref="Value"></see>所指向的资源文件的完整路径
        ''' </summary>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFullPath(DIR As String) As String
            Using directory As New TemporaryEnvironment(DIR)
                Dim url As String = Path.GetFullPath(Me.Value)
                Return url
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class
End Namespace
