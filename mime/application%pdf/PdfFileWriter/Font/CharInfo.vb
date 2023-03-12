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
