#Region "Microsoft.VisualBasic::25a299c5687992efbdfe843fabbf6e7b, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Pixel.vb"

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
'    Code Lines: 7
' Comment Lines: 0
'   Blank Lines: 3
'     File Size: 204.00 B


'     Class Pixel
' 
'         Properties: Scale, X, Y
' 
' 
' /********************************************************************************/

#End Region

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

        Public Overrides Function ToString() As String
            Return $"[{X},{Y} = {Scale.ToString("G4")}]"
        End Function

    End Structure
End Namespace
