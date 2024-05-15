#Region "Microsoft.VisualBasic::9840900542e2a39450c20adba6db31b7, gr\network-visualization\network_layout\SpringForce\Layout\AbstractRenderer.vb"

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

    '   Total Lines: 88
    '    Code Lines: 30
    ' Comment Lines: 47
    '   Blank Lines: 11
    '     File Size: 3.33 KB


    '     Class AbstractRenderer
    ' 
    '         Properties: PhysicsEngine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DirectDraw, Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file AbstractRenderer.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Abstract Renderer Interface
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
'An Interface for the Abstract Renderer Class.
'
'

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces

Namespace SpringForce

    Public MustInherit Class AbstractRenderer
        Implements IRenderer

        Public ReadOnly Property PhysicsEngine As IForceDirected
            Get
                Return forceDirected
            End Get
        End Property

        Protected forceDirected As IForceDirected

        Public Sub New(forceDirected As IForceDirected)
            Me.forceDirected = forceDirected
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="iTimeStep"><see cref="IForceDirected.Collide(Double)"/></param>
        ''' <param name="physicsUpdate"></param>
        Public Sub Draw(iTimeStep As Double, Optional physicsUpdate As Boolean = True) Implements IRenderer.Draw
            If physicsUpdate Then
                ' 计算力的变化
                Call forceDirected.Collide(iTimeStep)
            End If

            ' 清理画板
            Call Clear()
            Call DirectDraw()
        End Sub

        ''' <summary>
        ''' 不计算位置而直接更新绘图
        ''' </summary>
        Public Overridable Sub DirectDraw()
            forceDirected.EachEdge(Sub(edge As Edge, spring As Spring) drawEdge(edge, spring.A.position, spring.B.position))
            forceDirected.EachNode(Sub(node As Node, point As LayoutPoint) drawNode(node, point.position))
        End Sub

        Public MustOverride Sub Clear() Implements IRenderer.Clear
        Protected MustOverride Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Protected MustOverride Sub drawNode(iNode As Node, iPosition As AbstractVector)

    End Class
End Namespace
