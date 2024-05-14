#Region "Microsoft.VisualBasic::c6c2f8a0b2ef43875405d6134ab9ade4, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\GaussianTransformText.vb"

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

    '   Total Lines: 34
    '    Code Lines: 20
    ' Comment Lines: 6
    '   Blank Lines: 8
    '     File Size: 1.05 KB


    '     Module GaussianTransformText
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

    Module GaussianTransformText

        ''' <summary>
        ''' Saves the transform to the disk.  The samples are not stored, only the 
        ''' statistics.
        ''' </summary>
        ''' <param name="stream">The destination stream</param>
        ''' <param name="transform">The transform</param>
        Public Sub Write(stream As TextWriter, transform As GaussianTransform)
            Call stream.WriteLine(transform._means.Length)

            For i = 0 To transform._means.Length - 1
                stream.WriteLine("{0} {1}", transform._means(i), transform._stddevs(i))
            Next

            Call stream.Flush()
        End Sub

        Public Function ToString(transform As GaussianTransform) As String
            Dim sb As New StringBuilder

            Using text As New StringWriter(sb)
                Call Write(text, transform)
            End Using

            Return sb.ToString
        End Function
    End Module
End Namespace
