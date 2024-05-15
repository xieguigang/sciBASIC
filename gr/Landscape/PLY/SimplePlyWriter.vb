#Region "Microsoft.VisualBasic::2c41fa6a60f8e00aebe45022a02c38db, gr\Landscape\PLY\SimplePlyWriter.vb"

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

    '   Total Lines: 56
    '    Code Lines: 48
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 2.39 KB


    '     Module SimplePlyWriter
    ' 
    '         Function: WriteAsciiText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports System.IO
Imports System.Text

Namespace Ply

    Public Module SimplePlyWriter

        Public Function WriteAsciiText(pointCloud As IEnumerable(Of PointCloud),
                                       buffer As Stream,
                                       Optional colors As ScalerPalette = ScalerPalette.turbo,
                                       Optional levels As Integer = 200) As Boolean

            Using file As New StreamWriter(buffer, Encoding.ASCII) With {
                .AutoFlush = True,
                .NewLine = vbLf
            }
                Dim vertex As PointCloud() = pointCloud.ToArray
                Dim header As New Header With {
                    .comment = "Point Cloud Model",
                    .element_face = -1,
                    .element_vertex = vertex.Length,
                    .properties = {
                        New NamedValue(Of String)("x", "float"),
                        New NamedValue(Of String)("y", "float"),
                        New NamedValue(Of String)("z", "float"),
                        New NamedValue(Of String)("intensity", "float"),
                        New NamedValue(Of String)("color", "string")
                    }
                }
                Dim colorSet As String() = Designer _
                    .GetColors(colors.Description, levels) _
                    .Select(Function(c) c.ToHtmlColor) _
                    .ToArray
                Dim scale As New DoubleRange(From v As PointCloud In vertex Select v.intensity)
                Dim i As Integer
                Dim index As New DoubleRange(0, levels - 1)
                Dim clr As String

                Call header.WriteAsciiText(file)
                Call file.Flush()

                For Each point As PointCloud In vertex
                    i = scale.ScaleMapping(point.intensity, index)
                    clr = colorSet(i)

                    Call file.WriteLine($"{point.x.ToString("F3")} {point.y.ToString("F3")} {point.z.ToString("F3")} {point.intensity} {clr}")
                Next
            End Using

            Return True
        End Function
    End Module
End Namespace
