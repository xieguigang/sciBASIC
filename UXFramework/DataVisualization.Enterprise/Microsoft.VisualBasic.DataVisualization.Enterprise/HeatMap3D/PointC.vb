#Region "Microsoft.VisualBasic::f59132f30b668e1df068995de5c67f77, ..\visualbasic_App\UXFramework\DataVisualization.Enterprise\Microsoft.VisualBasic.DataVisualization.Enterprise\HeatMap3D\PointC.vb"

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
Imports System.Threading.Tasks
Imports System.Drawing


Namespace Gradiant3D
	Friend Class PointC
		Public pointf As New PointF()
		Public C As Single = 0
		Public ARGBArray As Integer() = New Integer(3) {}

		Public Sub New()
		End Sub

		Public Sub New(ptf As PointF, c__1 As Single)
			pointf = ptf
			C = c__1
		End Sub

		Public Sub New(ptf As PointF, c__1 As Single, argbArray__2 As Integer())
			pointf = ptf
			C = c__1
			ARGBArray = argbArray__2
		End Sub
	End Class
End Namespace
