#Region "Microsoft.VisualBasic::13f86fc41d0d3cb6b8f329ca70d723bb, Data_science\Graph\Model\Abstract\Edge.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class Edge
    ' 
    '     Properties: ID, U, V, weight
    ' 
    '     Function: GetHashCode, ToString
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
    Public Property weight As Double

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
            Return VertexEdge.EdgeKey(U.ID, V.ID)
        End Get
        Set(value As String)
            ' DO Nothing
        End Set
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetHashCode() As Integer
        Return ID.GetHashCode
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return $"({GetHashCode()}) {U} => {V}"
    End Function
End Class
