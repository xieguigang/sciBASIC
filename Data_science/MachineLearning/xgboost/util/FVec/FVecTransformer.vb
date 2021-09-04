Namespace util

    ''' <summary>
    ''' 构建矩阵所使用的方法
    ''' </summary>
    Public Module FVecTransformer

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">         float values </param>
        ''' <param name="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Single(), treatsZeroAsNA As Boolean) As FVec
            Return New FVecArray.FVecFloatArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">         double values </param>
        ''' <param name="treatsZeroAsNA"> treat zero as N/A if true </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Double(), treatsZeroAsNA As Boolean) As FVec
            Return New FVecArray.FVecDoubleArrayImpl(values, treatsZeroAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">          float values </param>
        ''' <param name="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Single(), treatsValueAsNA As Single) As FVec
            Return New FVecArray.FVecFloatArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from dense vector.
        ''' </summary>
        ''' <param name="values">          double values </param>
        ''' <param name="treatsValueAsNA"> treat specify value as N/A </param>
        ''' <returns> FVec </returns>
        Public Function fromArray(values As Double(), treatsValueAsNA As Double) As FVec
            Return New FVecArray.FVecDoubleArrayImplement(values, treatsValueAsNA)
        End Function

        ''' <summary>
        ''' Builds FVec from map.
        ''' </summary>
        ''' <param name="map"> map containing non-zero values </param>
        ''' <returns> FVec </returns>
        ''' <remarks>
        ''' 构建稀疏矩阵中的一行数据
        ''' </remarks>
        Public Function fromMap(Of T1 As IComparable)(map As IDictionary(Of Integer, T1)) As FVec
            Return New FVecMapImpl(Of T1)(map)
        End Function
    End Module

End Namespace