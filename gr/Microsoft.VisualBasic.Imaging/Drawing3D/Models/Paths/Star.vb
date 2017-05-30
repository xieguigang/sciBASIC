#Region "Microsoft.VisualBasic::1dd9d7673a4bd6a9ef54645e8b6d8216, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Paths\Star.vb"

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

Namespace Drawing3D.Models.Isometric.Paths


    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Star : Inherits Path3D

        Public Sub New(origin As Point3D, outerRadius#, innerRadius#, points%)
            MyBase.New()

            For i As Integer = 0 To points * 2 - 1
                Dim r As Double = If(i Mod 2 = 0, outerRadius, innerRadius)
                Dim p As New Point3D(
                    (r * Math.Cos(i * Math.PI / points)) + origin.X,
                    (r * Math.Sin(i * Math.PI / points)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class
End Namespace
