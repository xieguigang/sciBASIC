#Region "Microsoft.VisualBasic::91257570d262f8b0ec1d626493dbbba8, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Device\Light.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing3D.Device

    ''' <summary>
    ''' <see cref="Surface"/>的表面光照的计算
    ''' </summary>
    Public Module Light

        ''' <summary>
        ''' Makes the 3D graphic more natural.
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
                        Dim points As Point() = .points

                        color = color.Dark(dark)
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
    End Module
End Namespace
