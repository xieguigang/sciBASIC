#Region "Microsoft.VisualBasic::6b48a96c1a8ad817f74d09dbac6e20a7, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Output.vb"

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

    '   Total Lines: 54
    '    Code Lines: 38 (70.37%)
    ' Comment Lines: 4 (7.41%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (22.22%)
    '     File Size: 1.95 KB


    '     Class Output
    ' 
    '         Properties: probabilities, sortedClasses, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: feedNext, getDecision, layerFeedNext, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Convolutional

    Public Class Output : Inherits Layer

        Friend ReadOnly m_classes As String()

        Public ReadOnly Property sortedClasses As String()
        Public ReadOnly Property probabilities As Single()

        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Output
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer(), classes As String())
            Call MyBase.New(inputTensorDims)

            m_classes = classes
            probabilities = New Single(inputTensorDims(2) - 1) {}
            sortedClasses = New String(inputTensorDims(2) - 1) {}
        End Sub

        ''' <summary>
        ''' get a class label which its probability is the highest value.
        ''' </summary>
        ''' <returns></returns>
        Public Function getDecision() As String
            If inputTensor.data IsNot Nothing Then
                Call Array.Copy(m_classes, sortedClasses, m_classes.Length)
                Call Array.ConstrainedCopy(inputTensor.data, 0, probabilities, 0, m_classes.Length)
                Call Array.Sort(probabilities, sortedClasses)
                Call Array.Reverse(probabilities)
                Call Array.Reverse(sortedClasses)

                Call disposeInputTensor()
            End If

            Return sortedClasses(0)
        End Function

        Protected Overrides Function layerFeedNext() As Layer
            Throw New InvalidOperationException("the output layer cann't be feed to next layer!")
        End Function

        Public Overrides Function feedNext() As Layer
            Return layerFeedNext()
        End Function

        Public Overrides Function ToString() As String
            Return $"{m_classes.Length} class tags: [{m_classes.Take(6).JoinBy("; ")}...]"
        End Function
    End Class
End Namespace
