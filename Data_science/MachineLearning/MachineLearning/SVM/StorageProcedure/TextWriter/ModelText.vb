#Region "Microsoft.VisualBasic::d7900fd0f4e68c4e4cd3bc5261ce419f, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\ModelText.vb"

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

    '   Total Lines: 121
    '    Code Lines: 84 (69.42%)
    ' Comment Lines: 6 (4.96%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 31 (25.62%)
    '     File Size: 4.24 KB


    '     Module ModelText
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
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace SVM

    Module ModelText

        ''' <summary>
        ''' Writes a model to the provided stream.
        ''' </summary>
        ''' ''' <param name="output">The output stream</param>
        ''' ''' <param name="model">The model to write</param>
        Public Sub Write(output As TextWriter, model As Model)
            Dim param = model.parameter

            output.Write("svm_type {0}" & ASCII.LF, param.svmType)
            output.Write("kernel_type {0}" & ASCII.LF, param.kernelType)

            If param.kernelType = KernelType.POLY Then output.Write("degree {0}" & ASCII.LF, param.degree)
            If param.kernelType = KernelType.POLY OrElse param.kernelType = KernelType.RBF OrElse param.kernelType = KernelType.SIGMOID Then output.Write("gamma {0:0.000000}" & ASCII.LF, param.gamma)
            If param.kernelType = KernelType.POLY OrElse param.kernelType = KernelType.SIGMOID Then output.Write("coef0 {0:0.000000}" & ASCII.LF, param.coefficient0)
            Dim nr_class = model.numberOfClasses
            Dim l = model.supportVectorCount
            output.Write("nr_class {0}" & ASCII.LF, nr_class)
            output.Write("total_sv {0}" & ASCII.LF, l)

            If True Then
                output.Write("rho")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.rho(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.classLabels IsNot Nothing Then
                output.Write("label")

                For i = 0 To nr_class - 1
                    output.Write(" {0}", model.classLabels(i))
                Next

                output.Write(ASCII.LF)
            End If
            ' regression has probA only
            If model.pairwiseProbabilityA IsNot Nothing Then
                output.Write("probA")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.pairwiseProbabilityA(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.pairwiseProbabilityB IsNot Nothing Then
                output.Write("probB")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.pairwiseProbabilityB(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.numberOfSVPerClass IsNot Nothing Then
                output.Write("nr_sv")

                For i = 0 To nr_class - 1
                    output.Write(" {0}", model.numberOfSVPerClass(i))
                Next

                output.Write(ASCII.LF)
            End If

            output.Write("SV" & ASCII.LF)
            Dim sv_coef = model.supportVectorCoefficients
            Dim SV = model.supportVectors

            For i = 0 To l - 1

                For j = 0 To nr_class - 1 - 1
                    output.Write("{0:0.000000} ", sv_coef(j)(i))
                Next

                Dim p = SV(i)

                If p.Length = 0 Then
                    output.Write(ASCII.LF)
                    Continue For
                End If

                If param.kernelType = KernelType.PRECOMPUTED Then
                    output.Write("0:{0:0.000000}", CInt(p(0).value))
                Else
                    output.Write("{0}:{1:0.000000}", p(0).index, p(0).value)

                    For j = 1 To p.Length - 1
                        output.Write(" {0}:{1:0.000000}", p(j).index, p(j).value)
                    Next
                End If

                output.Write(ASCII.LF)
            Next

            output.Flush()
        End Sub

        Public Function ToString(model As Model) As String
            Dim sb As New StringBuilder

            Using text As New StringWriter(sb)
                Call Write(text, model)
            End Using

            Return sb.ToString
        End Function
    End Module
End Namespace
