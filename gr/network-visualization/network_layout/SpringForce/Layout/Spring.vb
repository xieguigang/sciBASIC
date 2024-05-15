#Region "Microsoft.VisualBasic::405ccc23ca8070c77f66e0889c306a29, gr\network-visualization\network_layout\SpringForce\Layout\Spring.vb"

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

    '   Total Lines: 93
    '    Code Lines: 41
    ' Comment Lines: 38
    '   Blank Lines: 14
    '     File Size: 2.92 KB


    '     Class Spring
    ' 
    '         Properties: A, B, K, length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class NearestPoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class BoundingBox
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Spring.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Spring Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the Spring Class.
'
'

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace SpringForce

    Public Class Spring

        Public Property A As LayoutPoint
        Public Property B As LayoutPoint
        Public Property length As Double
        Public Property K As Double

        Public Sub New(point1 As LayoutPoint, point2 As LayoutPoint, length As Double, K As Double)
            Me.A = point1
            Me.B = point2
            Me.length = length
            Me.K = K
        End Sub

        Public Overrides Function ToString() As String
            Return $"{A.node.ToString} ~ {B.node.ToString}"
        End Function
    End Class

    Public Class NearestPoint

        Public node As Node
        Public point As LayoutPoint
        Public distance As Double?

        Public Sub New()
            node = Nothing
            point = Nothing
            distance = Nothing
        End Sub

        Public Overrides Function ToString() As String
            Return $"{point} -> {node}"
        End Function
    End Class

    Public Class BoundingBox

        ' ~5% padding
        Public Const defaultBB As Double = 2.0F
        Public Const defaultPadding As Double = 0.07F

        Public topRightBack As AbstractVector
        Public bottomLeftFront As AbstractVector

        Public Sub New()
            topRightBack = Nothing
            bottomLeftFront = Nothing
        End Sub
    End Class
End Namespace
