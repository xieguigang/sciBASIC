#Region "Microsoft.VisualBasic::0cc9eaaa6358c600cb15106fa2096e9b, Data_science\DataMining\DBNCode\utils\TreeNode.vb"

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

    '   Total Lines: 112
    '    Code Lines: 84 (75.00%)
    ' Comment Lines: 5 (4.46%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (20.54%)
    '     File Size: 3.56 KB


    '     Class TreeNode
    ' 
    '         Properties: Children, Data, Leaf, Level, Root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: addChild, deleteUp, ToString
    ' 
    '         Sub: addChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports std = System.Math

Namespace utils

    Public Class TreeNode(Of T)

        Private dataField As T

        Private childrenField As IList(Of TreeNode(Of T))

        Private parent As TreeNode(Of T)

        Public Sub New(data As T)
            dataField = data
            childrenField = New List(Of TreeNode(Of T))()
        End Sub

        Public Overridable Function addChild(childData As T) As TreeNode(Of T)
            Dim childNode As TreeNode(Of T) = New TreeNode(Of T)(childData)
            childNode.parent = Me
            childrenField.Add(childNode)
            Return childNode
        End Function

        Public Overridable Sub addChild(childNode As TreeNode(Of T))
            childNode.parent = Me
            childrenField.Add(childNode)
        End Sub

        ''' <summary>
        ''' target must be a leaf 
        ''' </summary>
        ''' <param name="roots"></param>
        ''' <returns></returns>
        Public Overridable Function deleteUp(roots As IDictionary(Of T, TreeNode(Of T))) As IList(Of TreeNode(Of T))

            If Not Leaf Then
                Throw New ArgumentException(ToString() & " is not a leaf node.")
            End If
            Dim orphanChildren As IList(Of TreeNode(Of T)) = New List(Of TreeNode(Of T))()
            Dim parent As TreeNode(Of T) = Me.parent
            Dim child As TreeNode(Of T) = Me
            While parent IsNot Nothing
                Dim iter As IEnumerator(Of TreeNode(Of T)) = parent.childrenField.GetEnumerator()

                While iter.MoveNext()
                    Dim otherChild As TreeNode(Of T) = iter.Current
                    If otherChild IsNot child Then
                        otherChild.parent = Nothing
                        orphanChildren.Add(otherChild)
                    End If
                End While
                child = parent
                parent = parent.parent
            End While
            roots.Remove(child.Data)
            Return orphanChildren
        End Function

        Public Overridable ReadOnly Property Data As T
            Get
                Return dataField
            End Get
        End Property

        Public Overridable ReadOnly Property Children As IList(Of TreeNode(Of T))
            Get
                Return childrenField
            End Get
        End Property

        Public Overridable ReadOnly Property Root As Boolean
            Get
                Return parent Is Nothing
            End Get
        End Property

        Public Overridable ReadOnly Property Leaf As Boolean
            Get
                Return childrenField.Count = 0
            End Get
        End Property

        Public Overridable ReadOnly Property Level As Integer
            Get
                Return If(Root, 0, parent.Level + 1)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            Dim ls = ","
            Dim level = Me.Level

            Dim i = level

            While std.Max(Threading.Interlocked.Decrement(i), i + 1) > 0
                sb.Append(vbTab)
            End While
            sb.Append("-- " & Data.ToString() & ls)

            For Each childNode As TreeNode(Of T) In childrenField
                sb.Append(childNode)
            Next

            Return sb.ToString()
        End Function

    End Class

End Namespace

