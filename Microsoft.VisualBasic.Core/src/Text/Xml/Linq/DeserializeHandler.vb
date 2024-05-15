#Region "Microsoft.VisualBasic::3a7853c18fc37ddfc0bf3c2770fdd2ce, Microsoft.VisualBasic.Core\src\Text\Xml\Linq\DeserializeHandler.vb"

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
    '    Code Lines: 43
    ' Comment Lines: 24
    '   Blank Lines: 14
    '     File Size: 3.30 KB


    '     Class DeserializeHandler
    ' 
    '         Properties: ReplaceXmlns
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: LoadXml, RemoveXmlns, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml.Serialization

Namespace Text.Xml.Linq

    ''' <summary>
    ''' The xml deserialize helper
    ''' </summary>
    Public Class DeserializeHandler(Of T As Class)

        ReadOnly process As Func(Of String, String)
        ''' <summary>
        ''' Get type name that defined on the class type decoration with custom attributes:
        ''' <see cref="XmlTypeAttribute"/> or <see cref="XmlRootAttribute"/>
        ''' </summary>
        ReadOnly Tname$ = GetType(T).GetNodeNameDefine

        Public Property ReplaceXmlns As String

        Sub New(xmlNode As String)
            ' 2017-12-22
            ' 假若对象是存储在一个数组之中的，那么，可能会出现的一种情况就是
            ' 在类型的定义之中，使用了xmlelement重新定义了节点的名字
            ' 例如 <XmlElement("A")>
            ' 那么在生成的XML文件之中的节点名称就是A
            ' 但是元素A的类型定义却是 Public Class B ... End Class
            ' 因为A不等于B，所以将无法正确的加载XML节点数据
            ' 在这里进行名称的替换来消除这种错误
            If Tname = xmlNode Then
                ' 不需要进行替换
                process = Function(s) s
            Else
                Dim leftTag% = 1 + xmlNode.Length
                Dim rightTag% = 3 + xmlNode.Length

                ' 在这里不尝试做直接替换，可能会误杀其他的节点
                process = Function(block)
                              block = block.Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)
                              block = block.Substring(leftTag, block.Length - leftTag)
                              block = block.Substring(0, block.Length - rightTag)
                              block = "<" & Tname & block & "</" & Tname & ">"

                              Return block
                          End Function
            End If
        End Sub

        ReadOnly sb As New StringBuilder

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RemoveXmlns(xml As String) As String
            Return xml.Replace($"xmlns=""{ReplaceXmlns}""", "")
        End Function

        ''' <summary>
        ''' This method have bugs when deal with the xml when it have 
        ''' multiple xml namespace value
        ''' </summary>
        ''' <param name="xml"></param>
        ''' <returns></returns>
        Public Function LoadXml(xml As String, Optional variants As Type() = Nothing) As T
            Call sb.Clear()
            Call sb.AppendLine("<?xml version=""1.0"" encoding=""utf-16""?>")
            Call sb.AppendLine(process(xml))

            If Not ReplaceXmlns.StringEmpty Then
                Call sb.Replace($"xmlns=""{ReplaceXmlns}""", "")
            End If

            xml = sb.ToString

            ' 对调整好的Xml文档执行反序列化操作
            Return xml.LoadFromXml(Of T)(doNamespaceIgnorant:=True, variants:=variants)
        End Function

        Public Overrides Function ToString() As String
            Return $"LoadXml(Of {Tname})"
        End Function
    End Class
End Namespace
