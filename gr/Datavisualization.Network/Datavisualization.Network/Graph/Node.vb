#Region "Microsoft.VisualBasic::1eeedebee7d0ef2488f7ba865abb398b, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\LDM\Graph\Node.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
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
'@file Node.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Node Interface
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
'An Interface for the Node Class.
'
'

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Graph

    Public Class Node : Inherits ClassObject
        Implements INamedValue

        ''' <summary>
        ''' 在这里是用的是unique id进行初始化，对于Display title则可以在<see cref="NodeData.label"/>属性上面设置
        ''' </summary>
        ''' <param name="iId"></param>
        ''' <param name="iData"></param>
        Public Sub New(iId As String, Optional iData As NodeData = Nothing)
            If iData IsNot Nothing Then
                Data = iData.Clone
            End If

            ID = iId
            Pinned = False
        End Sub

        Sub New()
            Call Me.New(Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' The unique id of this node
        ''' </summary>
        ''' <returns></returns>
        Public Property ID As String Implements INamedValue.Key
        Public Property Data As NodeData
        Public Property Pinned As Boolean

        Public Overrides Function GetHashCode() As Integer
            Return ID.GetHashCode()
        End Function

        Public Overrides Function ToString() As String
            Return ID
        End Function

        Public Overrides Function Equals(obj As System.Object) As Boolean
            ' If parameter is null return false.
            If obj Is Nothing Then
                Return False
            End If

            ' If parameter cannot be cast to Point return false.
            Dim p As Node = TryCast(obj, Node)
            If DirectCast(p, System.Object) Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (ID = p.ID)
        End Function

        Public Overloads Function Equals(p As Node) As Boolean
            ' If parameter is null return false:
            If p Is Nothing Then
                Return False
            End If

            ' Return true if the fields match:
            Return (ID = p.ID)
        End Function

        Public Shared Operator =(a As Node, b As Node) As Boolean
            ' If one is null, but not both, return false.
            If a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            ' If both are null, or both are same instance, return true.
            If Object.ReferenceEquals(a, b) Then
                Return True
            End If

            ' Return true if the fields match:
            Return a.ID = b.ID
        End Operator

        Public Shared Operator <>(a As Node, b As Node) As Boolean
            Return Not (a = b)
        End Operator
    End Class
End Namespace
