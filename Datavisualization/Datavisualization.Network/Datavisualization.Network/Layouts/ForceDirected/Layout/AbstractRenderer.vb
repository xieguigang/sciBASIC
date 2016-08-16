#Region "Microsoft.VisualBasic::f68b49b8ca8209aad94b01ef589bdb0f, ..\VisualBasic_AppFramework\Datavisualization\Datavisualization.Network\Datavisualization.Network\Layouts\ForceDirected\Layout\AbstractRenderer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Parallel

Namespace Layouts

    Public MustInherit Class AbstractRenderer
        Implements IRenderer

        ''' <summary>
        ''' Running the drawing task in another thread?
        ''' </summary>
        ''' <returns></returns>
        Public Property Asynchronous As Boolean = False

        Protected forceDirected As IForceDirected

        Public Sub New(iForceDirected As IForceDirected)
            forceDirected = iForceDirected
        End Sub

        Dim taskQueue As New ThreadQueue

        Public Sub Draw(iTimeStep As Single) Implements IRenderer.Draw
            Call forceDirected.Calculate(iTimeStep)   '  计算力的变化
            Call Clear()    ' 清理画板

            If Asynchronous Then
                Call taskQueue.AddToQueue(AddressOf DirectDraw)
            Else
                Call DirectDraw()
            End If
        End Sub

        ''' <summary>
        ''' 不计算位置而直接更新绘图
        ''' </summary>
        Public Overridable Sub DirectDraw()
            forceDirected.EachEdge(Sub(edge As Edge, spring As Spring) drawEdge(edge, spring.point1.position, spring.point2.position))
            forceDirected.EachNode(Sub(node As Node, point As LayoutPoint) drawNode(node, point.position))
        End Sub

        Public MustOverride Sub Clear() Implements IRenderer.Clear
        Protected MustOverride Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Protected MustOverride Sub drawNode(iNode As Node, iPosition As AbstractVector)

    End Class
End Namespace
