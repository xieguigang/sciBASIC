#Region "Microsoft.VisualBasic::26d0d125fced7574791a9313a8ea8d9c, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElement.vb"

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

'   Total Lines: 86
'    Code Lines: 46 (53.49%)
' Comment Lines: 21 (24.42%)
'    - Xml Docs: 95.24%
' 
'   Blank Lines: 19 (22.09%)
'     File Size: 2.52 KB


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
' 
'         Function: ScaleTo
' 
'         Sub: Paint, WriteAscii
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace PostScript

    ''' <summary>
    ''' abstract model of the painting elements
    ''' </summary>
    Public MustInherit Class PSElement

        Friend MustOverride Function GetXy() As PointF

        Friend MustOverride Sub WriteAscii(ps As Writer)
        Friend MustOverride Sub Paint(g As IGraphics)

        ''' <summary>
        ''' scale the current element to new location
        ''' </summary>
        ''' <param name="scaleX"></param>
        ''' <param name="scaleY"></param>
        ''' <returns></returns>
        Friend MustOverride Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement

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

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New PsComment With {
                .binary = binary,
                .text = text
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return Nothing
        End Function
    End Class

End Namespace
