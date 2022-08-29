#Region "Microsoft.VisualBasic::0ca87b32785822fcba8410a4a210103f, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Pixel.vb"

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

'   Total Lines: 21
'    Code Lines: 15
' Comment Lines: 0
'   Blank Lines: 6
'     File Size: 581 B


'     Interface Pixel
' 
'         Properties: Scale, X, Y
' 
'     Structure PixelData
' 
'         Properties: Scale, X, Y
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing2D.HeatMap

    Public Interface Pixel
        Property X As Integer
        Property Y As Integer
        Property Scale As Double

    End Interface

    Public Structure PixelData : Implements Pixel

        Public Property X As Integer Implements Pixel.X
        Public Property Y As Integer Implements Pixel.Y
        Public Property Scale As Double Implements Pixel.Scale

        Sub New(p As Point, data As Double)
            X = p.X
            Y = p.Y
            Scale = data
        End Sub

        Sub New(x As Integer, y As Integer, scale As Double)
            Me.X = x
            Me.Y = y
            Me.Scale = scale
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X},{Y} = {Scale.ToString("G4")}]"
        End Function

    End Structure
End Namespace
