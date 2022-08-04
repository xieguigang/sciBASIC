#Region "Microsoft.VisualBasic::3c1effd093ee81592b4e63f356501f34, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\SpringForce\Layout\Point.vb"

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

    '   Total Lines: 131
    '    Code Lines: 64
    ' Comment Lines: 48
    '   Blank Lines: 19
    '     File Size: 4.64 KB


    '     Class LayoutPoint
    ' 
    '         Properties: acceleration, mass, node, position, velocity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Equals, GetHashCode, ToString
    ' 
    '         Sub: ApplyForce
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Point.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Point Interface
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
'An Interface for the Point Class.
'
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts.SpringForce

    ''' <summary>
    ''' The position of the node object in the fdg data model.
    ''' </summary>
    Public Class LayoutPoint

        Public Property position As AbstractVector
        Public Property node As Node
        Public Property mass As Double
            Get
                Return node.data.mass
            End Get
            Private Set
                node.data.mass = Value
            End Set
        End Property

        Public Property velocity As AbstractVector
        Public Property acceleration As AbstractVector

        Public Sub New(position As AbstractVector, velocity As AbstractVector, acceleration As AbstractVector, node As Node)
            Me.position = position
            Me.node = node
            Me.velocity = velocity
            Me.acceleration = acceleration
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return position.GetHashCode()
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            End If

            ' If parameter cannot be cast to Point return false.
            Dim p As LayoutPoint = TryCast(obj, LayoutPoint)

            If DirectCast(p, Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return position Is p.position
        End Function

        Public Overloads Function Equals(p As LayoutPoint) As Boolean
            ' If parameter is null return false:
            If p Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return position Is p.position
        End Function

        Public Shared Operator =(a As LayoutPoint, b As LayoutPoint) As Boolean
            ' If both are null, or both are same instance, return true.
            If a Is b Then
                Return True
            End If

            ' If one is null, but not both, return false.
            If a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (a.position = b.position)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As LayoutPoint, b As LayoutPoint) As Boolean
            Return Not (a = b)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub ApplyForce(force As AbstractVector)
            Call acceleration.Add(force / mass)
        End Sub

        Public Overrides Function ToString() As String
            Return $"({If(node.pinned, "pinned", "unpinned")}){node} [{position.x.ToString("F0")}, {position.y.ToString("F0")}, {position.z.ToString("F0")}]"
        End Function
    End Class
End Namespace
