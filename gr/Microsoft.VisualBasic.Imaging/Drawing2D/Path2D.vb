#Region "Microsoft.VisualBasic::3cfb5b954a89bec17c6bd5f52f0b6e4d, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Path2D.vb"

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
Imports System.Drawing.Drawing2D

Namespace Drawing2D

    Public Class Path2D

        Public ReadOnly Property Path As New GraphicsPath

        Dim last As PointF

        Public Sub MoveTo(x!, y!)
            last = New PointF(x, y)
        End Sub

        Public Sub LineTo(x!, y!)
            Dim p2 As New PointF(x, y)
            Call Path.AddLine(last, p2)
            last = p2
        End Sub

        Public Sub Rewind()
            Call Path.Reset()
        End Sub

        Public Sub CloseAllFigures()
            Call Path.CloseAllFigures()
        End Sub
    End Class
End Namespace
