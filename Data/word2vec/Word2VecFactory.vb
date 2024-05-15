#Region "Microsoft.VisualBasic::509993fba5d2e87882c6f794c40a1ba4, Data\word2vec\Word2VecFactory.vb"

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

    '   Total Lines: 67
    '    Code Lines: 44
    ' Comment Lines: 5
    '   Blank Lines: 18
    '     File Size: 1.80 KB


    ' Class Word2VecFactory
    ' 
    '     Function: build, setAlpha, setAlphaThresold, setFreqThresold, setMethod
    '               setNumOfThread, setSample, setVectorSize, setWindow
    ' 
    ' /********************************************************************************/

#End Region

Public Class Word2VecFactory


    Friend vectorSize As Integer = 200
    Friend windowSize As Integer = 5

    Friend freqThresold As Integer = 5
    Friend trainMethod As TrainMethod = TrainMethod.Skip_Gram


    Friend sample As Double = 0.001
    '        private int negativeSample = 0;


    Friend alpha As Double = 0.025, alphaThreshold As Double = 0.0001

    Friend numOfThread As Integer = 1

    Public Function setVectorSize(size As Integer) As Word2VecFactory
        vectorSize = size
        Return Me
    End Function

    Public Function setWindow(size As Integer) As Word2VecFactory
        windowSize = size
        Return Me
    End Function

    Public Function setFreqThresold(thresold As Integer) As Word2VecFactory
        freqThresold = thresold
        Return Me
    End Function

    Public Function setMethod(method As TrainMethod) As Word2VecFactory
        trainMethod = method
        Return Me
    End Function

    Public Function setSample(rate As Double) As Word2VecFactory
        sample = rate
        Return Me
    End Function

    '        public Factory setNegativeSample(int sample){
    '            negativeSample = sample;
    '            return this;
    '        }

    Public Function setAlpha(alpha As Double) As Word2VecFactory
        Me.alpha = alpha
        Return Me
    End Function

    Public Function setAlphaThresold(alpha As Double) As Word2VecFactory
        alphaThreshold = alpha
        Return Me
    End Function

    Public Function setNumOfThread(numOfThread As Integer) As Word2VecFactory
        Me.numOfThread = numOfThread
        Return Me
    End Function

    Public Function build() As Word2Vec
        Return New Word2Vec(Me)
    End Function
End Class
