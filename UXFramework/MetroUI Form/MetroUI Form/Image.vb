#Region "Microsoft.VisualBasic::dd09d89d7b90f5f2848d905e779a8073, ..\visualbasic_App\UXFramework\MetroUI Form\MetroUI Form\Image.vb"

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
Imports System.Drawing

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
    Dim [_Bitmap] As System.Drawing.Bitmap = null

    ''' <summary>
    ''' 图像控件的透明度
    ''' </summary>
    ''' <remarks></remarks>
    Dim _opacity As Integer = 255
#End Region

    ''' <summary>
    ''' null常数
    ''' </summary>
    ''' <remarks></remarks>
    Public Const null = Nothing

    ''' <summary>
    ''' 图像控件渐现动画绘制线程
    ''' </summary>
    ''' <remarks></remarks>
    Dim WithEvents TmrFadeInn As New Timers.Timer(1)
    ''' <summary>
    ''' 图像控件渐隐动画绘制线程
    ''' </summary>
    ''' <remarks></remarks>
    Dim WithEvents TmrFadeOut As New Timers.Timer(1)

#Region "控件用户自定义属性区域"
    Public ReadOnly Property RenderComplete As Boolean
        Get
            Return TmrFadeInn.Enabled AndAlso TmrFadeOut.Enabled
        End Get
    End Property

    Protected Friend Overloads Property Image As System.Drawing.Image
        Get
            Return _Bitmap
        End Get
        Set(value As System.Drawing.Image)
            If value Is Nothing Then
                Me.BorderStyle = Windows.Forms.BorderStyle.None
            Else
                _Bitmap = value
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
        Set(ByVal value As Integer)
            _opacity = value
            Call Me.Invalidate()
        End Set
    End Property
#End Region

    ''' <summary>
    ''' 启动控件的渐现动画的绘制线程
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FadeIn()
        tmrFadeInn.Enabled = True
        Call TmrFadeInn.Start()
    End Sub

    ''' <summary>
    ''' 启动控件的渐隐动画的绘制线程
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FadeOut()
        TmrFadeOut.Enabled = True
        Call TmrFadeOut.Start()
    End Sub

    Public Sub ChangeImage(Image As System.Drawing.Image)
        Dim bmp As New Drawing.Bitmap(Me.Size.Width, Me.Size.Height)
        Using gr = Graphics.FromImage(bmp)
            Call gr.DrawImage(Image, 0, 0, bmp.Width, bmp.Height)
        End Using
        Opacity = 0
        Me.Image = bmp
        Call FadeIn()
    End Sub

    Private Sub tmrFadeInn_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TmrFadeInn.Elapsed
        If Opacity + 5 < 255 Then Opacity += 5 Else Call TmrFadeInn.Stop()
    End Sub

    Private Sub tmrFadeOut_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TmrFadeOut.Elapsed
        If Opacity - 5 > 0 Then Opacity -= 5 Else Call TmrFadeOut.Stop()
    End Sub

    ''' <summary>
    ''' 复写了控件的绘制事件，仅当使用控件的Opacity属性更改透明度的时候，会调用Invalidate()过程，发生绘制事件
    ''' </summary>
    ''' <param name="pe"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnPaint(ByVal pe As System.Windows.Forms.PaintEventArgs)
        If [_Bitmap] Is null Then Return

        Dim BitmapBack = New Bitmap([_Bitmap].Width, [_Bitmap].Height)
        BitmapBack = [_Bitmap].Clone

        Dim [data] As BitmapData = BitmapBack.LockBits(New Rectangle(0, 0, [_Bitmap].Width, [_Bitmap].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim [nbyt] As Integer = data.Width * data.Height * 4
        Dim [rgbb](nbyt - 1) As Byte
        Dim [lptr] As IntPtr = [data].Scan0

        System.Runtime.InteropServices.Marshal.Copy(lptr, rgbb, 0, nbyt)

        For iLoop As Integer = 0 To nbyt - 1 Step 4
            If Not rgbb(iLoop + 3) = 0 Then rgbb(iLoop + 3) = _opacity
        Next

        System.Runtime.InteropServices.Marshal.Copy(rgbb, 0, [data].Scan0, rgbb.Length)
        BitmapBack.UnlockBits(data)
        pe.Graphics.DrawImage(BitmapBack, 0, 0, [_Bitmap].Width, [_Bitmap].Height)
    End Sub
End Class
