#Region "Microsoft.VisualBasic::83cc041357b3c8658634e71e4022f9b4, Data_science\MachineLearning\DeepLearning\CNN\ReadModelCNN.vb"

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

    '   Total Lines: 50
    '    Code Lines: 38 (76.00%)
    ' Comment Lines: 1 (2.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (22.00%)
    '     File Size: 1.75 KB


    '     Module ReadModelCNN
    ' 
    '         Function: (+2 Overloads) Read, ReadLayer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Layer = Microsoft.VisualBasic.MachineLearning.CNN.layers.Layer

Namespace CNN

    Public Module ReadModelCNN

        Public Function Read(file As Stream) As ConvolutionalNN
            Using rd As New BinaryReader(file)
                Dim magic As String = Encoding.ASCII.GetString(rd.ReadBytes(3))

                If magic <> "CNN" Then
                    Throw New InvalidDataException("error magic header!")
                End If

                Return Read(rd)
            End Using
        End Function

        Private Function Read(rd As BinaryReader) As ConvolutionalNN
            Dim layerNum As Integer = rd.ReadInt32
            Dim layers As New LayerBuilder(initialized:=True)

            For i As Integer = 0 To layerNum - 1
                Call layers.add(ReadLayer(rd))
            Next

            Return New ConvolutionalNN(layers)
        End Function

        Private Function ReadLayer(rd As BinaryReader) As Layer
            If 0 <> rd.ReadInt64() Then
                Throw New InvalidDataException("invalid file format!")
            End If

            ' verify the stream parser by use this flag data
            Dim type As LayerTypes = CType(rd.ReadInt32, LayerTypes)
            Dim layer As Layer = DirectCast(New ObjectInputStream(rd).ReadObject, Layer)

            If type <> layer.Type Then
                Throw New InvalidDataException("The CNN layer type mis-matched!")
            Else
                Return layer
            End If
        End Function
    End Module
End Namespace
