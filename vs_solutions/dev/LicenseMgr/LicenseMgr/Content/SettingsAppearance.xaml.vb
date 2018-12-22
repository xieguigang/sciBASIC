#Region "Microsoft.VisualBasic::e15718872eca7d47151e4da3bb91d1c2, vs_solutions\dev\LicenseMgr\LicenseMgr\Content\SettingsAppearance.xaml.vb"

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

    '     Class SettingsAppearance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

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
