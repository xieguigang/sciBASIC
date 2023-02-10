#Region "Microsoft.VisualBasic::b4c004b0b0e8f696931c474bd074ddf1, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\Pixel.vb"

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

    '   Total Lines: 67
    '    Code Lines: 35
    ' Comment Lines: 20
    '   Blank Lines: 12
    '     File Size: 1.71 KB


    '     Interface Pixel
    ' 
    '         Properties: Scale, X, Y
    ' 
    '     Structure PixelData
    ' 
    '         Properties: isEmpty, Scale, X, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing2D.HeatMap

    ''' <summary>
    ''' a generic data model for <see cref="HeatMapRaster(Of T)"/>
    ''' </summary>
    Public Interface Pixel

        ''' <summary>
        ''' the x axis data
        ''' </summary>
        ''' <returns></returns>
        Property X As Integer
        ''' <summary>
        ''' the y axis data
        ''' </summary>
        ''' <returns></returns>
        Property Y As Integer
        ''' <summary>
        ''' the color scale data
        ''' </summary>
        ''' <returns></returns>
        Property Scale As Double

    End Interface

    Public Structure PixelData : Implements Pixel

        Public Property X As Integer Implements Pixel.X
        Public Property Y As Integer Implements Pixel.Y
        Public Property Scale As Double Implements Pixel.Scale

        Public ReadOnly Property isEmpty As Boolean
            Get
                Return X = 0 AndAlso Y = 0 AndAlso Scale = 0.0
            End Get
        End Property

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

        ''' <summary>
        ''' spot data is zero
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{X},{Y} = {Scale.ToString("G4")}]"
        End Function

    End Structure
End Namespace
