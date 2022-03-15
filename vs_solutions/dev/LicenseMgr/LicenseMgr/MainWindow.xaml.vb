#Region "Microsoft.VisualBasic::19cb1e767ff1e798f4b608f4f4e83f72, sciBASIC#\vs_solutions\dev\LicenseMgr\LicenseMgr\MainWindow.xaml.vb"

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


    ' Code Statistics:

    '   Total Lines: 18
    '    Code Lines: 9
    ' Comment Lines: 7
    '   Blank Lines: 2
    '     File Size: 495.00 B


    ' Class MainWindow
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: ModernWindow_Initialized
    ' 
    ' /********************************************************************************/

#End Region

Imports FirstFloor.ModernUI.Windows.Controls
'Imports Microsoft.VisualBasic.Windows.Forms

''' <summary>
''' Interaction logic for MainWindow.xaml
''' </summary>
Partial Public Class MainWindow
    Inherits ModernWindow
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ModernWindow_Initialized(sender As Object, e As EventArgs)
        '  If VistaSecurity.IsAdmin() Then
        '  Me.Title += " (Elevated)"
        '  End If
    End Sub
End Class
