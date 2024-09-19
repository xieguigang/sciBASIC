Namespace LevenbergMarquardt

    ' Created by duy on 14/4/15.

    ''' <summary>
    ''' LmDatumError is an interface for evaluating error, Jacobian matrix and
    ''' Hessian matrix of a single piece of observed data
    ''' </summary>
    Public MustInherit Class LmDatumError
        ''' <summary>
        ''' Gets the total number of observed data
        ''' </summary>
        ''' <returns> An integer which is the number of observed data </returns>
        MustOverride ReadOnly Property NumData As Integer

        ''' <summary>
        ''' Evaluates value of the error function for the k-th observed data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model
        ''' @return </param>
        MustOverride Function eval(dataIdx As Integer, optParams As Double()) As Double

        ''' <summary>
        ''' Evaluates the Jacobian vector of the error function for the k-th observed
        ''' data that corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model
        ''' @return </param>
        MustOverride Function jacobian(dataIdx As Integer, optParams As Double()) As Double()

        ''' <summary>
        ''' Evaluates the Hessian matrix of the error function for the k-th observed
        ''' data that corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <param name="approxHessianFlg"> A boolean flag to indicate whether the Hessian
        '''                         matrix can be approximated instead of having to be
        '''                         computed exactly
        ''' @return </param>
        MustOverride Function hessian(dataIdx As Integer, optParams As Double(), approxHessianFlg As Boolean) As Double()()

        ''' <summary>
        ''' Evaluates the Hessian matrix of the error function for the k-th observed
        ''' data that corresponds to the parameter vector. The Hessian matrix is
        ''' computed exactly
        ''' </summary>
        ''' <param name="dataIdx"> </param>
        ''' <param name="optParams">
        ''' @return </param>
        Protected Function hessian(dataIdx As Integer, optParams As Double()) As Double()()
            Return hessian(dataIdx, optParams, False)
        End Function
    End Class

End Namespace
