#Region "Microsoft.VisualBasic::9b9d96922996dfc47e0780fdc2d1dca0, Data_science\DataMining\DataMining\Clustering\CMeans\Program.vb"

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

    ' Class Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions




Public Class Program
    Public Shared Sub Main(args As String())
        Dim data As ClusterEntity() = Enumerable.Range(0, 100) _
            .[Select](Function(x)
                          Return New ClusterEntity With {
                              .uid = x.ToString,
                              .entityVector = New Double() {randf.randf(0, 99999), randf.randf(-1000, 999), randf.randf(1, 10), randf.randf(-1000000, 10), randf.randf(-1000000, 10000)}
                          }
                      End Function) _
            .ToArray()
        Dim result As Classify() = CMeans.CMeans(10, data)

        For Each item In result
            Console.WriteLine($"===== {item.Id} (Count:{item.members.Count}) =====")

            For Each item2 In item.members.OrderBy(Function(x) x.entityVector.Average())
                Console.WriteLine(item2.ToString)
            Next
        Next

        Console.ReadKey()
    End Sub
End Class
