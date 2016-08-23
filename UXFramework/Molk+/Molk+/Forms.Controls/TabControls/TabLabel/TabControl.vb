#Region "Microsoft.VisualBasic::c4616904355180f9cd95408d67146df7, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\TabLabel\TabControl.vb"

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

Imports System.ComponentModel

Namespace Windows.Forms.Controls.TabControl.TabLabel

    Public Class TabControl : Inherits Windows.Forms.Controls.TabControl.ITabControl(Of Windows.Forms.Controls.TabControl.TabLabel.TabLabel)

        Dim _TabLabelWidth As Integer

        <DefaultValue(30)>
        Public Property TabLabelWidth As Integer
            Get
                Return Me._TabLabelWidth
            End Get
            Set(value As Integer)
                Me._TabLabelWidth = value
            End Set
        End Property

        Public Overrides Sub AddTabPage(Name As String, Control As Control, Optional TabCloseEventHandle As Action = Nothing)
            Dim Tab = MyBase.InternalAddTabPage(Name, Control, TabCloseEventHandle)
            Dim NewPanel = Tab._InternalTabPageControlItem

            NewPanel.Size = New Size With {.Width = Width - _TabLabelWidth - 5, .Height = Height}
            NewPanel.Location = New Point With {.X = _TabLabelWidth + 5, .Y = 0}
            NewPanel.BringToFront()

            Tab.Size = New Size With {.Width = _TabLabelWidth, .Height = Tab.Height}
            Tab.Location = New Point With {.X = 0, .Y = Tab.Height * (MyBase._Tabs - 1) + 5}

            Call Me.ActiveTabPage(Name)
        End Sub
    End Class
End Namespace
