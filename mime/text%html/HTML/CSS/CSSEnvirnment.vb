#Region "Microsoft.VisualBasic::fb073aeefc6f4ca83d980a5a40daa778, mime\text%html\HTML\CSS\CSSEnvirnment.vb"

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

    '     Class CSSEnvirnment
    ' 
    '         Properties: baseFont, baseLine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetFontByScale
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace HTML.CSS

    Public Class CSSEnvirnment

        Public ReadOnly Property baseFont As Font
        Public ReadOnly Property baseLine As Pen

        Sub New(basefont As Font, baseline As Pen)
            Me.baseFont = basefont
            Me.baseLine = baseline
        End Sub

        Public Function GetFontByScale(em As Single) As Font
            Dim newSize As Single = em * baseFont.Size
            Dim newFont As New Font(baseFont.FontFamily, newSize, baseFont.Style)

            Return newFont
        End Function

    End Class
End Namespace
