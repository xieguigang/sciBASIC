#Region "Microsoft.VisualBasic::4cd3d0e87cef699f450f2bc357e4ea55, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\Image\Image.vb"

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

Imports System.Drawing.Imaging

Namespace Windows.Forms.Controls

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Image : Inherits System.Windows.Forms.PictureBox

#Region "图像控件属性值存储区域"
        ''' <summary>
        ''' 背景图像数据的存放位置
        ''' </summary>
        ''' <remarks></remarks>
        Dim [Bitmap] As System.Drawing.Bitmap = Nothing

        ''' <summary>
        ''' 图像控件的透明度
        ''' </summary>
        ''' <remarks></remarks>
        Dim _opacity As Integer = 255
#End Region

        ''' <summary>
        ''' 图像控件渐现动画绘制线程
        ''' </summary>
        ''' <remarks></remarks>
        Dim WithEvents tmrFadeInn As New Timers.Timer(1)
        ''' <summary>
        ''' 图像控件渐隐动画绘制线程
        ''' </summary>
        ''' <remarks></remarks>
        Dim WithEvents tmrFadeOut As New Timers.Timer(1)

#Region "控件用户自定义属性区域"
        ''' <summary>
        ''' 设置或者获取图像控件的背景图片
        ''' </summary>
        ''' <value>图像控件的背景图片</value>
        ''' <returns>图像控件的背景图片</returns>
        ''' <remarks></remarks>
        Public Overloads Property Image As System.Drawing.Image
            Get
                Return [Bitmap]
            End Get
            Set( value As System.Drawing.Image)
                If value Is Nothing Then
                    Me.BorderStyle = System.Windows.Forms.BorderStyle.None
                Else
                    [Bitmap] = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 在这里获取或者设置控件的透明度（0-255）
        ''' </summary>
        ''' <value>控件的透明度</value>
        ''' <returns>控件的透明度</returns>
        ''' <remarks></remarks>
        Public Property Opacity As Integer
            Get
                Return _opacity
            End Get
            Set( value As Integer)
                _opacity = value
                Me.Invalidate()
            End Set
        End Property
#End Region

        ''' <summary>
        ''' 启动控件的渐现动画的绘制线程
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub FadeIn()
            tmrFadeInn.Enabled = True
            tmrFadeInn.Start()
        End Sub

        ''' <summary>
        ''' 启动控件的渐隐动画的绘制线程
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub FadeOut()
            tmrFadeOut.Enabled = True
            tmrFadeOut.Start()
        End Sub

        Private Sub tmrFadeInn_Elapsed( sender As Object,  e As System.Timers.ElapsedEventArgs) Handles tmrFadeInn.Elapsed
            If Opacity + 5 < 255 Then Opacity += 5 Else tmrFadeInn.Stop()
        End Sub

        Private Sub tmrFadeOut_Elapsed( sender As Object,  e As System.Timers.ElapsedEventArgs) Handles tmrFadeOut.Elapsed
            If Opacity - 5 > 0 Then Opacity -= 5 Else tmrFadeOut.Stop()
        End Sub

        ''' <summary>
        ''' 复写了控件的绘制事件，仅当使用控件的Opacity属性更改透明度的时候，会调用Invalidate()过程，发生绘制事件
        ''' </summary>
        ''' <param name="pe"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub OnPaint( pe As System.Windows.Forms.PaintEventArgs)
            If [Bitmap] Is Nothing Then Return

            Dim BitmapBack = New Bitmap(Bitmap.Width, Bitmap.Height)
            BitmapBack = Bitmap.Clone

            Dim [data] As BitmapData = BitmapBack.LockBits(New Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
            Dim [nbyt] As Integer = data.Width * data.Height * 4
            Dim [rgbb](nbyt - 1) As Byte
            Dim [lptr] As IntPtr = [data].Scan0

            System.Runtime.InteropServices.Marshal.Copy(lptr, rgbb, 0, nbyt)

            For iLoop As Integer = 0 To nbyt - 1 Step 4
                If Not rgbb(iLoop + 3) = 0 Then rgbb(iLoop + 3) = _opacity
            Next

            System.Runtime.InteropServices.Marshal.Copy(rgbb, 0, [data].Scan0, rgbb.Length)
            BitmapBack.UnlockBits(data)
            pe.Graphics.DrawImage(BitmapBack, 0, 0, Bitmap.Width, Bitmap.Height)
        End Sub
    End Class
End Namespace
