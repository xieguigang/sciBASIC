#Region "Microsoft.VisualBasic::53721770872fbad4bb67b0fd53cf1dd5, Data_science\DataMining\DataMining\test\roc_test2.vb"

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

    '   Total Lines: 15
    '    Code Lines: 11 (73.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (26.67%)
    '     File Size: 573 B


    ' Module roc_test2
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Scripting.Runtime

Module roc_test2

    Sub Main()
        Dim predicts = "E:\biodeep\biodeepdb_v3\biodeepdb_v3\workspace\202410-mslearn\networking_pos_roc\predicts.txt".ReadAllLines.AsDouble
        Dim labels = "E:\biodeep\biodeepdb_v3\biodeepdb_v3\workspace\202410-mslearn\networking_pos_roc\label.txt".ReadAllLines.AsDouble

        Dim test = RegressionROC.ROC(predicts, labels, n:=500).ToArray
        Dim auc As Double = test.AUC

        Pause()
    End Sub
End Module
