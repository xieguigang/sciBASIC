#Region "Microsoft.VisualBasic::61e9fc34e7c4e1ee45cd3c7ed18d81f2, mime\application%pdf\PdfFileWriter\Font\CharInfo.vb"

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

    '   Total Lines: 92
    '    Code Lines: 35 (38.04%)
    ' Comment Lines: 47 (51.09%)
    '    - Xml Docs: 87.23%
    ' 
    '   Blank Lines: 10 (10.87%)
    '     File Size: 2.92 KB


    ' Class CharInfo
    ' 
    '     Properties: ActiveChar, CharCode, DesignBBoxBottom, DesignBBoxLeft, DesignBBoxRight
    '                 DesignBBoxTop, DesignWidth, GlyphIndex, Type0Font
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: CompareTo
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' One character/Glyph information class
''' </summary>
''' <remarks>
''' This class defines all the information required to display a
''' character in the output document. Each character has an
''' associated glyph. The glyph geometry is defined in a square.
''' The square is DesignHeight by DesignHeight.
''' </remarks>
Public Class CharInfo : Implements IComparable(Of CharInfo)

    ''' <summary>
    ''' Character code
    ''' </summary>
    Public Property CharCode As Integer
    ''' <summary>
    ''' Glyph index
    ''' </summary>
    Public Property GlyphIndex As Integer
    ''' <summary>
    ''' Active character
    ''' </summary>
    Public Property ActiveChar As Boolean
    ''' <summary>
    ''' Character code is greater than 255
    ''' </summary>
    Public Property Type0Font As Boolean
    ''' <summary>
    ''' Bounding box left in design units
    ''' </summary>
    Public Property DesignBBoxLeft As Integer
    ''' <summary>
    ''' Bounding box bottom in design units
    ''' </summary>
    Public Property DesignBBoxBottom As Integer
    ''' <summary>
    ''' Bounding box right in design units
    ''' </summary>
    Public Property DesignBBoxRight As Integer
    ''' <summary>
    ''' Bounding box top in design units
    ''' </summary>
    Public Property DesignBBoxTop As Integer
    ''' <summary>
    ''' Character width in design units
    ''' </summary>
    Public Property DesignWidth As Integer

    Friend NewGlyphIndex As Integer
    Friend GlyphData As Byte()
    Friend Composite As Boolean

    Friend Sub New(CharCode As Integer, GlyphIndex As Integer, DC As FontApi)
        ' save char code and glyph index
        Me.CharCode = CharCode
        Me.GlyphIndex = GlyphIndex
        NewGlyphIndex = -1
        Type0Font = CharCode >= 256 OrElse GlyphIndex = 0

        ' Bounding Box
        Dim BBoxWidth As Integer = DC.ReadInt32()
        Dim BBoxHeight As Integer = DC.ReadInt32()
        DesignBBoxLeft = DC.ReadInt32()
        DesignBBoxTop = DC.ReadInt32()
        DesignBBoxRight = DesignBBoxLeft + BBoxWidth
        DesignBBoxBottom = DesignBBoxTop - BBoxHeight

        ' glyph advance horizontal and vertical
        DesignWidth = DC.ReadInt16()
        'DesignHeight = DC.ReadInt16();
        Return
    End Sub


    ' constructor for search and sort


    Friend Sub New(GlyphIndex As Integer)
        ' save char code and glyph index
        Me.GlyphIndex = GlyphIndex
        Return
    End Sub

    ''' <summary>
    ''' Compare two glyphs for sort and binary search
    ''' </summary>
    ''' <param name="Other">Other CharInfo</param>
    ''' <returns>Compare result</returns>
    Public Function CompareTo(Other As CharInfo) As Integer Implements IComparable(Of CharInfo).CompareTo
        Return GlyphIndex - Other.GlyphIndex
    End Function
End Class
