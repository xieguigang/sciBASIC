Namespace NlpVec

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

End Namespace