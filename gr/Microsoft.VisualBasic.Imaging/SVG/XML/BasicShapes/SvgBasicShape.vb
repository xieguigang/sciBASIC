#Region "Microsoft.VisualBasic::ebb5ab1feba4db7b6380f3ca7ffab47a, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\SvgBasicShape.vb"

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

    '   Total Lines: 27
    '    Code Lines: 8 (29.63%)
    ' Comment Lines: 16 (59.26%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 3 (11.11%)
    '     File Size: 1.25 KB


    '     Class SvgBasicShape
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' ## Basic shapes
    ''' 
    ''' There are several basic shapes used for most SVG drawing. The purpose of these shapes 
    ''' is fairly obvious from their names. Some of the parameters that determine their position 
    ''' and size are given, but an element reference would probably contain more accurate and 
    ''' complete descriptions along with other properties that won't be covered in here. 
    ''' However, since they're used in most SVG documents, it's necessary to give them some 
    ''' sort of introduction.
    '''
    ''' To insert a shape, you create an element in the document. Different elements correspond 
    ''' to different shapes And take different parameters to describe the size And position of 
    ''' those shapes. Some are slightly redundant in that they can be created by other shapes, 
    ''' but they're all there for your convenience and to keep your SVG documents as short and 
    ''' as readable as possible. 
    ''' </summary>
    Public MustInherit Class SvgBasicShape : Inherits SvgElement

        Protected Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub
    End Class
End Namespace
