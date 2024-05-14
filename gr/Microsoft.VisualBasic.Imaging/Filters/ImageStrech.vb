#Region "Microsoft.VisualBasic::84ba2e392bce626c33be750feb519490, gr\Microsoft.VisualBasic.Imaging\Filters\ImageStrech.vb"

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

    '   Total Lines: 44
    '    Code Lines: 27
    ' Comment Lines: 11
    '   Blank Lines: 6
    '     File Size: 1.62 KB


    '     Class ImageStrech
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: pull, Resize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Linq

Namespace Filters

    Public Class ImageStrech

        ''' <summary>
        ''' [x => [y => pixel]]
        ''' </summary>
        ReadOnly matrix As Dictionary(Of Integer, Dictionary(Of Integer, Pixel))
        ''' <summary>
        ''' the original image size
        ''' </summary>
        ReadOnly dims As Rectangle

        Sub New(img As IRasterGrayscaleHeatmap)
            matrix = img.GetRasterPixels _
                .GroupBy(Function(p) p.X) _
                .ToDictionary(Function(c) c.Key,
                              Function(c)
                                  Return c.ToDictionary(Function(p) p.Y)
                              End Function)
            dims = New Polygon2D(pull).GetDimension
        End Sub

        Private Function pull() As IEnumerable(Of Pixel)
            Return matrix.Values.Select(Function(c) c.Values).IteratesALL
        End Function

        ''' <summary>
        ''' Resize and strech the current image matrix data
        ''' </summary>
        ''' <param name="newSize"></param>
        ''' <returns></returns>
        Public Iterator Function Resize(newSize As Size) As IEnumerable(Of Pixel)
            Dim originalSize As Size = dims.Size
            Dim ratioX As Double = newSize.Width / originalSize.Width
            Dim ratioY As Double = newSize.Height / originalSize.Height
        End Function
    End Class
End Namespace
