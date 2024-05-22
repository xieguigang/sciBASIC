#Region "Microsoft.VisualBasic::c6cf39884cba05127173794f747be79e, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\ASCIIArt\WeightedChar.vb"

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

    '   Total Lines: 33
    '    Code Lines: 17 (51.52%)
    ' Comment Lines: 11 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 1014 B


    '     Class WeightedChar
    ' 
    '         Properties: Character, CharacterImage, Weight
    ' 
    '         Function: getDefaultCharSet, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace Drawing2D.Text.ASCIIArt

    ''' <summary>
    ''' a pixel char
    ''' </summary>
    Public Class WeightedChar

        ''' <summary>
        ''' a char that represent a pixel on the source bitmap
        ''' </summary>
        ''' <returns></returns>
        Public Property Character As String
        Public Property CharacterImage As Bitmap
        ''' <summary>
        ''' the gray scale value
        ''' </summary>
        ''' <returns></returns>
        Public Property Weight As Double

        Public Overrides Function ToString() As String
            Return $"{Character} ({Weight})"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function getDefaultCharSet() As [Default](Of  WeightedChar())
            Return CharSet.GenerateFontWeights()
        End Function
    End Class
End Namespace
