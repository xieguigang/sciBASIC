#Region "Microsoft.VisualBasic::62865e892fb4d8b58978d835778f2699, Data_science\Visualization\test\Module3.vb"

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

    ' Module Module3
    ' 
    '     Sub: Boxplot, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO

Module Module3

    Sub Main()
        Call Boxplot()
    End Sub

    Sub Boxplot()
        Dim data = DataSet.LoadDataSet("C:\Users\xieguigang\Desktop\8.4\ko-lv3.csv")
        Dim [case] = {"20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1", "13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2", "7_4", "6_4", "1_3", "1_4", "1_5"}
        Dim control = data.PropertyNames.AsList - [case]


        Pause()
    End Sub
End Module
