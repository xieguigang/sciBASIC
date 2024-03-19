﻿#Region "Microsoft.VisualBasic::d9f5e13b8cffab7e256f8ae97c4b054c, sciBASIC#\Data_science\Graph\Model\Abstract\Edge.vb"

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

    '   Total Lines: 57
    '    Code Lines: 21
    ' Comment Lines: 31
    '   Blank Lines: 5
    '     File Size: 1.93 KB


    ' Class Edge
    ' 
    '     Properties: ID, U, V, weight
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports V = Microsoft.VisualBasic.Data.GraphTheory.Vertex

''' <summary>
''' Direction: ``<see cref="U"/> -> <see cref="V"/>``.
''' (节点之间的边)
''' </summary>
''' <remarks>
''' 如果边对象是一个有向边的话，那么<see cref="U"/>就是父节点，<see cref="V"/>就是<see cref="U"/>的子节点
''' </remarks>
Public Class Edge(Of Vertex As V) : Implements INamedValue

    ''' <summary>
    ''' The source node
    ''' </summary>
    ''' <returns></returns>
    Public Property U As Vertex
    ''' <summary>
    ''' The target node
    ''' </summary>
    ''' <returns></returns>
    Public Property V As Vertex

    Public Overridable Property weight As Double

    ''' <summary>
    ''' ReadOnly unique-ID
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 唯一标识符使用的是<see cref="V"/>的ID属性，而不是使用Label生成的
    ''' </remarks> 
    Public Overridable Property ID As String Implements IKeyedEntity(Of String).Key
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return $"[{U.ID}]{U.label} -> [{V.ID}]{V.label}"
        End Get
        Set(value As String)
            ' DO Nothing
        End Set
    End Property

    ' 20220415 this function will makes the list removes 
    ' in graph model too slow!
    ' removes this method overrides
    '
    '<MethodImpl(MethodImplOptions.AggressiveInlining)>
    'Public Overrides Function GetHashCode() As Integer
    '    Return ID.GetHashCode
    'End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"({GetHashCode()}) {U} => {V}"
    End Function
End Class
