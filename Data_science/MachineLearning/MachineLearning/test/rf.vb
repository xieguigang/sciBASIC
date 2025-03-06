#Region "Microsoft.VisualBasic::04d5d70908ac3a8940c73146ec298724, Data_science\MachineLearning\MachineLearning\test\rf.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 934 B


    ' Module rf
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports Microsoft.VisualBasic.Scripting.Runtime

Module rf

    Sub Main()
        Dim y As New List(Of Double)
        Dim id As New List(Of String)
        Dim v As New List(Of Double())

        For Each line In "G:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\MachineLearning\RandomForests\training_regression.txt".IterateAllLines

            Dim t = line.StringSplit("\s+")
            y.Add(Val(t(0)))
            id.Add(t(1))
            v.Add(t.Skip(2).AsDouble)
        Next

        Dim ref As New Data With {
            .attributeNames = v(0).Sequence(offSet:=1).Select(Function(i) $"#{i}").ToArray,
            .Genotype = v.ToArray,
            .ID = id.ToArray,
            .phenotype = y.ToArray
        }
        Dim tree As New RanFog
        Dim result = tree.Run(ref)

        Pause()
    End Sub
End Module
