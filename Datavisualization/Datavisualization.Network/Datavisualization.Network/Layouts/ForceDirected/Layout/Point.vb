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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Layouts

    ''' <summary>
    ''' The position of the node object in the fdg data model.
    ''' </summary>
    Public Class LayoutPoint

        Public Sub New(iPosition As AbstractVector, iVelocity As AbstractVector, iAcceleration As AbstractVector, iNode As Node)
            position = iPosition
            node = iNode
            velocity = iVelocity
            acceleration = iAcceleration
        End Sub

        Public Overrides Function GetHashCode() As Integer
            Return position.GetHashCode()
        End Function

        Public Overrides Function Equals(obj As System.Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            End If

            ' If parameter cannot be cast to Point return false.
            Dim p As LayoutPoint = TryCast(obj, LayoutPoint)
            If DirectCast(p, System.Object) Is Nothing Then
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

        Public Shared Operator <>(a As LayoutPoint, b As LayoutPoint) As Boolean
            Return Not (a = b)
        End Operator

        Public Sub ApplyForce(force As AbstractVector)
            acceleration.Add(force / mass)
        End Sub

        Public Property position() As AbstractVector
        Public Property node() As Node
        Public Property mass() As Single
            Get
                Return node.Data.mass
            End Get
            Private Set
                node.Data.mass = Value
            End Set
        End Property

        Public Property velocity() As AbstractVector
        Public Property acceleration() As AbstractVector

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace