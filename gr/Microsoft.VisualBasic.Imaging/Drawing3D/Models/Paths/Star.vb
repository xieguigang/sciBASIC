#Region "Microsoft.VisualBasic::9e5a79a4de02f81bfde8fb54488d60b1, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Paths\Star.vb"

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

    '   Total Lines: 25
    '    Code Lines: 17 (68.00%)
    ' Comment Lines: 3 (12.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.00%)
    '     File Size: 765 B


    '     Class Star
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports stdNum = System.Math

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
                    (r * Cos(i * stdNum.PI / points)) + origin.X,
                    (r * Sin(i * stdNum.PI / points)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class
End Namespace
