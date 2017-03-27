#Region "Microsoft.VisualBasic::2129c9ed25d42ead296432c805cc3873, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\GraphicsRegion.vb"

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
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D

    ''' <summary>
    ''' 绘图区域的参数
    ''' </summary>
    Public Structure GraphicsRegion

        ''' <summary>
        ''' 整张画布的大小
        ''' </summary>
        Public Size As Size
        ''' <summary>
        ''' 画布的边留白
        ''' </summary>
        Public Padding As Padding

        Public ReadOnly Property Bottom As Integer
            Get
                Return Size.Height - Padding.Bottom
            End Get
        End Property

        ''' <summary>
        ''' 整张画布出去margin部分剩余的可供绘图的区域
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PlotRegion As Rectangle
            Get
                Dim topLeft As New Point(Padding.Left, Padding.Top)
                Dim size As New Size(
                    Me.Size.Width - Padding.Horizontal,
                    Me.Size.Height - Padding.Vertical)

                Return New Rectangle(topLeft, size)
            End Get
        End Property

        ''' <summary>
        ''' 整张画布的大小区域
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntireArea As Rectangle
            Get
                Return New Rectangle(New Point, Size)
            End Get
        End Property

        Public Function TopCentra(size As Size) As Point
            Dim left = (Me.Size.Width - size.Width) / 2
            Dim top = (Padding.Top - size.Height) / 2
            Return New Point(left, top)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
