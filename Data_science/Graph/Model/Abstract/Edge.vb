Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports V = Microsoft.VisualBasic.Data.GraphTheory.Vertex

Public Class VertexEdge : Inherits Edge(Of V)

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
            Return VertexEdge.EdgeKey(U.ID, V.ID)
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
