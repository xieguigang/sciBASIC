
''' <summary>
''' IComparer class for new glyph index sort
''' </summary>
Friend Class SortByNewIndex : Implements IComparer(Of CharInfo)

    Public Function Compare(CharOne As CharInfo, CharTwo As CharInfo) As Integer Implements IComparer(Of CharInfo).Compare
        Return CharOne.NewGlyphIndex - CharTwo.NewGlyphIndex
    End Function
End Class