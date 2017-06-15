Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Extensions

    ''' <summary>
    ''' 假若目标图像是svg类型，则会被合成为gdi图像，如果是gdi图像，则会被直接转换
    ''' </summary>
    ''' <param name="img"></param>
    ''' <returns></returns>
    <Extension> Public Function AsGDIImage(img As GraphicsData) As Image
        If img.Driver = Drivers.GDI Then
            Return DirectCast(img, ImageData).Image
        Else
            ' 将svg矢量图合成为gdi图像
            Return DirectCast(img, SVGData).Render
        End If
    End Function
End Module
