#Region "Microsoft.VisualBasic::2d007928d139d4b53130b8518c34e8c5, gr\network-visualization\Datavisualization.Network\IO\Generic\IDynamicsTable.vb"

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

    '     Class IDynamicsTable
    ' 
    '         Properties: Properties
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection

Namespace FileStream.Generic

    ''' <summary>
    ''' The network graph object contains the dynamics property for contains the extra information of the object.
    ''' </summary>
    Public MustInherit Class IDynamicsTable : Inherits DynamicPropertyBase(Of String)

        ''' <summary>
        ''' The dynamics property table of this network component
        ''' </summary>
        ''' <returns></returns>
        <Meta(GetType(String))> Public Overrides Property Properties As Dictionary(Of String, String)
            Get
                Return MyBase.Properties
            End Get
            Set(value As Dictionary(Of String, String))
                MyBase.Properties = value
            End Set
        End Property
    End Class
End Namespace
