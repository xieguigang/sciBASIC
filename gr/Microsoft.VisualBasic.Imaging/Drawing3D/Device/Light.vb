Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing3D.Device

    ''' <summary>
    ''' <see cref="Surface"/>的表面光照的计算
    ''' </summary>
    Public Module Light

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="surfaces">经过投影和排序之后得到的多边形的缓存对象</param>
        ''' <returns></returns>
        <Extension>
        Public Function Illumination(surfaces As IEnumerable(Of Polygon)) As IEnumerable(Of Polygon)
            Dim array As Polygon() = surfaces.ToArray
            Dim steps! = 1.0! / array.Length
            Dim dark! = 1.0!

            For i As Integer = 0 To array.Length - 1  ' 不能够打乱经过painter算法排序的结果，所以使用for循环
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