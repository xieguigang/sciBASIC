#Region "Microsoft.VisualBasic::e1b0fc4314267431ba6cc9dd76a7ad31, Data_science\DataMining\UMAP\test\Program.vb"

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

    '   Total Lines: 126
    '    Code Lines: 96
    ' Comment Lines: 3
    '   Blank Lines: 27
    '     File Size: 5.74 KB


    '     Class Program
    ' 
    '         Sub: Main, RunTest
    ' 
    '     Class LabelledVector
    ' 
    '         Properties: UID, Vector
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Tester
    Friend Class Program

        Const test_data = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\MNIST-LabelledVectorArray-60000x100.msgpack"

        Public Shared Sub RunTest(data As LabelledVector())
            Dim timer = Stopwatch.StartNew()
            Dim umap = New Umap(
                distance:=AddressOf DistanceFunctions.CosineForNormalizedVectors,
                progressReporter:=AddressOf RunSlavePipeline.SendProgress)
            Console.WriteLine("Initialize fit..")
            Dim rawMatrix = data.[Select](Function(entry) entry.Vector.Select(Function(s) CDbl(s)).ToArray).ToArray()
            Dim nEpochs = umap.InitializeFit(rawMatrix)
            Console.WriteLine("- Done")
            Console.WriteLine()
            Console.WriteLine("Calculating..")

            For i = 0 To nEpochs - 1
                umap.Step()

                If i Mod 10 = 0 Then
                    Console.WriteLine($"- Completed {i + 1} of {nEpochs}")
                End If
            Next

            Console.WriteLine("- Done")
            Dim embeddings = umap.GetEmbedding().[Select](Function(vector) New With {
                .X = vector(0),
                .Y = vector(1)
            }).ToArray()
            timer.Stop()
            Console.WriteLine("Time taken: " & timer.Elapsed.Lanudry)

            ' Fit the vectors to a 0-1 range (this isn't necessary if feeding these values down from a server to a browser to draw with Plotly because ronend because Plotly scales the axes to the data)
            Dim minX = embeddings.Min(Function(vector) vector.X)
            Dim rangeX = embeddings.Max(Function(vector) vector.X) - minX
            Dim minY = embeddings.Min(Function(vector) vector.Y)
            Dim rangeY = embeddings.Max(Function(vector) vector.Y) - minY
            Dim scaledEmbeddings = embeddings.[Select](Function(vector) (X:=(vector.X - minX) / rangeX, Y:=(vector.Y - minY) / rangeY)).ToArray()
            Const width = 1600
            Const height = 1200

            Using bitmap = New Bitmap(width, height)

                Using g = Graphics.FromImage(bitmap)
                    g.FillRectangle(Brushes.DarkBlue, 0, 0, width, height)
                    g.SmoothingMode = SmoothingMode.HighQuality
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality

                    Using font = New Font("Tahoma", 6)

                        For Each vectorUid In scaledEmbeddings.Zip(data, Function(vector, entry) (vector, entry.UID))
                            Dim vector = vectorUid.vector
                            Dim uid = vectorUid.UID

                            g.DrawString(uid, font, Brushes.White, vector.X * width, vector.Y * height)
                        Next
                    End Using
                End Using

                bitmap.Save("Output-Label.png")
            End Using

            Dim colors = "#006400,#00008b,#b03060,#ff4500,#ffd700,#7fff00,#00ffff,#ff00ff,#6495ed,#ffdab9".Split(","c).[Select](Function(c) ColorTranslator.FromHtml(c)).[Select](Function(c) New SolidBrush(c)).ToArray()

            Using bitmap = New Bitmap(width, height)

                Using g = Graphics.FromImage(bitmap)
                    g.FillRectangle(Brushes.White, 0, 0, width, height)
                    g.SmoothingMode = SmoothingMode.HighQuality
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality

                    For Each vectorUid In scaledEmbeddings.Zip(data, Function(vector, entry) (vector, entry.UID))
                        Dim vector = vectorUid.vector
                        Dim uid = vectorUid.UID

                        g.FillEllipse(colors(Integer.Parse(uid)), CSng(vector.X * width), CSng(vector.Y * height), 10, 10)
                        ' g.FillEllipse(colors.First, CSng(vector.X * width), CSng(vector.Y * height), 5, 5)
                    Next
                End Using

                bitmap.Save("E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\MNIST-LabelledVectorArray-60000x100.png")
            End Using

            Console.WriteLine("Generated visualisation images")
            Console.WriteLine("Press [Enter] to terminuate..")
            Console.ReadLine()
        End Sub

        Public Shared Sub Main()
            ' Note: The MNIST data here consist of normalized vectors (so the CosineForNormalizedVectors distance function can be safely used)
            Dim data = MsgPackSerializer.Deserialize(Of LabelledVector())(File.ReadAllBytes(test_data))

            Call RunTest(data.Take(20_000).ToArray)
        End Sub
    End Class

    Public NotInheritable Class LabelledVector

        <MessagePackMember(0)>
        Public Property UID As String
        <MessagePackMember(1)>
        Public Property Vector As Single()

        Sub New()

        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
