#Region "Microsoft.VisualBasic::29d245a92611b606e08f0c735f70ca31, gr\network-visualization\network_layout\Cola\Layout3D\Link3D.vb"

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

    '   Total Lines: 27
    '    Code Lines: 21 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 800 B


    '     Class Link3D
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: actualLength
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports stdNum = System.Math

Namespace Cola

    Public Class Link3D

        Public length As Double
        Public source As Integer
        Public target As Integer

        Sub New()
        End Sub

        Public Sub New(source As Integer, target As Integer)
            Me.source = source
            Me.target = target
        End Sub

        Public Function actualLength(x As Double()()) As Double
            Return stdNum.Sqrt(x.Reduce(Function(c As Double, v As Double())
                                          Dim dx = v(Me.target) - v(Me.source)
                                          Return c + dx * dx
                                      End Function, 0))
        End Function
    End Class
End Namespace
