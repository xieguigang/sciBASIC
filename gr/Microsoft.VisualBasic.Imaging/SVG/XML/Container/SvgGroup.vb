#Region "Microsoft.VisualBasic::446b8996188619428f6740f4a326c96b, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgGroup.vb"

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

    '   Total Lines: 24
    '    Code Lines: 13 (54.17%)
    ' Comment Lines: 7 (29.17%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 4 (16.67%)
    '     File Size: 870 B


    '     Class SvgGroup
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;g> SVG element is a container used to group other SVG elements.
    '''
    ''' Transformations applied To the &lt;g> element are performed On its child elements, 
    ''' And its attributes are inherited by its children. It can also group multiple 
    ''' elements To be referenced later With the &lt;use> element.
    ''' </summary>
    Public NotInheritable Class SvgGroup : Inherits SvgContainer

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgGroup
            Dim element = parent.OwnerDocument.CreateElement("g")
            parent.AppendChild(element)
            Return New SvgGroup(element)
        End Function
    End Class
End Namespace
