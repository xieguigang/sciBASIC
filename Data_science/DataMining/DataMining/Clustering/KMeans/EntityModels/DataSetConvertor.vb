#Region "Microsoft.VisualBasic::d54ac1ed316e767c411e27de3a501a6b, Data_science\DataMining\DataMining\Clustering\KMeans\EntityModels\DataSetConvertor.vb"

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

    '   Total Lines: 52
    '    Code Lines: 41 (78.85%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (21.15%)
    '     File Size: 1.83 KB


    '     Class DataSetConvertor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) GetObjects, GetPoints, GetVectors, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace KMeans

    Public Class DataSetConvertor

        ReadOnly maps As String()

        Sub New(rawInput As EntityClusterModel())
            maps = rawInput _
            .Select(Function(a) a.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray
        End Sub

        Public Iterator Function GetPoints(rawInput As EntityClusterModel()) As IEnumerable(Of Lloyds.Point)
            For Each xi As EntityClusterModel In rawInput
                Yield New Lloyds.Point(xi(maps)) With {.uid = xi.ID}
            Next
        End Function

        Public Iterator Function GetVectors(rawInput As EntityClusterModel()) As IEnumerable(Of ClusterEntity)
            For Each xi As EntityClusterModel In rawInput
                Yield xi.ToModel(projection:=maps)
            Next
        End Function

        Public Iterator Function GetObjects(Of T As ClusterEntity)(cluster As IEnumerable(Of T)) As IEnumerable(Of EntityClusterModel)
            For Each xi As ClusterEntity In cluster
                Yield xi.ToDataModel(maps)
            Next
        End Function

        Public Iterator Function GetObjects(Of T As ClusterEntity)(cluster As IEnumerable(Of T), setClass As Integer) As IEnumerable(Of EntityClusterModel)
            Dim yi As EntityClusterModel

            For Each xi As ClusterEntity In cluster
                yi = xi.ToDataModel(maps)
                yi.Cluster = setClass

                Yield yi
            Next
        End Function

        Public Overrides Function ToString() As String
            Return maps.GetJson
        End Function
    End Class
End Namespace
