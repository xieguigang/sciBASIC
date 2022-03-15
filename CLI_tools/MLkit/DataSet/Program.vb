#Region "Microsoft.VisualBasic::982b9381f238719b604c8a0ee77b1906, sciBASIC#\CLI_tools\MLkit\DataSet\Program.vb"

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

    '   Total Lines: 67
    '    Code Lines: 53
    ' Comment Lines: 5
    '   Blank Lines: 9
    '     File Size: 2.76 KB


    ' Module Program
    ' 
    '     Function: AnalysisFoldChange, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Excel = Microsoft.VisualBasic.Data.csv.IO.DataSet

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    ''' <summary>
    ''' target应该是只有0和非零这两种结果的
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/foldchange")>
    <Usage("/foldchange /in <dataset.Xml> [/out <result.csv>]")>
    Public Function AnalysisFoldChange(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.foldchange.csv"
        Dim dataset As DataSet = [in].LoadXml(Of DataSet)
        Dim result As New Dictionary(Of String, Excel)
        Dim targetName$
        Dim vectorGroup As IGrouping(Of String, Sample)()
        Dim zero, nonZero As IGrouping(Of String, Sample)
        Dim namesOf = dataset.NormalizeMatrix.names

        Call namesOf _
            .DoEach(Sub(name)
                        Call result.Add(name, New Excel With {.ID = name})
                    End Sub)

        For i As Integer = 0 To dataset.output.Length - 1
            targetName = dataset.output(i)
            vectorGroup = dataset.DataSamples _
                .AsEnumerable _
                .GroupBy(Function(d)
                             If d.target(i) = 0.0 Then
                                 Return "0"
                             Else
                                 Return "1"
                             End If
                         End Function) _
                .ToArray

            zero = vectorGroup.FirstOrDefault(Function(g) g.Key = "0")
            nonZero = vectorGroup.FirstOrDefault(Function(g) g.Key = "1")

            For j As Integer = 0 To namesOf.Length - 1
                Dim A As Double() = zero.Select(Function(d) d.status(j)).ToArray
                Dim B As Double() = nonZero.Select(Function(d) d.status(j)).ToArray
                Dim foldchange# = B.Average / A.Average
                Dim pvalue# = t.Test(A, B).Pvalue

                result(namesOf(j)).Add($"FoldChange(Of {targetName})", foldchange)
                result(namesOf(j)).Add($"Log2FC(Of {targetName})", Math.Log(foldchange, 2))
                result(namesOf(j)).Add($"p.value(Of {targetName})", pvalue)
            Next
        Next

        Return result.Values.SaveTo(out).CLICode
    End Function
End Module
