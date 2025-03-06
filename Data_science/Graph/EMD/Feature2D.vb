#Region "Microsoft.VisualBasic::ccf9da8d99526842c9793969cf7f7866, Data_science\Graph\EMD\Feature2D.vb"

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

    '   Total Lines: 30
    '    Code Lines: 21 (70.00%)
    ' Comment Lines: 4 (13.33%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 825 B


    '     Class Feature2D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: groundDist, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace EMD
    ''' <summary>
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' 
    ''' </summary>
    Public Class Feature2D
        Implements Feature

        Private x As Double
        Private y As Double

        Public Sub New(x As Double, y As Double)
            Me.x = x
            Me.y = y
        End Sub

        Public Overridable Function groundDist(f As Feature) As Double Implements Feature.groundDist
            Dim f2d = CType(f, Feature2D)
            Dim deltaX = x - f2d.x
            Dim deltaY = y - f2d.y
            Return std.Sqrt(deltaX * deltaX + deltaY * deltaY)
        End Function

        Public Overrides Function ToString() As String
            Return $"({x}, {y})"
        End Function
    End Class
End Namespace
