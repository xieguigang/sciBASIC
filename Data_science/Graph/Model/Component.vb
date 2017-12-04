#Region "Microsoft.VisualBasic::51bb879b85dce82c948277ce58041b4a, ..\sciBASIC#\Data_science\Graph\Model\Component.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports V = Microsoft.VisualBasic.Data.Graph.Vertex

''' <summary>
''' 图之中的节点
''' </summary>
Public Class Vertex : Implements INamedValue
    Implements IAddressOf

    ''' <summary>
    ''' The unique id of this node
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property Label As String Implements IKeyedEntity(Of String).Key
    ''' <summary>
    ''' Array index
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property ID As Integer Implements IAddress(Of Integer).Address

    Public Overrides Function ToString() As String
        Return $"({ID}) {Label}"
    End Function
End Class

Public Class Edge : Inherits Edge(Of V)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function EdgeKey(U%, V%) As String
        Return $"{U}-{V}"
    End Function
End Class

''' <summary>
''' Direction: ``<see cref="U"/> -> <see cref="V"/>``.
''' (节点之间的边)
''' </summary>
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
    Public Property Weight As Double

    ''' <summary>
    ''' ReadOnly unique-ID
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' 唯一标识符使用的是<see cref="V"/>的ID属性，而不是使用Label生成的
    ''' </remarks> 
    Friend Property Key As String Implements IKeyedEntity(Of String).Key
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Edge.EdgeKey(U.ID, V.ID)
        End Get
        Set(value As String)
            ' DO Nothing
        End Set
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetHashCode() As Integer
        Return Key.GetHashCode
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"({GetHashCode()}) {U} => {V}"
    End Function
End Class
