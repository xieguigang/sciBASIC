#Region "Microsoft.VisualBasic::9b861038d82a007f91fd280eb781bcd6, G:/GCModeller/src/runtime/sciBASIC#/gr/Microsoft.VisualBasic.Imaging//SVG/XML/Container/SvgClipPath.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 975 B


    '     Class SvgClipPath
    ' 
    '         Properties: ClipPathUnits
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    Public NotInheritable Class SvgClipPath
        Inherits SvgContainer

        Public Property ClipPathUnits As SvgClipPathUnits
            Get
                Return Element.GetAttribute(Of SvgClipPathUnits)("clipPathUnits", SvgClipPathUnits.UserSpaceOnUse)
            End Get
            Set(value As SvgClipPathUnits)
                Element.SetAttribute("clipPathUnits", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgClipPath
            Dim element = parent.OwnerDocument.CreateElement("clipPath")
            parent.AppendChild(element)
            Return New SvgClipPath(element)
        End Function
    End Class
End Namespace

