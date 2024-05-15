#Region "Microsoft.VisualBasic::c04acc43871b352f3eab711d4db50b03, Data_science\MachineLearning\DeepLearning\CNN\SaveModelCNN.vb"

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
    '    Code Lines: 29
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 1.29 KB


    '     Module SaveModelCNN
    ' 
    '         Sub: (+3 Overloads) Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace CNN

    Public Module SaveModelCNN

        Public Sub Write(cnn As ConvolutionalNN, file As Stream)
            Using writer As New BinaryWriter(file, Encoding.ASCII)
                Call Write(cnn, writer)
                Call writer.Flush()
            End Using
        End Sub

        Private Sub Write(cnn As ConvolutionalNN, wr As BinaryWriter)
            Call wr.Write(Encoding.ASCII.GetBytes("CNN"))
            Call wr.Write(cnn.layerNum)

            For i As Integer = 0 To cnn.LayerNum - 1
                Call Write(layer:=cnn(i), wr)
            Next
        End Sub

        Private Sub Write(layer As Layer, wr As BinaryWriter)
            Call wr.Write(0&)
            Call wr.Write(CInt(layer.Type))
            Call wr.Flush()

            Dim save As New ObjectOutputStream(wr)
            ' do not close/dispose the stream at here
            ' or the file stream data will be close at here
            ' so that we can not save the next layer object
            Call save.WriteObject(layer)
            Call save.Flush()
        End Sub
    End Module
End Namespace
