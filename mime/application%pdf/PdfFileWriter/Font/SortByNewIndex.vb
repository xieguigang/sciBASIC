﻿#Region "Microsoft.VisualBasic::47c2d384e601ae21240a695c98a386fa, G:/GCModeller/src/runtime/sciBASIC#/mime/application%pdf//PdfFileWriter/Font/SortByNewIndex.vb"

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

    '   Total Lines: 10
    '    Code Lines: 5
    ' Comment Lines: 3
    '   Blank Lines: 2
    '     File Size: 359 B


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
