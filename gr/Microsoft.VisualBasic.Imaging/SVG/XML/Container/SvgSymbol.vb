#Region "Microsoft.VisualBasic::44b03590a9286dddcae6459b688a0abf, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgSymbol.vb"

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

    '   Total Lines: 37
    '    Code Lines: 22
    ' Comment Lines: 9
    '   Blank Lines: 6
    '     File Size: 1.29 KB


    '     Class SvgSymbol
    ' 
    '         Properties: overflow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;symbol> element is used to define graphical template 
    ''' objects which can be instantiated by a &lt;use> element.
    ''' 
    ''' The use Of &lt;symbol> elements For graphics that are used multiple 
    ''' times In the same document adds Structure And semantics. Documents 
    ''' that are rich In Structure may be rendered graphically, As speech,
    ''' Or As Braille, And thus promote accessibility.
    ''' </summary>
    Public Class SvgSymbol : Inherits SvgContainer

        Public Property overflow As String
            Get
                Return Element.GetAttribute("overflow", defaultValue:="")
            End Get
            Set(value As String)
                Element.SetAttribute("overflow", value)
            End Set
        End Property

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgSymbol
            Dim element = parent.OwnerDocument.CreateElement("symbol")
            parent.AppendChild(element)
            Return New SvgSymbol(element)
        End Function
    End Class
End Namespace
