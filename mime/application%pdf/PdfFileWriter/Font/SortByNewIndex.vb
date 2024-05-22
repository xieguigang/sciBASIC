#Region "Microsoft.VisualBasic::9b5bfe3d7cefada0de4f7c68d264e0ce, mime\application%pdf\PdfFileWriter\Font\SortByNewIndex.vb"

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

    '   Total Lines: 9
    '    Code Lines: 5 (55.56%)
    ' Comment Lines: 3 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (11.11%)
    '     File Size: 357 B


    ' Class SortByNewIndex
    ' 
    '     Function: Compare
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' IComparer class for new glyph index sort
''' </summary>
Friend Class SortByNewIndex : Implements IComparer(Of CharInfo)

    Public Function Compare(CharOne As CharInfo, CharTwo As CharInfo) As Integer Implements IComparer(Of CharInfo).Compare
        Return CharOne.NewGlyphIndex - CharTwo.NewGlyphIndex
    End Function
End Class
