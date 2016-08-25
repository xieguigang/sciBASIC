#Region "Microsoft.VisualBasic::16d5c8abf379d852a3e72fb84f01914a, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\BusyLoader\AjaxLoaderSquaresCircles.vb"

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

Public Class AjaxLoaderSquaresCircles

    Protected Shared ReadOnly Resource As Image() = New Image() {My.Resources._1, My.Resources._2, My.Resources._3, My.Resources._4, My.Resources._5, My.Resources._6, My.Resources._7, My.Resources._8}

    Dim i As Integer
    Dim _InternalResource As Image()

    Private Sub AjaxLoaderSquaresCircles_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BackColor = BackColor
        Enabled = True
    End Sub

    Public Shadows Property Enabled As Boolean
        Get
            Return TimerRunningLoader.Enabled
        End Get
        Set(value As Boolean)
            TimerRunningLoader.Enabled = value
        End Set
    End Property

    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            MyBase.BackColor = value
            _InternalResource = (From res As Image In AjaxLoaderSquaresCircles.Resource Select InternalTransparentImage(res)).ToArray
        End Set
    End Property

    Private Function InternalTransparentImage(res As Image) As Image
        Dim Bitmap As Bitmap = New Bitmap(DirectCast(res.Clone, Image))

        For i As Integer = 0 To Bitmap.Width - 1
            For j As Integer = 0 To Bitmap.Height - 1
                Dim Cl As Color = Bitmap.GetPixel(i, j)
                If 255 - Cl.R < 5 AndAlso 255 - Cl.G < 5 AndAlso 255 - Cl.B < 5 Then
                    Call Bitmap.SetPixel(i, j, BackColor)
                End If
            Next
        Next

        Return Bitmap
    End Function

    Private Function InternalGetResource() As Image
        If i < Resource.Length Then
            Dim j As Integer = i
            i += 1
            Return _InternalResource(j)
        Else
            i = 1
            Return _InternalResource(0)
        End If
    End Function

    Private Sub TimerRunningLoader_Tick(sender As Object, e As EventArgs) Handles TimerRunningLoader.Tick
        Me.PictureBox1.BackgroundImage = InternalGetResource()
    End Sub
End Class
