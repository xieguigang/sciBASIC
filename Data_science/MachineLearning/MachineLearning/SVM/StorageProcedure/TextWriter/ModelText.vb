#Region "Microsoft.VisualBasic::5559f4fb8d58a18ba65e926124908765, Data_science\MachineLearning\MachineLearning\SVM\StorageProcedure\TextWriter\ModelText.vb"

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

    '     Module ModelText
    ' 
    '         Function: (+2 Overloads) Read
    ' 
    '         Sub: (+2 Overloads) Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Text

Namespace SVM

    Module ModelText

        ''' <summary>
        ''' Reads a Model from the provided file.
        ''' </summary>
        ''' ''' <param name="filename">The name of the file containing the Model</param>
        ''' <returns>the Model</returns>
        Public Function Read(filename As String) As Model
            Dim input = File.OpenRead(filename)

            Try
                Return Read(input)
            Finally
                input.Close()
            End Try
        End Function

        ''' <summary>
        ''' Reads a Model from the provided stream.
        ''' </summary>
        ''' ''' <param name="stream">The stream from which to read the Model.</param>
        ''' <returns>the Model</returns>
        Public Function Read(stream As Stream) As Model
            Dim input As StreamReader = New StreamReader(stream)

            ' read parameters

            Dim model As Model = New Model()
            Dim param As Parameter = New Parameter()
            model.Parameter = param
            model.Rho = Nothing
            model.PairwiseProbabilityA = Nothing
            model.PairwiseProbabilityB = Nothing
            model.ClassLabels = Nothing
            model.NumberOfSVPerClass = Nothing
            Dim headerFinished = False

            While Not headerFinished
                Dim line As String = input.ReadLine()
                Dim cmd, arg As String
                Dim splitIndex = line.IndexOf(" "c)

                If splitIndex >= 0 Then
                    cmd = line.Substring(0, splitIndex)
                    arg = line.Substring(splitIndex + 1)
                Else
                    cmd = line
                    arg = ""
                End If

                arg = arg.ToLower()
                Dim i, n As Integer

                Select Case cmd
                    Case "svm_type"
                        param.SvmType = CType([Enum].Parse(GetType(SvmType), arg.ToUpper()), SvmType)
                    Case "kernel_type"
                        If Equals(arg, "polynomial") Then arg = "poly"
                        param.KernelType = CType([Enum].Parse(GetType(KernelType), arg.ToUpper()), KernelType)
                    Case "degree"
                        param.Degree = Integer.Parse(arg)
                    Case "gamma"
                        param.Gamma = Double.Parse(arg)
                    Case "coef0"
                        param.Coefficient0 = Double.Parse(arg)
                    Case "nr_class"
                        model.NumberOfClasses = Integer.Parse(arg)
                    Case "total_sv"
                        model.SupportVectorCount = Integer.Parse(arg)
                    Case "rho"
                        n = CInt(model.NumberOfClasses * (model.NumberOfClasses - 1) / 2)
                        model.Rho = New Double(n - 1) {}
                        Dim rhoParts As String() = arg.Split()

                        For i = 0 To n - 1
                            model.Rho(i) = Double.Parse(rhoParts(i))
                        Next

                    Case "label"
                        n = model.NumberOfClasses
                        model.ClassLabels = New Integer(n - 1) {}
                        Dim labelParts As String() = arg.Split()

                        For i = 0 To n - 1
                            model.ClassLabels(i) = Integer.Parse(labelParts(i))
                        Next

                    Case "probA"
                        n = CInt(model.NumberOfClasses * (model.NumberOfClasses - 1) / 2)
                        model.PairwiseProbabilityA = New Double(n - 1) {}
                        Dim probAParts As String() = arg.Split()

                        For i = 0 To n - 1
                            model.PairwiseProbabilityA(i) = Double.Parse(probAParts(i))
                        Next

                    Case "probB"
                        n = CInt(model.NumberOfClasses * (model.NumberOfClasses - 1) / 2)
                        model.PairwiseProbabilityB = New Double(n - 1) {}
                        Dim probBParts As String() = arg.Split()

                        For i = 0 To n - 1
                            model.PairwiseProbabilityB(i) = Double.Parse(probBParts(i))
                        Next

                    Case "nr_sv"
                        n = model.NumberOfClasses
                        model.NumberOfSVPerClass = New Integer(n - 1) {}
                        Dim nrsvParts As String() = arg.Split()

                        For i = 0 To n - 1
                            model.NumberOfSVPerClass(i) = Integer.Parse(nrsvParts(i))
                        Next

                    Case "SV"
                        headerFinished = True
                    Case Else
                        Throw New Exception("Unknown text in model file")
                End Select
            End While

            ' read sv_coef and SV

            Dim m = model.NumberOfClasses - 1
            Dim l = model.SupportVectorCount
            model.SupportVectorCoefficients = New Double(m - 1)() {}

            For i = 0 To m - 1
                model.SupportVectorCoefficients(i) = New Double(l - 1) {}
            Next

            model.SupportVectors = New Node(l - 1)() {}

            For i = 0 To l - 1
                Dim parts As String() = input.ReadLine().Trim().Split()

                For k = 0 To m - 1
                    model.SupportVectorCoefficients(k)(i) = Double.Parse(parts(k))
                Next

                Dim n = parts.Length - m
                model.SupportVectors(i) = New Node(n - 1) {}

                For j = 0 To n - 1
                    Dim nodeParts = parts(m + j).Split(":"c)
                    model.SupportVectors(i)(j) = New Node()
                    model.SupportVectors(i)(j).Index = Integer.Parse(nodeParts(0))
                    model.SupportVectors(i)(j).Value = Double.Parse(nodeParts(1))
                Next
            Next

            Return model
        End Function

        ''' <summary>
        ''' Writes a model to the provided filename.  This will overwrite any previous data in the file.
        ''' </summary>
        ''' ''' <param name="filename">The desired file</param>
        ''' ''' <param name="model">The Model to write</param>
        Public Sub Write(filename As String, model As Model)
            Dim stream = File.Open(filename, FileMode.Create)

            Try
                Write(stream, model)
            Finally
                stream.Close()
            End Try
        End Sub

        ''' <summary>
        ''' Writes a model to the provided stream.
        ''' </summary>
        ''' ''' <param name="stream">The output stream</param>
        ''' ''' <param name="model">The model to write</param>
        Public Sub Write(stream As Stream, model As Model)
            Dim output As StreamWriter = New StreamWriter(stream)
            Dim param = model.Parameter

            output.Write("svm_type {0}" & ASCII.LF, param.SvmType)
            output.Write("kernel_type {0}" & ASCII.LF, param.KernelType)

            If param.KernelType = KernelType.POLY Then output.Write("degree {0}" & ASCII.LF, param.Degree)
            If param.KernelType = KernelType.POLY OrElse param.KernelType = KernelType.RBF OrElse param.KernelType = KernelType.SIGMOID Then output.Write("gamma {0:0.000000}" & ASCII.LF, param.Gamma)
            If param.KernelType = KernelType.POLY OrElse param.KernelType = KernelType.SIGMOID Then output.Write("coef0 {0:0.000000}" & ASCII.LF, param.Coefficient0)
            Dim nr_class = model.NumberOfClasses
            Dim l = model.SupportVectorCount
            output.Write("nr_class {0}" & ASCII.LF, nr_class)
            output.Write("total_sv {0}" & ASCII.LF, l)

            If True Then
                output.Write("rho")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.Rho(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.ClassLabels IsNot Nothing Then
                output.Write("label")

                For i = 0 To nr_class - 1
                    output.Write(" {0}", model.ClassLabels(i))
                Next

                output.Write(ASCII.LF)
            End If
            ' regression has probA only
            If model.PairwiseProbabilityA IsNot Nothing Then
                output.Write("probA")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.PairwiseProbabilityA(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.PairwiseProbabilityB IsNot Nothing Then
                output.Write("probB")

                For i As Integer = 0 To CInt(nr_class * (nr_class - 1) / 2) - 1
                    output.Write(" {0:0.000000}", model.PairwiseProbabilityB(i))
                Next

                output.Write(ASCII.LF)
            End If

            If model.NumberOfSVPerClass IsNot Nothing Then
                output.Write("nr_sv")

                For i = 0 To nr_class - 1
                    output.Write(" {0}", model.NumberOfSVPerClass(i))
                Next

                output.Write(ASCII.LF)
            End If

            output.Write("SV" & ASCII.LF)
            Dim sv_coef = model.SupportVectorCoefficients
            Dim SV = model.SupportVectors

            For i = 0 To l - 1

                For j = 0 To nr_class - 1 - 1
                    output.Write("{0:0.000000} ", sv_coef(j)(i))
                Next

                Dim p = SV(i)

                If p.Length = 0 Then
                    output.Write(ASCII.LF)
                    Continue For
                End If

                If param.KernelType = KernelType.PRECOMPUTED Then
                    output.Write("0:{0:0.000000}", CInt(p(0).Value))
                Else
                    output.Write("{0}:{1:0.000000}", p(0).Index, p(0).Value)

                    For j = 1 To p.Length - 1
                        output.Write(" {0}:{1:0.000000}", p(j).Index, p(j).Value)
                    Next
                End If

                output.Write(ASCII.LF)
            Next

            output.Flush()
        End Sub
    End Module
End Namespace


