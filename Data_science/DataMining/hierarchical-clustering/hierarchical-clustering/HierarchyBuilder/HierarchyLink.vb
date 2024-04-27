﻿#Region "Microsoft.VisualBasic::5ee29fb5759c35da6ddb646ec1a6f0c8, G:/GCModeller/src/runtime/sciBASIC#/Data_science/DataMining/hierarchical-clustering/hierarchical-clustering//HierarchyBuilder/HierarchyLink.vb"

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

    '   Total Lines: 64
    '    Code Lines: 47
    ' Comment Lines: 4
    '   Blank Lines: 13
    '     File Size: 2.20 KB


    '     Class HierarchyLink
    ' 
    '         Properties: HashKey
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __compareTo, compareTo, LessThan, ToString
    ' 
    '     Module LinkHashCode
    ' 
    '         Function: (+2 Overloads) hashCodePair
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Hierarchy

    Public Class HierarchyLink : Implements IComparable, IComparable(Of HierarchyLink), IReadOnlyId

        Public ReadOnly Tree As HierarchyTreeNode
        Public ReadOnly Property HashKey As String Implements IReadOnlyId.Identity

        Friend removed As Boolean = False

        Sub New(p As HierarchyTreeNode)
            Tree = p
            HashKey = p.hashCodePair
        End Sub

        Public Function compareTo(o As HierarchyLink) As Integer Implements IComparable(Of HierarchyLink).CompareTo
            Return Tree.compareTo(o.Tree)
        End Function

        Public Overrides Function ToString() As String
            Return HashKey
        End Function

        Private Function __compareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return compareTo(obj)
        End Function

        Public Shared Function LessThan() As Func(Of HierarchyLink, HierarchyLink, Boolean)
            Return Function(a, b)
                       If a.compareTo(b) < 0 Then
                           Return True
                       Else
                           Return False
                       End If
                   End Function
        End Function
    End Class

    Module LinkHashCode

        ''' <summary>
        ''' Compute some kind of unique ID for a given cluster pair. </summary>
        ''' <returns> The ID </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function hashCodePair(link As HierarchyTreeNode) As String
            Return hashCodePair(link.Left(), link.Right())
        End Function

        Public Function hashCodePair(lCluster As Cluster, rCluster As Cluster) As String
            Dim lName = lCluster.Name
            Dim rName = rCluster.Name

            If lName.CompareTo(rName) < 0 Then
                Return lName & "~~~" & rName
            Else
                Return rName & "~~~" & lName
            End If
        End Function
    End Module
End Namespace
