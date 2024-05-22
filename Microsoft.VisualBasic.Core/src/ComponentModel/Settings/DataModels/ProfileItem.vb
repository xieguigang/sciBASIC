#Region "Microsoft.VisualBasic::c9dcb5e3f47f6ca3fbd2267da011ed69, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\DataModels\ProfileItem.vb"

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

    '   Total Lines: 81
    '    Code Lines: 35 (43.21%)
    ' Comment Lines: 37 (45.68%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (11.11%)
    '     File Size: 3.17 KB


    '     Enum ValueTypes
    ' 
    '         [Double], [Integer], Directory, File, Text
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ProfileItem
    ' 
    '         Properties: Description, Name, Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class ProfileNodeItem
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 这个并不是指宿主属性的数据类型，而是指代这一数据类型所代表的具体的实际对象
    ''' </summary>
    Public Enum ValueTypes
        ''' <summary>
        ''' 这个字符串的值是一个文件夹
        ''' </summary>
        Directory
        ''' <summary>
        ''' 这个字符串的值是一个文件的路径
        ''' </summary>
        File
        ''' <summary>
        ''' 普通的文本字符串
        ''' </summary>
        Text
        ''' <summary>
        ''' 带有小数的数值
        ''' </summary>
        [Double]
        ''' <summary>
        ''' 整数
        ''' </summary>
        [Integer]
    End Enum

    ''' <summary>
    ''' The simple configuration mapping node in the current profile data, the data type of this node 
    ''' object should be just the simplest data type such as String, Integer, Long, Double, Boolean.
    ''' (当前的配置节点为一个简单节点，即目标属性的属性值类型的字符串，数字或者逻辑值等最基本的数据类型)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ProfileItem : Inherits Attribute
        Implements IKeyValuePairObject(Of String, String)
        Implements INamedValue

        <XmlAttribute> Public Overridable Property Name As String Implements IKeyValuePairObject(Of String, String).Key, INamedValue.Key
        <XmlAttribute> Public Overridable Property Description As String Implements IKeyValuePairObject(Of String, String).Value

        ''' <summary>
        ''' 默认的数据类型是字符串类型
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Type As ValueTypes = ValueTypes.Text

        Sub New()
        End Sub

        ''' <summary>
        ''' Initialize a node in the settings xml document.
        ''' </summary>
        ''' <param name="NodeName">The name of the node in the document xml file</param>
        ''' <param name="NodeDescription">The brief introduction information about this profile node.</param>
        Sub New(NodeName As String, Optional NodeDescription As String = "")
            Name = NodeName
            Description = NodeDescription
        End Sub

        Public Overrides Function ToString() As String
            If Not String.IsNullOrEmpty(Description) Then
                Return String.Format("{0}: {1}", Name, Description)
            Else
                Return Name
            End If
        End Function
    End Class

    ''' <summary>
    ''' 当前的配置节点为一个复杂数据类型的配置节点，即目标属性的属性类型为一个Class对象
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ProfileNodeItem : Inherits Attribute
    End Class
End Namespace
