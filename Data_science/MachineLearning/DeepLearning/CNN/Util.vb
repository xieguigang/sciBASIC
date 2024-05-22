#Region "Microsoft.VisualBasic::509431b07f0cc6ca77c7a959c5f44aea, Data_science\MachineLearning\DeepLearning\CNN\Util.vb"

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

    '   Total Lines: 27
    '    Code Lines: 18 (66.67%)
    ' Comment Lines: 6 (22.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (11.11%)
    '     File Size: 883 B


    '     Module Util
    ' 
    '         Function: randomPerm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace CNN

    Module Util

        ''' <summary>
        ''' the batch epochs helper
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="batchSize"></param>
        ''' <returns></returns>
        Public Function randomPerm(size As Integer, batchSize As Integer) As Integer()
            Dim [set] As ISet(Of Integer?) = New HashSet(Of Integer?)()
            While [set].Count < batchSize
                [set].Add(randf.Next(size))
            End While
            Dim randPerm = New Integer(batchSize - 1) {}
            Dim i As i32 = 0
            For Each value In [set]
                randPerm(++i) = value.Value
            Next
            Return randPerm
        End Function
    End Module
End Namespace
