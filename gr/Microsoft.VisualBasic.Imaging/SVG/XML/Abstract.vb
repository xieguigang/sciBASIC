#Region "Microsoft.VisualBasic::ad101507b791ca46e01b024eb2346a8f, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\XML\Abstract.vb"

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

'   Total Lines: 74
'    Code Lines: 40
' Comment Lines: 25
'   Blank Lines: 9
'     File Size: 2.68 KB


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

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports htmlNode = Microsoft.VisualBasic.MIME.Html.XmlMeta.Node

Namespace SVG.XML

    ''' <summary>
    ''' The basically SVG XML document node, it can be tweaks on the style by using CSS
    ''' </summary>
    Public MustInherit Class node : Inherits htmlNode
        Implements CSSLayer, IAddressOf

        <XmlAttribute> Public Property fill As String
        <XmlAttribute("fill-opacity"), DefaultValue(1)>
        Public Property fillOpacity As Double = 1

        ''' <summary>
        ''' the stroke color, value of this property should be html color code
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property stroke As String = "#000000"
        <XmlAttribute("stroke-opacity"), DefaultValue(1)>
        Public Property strokeOpacity As Double = 1
        <XmlAttribute("stroke-width"), DefaultValue(1)>
        Public Property strokeWidth As Double = 1
        <XmlAttribute("stroke-linecap")>
        Public Property strokeLinecap As String
        <XmlAttribute("stroke-linejoin")>
        Public Property strokeLinejoin As String
        <XmlAttribute("stroke-dasharray")>
        Public Property strokeDashArray As Integer()
        <XmlAttribute> Public Property filter As String
        <XmlAttribute("transform")>
        Public Overridable Property transform As String
        <XmlAttribute("transform-origin")>
        Public Property transformOrigin As String ' don't include BOM
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
        ''' 
        ''' 1. 空值的时候表示使用默认注释信息
        ''' 2. 空字符串或者空格表示没有注释信息
        ''' 3. 其他字符串的时候则使用给定的字符串做注释
        ''' </summary>
        <XmlIgnore> Public XmlCommentValue$ = ""

        ''' <summary>
        ''' Read Only
        ''' </summary>
        ''' <returns></returns>
        <XmlAnyElement("gComment")> Public Property XmlComment As XmlComment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If XmlCommentValue Is Nothing Then
                    Return XmlCommentValue.CreateComment()
                ElseIf XmlCommentValue.StringEmpty Then
                    Return Nothing
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
            Return $"id={id}; # {XmlCommentValue}"
        End Function
    End Class
End Namespace
