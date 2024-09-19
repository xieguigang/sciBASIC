Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace LevenbergMarquardt

    ''' <summary>
    ''' Created by duy on 1/4/15.
    ''' </summary>
    Public Class LmSumError : Inherits LmModelError
        ' Instance to compute the error of a model on a single instance of data
        Private datumErrorField As LmDatumError

        Public Sub New(datumError As LmDatumError)
            datumErrorField = datumError
        End Sub

        Public Overridable ReadOnly Property DatumError As LmDatumError
            Get
                Return datumErrorField
            End Get
        End Property

        ''' <summary>
        ''' Evaluates the error function with input optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Double value of the error function </returns>
        Public Overrides Function eval(optParams As Double()) As Double
            Dim numData = datumErrorField.NumData
            Dim sumError = 0.0

            Dim i = 0

            While i < numData
                sumError += datumErrorField.eval(i, optParams)
                Threading.Interlocked.Increment(i)
            End While

            Return sumError
        End Function

        ''' <summary>
        ''' Computes the Jacobian vector of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Jacobian vector of the error function </returns>
        Public Overrides Function jacobian(optParams As Double()) As Double()
            Dim numData = datumErrorField.NumData
            Dim numParams = optParams.Length
            Dim jacobianVector = New Double(numParams - 1) {}

            Dim i = 0

            While i < numParams
                jacobianVector(i) = 0.0
                Threading.Interlocked.Increment(i)
            End While

            i = 0

            While i < numData
                Dim datumJacobian = datumErrorField.jacobian(i, optParams)
                Dim j = 0

                While j < numParams
                    jacobianVector(j) += datumJacobian(j)
                    Threading.Interlocked.Increment(j)
                End While

                Threading.Interlocked.Increment(i)
            End While

            Return jacobianVector
        End Function

        ''' <summary>
        ''' Computes the Hessian matrix of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix can be approximated instead of having to be
        '''                         computed exactly. </param>
        ''' <returns> Hessian matrix of the error function </returns>
        Public Overrides Function hessian(optParams As Double(), approxHessianFlg As Boolean) As Double()()
            Dim numData = datumErrorField.NumData
            Dim numParams = optParams.Length
            ' JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            ' ORIGINAL LINE: double[][] hessianMat = new double[numParams][numParams];
            Dim hessianMat = RectangularArray.Matrix(Of Double)(numParams, numParams)

            Dim i = 0

            While i < numParams
                Dim j = 0

                While j < numParams
                    hessianMat(i)(j) = 0.0
                    Threading.Interlocked.Increment(j)
                End While

                Threading.Interlocked.Increment(i)
            End While

            Dim k = 0

            While k < numData
                Dim datumHessian = datumErrorField.hessian(k, optParams, approxHessianFlg)

                i = 0

                While i < numParams
                    Dim j = 0

                    While j < numParams
                        hessianMat(i)(j) += datumHessian(i)(j)
                        Threading.Interlocked.Increment(j)
                    End While

                    Threading.Interlocked.Increment(i)
                End While

                Threading.Interlocked.Increment(k)
            End While

            Return hessianMat
        End Function
    End Class

End Namespace
