#Region "Microsoft.VisualBasic::af217e9a43c8c194e99aca74c99412b0, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Path2D.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
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



' /********************************************************************************/

' Summaries:

'     Class Path2D
' 
'         Properties: Path
' 
'         Sub: CloseAllFigures, (+2 Overloads) LineTo, (+2 Overloads) MoveTo, Rewind
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices

Namespace Drawing2D

    ''' <summary>
    ''' 通过模拟HTML之中的svg path的绘图操作来将html svg path转换为gdi+ path对象
    ''' </summary>
    Public Class Path2D

        Public ReadOnly Property Path As New GraphicsPath

        Dim last As PointF

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub MoveTo(x!, y!)
            last = New PointF(x, y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub MoveTo(location As PointF)
            last = location
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub LineTo(x!, y!)
            Call LineTo(New PointF(x, y))
        End Sub

        Public Sub LineTo(location As PointF)
            Call Path.AddLine(last, location)
            last = location
        End Sub

        ''' <summary>
        ''' 重置
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Rewind()
            Call Path.Reset()
        End Sub

        ''' <summary>
        ''' 关闭所有打开的数字，在此路径，并开始一个新图形。 通过将行从其终结点连接到其起始点，则关闭每个打开的图形。
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub CloseAllFigures()
            Call Path.CloseAllFigures()
        End Sub

        Public Overrides Function ToString() As String
            Return Path.ToString
        End Function
    End Class
End Namespace
