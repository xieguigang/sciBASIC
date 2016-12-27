#Region "Microsoft.VisualBasic::5386687f6a3b1945115dc69dea2fbb24, ..\sciBASIC#\CLI_tools\LicenseMgr\LicenseMgr\Content\SettingsAppearance.xaml.vb"

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

Imports System.Windows.Controls

Namespace Content
    ''' <summary>
    ''' Interaction logic for SettingsAppearance.xaml
    ''' </summary>
    Public Class SettingsAppearance
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            ' a simple view model for appearance configuration
            Me.DataContext = New SettingsAppearanceViewModel()
        End Sub
    End Class
End Namespace
