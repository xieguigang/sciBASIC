#Region "Microsoft.VisualBasic::f0da6460ad27eb002707f419ee229263, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\Layout\Point.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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

Namespace Layouts

    ''' <summary>
    ''' The position of the node object in the fdg data model.
    ''' </summary>
    Public Class LayoutPoint

        Public Property position As AbstractVector
        Public Property node As Node
        Public Property mass As Single
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
            If DirectCast(p, Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return position Is p.position
        End Function

        Public Shared Operator =(a As LayoutPoint, b As LayoutPoint) As Boolean
            ' If both are null, or both are same instance, return true.
            If System.[Object].ReferenceEquals(a, b) Then
                Return True
            End If

            ' If one is null, but not both, return false.
            If (DirectCast(a, Object) Is Nothing) OrElse (DirectCast(b, Object) Is Nothing) Then
                Return False
            End If

            ' Return true if the fields match:
            Return (a.position = b.position)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As LayoutPoint, b As LayoutPoint) As Boolean
            Return Not (a = b)
        End Operator

        Public Sub ApplyForce(force As AbstractVector)
            acceleration.Add(force / mass)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
