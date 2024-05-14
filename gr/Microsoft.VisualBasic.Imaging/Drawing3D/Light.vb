#Region "Microsoft.VisualBasic::d4133a8c8b8f37a5867fbccc38ab6f39, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Light.vb"

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

    '   Total Lines: 103
    '    Code Lines: 61
    ' Comment Lines: 27
    '   Blank Lines: 15
    '     File Size: 4.07 KB


    '     Module Light
    ' 
    '         Function: Illumination, (+2 Overloads) Lighting
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric

Namespace Drawing3D.Device

    ''' <summary>
    ''' <see cref="Surface"/>的表面光照的计算
    ''' </summary>
    Public Module Light

        ''' <summary>
        ''' Makes the 3D graphic more natural.(旧的光照模型，不推荐使用)
        ''' </summary>
        ''' <param name="surfaces">
        ''' Polygon buffer.(经过投影和排序之后得到的多边形的缓存对象)
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Illumination(surfaces As IEnumerable(Of Polygon)) As IEnumerable(Of Polygon)
            Dim array As Polygon() = surfaces.ToArray
            Dim steps! = 0.75! / array.Length
            Dim dark! = 1.0!

            ' 不能够打乱经过painter算法排序的结果，所以使用for循环
            For i As Integer = 0 To array.Length - 1
                With array(i)
                    If TypeOf .brush Is SolidBrush Then
                        Dim color As Color = DirectCast(.brush, SolidBrush).Color
                        Dim points As PointF() = .points

                        color = color.Darken(dark)
                        array(i) = New Polygon With {
                            .brush = New SolidBrush(color),
                            .points = points
                        }
                    End If
                End With

                dark -= steps
            Next

            Return array
        End Function

        ''' <summary>
        ''' 对模型之中的某一个表面进行光照处理
        ''' </summary>
        ''' <param name="path">这个对象是一个平面</param>
        ''' <param name="color"></param>
        ''' <param name="lightColor">光源的颜色，最常用的光源颜色为白色``<see cref="Color.White"/>``</param>
        ''' <returns></returns>
        <Extension>
        Public Function Lighting(path As Path3D, lightAngle As Point3D, color As Color, colorDifference#, lightColor As Color) As Color
            Return path.Points _
                .ToArray _
                .Lighting(lightAngle, color, colorDifference, lightColor)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vertices">构成模型之中的一个表面的顶点数据</param>
        ''' <param name="lightAngle"></param>
        ''' <param name="color"></param>
        ''' <param name="colorDifference#"></param>
        ''' <param name="lightColor"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Lighting(vertices As Point3D(), lightAngle As Point3D, color As Color, colorDifference#, lightColor As Color) As Color
            Dim p1 As Point3D = vertices(1)
            Dim p2 As Point3D = vertices(0)
            Dim i As Double = p2.X - p1.X
            Dim j As Double = p2.Y - p1.Y
            Dim k As Double = p2.Z - p1.Z

            If vertices.Length < 3 Then
                Return color
            End If

            p1 = vertices(2)
            p2 = vertices(1)

            Dim i2 As Double = p2.X - p1.X
            Dim j2 As Double = p2.Y - p1.Y
            Dim k2 As Double = p2.Z - p1.Z
            Dim i3 As Double = j * k2 - j2 * k
            Dim j3 As Double = -1 * (i * k2 - i2 * k)
            Dim k3 As Double = i * j2 - i2 * j
            Dim magnitude As Double = Sqrt(i3 * i3 + j3 * j3 + k3 * k3)

            i = If(magnitude = 0, 0, i3 / magnitude)
            j = If(magnitude = 0, 0, j3 / magnitude)
            k = If(magnitude = 0, 0, k3 / magnitude)

            Dim brightness As Double = i * lightAngle.X + j * lightAngle.Y + k * lightAngle.Z

            Return HSLColor.GetHSL(color).Lighten(brightness * colorDifference, lightColor)
        End Function
    End Module
End Namespace
