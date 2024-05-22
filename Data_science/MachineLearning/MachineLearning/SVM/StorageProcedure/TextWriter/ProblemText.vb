#Region "Microsoft.VisualBasic::ce34067740e8a46aa4be2900fe2f7d03, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\ProblemText.vb"

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

    '   Total Lines: 39
    '    Code Lines: 25 (64.10%)
    ' Comment Lines: 5 (12.82%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (23.08%)
    '     File Size: 1.13 KB


    '     Module ProblemText
    ' 
    '         Function: ToString
    ' 
    '         Sub: Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Text
Imports stdNum = System.Math

Namespace SVM

    Module ProblemText

        ''' <summary>
        ''' Writes a problem to a stream.
        ''' </summary>
        ''' <param name="output">The stream to write the problem to.</param>
        ''' <param name="problem">The problem to write.</param>
        Public Sub Write(output As TextWriter, problem As Problem)
            For i = 0 To problem.count - 1
                output.Write(problem.Y(i))

                For j = 0 To problem.X(i).Length - 1
                    output.Write(" {0}:{1:0.000000}", problem.X(i)(j).index, problem.X(i)(j).value)
                Next

                output.Write(ASCII.LF)
            Next

            output.Flush()
        End Sub

        Public Function ToString(problem As Problem) As String
            Dim sb As New StringBuilder

            Using text As New StringWriter(sb)
                Call Write(text, problem)
            End Using

            Return sb.ToString
        End Function
    End Module
End Namespace
