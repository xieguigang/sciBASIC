#Region "Microsoft.VisualBasic::77b1d8b3bd0101d9d17644e8fe0bffa4, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElement.vb"

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

    '   Total Lines: 72
    '    Code Lines: 39 (54.17%)
    ' Comment Lines: 15 (20.83%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 18 (25.00%)
    '     File Size: 1.90 KB


    '     Class PSElement
    ' 
    ' 
    ' 
    '     Class PSElement
    ' 
    '         Properties: shape
    ' 
    '     Class PsComment
    ' 
    '         Properties: binary, text
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace PostScript

    ''' <summary>
    ''' abstract model of the painting elements
    ''' </summary>
    Public MustInherit Class PSElement

        Friend MustOverride Sub WriteAscii(ps As Writer)
        Friend MustOverride Sub Paint(g As IGraphics)

    End Class

    ''' <summary>
    ''' An postscript graphics element
    ''' </summary>
    Public MustInherit Class PSElement(Of S As Shape) : Inherits PSElement

        Public Property shape As S

    End Class

    Public Class PsComment : Inherits PSElement

        Public Property binary As Boolean
        ''' <summary>
        ''' this text data will be base64 data string if is <see cref="binary"/> metadata
        ''' </summary>
        ''' <returns></returns>
        Public Property text As String

        Sub New()
        End Sub

        Sub New(binary As Byte())
            Me.binary = True
            Me.text = binary.ToBase64String
        End Sub

        Sub New(text As String)
            Me.text = text
            Me.binary = False
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            If binary Then
                Call ps.comment("binary meta data")

                ' split into parts by 160 chars
                For Each part As String In text.Chunks(160)
                    Call ps.comment(part)
                Next

                Call ps.comment("EOF_binarydata")
            Else
                Call ps.note(text)
            End If
        End Sub

        ''' <summary>
        ''' do nothing on comment node
        ''' </summary>
        ''' <param name="g"></param>
        Friend Overrides Sub Paint(g As IGraphics)
        End Sub
    End Class

End Namespace


