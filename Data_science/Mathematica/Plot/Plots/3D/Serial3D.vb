#Region "Microsoft.VisualBasic::68db3fb50a2750175cb16cb97c40def0, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Serial3D.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    ''' <summary>
    ''' Scatter serial data in 3D
    ''' </summary>
    Public Class Serial3D

        Public Property Title As String
        Public Property Color As Color
        Public Property Shape As LegendStyles
        Public Property Points As Point3D()
        Public Property PointSize As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Shape.ToString}, {Color.ToString}] {Title}"
        End Function
    End Class
End Namespace
