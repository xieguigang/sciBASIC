'! 
'@file PhysicsData.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief PhysicsData Interface
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
'An Interface for the PhysicsData Class.
'
'

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Layouts.Graph

    Public Class NodeData
        Inherits GraphData
        Public Sub New()
            MyBase.New()
            mass = 1.0F
            initialPostion = Nothing
            ' for merging the graph
            origID = ""
        End Sub
        Public Property mass() As Single
            Get
                Return m_mass
            End Get
            Set
                m_mass = Value
            End Set
        End Property
        Private m_mass As Single

        Public Property initialPostion() As AbstractVector
            Get
                Return m_initialPostion
            End Get
            Set
                m_initialPostion = Value
            End Set
        End Property
        Private m_initialPostion As AbstractVector
        Public Property origID() As String
            Get
                Return m_origID
            End Get
            Set
                m_origID = Value
            End Set
        End Property
        Private m_origID As String

    End Class
    Public Class EdgeData
        Inherits GraphData
        Public Sub New()
            MyBase.New()
            length = 1.0F
        End Sub
        Public Property length() As Single
            Get
                Return m_length
            End Get
            Set
                m_length = Value
            End Set
        End Property
        Private m_length As Single
    End Class
    Public Class GraphData
        Public Sub New()
            label = ""
        End Sub


        Public Property label() As String
            Get
                Return m_label
            End Get
            Set
                m_label = Value
            End Set
        End Property
        Private m_label As String


    End Class
End Namespace