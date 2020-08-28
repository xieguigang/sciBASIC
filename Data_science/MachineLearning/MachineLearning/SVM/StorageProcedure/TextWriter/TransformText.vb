#Region "Microsoft.VisualBasic::939f454935f73ed79a038a06271ce13e, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\TransformText.vb"

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
    '         Function: (+2 Overloads) Read
    ' 
    '         Sub: (+2 Overloads) Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace SVM

    Module TransformText

        ''' <summary>
        ''' Writes this Range transform to a stream.
        ''' </summary>
        ''' <param name="stream">The stream to write to</param>
        ''' <param name="r">The range to write</param>
        Public Sub Write(stream As Stream, r As RangeTransform)
            Dim output As StreamWriter = New StreamWriter(stream)
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

        ''' <summary>
        ''' Writes this Range transform to a file.    This will overwrite any previous data in the file.
        ''' </summary>
        ''' <param name="outputFile">The file to write to</param>
        ''' <param name="r">The Range to write</param>
        Public Sub Write(outputFile As String, r As RangeTransform)
            Dim s = File.Open(outputFile, FileMode.Create)

            Try
                Write(s, r)
            Finally
                s.Close()
            End Try
        End Sub

        ''' <summary>
        ''' Reads a Range transform from a file.
        ''' </summary>
        ''' <param name="inputFile">The file to read from</param>
        ''' <returns>The Range transform</returns>
        Public Function Read(inputFile As String) As RangeTransform
            Dim s = File.OpenRead(inputFile)

            Try
                Return Read(s)
            Finally
                s.Close()
            End Try
        End Function

        ''' <summary>
        ''' Reads a Range transform from a stream.
        ''' </summary>
        ''' <param name="stream">The stream to read from</param>
        ''' <returns>The Range transform</returns>
        Public Function Read(stream As Stream) As RangeTransform
            Dim input As StreamReader = New StreamReader(stream)
            Dim length As Integer = Integer.Parse(input.ReadLine())
            Dim inputStart = New Double(length - 1) {}
            Dim inputScale = New Double(length - 1) {}
            Dim parts As String() = input.ReadLine().Split()

            For i = 0 To length - 1
                inputStart(i) = Double.Parse(parts(i))
            Next

            parts = input.ReadLine().Split()

            For i = 0 To length - 1
                inputScale(i) = Double.Parse(parts(i))
            Next

            parts = input.ReadLine().Split()

            Dim outputStart = Double.Parse(parts(0))
            Dim outputScale = Double.Parse(parts(1))

            Return New RangeTransform(inputStart, inputScale, outputStart, outputScale, length)
        End Function
    End Module
End Namespace
