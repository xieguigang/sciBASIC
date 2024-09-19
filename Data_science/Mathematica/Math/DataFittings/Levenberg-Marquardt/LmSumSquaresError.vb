Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix

Namespace LevenbergMarquardt

    ''' <summary>
    ''' Created by duy on 27/1/15.
    ''' </summary>
    Public Class LmSumSquaresError : Inherits LmModelError
        Private modelField As LmScalarModel

        Public Sub New(model As LmScalarModel)
            modelField = model
        End Sub

        Public Overridable Property Model As LmScalarModel
            Get
                Return modelField
            End Get
            Set(value As LmScalarModel)
                modelField = value
            End Set
        End Property


        ''' <summary>
        ''' Evaluates the error function with input optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Double value of the error function </returns>
        Public Overrides Function eval(optParams As Double()) As Double
            Dim measuredOutputs = modelField.MeasuredData
            Dim numData = measuredOutputs.Length

            Dim errorSum = 0.0
            Dim i = 0

            While i < numData
                Dim [error] = measuredOutputs(i) - modelField.eval(i, optParams)
                errorSum += [error] * [error]
                Threading.Interlocked.Increment(i)
            End While

            Return 0.5 * errorSum
        End Function

        ''' <summary>
        ''' Computes the Jacobian vector of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <returns> Jacobian vector of the error function </returns>
        Public Overrides Function jacobian(optParams As Double()) As Double()
            Dim measuredOutputs = modelField.MeasuredData
            Dim numData = measuredOutputs.Length
            Dim numParams = optParams.Length

            Dim jVector = New Double(numParams - 1) {}
            Dim i = 0

            While i < numParams
                jVector(i) = 0.0
                Threading.Interlocked.Increment(i)
            End While

            i = 0

            While i < numData
                Dim modelJacobian = modelField.jacobian(i, optParams)
                Dim [error] = measuredOutputs(i) - modelField.eval(i, optParams)

                For k = 0 To numParams - 1
                    jVector(k) -= modelJacobian(k) * [error]
                Next

                Threading.Interlocked.Increment(i)
            End While

            Return jVector
        End Function

        ''' <summary>
        ''' Computes the Hessian matrix of the error function with input
        ''' optimization parameter values
        ''' </summary>
        ''' <param name="optParams"> A vector of real values of parameters used in optimizing
        '''                  the error function </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix can be approximated instead of having to be
        '''                         computed exactly. If {@code true}, the Hessian
        '''                         matrix will be approximated based on the Jacobian
        '''                         matrix </param>
        ''' <returns> Hessian matrix of the error function </returns>
        Public Overrides Function hessian(optParams As Double(), approxHessianFlg As Boolean) As Double()()
            Dim measuredOutputs = modelField.MeasuredData
            Dim numData = measuredOutputs.Length
            Dim numParams = optParams.Length

            ' JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
            ' ORIGINAL LINE: double[][] jVectors = new double[numData][numParams];
            Dim jVectors = RectangularArray.Matrix(Of Double)(numData, numParams)
            Dim i = 0

            While i < numData
                jVectors(i) = modelField.jacobian(i, optParams)
                Threading.Interlocked.Increment(i)
            End While
            Dim jacobianMat As Matrix = New Matrix(jVectors)
            Dim hessianMat As Matrix = CType(jacobianMat.Transpose(), Matrix) * jacobianMat

            If Not approxHessianFlg Then
                i = 0

                While i < numData
                    Dim [error] = measuredOutputs(i) - modelField.eval(i, optParams)
                    Dim modelHessian As Matrix = New Matrix(modelField.hessian(i, optParams))
                    hessianMat -= modelHessian * [error]
                    Threading.Interlocked.Increment(i)
                End While
            End If

            Return hessianMat.ArrayPack(deepcopy:=True)
        End Function
    End Class

End Namespace
