#Region "Microsoft.VisualBasic::878226dba155e65735e2547348735533, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Obsolete\Analysis.vb"

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

    '   Total Lines: 76
    '    Code Lines: 0
    ' Comment Lines: 63
    '   Blank Lines: 13
    '     File Size: 3.03 KB


    ' 
    ' /********************************************************************************/

#End Region

'Imports System.Runtime.CompilerServices
'Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
'Imports Microsoft.VisualBasic.ComponentModel.Ranges
'Imports Microsoft.VisualBasic.DataMining.KMeans
'Imports Microsoft.VisualBasic.Emit.Delegates
'Imports Microsoft.VisualBasic.Language
'Imports Microsoft.VisualBasic.Language.UnixBash
'Imports Microsoft.VisualBasic.Linq
'Imports Microsoft.VisualBasic.Math.Calculus
'Imports Microsoft.VisualBasic.Serialization.JSON

'''' <summary>
'''' 这个方法的结果可能结果不是太好
'''' </summary>
'Public Module Analysis

'    Public Delegate Function GetPoints(vals As Double()) As Integer()

'    ''' <summary>
'    ''' 
'    ''' </summary>
'    ''' <param name="DIR"></param>
'    ''' <param name="analysis"></param>
'    ''' <param name="clustersExpected">最终的结果的聚类分组cluster的数量</param>
'    ''' <returns></returns>
'    Public Function GroupsAnalysis(DIR As String, analysis As Dictionary(Of String, GetPoints), clustersExpected As Integer) As Dictionary(Of String, Dictionary(Of String, DoubleRange))
'        Dim codes As New List(Of NamedValue(Of Dictionary(Of String, Double)))

'        For Each file As String In ls - l - r - wildcards("*.csv") <= DIR
'            Dim outData As ODEsOut = ODEsOut.LoadFromDataFrame(file)
'            Dim ps As New List(Of Integer)

'            For Each m In analysis
'                ps += m.Value(outData.y(m.Key).x)
'            Next

'            codes += New NamedValue(Of Dictionary(Of String, Double)) With {
'                .Name = ps.JoinBy(","),
'                .x = outData.params
'            }
'        Next

'        Dim datasets As Entity() = codes.ToArray(
'            Function(x) New Entity With {
'                .uid = x.Name,
'                .Properties = x.Name.Split(","c).Select(AddressOf Val)
'        })
'        Dim clusters = ClusterDataSet(clustersExpected, datasets)
'        Dim out As New Dictionary(Of String, Dictionary(Of String, DoubleRange))
'        Dim bufs = codes.GroupBy(Function(x) x.Name).ToDictionary(Function(g) g.First.Name, Function(g) g.ToArray)

'        For Each cluster As KMeansCluster(Of Entity) In clusters
'            Dim key As String = cluster.ClusterMean.JoinBy(",")
'            Dim value As New Dictionary(Of String, DoubleRange)
'            Dim data As New List(Of KeyValuePair(Of String, Double))

'            For Each x As Entity In cluster
'                data += bufs(x.uid) _
'                    .Select(Function(d) d.x.ToArray) _
'                    .IteratesALL
'            Next

'            Dim pg = data.GroupBy(Function(p) p.Key)

'            For Each p In pg
'                Dim array = p.ToArray
'                Dim vals = array.Select(Function(x) x.Value)
'                value(p.Key) = New DoubleRange(vals.Min, vals.Max)
'            Next

'            out(key) = value
'        Next

'        Return out
'    End Function
'End Module
