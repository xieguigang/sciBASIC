#Region "Microsoft.VisualBasic::57dc445f71767694511cb9c312d6aff5, ..\sciBASIC#\gr\Datavisualization.Network\EpForceDirectedGraphDemo\DoubleBufferPanel.vb"

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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Namespace EpForceDirectedGraphDemo
	Class DoubleBufferPanel
		Inherits Panel

		Public Sub New()
			MyBase.New()






			Me.DoubleBuffered = True



			Me.UpdateStyles()
		End Sub


	End Class

End Namespace
