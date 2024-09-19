Namespace LevenbergMarquardt

    ' Created by duy on 27/1/15.


    ''' <summary>
    ''' LmScalarModel is an interface for models (functions) whose ranges are
    ''' single real-valued numbers
    ''' </summary>
    Public Interface LmScalarModel
        ''' <summary>
        ''' Gets the vector of real numbers containing measured output data,
        ''' </summary>
        ReadOnly Property MeasuredData As Double()

        ''' <summary>
        ''' Evaluates the model's estimated output for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Estimated output value produced by the model </returns>
        Function eval(dataIdx As Integer, optParams As Double()) As Double

        ''' <summary>
        ''' Computes the model's Jacobian vector for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Jacobian vector of the model for the specified input data </returns>
        Function jacobian(dataIdx As Integer, optParams As Double()) As Double()

        ''' <summary>
        ''' Computes the model's Hessian matrix for the k-th input data that
        ''' corresponds to the parameter vector
        ''' </summary>
        ''' <param name="dataIdx"> The index of the input data </param>
        ''' <param name="optParams"> A vector of real values of parameters in the model </param>
        ''' <returns> Hessian matrix of the model for the specified input data </returns>
        Function hessian(dataIdx As Integer, optParams As Double()) As Double()()
    End Interface

End Namespace
