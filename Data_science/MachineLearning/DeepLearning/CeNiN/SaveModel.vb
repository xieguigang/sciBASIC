#Region "Microsoft.VisualBasic::d1c1c50955ba92516cfc23eb9ed9c312, Data_science\MachineLearning\DeepLearning\CeNiN\SaveModel.vb"

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

    '   Total Lines: 111
    '    Code Lines: 90 (81.08%)
    ' Comment Lines: 4 (3.60%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (15.32%)
    '     File Size: 3.80 KB


    '     Module SaveModel
    ' 
    '         Function: Save
    ' 
    '         Sub: (+6 Overloads) save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Convolutional

    Public Module SaveModel

        <Extension>
        Public Function Save(model As CeNiN, file As Stream) As Boolean
            Using writer As New BinaryWriter(file, Encoding.ASCII)
                Try
                    Call model.save(writer)
                    Call writer.Flush()
                Catch ex As Exception
                    Call App.LogException(ex)
                    Return False
                End Try
            End Using

            Return True
        End Function

        <Extension>
        Private Sub save(model As CeNiN, wr As BinaryWriter)
            Call wr.Write(Encoding.ASCII.GetBytes(CeNiN.CeNiN_FILE_HEADER))
            Call wr.Write(model.layerCount)

            For Each i As Integer In model.inputSize
                Call wr.Write(i)
            Next
            For Each a As Single In model.inputLayer.avgPixel
                Call wr.Write(a)
            Next

            ' write for each layer
            For Each layer As Layer In From cv As Layer
                                       In model.layers
                                       Where Not (TypeOf cv Is Input OrElse TypeOf cv Is Output)

                Call model.save(layer, wr)
            Next

            wr.Write("EOF")
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Layer, wr As BinaryWriter)
            Call wr.Write(layer.type.Description)

            Select Case layer.type
                Case CNN.LayerTypes.Convolution : Call model.save(DirectCast(layer, Convolution), wr)
                Case CNN.LayerTypes.ReLU : Call model.save(DirectCast(layer, ReLU), wr)
                Case CNN.LayerTypes.Pool : Call model.save(DirectCast(layer, Pool), wr)
                Case CNN.LayerTypes.SoftMax : Call model.save(DirectCast(layer, SoftMax), wr)
                Case Else
                    Throw New InvalidDataException(layer.type.ToString)
            End Select

            wr.Flush()
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As SoftMax, wr As BinaryWriter)
            ' class count
            Call wr.Write(model.classCount)

            For Each name As String In model.outputLayer.m_classes
                Call wr.Write(name)
            Next
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Pool, wr As BinaryWriter)
            For Each pad As Integer In layer.pad
                Call wr.Write(CByte(pad))
            Next
            For Each p As Integer In layer.pool
                Call wr.Write(CByte(p))
            Next
            For Each s As Integer In layer.stride
                Call wr.Write(CByte(s))
            Next
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As ReLU, wr As BinaryWriter)
            ' do nothing
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Convolution, wr As BinaryWriter)
            ' write pad data
            For Each pad As Integer In layer.pad
                Call wr.Write(CByte(pad))
            Next
            For Each d As Integer In layer.weights.Dimensions
                Call wr.Write(d)
            Next
            For Each stride As Integer In layer.stride
                Call wr.Write(CByte(stride))
            Next
            For Each w As Single In layer.weights.data
                Call wr.Write(w)
            Next
            For Each b As Single In layer.biases.data
                Call wr.Write(b)
            Next
        End Sub
    End Module
End Namespace
