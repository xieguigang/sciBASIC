#Region "Microsoft.VisualBasic::e81fcfa9dcbd52e109a5f4ee1bf4f76c, Data_science\Graph\Model\Tree.vb"

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

' Class Tree
' 
'     Properties: Data
' 
'     Constructor: (+1 Overloads) Sub New
' 
' Class AbstractTree
' 
'     Properties: Childs, Count, IsLeaf, IsRoot, Parent
'                 QualifyName
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: (+2 Overloads) CountLeafs, EnumerateChilds, ToString
' 
' /********************************************************************************/

#End Region

''' <summary>
''' Tree node with data..(可以直接被使用的树对象类型)
''' </summary>
''' <typeparam name="T"></typeparam>
Public Class Tree(Of T, K) : Inherits AbstractTree(Of Tree(Of T, K), K)

    Public Property Data As T

    Sub New(Optional qualDeli$ = ".")
        MyBase.New(qualDeli)
    End Sub
End Class

''' <summary>
''' 使用字符串<see cref="String"/>作为键名的树节点
''' </summary>
''' <typeparam name="T"></typeparam>
Public Class Tree(Of T) : Inherits Tree(Of T, String)
End Class