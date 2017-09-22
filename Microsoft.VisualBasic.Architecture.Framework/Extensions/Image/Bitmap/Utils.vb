Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports sys = System.Math

Namespace Imaging.BitmapImage

    ''' <summary>
    ''' Tools function for processing on <see cref="Image"/>/<see cref="Bitmap"/>
    ''' </summary>
    Public Module Utils

        ''' <summary>
        ''' 图片剪裁小方块区域
        ''' </summary>
        ''' <param name="pos">左上角的坐标位置</param>
        ''' <param name="size">剪裁的区域的大小</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Image.Corp")>
        <Extension> Public Function ImageCrop(source As Image, pos As Point, size As Size) As Image
            SyncLock source
                Dim CloneRect As New Rectangle(pos, size)
                Dim CloneBitmap As Bitmap = CType(source.Clone, Bitmap)
                Dim crop As Bitmap = CloneBitmap.Clone(CloneRect, source.PixelFormat)
                Return crop
            End SyncLock
        End Function

        <ExportAPI("Image.Resize")>
        Public Function Resize(Image As Image, newSize As Size) As Image
            SyncLock Image
                Using g As Graphics2D = newSize.CreateGDIDevice
                    With g
                        Call .DrawImage(Image, 0, 0, newSize.Width, newSize.Height)
                        Return .ImageResource
                    End With
                End Using
            End SyncLock
        End Function

        ''' <summary>
        ''' 图片剪裁为圆形的头像
        ''' </summary>
        ''' <param name="resAvatar">要求为正方形或者近似正方形</param>
        ''' <param name="OutSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function TrimRoundAvatar(resAvatar As Image, OutSize As Integer) As Image
            If resAvatar Is Nothing Then
                Return Nothing
            End If

            SyncLock resAvatar
                Dim Bitmap As New Bitmap(OutSize, OutSize)

                resAvatar = DirectCast(resAvatar.Clone, Image)
                resAvatar = Resize(resAvatar, Bitmap.Size)

                Using g = Graphics.FromImage(Bitmap)
                    Dim image As New TextureBrush(resAvatar)

                    With g
                        .CompositingQuality = CompositingQuality.HighQuality
                        .InterpolationMode = InterpolationMode.HighQualityBicubic
                        .SmoothingMode = SmoothingMode.HighQuality
                        .TextRenderingHint = TextRenderingHint.ClearTypeGridFit

                        Call .FillPie(image, Bitmap.EntireImage, 0, 360)
                    End With

                    Return Bitmap
                End Using
            End SyncLock
        End Function

        ''' <summary>
        ''' 羽化
        ''' </summary>
        ''' <param name="Image"></param>
        ''' <param name="y1"></param>
        ''' <param name="y2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function Vignette(image As Image, y1%, y2%, Optional renderColor As Color = Nothing) As Image
            Using g As Graphics2D = image.CreateCanvas2D
                With g
                    Dim alpha As Integer = 0
                    Dim delta = (Math.PI / 2) / sys.Abs(y1 - y2)
                    Dim offset As Double = 0

                    If renderColor = Nothing OrElse renderColor.IsEmpty Then
                        renderColor = Color.White
                    End If

                    For y As Integer = y1 To y2
                        Dim color As Color = Color.FromArgb(alpha, renderColor.R, renderColor.G, renderColor.B)
                        Dim pen As New Pen(color)

                        .DrawLine(pen, New Point(0, y), New Point(.Width, y))
                        alpha = CInt(255 * sys.Sin(offset) ^ 2)
                        offset += delta
                    Next

                    Dim rect As New Rectangle With {
                        .Location = New Point(0, y2),
                        .Size = New Size(.Width, .Height - y2)
                    }
                    Call .FillRectangle(New SolidBrush(renderColor), rect)

                    Return .ImageResource
                End With
            End Using
        End Function

        ''' <summary>
        ''' 将图像的多余的空白处给剪裁掉，确定边界，然后进行剪裁，使用这个函数需要注意下设置空白色，默认使用的空白色为<see cref="Color.White"/>
        ''' </summary>
        ''' <param name="res"></param>
        ''' <param name="margin"></param>
        ''' <param name="blankColor">默认白色为空白色</param>
        ''' <returns></returns>
        <ExportAPI("Image.CorpBlank")>
        <Extension> Public Function CorpBlank(res As Image,
                                              Optional margin% = 0,
                                              Optional blankColor As Color = Nothing,
                                              <CallerMemberName>
                                              Optional trace$ = Nothing) As Image

            If blankColor.IsNullOrEmpty Then
                blankColor = Color.White
            ElseIf blankColor.Name = NameOf(Color.Transparent) Then
                ' 系统的transparent颜色为 0,255,255,255
                ' 但是bitmap之中的transparent为 0,0,0,0
                ' 在这里要变换一下
                blankColor = New Color
            End If

            Dim bmp As BitmapBuffer
            Dim top%, left%

            Try
                bmp = BitmapBuffer.FromImage(res)
            Catch ex As Exception

                ' 2017-9-21 ???
                ' 未经处理的异常: 
                ' System.ArgumentException: 参数无效。
                '    在 System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format)
                '    在 System.Drawing.Bitmap..ctor(Image original, Int32 width, Int32 height)
                '    在 System.Drawing.Bitmap..ctor(Image original)

                ex = New Exception(trace & " --> " & res.Size.GetJson, ex)
                Throw ex
            End Try

            ' top
            For top = 0 To res.Height - 1
                Dim find As Boolean = False

                For left = 0 To res.Width - 1
                    Dim p = bmp.GetPixel(left, top)

                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            Dim region As New Rectangle(0, top, res.Width, res.Height - top)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = BitmapBuffer.FromImage(res)

            ' left
            For left = 0 To res.Width - 1
                Dim find As Boolean = False

                For top = 0 To res.Height - 1
                    Dim p = bmp.GetPixel(left, top)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(left, 0, res.Width - left, res.Height)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = BitmapBuffer.FromImage(res)

            Dim right As Integer
            Dim bottom As Integer

            ' bottom
            For bottom = res.Height - 1 To 0 Step -1
                Dim find As Boolean = False

                For right = res.Width - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, res.Width, bottom)
            res = res.ImageCrop(region.Location, region.Size)
            bmp = BitmapBuffer.FromImage(res)

            ' right
            For right = res.Width - 1 To 0 Step -1
                Dim find As Boolean = False

                For bottom = res.Height - 1 To 0 Step -1
                    Dim p = bmp.GetPixel(right, bottom)
                    If Not GDIColors.Equals(p, blankColor) Then
                        ' 在这里确定了左右
                        find = True
                        Exit For
                    End If
                Next

                If find Then
                    Exit For
                End If
            Next

            region = New Rectangle(0, 0, right, res.Height)
            res = res.ImageCrop(region.Location, region.Size)

            If margin > 0 Then
                With New Size(res.Width + margin * 2, res.Height + margin * 2).CreateGDIDevice
                    Call .Clear(blankColor)
                    Call .DrawImage(res, New Point(margin, margin))

                    Return .ImageResource
                End With
            Else
                Return res
            End If
        End Function
    End Module
End Namespace