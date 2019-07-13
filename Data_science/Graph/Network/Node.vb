#Region "Microsoft.VisualBasic::9508b735362dc18dd9b06988b7d1d3c8, Data_science\Graph\Network\Node.vb"

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

    '     Class Node
    ' 
    '         Properties: degree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Network

    ''' <summary>
    ''' A network node model
    ''' </summary>
    Public Class Node : Inherits Vertex

        ''' <summary>
        ''' Node connection counts: [point_to_this_node, point_from_this_node]
        ''' </summary>
        ''' <returns></returns>
        Public Property degree As (In%, Out%)

    End Class
End Namespace
