#Region "Microsoft.VisualBasic::4871c8ae970e9ffd9ecef3a6f579c9a3, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Abstract.vb"

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

'     Class node
' 
'         Properties: attributes, fill, filter, stroke, XmlComment
'                     zIndex
' 
'         Function: ToString
' 
'         Sub: Assign
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
Imports htmlNode = Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta.Node

Namespace SVG.XML

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node : Inherits htmlNode
        Implements CSSLayer, IAddressOf

        <XmlAttribute> Public Property fill As String
        <XmlAttribute> Public Property stroke As String
        <XmlAttribute> Public Property filter As String

        ''' <summary>
        ''' Css layer index, this will controls the rendering order of the graphics layer.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("z-index")>
        Public Property zIndex As Integer Implements CSSLayer.zIndex, IAddressOf.Address

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 暂时未找到动态属性的解决方法，暂时忽略掉
        ''' </remarks>
        <XmlIgnore>
        Public Property attributes As Dictionary(Of String, String)

        ''' <summary>
        ''' 对当前的文档节点/图层信息的注释
        ''' </summary>
        Public XmlCommentValue$

        ''' <summary>
        ''' Read Only
        ''' </summary>
        ''' <returns></returns>
        <XmlAnyElement("gComment")> Public Property XmlComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If XmlCommentValue.StringEmpty Then
                    Return XmlCommentValue.CreateComment()
                Else
                    Return New XmlDocument().CreateComment(XmlCommentValue)
                End If
            End Get
            Set
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            zIndex = address
        End Sub

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class
End Namespace
