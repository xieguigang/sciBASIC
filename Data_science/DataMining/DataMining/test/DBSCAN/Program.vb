#Region "Microsoft.VisualBasic::fd2677b36fa39f34cb95a4defda21d5c, Data_science\DataMining\DataMining\test\DBSCAN\Program.vb"

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

    '   Total Lines: 32
    '    Code Lines: 23 (71.88%)
    ' Comment Lines: 2 (6.25%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (21.88%)
    '     File Size: 1.47 KB


    ' Class Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.DBSCAN

Class Program
    Friend Shared Sub Main(args As String())
        Dim featureData As MyCustomDatasetItem() = {}

        Dim testPoints As New List(Of MyCustomDatasetItem)()
        For i As Integer = 0 To 999
            'points around (1,1) with most 1 distance
            testPoints.Add(New MyCustomDatasetItem(1, 1 + (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(1, 1 - (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(1 - (CSng(i) / 1000), 1))
            testPoints.Add(New MyCustomDatasetItem(1 + (CSng(i) / 1000), 1))

            'points around (5,5) with most 1 distance
            testPoints.Add(New MyCustomDatasetItem(5, 5 + (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(5, 5 - (CSng(i) / 1000)))
            testPoints.Add(New MyCustomDatasetItem(5 - (CSng(i) / 1000), 5))
            testPoints.Add(New MyCustomDatasetItem(5 + (CSng(i) / 1000), 5))
        Next
        featureData = testPoints.ToArray()
        Dim clusters As HashSet(Of MyCustomDatasetItem()) = Nothing

        Dim dbs = New DbscanAlgorithm(Of MyCustomDatasetItem)(Function(x, y) Math.Sqrt(((x.X - y.X) * (x.X - y.X)) + ((x.Y - y.Y) * (x.Y - y.Y))))
        Dim result = dbs.ComputeClusterDBSCAN(allPoints:=featureData, epsilon:=0.01, minPts:=10)



        Console.ReadKey()
    End Sub
End Class
