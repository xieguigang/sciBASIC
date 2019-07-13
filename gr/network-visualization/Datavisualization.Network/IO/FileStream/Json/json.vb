#Region "Microsoft.VisualBasic::74f4cf58c885fcfdef95af796520d1d1, gr\network-visualization\Datavisualization.Network\IO\FileStream\Json\json.vb"

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

    '     Class net
    ' 
    '         Properties: edges, nodes, style, types
    ' 
    '         Function: ToString
    ' 
    '     Class edges
    ' 
    '         Properties: A, B, Data, id, source
    '                     target, value, weight
    ' 
    '         Function: ToString
    ' 
    '     Class node
    ' 
    '         Properties: Data, degree, id, name, type
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FileStream.Json

    Public Class net

        Public Property nodes As node()
        Public Property edges As edges()

        ''' <summary>
        ''' 优先加载的样式名称
        ''' </summary>
        ''' <returns></returns>
        Public Property style As String
        ''' <summary>
        ''' All unique vaue of the property <see cref="node.type"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property types As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class edges : Implements IInteraction

        Public Property source As Integer
        Public Property target As Integer
        Public Property A As String Implements IInteraction.source
        Public Property B As String Implements IInteraction.target
        Public Property value As String
        Public Property weight As String
        Public Property id As String
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class node : Implements INamedValue

        Public Property id As Integer
        Public Property name As String Implements INamedValue.Key
        Public Property degree As Integer
        Public Property type As String
        Public Property Data As Dictionary(Of String, String)

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
