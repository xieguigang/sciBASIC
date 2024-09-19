Imports Microsoft.VisualBasic.Linq
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports std = System.Math

Namespace LevenbergMarquardt

    ''' <summary>
    ''' levenberg-marquardt
    ''' 
    ''' A lightweight implementation of Levenberg-Marquardt algorithm
    ''' 
    ''' ### Augmented normal equation
    ''' 
    ''' ```
    ''' (H + uI) * h = -g
    ''' ```
    ''' 
    ''' where:
    ''' 
    ''' + H is the Hessian matrix of the chi-squared error function
    ''' + g is the gradient (Jacobian) vector of the chi-squared error function
    ''' + u is the damping value
    ''' 
    ''' ### Adjusting damping value
    ''' 
    ''' Damping value is adjusted at each iteration. The adjustment follows the 
    ''' algorithm presented in Methods for non-linear least squares problems by
    ''' Kaj Madsen, Hans Bruun Nielsen, Ole Tingleff. The lecture note can be 
    ''' downloaded [here](http://www.imm.dtu.dk/pubdb/views/edoc_download.php/3215/pdf/imm3215.pdf).
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/truongduy134/levenberg-marquardt
    ''' </remarks>
    Public Class LmSolver
        ' Configuration parameters for Levenberg-Marquadt algorithm
        Private dampingFactorField As Double
        Private maxNumIterField As Integer
        Private gradientEpsilonField As Double
        Private changeEpsilonField As Double

        Private errorFuncField As LmModelError

        Public Sub New(inErrorFunc As LmModelError)
            dampingFactorField = 0.001
            maxNumIterField = 10
            gradientEpsilonField = 0.00000001
            changeEpsilonField = 0.00000001
            errorFuncField = inErrorFunc
        End Sub

        Public Sub New(errorFunc As LmModelError, damping As Double, maxNumIter As Integer, gradientEpsilon As Double, changeEpsilon As Double)
            dampingFactorField = damping
            maxNumIterField = maxNumIter
            gradientEpsilonField = gradientEpsilon
            changeEpsilonField = changeEpsilon
            errorFuncField = errorFunc
        End Sub

        Public Overridable Property DampingFactor As Double
            Get
                Return dampingFactorField
            End Get
            Set(value As Double)
                dampingFactorField = value
            End Set
        End Property


        Public Overridable Property MaxNumIter As Integer
            Get
                Return maxNumIterField
            End Get
            Set(value As Integer)
                maxNumIterField = value
            End Set
        End Property

        Public Overridable ReadOnly Property ErrorFunc As LmModelError
            Get
                Return errorFuncField
            End Get
        End Property


        Public Overridable Property GradientEpsilon As Double
            Get
                Return gradientEpsilonField
            End Get
            Set(value As Double)
                gradientEpsilonField = value
            End Set
        End Property


        Public Overridable Property ChangeEpsilon As Double
            Get
                Return changeEpsilonField
            End Get
            Set(value As Double)
                changeEpsilonField = value
            End Set
        End Property


        ''' <summary>
        ''' Applies Levenberg-Marquadt algorithm on the input error function with the
        ''' input initial guess of optimization parameters
        ''' </summary>
        ''' <param name="optParams"> A vector of initial guess of values of parameters
        '''                  for optimization </param>
        ''' <param name="paramHandler"> A handler which is called to adjust values of
        '''                     the Levenberg-Marquadt parameters after they are
        '''                     updated at the end of each iteration in the algorithm.
        '''                     If {@code paramHandler} is null, no further adjustment
        '''                     to the updated parameters is performed. This is useful
        '''                     when Levenberg-Marquadt algorithm is performed on
        '''                     structures such as quaternions. Note that the
        '''                     way updated parameters are modified can affect
        '''                     correctness of the Levenberg-Marquadt algorithm </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix used in the Levenberg-Marquadt algorithm
        '''                         should be approximated or computed exactly. If
        '''                         {@code true}, the Hessian matrix will be
        '''                         approximated by the Jacobian matrix </param>
        Public Overridable Sub solve(optParams As Double(), paramHandler As LmParamHandler, approxHessianFlg As Boolean)
            Dim iter = 0
            Dim numOptParams = optParams.Length
            Dim penaltyFactor = 2.0
            Dim lambda = 0.0
            Dim i = 0
            Dim errValue = errorFuncField.eval(optParams)

            While iter < maxNumIterField
                iter += 1

                ' Compute gradient vector
                Dim gradient = errorFuncField.jacobian(optParams)
                Dim gradientMat As Matrix = New Matrix(gradient, numOptParams)
                If gradientMat.NormInf() < gradientEpsilonField Then
                    Exit While
                End If

                ' Compute modified Hessian matrix
                Dim modifiedHessian = errorFuncField.hessian(optParams, approxHessianFlg)
                If iter = 1 Then
                    ' Initialize damping value on the first iteration
                    Dim diagonalMax = modifiedHessian(0)(0)
                    i = 1

                    While i < modifiedHessian.Length
                        diagonalMax = std.Max(diagonalMax, modifiedHessian(i)(i))
                        Threading.Interlocked.Increment(i)
                    End While
                    lambda = dampingFactorField * diagonalMax
                End If
                i = 0

                While i < numOptParams
                    modifiedHessian(i)(i) += lambda
                    Threading.Interlocked.Increment(i)
                End While
                Dim modifiedHessianMat As Matrix = New Matrix(modifiedHessian)

                ' Solve augmented normal equation
                Dim direction = JamaHelper.solvePSDMatrixEq(modifiedHessianMat, -gradientMat)
                If direction Is Nothing Then
                    ' Modified Hessian matrix is not positive definite
                    lambda *= penaltyFactor
                    penaltyFactor *= 2.0
                    Continue While
                End If

                ' Stop if the change in optimized parameter vectors is negligible
                Dim paramVector As Matrix = New Matrix(optParams, numOptParams)
                If direction.NormF() < changeEpsilonField * (paramVector.NormF() + changeEpsilonField) Then
                    Exit While
                End If

                Dim newOptParams As Double() = CType(paramVector + direction, Matrix).RowVectors().IteratesALL().ToArray()

                ' Compute gain ratio between actual and predicted gain
                Dim newErrValue = errorFuncField.eval(newOptParams)
                Dim predictedGain = 0.5 * (lambda * JamaHelper.dotProduct(direction, direction) - JamaHelper.dotProduct(gradientMat, direction))
                Dim gainRatio = (errValue - newErrValue) / predictedGain

                ' Update optimized parameter vector and
                ' damping value in augmented normal equation
                If gainRatio > 0 Then
                    i = 0

                    While i < numOptParams
                        optParams(i) = newOptParams(i)
                        Threading.Interlocked.Increment(i)
                    End While
                    errValue = newErrValue
                    penaltyFactor = 2.0
                    lambda *= std.Max(1.0 / 3.0, 1 - std.Pow(2.0 * gainRatio - 1, 3))
                Else
                    lambda *= penaltyFactor
                    penaltyFactor *= 2.0
                End If

                ' Adjust updated values of the parameters
                If paramHandler IsNot Nothing Then
                    paramHandler.adjust(optParams)
                End If
            End While
        End Sub

        ''' <summary>
        ''' Applies Levenberg-Marquadt algorithm on the input error function with the
        ''' input initial guess of optimization parameters. Note that the Hessian
        ''' matrix used in the Levenberg-Marquadt will be computed exactly
        ''' </summary>
        ''' <param name="optParams"> A vector of initial guess of values of parameters
        '''                  for optimization </param>
        Public Overridable Sub solve(optParams As Double())
            solve(optParams, Nothing, False)
        End Sub

        ''' <summary>
        ''' Applies Levenberg-Marquadt algorithm on the input error function with the
        ''' input initial guess of optimization parameters.
        ''' </summary>
        ''' <param name="optParams"> A vector of initial guess of values of parameters
        '''                  for optimization </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix used in the Levenberg-Marquadt algorithm
        '''                         should be approximated or computed exactly. If
        '''                         {@code true}, the Hessian matrix will be
        '''                         approximated by the Jacobian matrix </param>
        Public Overridable Sub solve(optParams As Double(), approxHessianFlg As Boolean)
            solve(optParams, Nothing, approxHessianFlg)
        End Sub

        ''' <summary>
        ''' Applies Levenberg-Marquadt algorithm on the input error function with the
        ''' input initial guess of optimization parameters. Note that the Hessian
        ''' matrix used in the Levenberg-Marquadt will be computed exactly
        ''' </summary>
        ''' <param name="optParams"> A vector of initial guess of values of parameters
        '''                  for optimization </param>
        ''' <param name="paramHandler"> A handler which is called to adjust values of
        '''                     the Levenberg-Marquadt parameters after they are
        '''                     updated at the end of each iteration in the algorithm.
        '''                     If {@code paramHandler} is null, no further adjustment
        '''                     to the updated parameters is performed. This is useful
        '''                     when Levenberg-Marquadt algorithm is performed on
        '''                     structures such as quaternions. Note that the
        '''                     way updated parameters are modified can affect
        '''                     correctness of the Levenberg-Marquadt algorithm </param>
        Public Overridable Sub solve(optParams As Double(), paramHandler As LmParamHandler)
            solve(optParams, paramHandler, False)
        End Sub

        Private Shared Function approximateHessian(jacobian As Matrix) As Matrix
            Return CType(jacobian.Transpose(), Matrix) * jacobian
        End Function
    End Class

End Namespace
