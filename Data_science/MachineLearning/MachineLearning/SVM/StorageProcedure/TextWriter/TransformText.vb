#Region "Microsoft.VisualBasic::4da5806c01cb4fc0ec41cdcdce2c65aa, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\TransformText.vb"

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

    '     Module TransformText
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

Namespace SVM

    Module TransformText

        ''' <summary>
        ''' Writes this Range transform to a stream.
        ''' </summary>
        ''' <param name="output">The stream to write to</param>
        ''' <param name="r">The range to write</param>
        Public Sub Write(output As TextWriter, r As RangeTransform)
            output.WriteLine(r._length)
            output.Write(r._inputStart(0))

            For i = 1 To r._inputStart.Length - 1
                output.Write(" " & r._inputStart(i))
            Next

            output.WriteLine()
            output.Write(r._inputScale(0))

            For i = 1 To r._inputScale.Length - 1
                output.Write(" " & r._inputScale(i))
            Next

            output.WriteLine()
            output.WriteLine("{0} {1}", r._outputStart, r._outputScale)
            output.Flush()
        End Sub

        Public Function ToString(transform As RangeTransform) As String
            Dim sb As New StringBuilder

            Using text As New StringWriter(sb)
                Call Write(text, transform)
            End Using

            Return sb.ToString
        End Function
    End Module
End Namespace
