#Region "Microsoft.VisualBasic::679aef2947f847067d335a68da1368fb, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\ControlComponent.vb"

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

Imports System.Runtime.CompilerServices

Public Module ControlComponent

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Hook"></param>
    ''' <param name="InputHandle"></param>
    ''' <param name="Location"></param>
    ''' <param name="Width"></param>
    ''' <param name="DefaultValue">默认值，当用户没有输入任何字符串直接点击确定的时候，返回这个预设值</param>
    ''' <remarks></remarks>
    <Extension> Public Sub InputBoxShowDialog(Hook As Control, InputHandle As Action(Of String), Location As Point, Optional Width As Integer = -1, Optional DefaultValue As String = "")
        Call MolkPlusTheme.UserInputControl.Input(Hook, InputHandle, Location, Width, DefaultValue)
    End Sub

    <Extension> Public Sub DrawBorder(ByRef Control As Control, Color As Color)
        Using Gr As Graphics = Graphics.FromHwnd(Control.Handle)
            Dim Rect = New Rectangle(New Point(1, 1), New Size(Control.Width - 2, Control.Height - 2))
            Call Gr.DrawRectangle(New Pen(Color, 1), Rect)
        End Using
    End Sub
End Module

Public Delegate Sub InvokeHandle()
