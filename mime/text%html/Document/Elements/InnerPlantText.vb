#Region "Microsoft.VisualBasic::b86dcb5621984a40262d292c2630cb3f, mime\text%html\Document\Elements\InnerPlantText.vb"

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

    '   Total Lines: 55
    '    Code Lines: 32
    ' Comment Lines: 11
    '   Blank Lines: 12
    '     File Size: 1.62 KB


    '     Class InnerPlantText
    ' 
    '         Properties: InnerText, IsEmpty, IsPlantText, nodeName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetHtmlText, GetPlantText, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace Document

    ''' <summary>
    ''' Plant text inner the html.(HTML文档内的纯文本对象)
    ''' </summary>
    Public Class InnerPlantText : Implements IXmlNode

        Public Overridable Property InnerText As String

        Public Overridable ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(InnerText)
            End Get
        End Property

        Public Overridable ReadOnly Property IsPlantText As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' 纯文本节点没有标签名称
        ''' </summary>
        ''' <returns></returns>
        Private ReadOnly Property nodeName As String = Nothing Implements IXmlNode.nodeName

        Sub New()
        End Sub

        Sub New(text As String)
            InnerText = text
        End Sub

        Public Overrides Function ToString() As String
            Return InnerText Or EmptyString
        End Function

        Public Overridable Function GetHtmlText() As String
            Return InnerText
        End Function

        ''' <summary>
        ''' only use the ``&lt;br />`` tag as the new line in html text
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function GetPlantText() As String Implements IXmlNode.GetInnerText
            Return InnerText.UnescapeHTML.Trim(ASCII.CR, ASCII.LF)
        End Function
    End Class

End Namespace
