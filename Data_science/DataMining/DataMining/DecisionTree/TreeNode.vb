#Region "Microsoft.VisualBasic::206c360f4227160dbca004c6ee221862, Data_science\DataMining\DataMining\DecisionTree\TreeNode.vb"

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

    '   Total Lines: 37
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.09 KB


    '     Class TreeNode
    ' 
    '         Properties: attributes, childNodes, edge, index, isLeaf
    '                     name
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DecisionTree

    Public Class TreeNode

        Public Property name As String
        Public Property edge As String
        Public Property attributes As Attributes
        Public Property childNodes As List(Of TreeNode)
        Public Property index As Integer
        Public Property isLeaf As Boolean

        Public Sub New(name As String, tableIndex As Integer, attributes As Attributes, edge As String)
            Me.name = name
            Me.index = tableIndex
            Me.attributes = attributes
            Me.childNodes = New List(Of TreeNode)()
            Me.edge = edge
        End Sub

        Public Sub New(isleaf As Boolean, name As String, edge As String)
            Me.isLeaf = isleaf
            Me.name = name
            Me.edge = edge
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            If isLeaf Then
                Return $"[{name}]"
            Else
                Return name
            End If
        End Function
    End Class
End Namespace
