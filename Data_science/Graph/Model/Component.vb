Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

''' <summary>
''' 图之中的节点
''' </summary>
Public Class Vertex : Implements INamedValue
    Implements IAddressOf

    <XmlAttribute> Public Property Label As String Implements IKeyedEntity(Of String).Key
    <XmlAttribute> Public Property ID As Integer Implements IAddress(Of Integer).Address

    Public Overrides Function ToString() As String
        Return $"({ID}) {Label}"
    End Function
End Class

''' <summary>
''' Direction: ``<see cref="U"/> -> <see cref="V"/>``.
''' (节点之间的边)
''' </summary>
Public Class Edge : Implements INamedValue

    Public Property U As Vertex
    Public Property V As Vertex
    Public Property Weight As Double

    ''' <summary>
    ''' ReadOnly unique-ID
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Friend Property Key As String Implements IKeyedEntity(Of String).Key
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return $"{U.ID}-{V.ID}"
        End Get
        Set(value As String)
            ' DO Nothing
        End Set
    End Property

    Public Overrides Function GetHashCode() As Integer
        Return Key.GetHashCode
    End Function

    Public Overrides Function ToString() As String
        Return $"({GetHashCode()}) {U} => {V}"
    End Function
End Class
