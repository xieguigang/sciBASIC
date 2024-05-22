#Region "Microsoft.VisualBasic::928207e415a54cd7822190ea41ccd707, Data_science\DataMining\DataMining\test\KmedoidsTest.vb"

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

    '   Total Lines: 75
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 58 (77.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (22.67%)
    '     File Size: 2.81 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::c5ba3ab82223fe16192fb52cae5c2a64, Data_science\DataMining\DataMining\test\KmedoidsTest.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    ' Module KmedoidsTest
'    ' 
'    '     Sub: Main
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Data.csv.IO
'Imports Microsoft.VisualBasic.DataMining.KMeans
'Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
'Imports Microsoft.VisualBasic.Linq
'Imports Microsoft.VisualBasic.Data.csv

'Module KmedoidsTest

'    Sub Main()

'        Dim data = DataSet.LoadDataSet("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\bezdekIris.csv", uidMap:="class").ToArray
'        Dim names = data.PropertyNames
'        Dim points = data.Select(Function(d)
'                                     Return New ClusterEntity With {.entityVector = d(names), .uid = d.ID}
'                                 End Function).ToArray

'        Dim result = points.DoKMedoids(3, 1000)
'        Dim table = result.Select(Function(e)
'                                      Dim properties = names.SeqIterator.ToDictionary(Function(i) i.value, Function(i) e.Properties(i).ToString)

'                                      properties.Add("cluster", e.cluster)

'                                      Return New EntityObject With {
'                                        .ID = e.uid,
'                                        .Properties = properties
'                                      }
'                                  End Function).ToArray

'        Call table.SaveTo("X:/test.csv")

'        Pause()

'    End Sub
'End Module
