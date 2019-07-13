#Region "Microsoft.VisualBasic::f553a75e1013a377c589b05ccb818213, Data_science\Graph\Model\Abstract\Vertex.vb"

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

    ' Class Vertex
    ' 
    '     Properties: ID, Label
    ' 
    '     Function: ToString
    ' 
    '     Sub: Assign
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

''' <summary>
''' 图之中的节点
''' </summary>
''' 
<DataContract>
Public Class Vertex : Implements INamedValue
    Implements IAddressOf

    ''' <summary>
    ''' The unique id of this node
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property Label As String Implements IKeyedEntity(Of String).Key
    ''' <summary>
    ''' Array index.(使用数字表示的唯一标识符)
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property ID As Integer Implements IAddress(Of Integer).Address

    Public Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
        ID = address
    End Sub

    Public Overrides Function ToString() As String
        Return $"({ID}) {Label}"
    End Function
End Class
