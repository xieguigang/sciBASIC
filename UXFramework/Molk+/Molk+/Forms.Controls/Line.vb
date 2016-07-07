#Region "Microsoft.VisualBasic::0f41bbe988161935c6305f4077ea3ec9, ..\VisualBasic_AppFramework\UXFramework\Molk+\Molk+\Forms.Controls\Line.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.ComponentModel
Imports Microsoft.VisualBasic.Imaging

Public Class Line
    Private Sub Line_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Line_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Width <= 0 OrElse Height <= 0 Then
            Return
        End If

        Dim Gr = Me.Size.CreateGDIDevice(BackColor)

        If Me.Size.Width >= Me.Size.Height Then
            '画横线
            Call Gr.Graphics.DrawLine(LinePen, New Point(0, Height / 2), New Point(Width, Height / 2))

        Else
            '画数显

            Call Gr.Graphics.DrawLine(LinePen, New Point(Width / 2, 0), New Point(Width / 2, Height))

        End If

        Me.BackgroundImage = Gr.ImageResource
    End Sub

    <DefaultValue(1.5)>
    Public Property PenWidth As Single
        Get
            If LinePen Is Nothing Then
                Return 1
            Else
                Return LinePen.Width
            End If
        End Get
        Set(value As Single)
            LinePen = New Pen(New SolidBrush(ForeColor), value)
            Call Line_Resize(Nothing, Nothing)
        End Set
    End Property

    Dim _LinePen As New Pen(New SolidBrush(ForeColor), 2)

    Public Property LinePen As Pen
        Get
            Return _LinePen
        End Get
        Set(value As Pen)
            _LinePen = value
            Call Line_Resize(Nothing, Nothing)
        End Set
    End Property
End Class

