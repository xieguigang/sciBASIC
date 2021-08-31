Imports System
Imports FVec = biz.k11i.xgboost.util.FVec
Imports ModelReader = biz.k11i.xgboost.util.ModelReader

Namespace biz.k11i.xgboost.gbm


    ''' <summary>
    ''' Linear booster implementation
    ''' </summary>
    <Serializable>
    Public Class GBLinear
        Inherits GBBase

        Private mparam As ModelParam
        Private weights As Single()

        Friend Sub New()
            ' do nothing
        End Sub

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: @Override public void loadModel(biz.k11i.xgboost.util.ModelReader reader, boolean ignored_with_pbuffer) throws java.io.IOException
        Public Overrides Sub loadModel(ByVal reader As ModelReader, ByVal ignored_with_pbuffer As Boolean)
            mparam = New ModelParam(reader)
            reader.readInt() ' read padding
            weights = reader.readFloatArray((mparam.num_feature + 1) * mparam.num_output_group)
        End Sub

        Public Overrides Function predict(ByVal feat As FVec, ByVal ntree_limit As Integer) As Double()
            Dim preds = New Double(mparam.num_output_group - 1) {}
            Dim gid = 0

            While gid < mparam.num_output_group
                preds(gid) = pred(feat, gid)
                Threading.Interlocked.Increment(gid)
            End While

            Return preds
        End Function

        Public Overrides Function predictSingle(ByVal feat As FVec, ByVal ntree_limit As Integer) As Double
            If mparam.num_output_group <> 1 Then
                Throw New InvalidOperationException("Can't invoke predictSingle() because this model outputs multiple values: " & mparam.num_output_group)
            End If

            Return pred(feat, 0)
        End Function

        Friend Overridable Function pred(ByVal feat As FVec, ByVal gid As Integer) As Double
            Dim psum As Double = bias(gid)
            Dim featValue As Double
            Dim fid = 0

            While fid < mparam.num_feature
                featValue = feat.fvalue(fid)

                If Not Double.IsNaN(featValue) Then
                    psum += featValue * weight(fid, gid)
                End If

                Threading.Interlocked.Increment(fid)
            End While

            Return psum
        End Function

        Public Overrides Function predictLeaf(ByVal feat As FVec, ByVal ntree_limit As Integer) As Integer()
            Throw New NotSupportedException("gblinear does not support predict leaf index")
        End Function

        Friend Overridable Function weight(ByVal fid As Integer, ByVal gid As Integer) As Single
            Return weights(fid * mparam.num_output_group + gid)
        End Function

        Friend Overridable Function bias(ByVal gid As Integer) As Single
            Return weights(mparam.num_feature * mparam.num_output_group + gid)
        End Function

        <Serializable>
        Friend Class ModelParam
            ' ! \brief number of features 
            Friend ReadOnly num_feature As Integer
            ' !
            '  \brief how many output group a single instance can produce
            '   this affects the behavior of number of output we have:
            '     suppose we have n instance and k group, output will be k*n
            ' 
            Friend ReadOnly num_output_group As Integer
            ' ! \brief reserved parameters 
            Friend ReadOnly reserved As Integer()

            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: Method 'throws' clauses are not available in .NET:
            'ORIGINAL LINE: ModelParam(biz.k11i.xgboost.util.ModelReader reader) throws java.io.IOException
            Friend Sub New(ByVal reader As ModelReader)
                num_feature = reader.readInt()
                num_output_group = reader.readInt()
                reserved = reader.readIntArray(32)
                reader.readInt() ' read padding
            End Sub
        End Class
    End Class
End Namespace
