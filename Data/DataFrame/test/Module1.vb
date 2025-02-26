#Region "Microsoft.VisualBasic::d5456572de2f3f7c531290618c1a3176, Data\DataFrame\test\Module1.vb"

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

    '   Total Lines: 31
    '    Code Lines: 24 (77.42%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (22.58%)
    '     File Size: 1022 B


    ' Module Module1
    ' 
    '     Sub: mAIN
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.IO

Module Module1

    Sub mAIN()
        Dim data = DataSet.LoadDataSet("P:\Resources\RABV_24h.csv").ToDictionary(distinct:=True)

        For Each item In DataSet.LoadDataSet("P:\Resources\RABV_48h.csv")
            If Not data.ContainsKey(item.ID) Then
                data.Add(item.ID, New DataSet With {.ID = item.ID})
            End If

            For Each p In item.Properties
                data(item.ID)($"[48h]{p.Key}") = p.Value
            Next
        Next

        For Each item In DataSet.LoadDataSet("P:\Resources\RABV_72h.csv")
            If Not data.ContainsKey(item.ID) Then
                data.Add(item.ID, New DataSet With {.ID = item.ID})
            End If

            For Each p In item.Properties
                data(item.ID)($"[72h]{p.Key}") = p.Value
            Next
        Next

        Call data.SaveTo("P:\Resources\RABV_Table sample.csv")
    End Sub
End Module
