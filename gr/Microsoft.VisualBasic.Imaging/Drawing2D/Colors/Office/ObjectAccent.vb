#Region "Microsoft.VisualBasic::4ee27ea9075425ce659d8161e63648aa, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Office\ObjectAccent.vb"

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

    '     Structure ObjectColor
    ' 
    '         Properties: sysClr
    ' 
    '     Structure Accent
    ' 
    '         Properties: srgbClr
    ' 
    '     Structure sysClr
    ' 
    '         Properties: lastClr, val
    ' 
    '         Function: ToString
    ' 
    '     Structure srgbClr
    ' 
    '         Properties: Color, val
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.OfficeAccent

    Public Structure ObjectColor
        Public Property sysClr As sysClr
    End Structure

    Public Structure Accent
        Public Property srgbClr As srgbClr
    End Structure

    Public Structure sysClr
        <XmlAttribute> Public Property val As String
        <XmlAttribute> Public Property lastClr As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    ''' <summary>
    ''' 颜色值
    ''' </summary>
    Public Structure srgbClr

        ''' <summary>
        ''' 颜色值
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property val As String

        Public ReadOnly Property Color As Color
            Get
                Return ColorTranslator.FromHtml("#" & val)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return val
        End Function
    End Structure
End Namespace
