#Region "Microsoft.VisualBasic::8abe50b19768480e0545cc65b3e0ca6f, Data_science\Mathematica\SignalProcessing\SignalProcessing\KalmanFilter\Algorithm.vb"

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

    '   Total Lines: 208
    '    Code Lines: 66 (31.73%)
    ' Comment Lines: 126 (60.58%)
    '    - Xml Docs: 48.41%
    ' 
    '   Blank Lines: 16 (7.69%)
    '     File Size: 9.21 KB


    '     Module Algorithm
    ' 
    '         Function: kalman1_filter, kalman2_filter
    ' 
    '         Sub: kalman1_init, kalman2_init
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace KalmanFilter

    Public Module Algorithm

        ' 
        '  @brief   
        '    Init fields of structure @kalman1_state.
        '    I make some defaults in this init function:
        '      A = 1;
        '      H = 1; 
        '    and @q,@r are valued after prior tests.
        ' 
        '    NOTES: Please change A,H,q,r according to your application.
        ' 
        '  @inputs  
        '    state - Klaman filter structure
        '    init_x - initial x state value   
        '    init_p - initial estimated error convariance
        '  @outputs 
        '  @retval  
        ' 

        ''' <summary>
        ''' Initializes the 1-dimensional Kalman filter state structure
        ''' </summary>
        ''' <param name="state">The Kalman filter state structure to initialize</param>
        ''' <param name="init_x">Initial state value (x0)</param>
        ''' <param name="init_p">Initial error covariance estimate (p0)</param>
        ''' <remarks>
        ''' Default configuration:
        ''' <list type="bullet">
        ''' <item><description>State transition matrix (A) = 1.0</description></item>
        ''' <item><description>Observation matrix (H) = 1.0</description></item>
        ''' <item><description>Process noise covariance (q) = 200.0</description></item>
        ''' <item><description>Measurement noise covariance (r) = 500.0</description></item>
        ''' </list>
        ''' Note: These parameters should be tuned for specific applications
        ''' </remarks>
        <Extension>
        Public Sub kalman1_init(state As kalman1_state, init_x As Single, init_p As Single)
            state.x = init_x
            state.p = init_p
            state.A = 1.0F
            state.H = 1.0F
            state.q = 200.0F '10e-6;  /* predict noise convariance 
            state.r = 500.0F '10e-5;  /* measure error convariance 
        End Sub

        ' 
        '  @brief   
        '    1 Dimension Kalman filter
        '  @inputs  
        '    state - Klaman filter structure
        '    z_measure - Measure value
        '  @outputs 
        '  @retval  
        '    Estimated result
        ' 

        ''' <summary>
        ''' Performs 1-dimensional Kalman filtering
        ''' </summary>
        ''' <param name="state">Kalman filter state structure</param>
        ''' <param name="z_measure">Measurement value</param>
        ''' <returns>Filtered state estimate</returns>
        ''' <remarks>
        ''' Implements standard Kalman filter equations:
        ''' 1. Prediction step (state and covariance)
        ''' 2. Measurement update (Kalman gain, state correction, covariance update)
        ''' </remarks>
        <Extension>
        Public Function kalman1_filter(state As kalman1_state, z_measure As Single) As Single
            ' Predict 
            state.x = state.A * state.x
            state.p = state.A * state.A * state.p + state.q ' p(n|n-1)=A^2*p(n-1|n-1)+q

            ' Measurement 
            state.gain = state.p * state.H / (state.p * state.H * state.H + state.r)
            state.x = state.x + state.gain * (z_measure - state.H * state.x)
            state.p = (1 - state.gain * state.H) * state.p

            Return state.x
        End Function

        ' 
        '  @brief   
        '    Init fields of structure @kalman1_state.
        '    I make some defaults in this init function:
        '      A = {{1, 0.1}, {0, 1}};
        '      H = {1,0}; 
        '    and @q,@r are valued after prior tests. 
        ' 
        '    NOTES: Please change A,H,q,r according to your application.
        ' 
        '  @inputs  
        '  @outputs 
        '  @retval  
        ' 

        ''' <summary>
        ''' Initializes the 2-dimensional Kalman filter state structure
        ''' </summary>
        ''' <param name="state">The Kalman filter state structure to initialize</param>
        ''' <param name="init_x">Initial state vector [x0, x1]</param>
        ''' <param name="init_p">Initial error covariance matrix (2x2)</param>
        ''' <remarks>
        ''' Default configuration:
        ''' <list type="bullet">
        ''' <item><description>State transition matrix (A) = [[1, 0.1], [0, 1]]</description></item>
        ''' <item><description>Observation matrix (H) = [1, 0]</description></item>
        ''' <item><description>Process noise covariance (q) = [1e-6, 1e-6]</description></item>
        ''' <item><description>Measurement noise covariance (r) = 1e-6</description></item>
        ''' </list>
        ''' Note: These parameters should be tuned for specific applications
        ''' </remarks>
        <Extension>
        Public Sub kalman2_init(state As kalman2_state, init_x As Single(), init_p As Single()())
            state.x(0) = init_x(0)
            state.x(1) = init_x(1)
            state.p(0)(0) = init_p(0)(0)
            state.p(0)(1) = init_p(0)(1)
            state.p(1)(0) = init_p(1)(0)
            state.p(1)(1) = init_p(1)(1)
            'state->A       = {{1, 0.1}, {0, 1}};
            state.A(0)(0) = 1.0F
            state.A(0)(1) = 0.1F
            state.A(1)(0) = 0F
            state.A(1)(1) = 1.0F
            'state->H       = {1,0};
            state.H(0) = 1.0F
            state.H(1) = 0F
            'state->q       = {{10e-6,0}, {0,10e-6}};  /* measure noise convariance 
            state.q(0) = 0.000001F
            state.q(1) = 0.000001F
            state.r = 0.000001F ' estimated error convariance
        End Sub

        ' 
        '  @brief   
        '    2 Dimension kalman filter
        '  @inputs  
        '    state - Klaman filter structure
        '    z_measure - Measure value
        '  @outputs 
        '    state->x[0] - Updated state value, Such as angle,velocity
        '    state->x[1] - Updated state value, Such as diffrence angle, acceleration
        '    state->p    - Updated estimated error convatiance matrix
        '  @retval  
        '    Return value is equals to state->x[0], so maybe angle or velocity.
        ' 

        ''' <summary>
        ''' Performs 2-dimensional Kalman filtering
        ''' </summary>
        ''' <param name="state">Kalman filter state structure</param>
        ''' <param name="z_measure">Measurement value</param>
        ''' <returns>Primary state estimate (x[0])</returns>
        ''' <remarks>
        ''' Updates both state vector elements (typically [position, velocity] or [angle, angular velocity])
        ''' and covariance matrix. Implements:
        ''' <list type="number">
        ''' <item><description>State prediction</description></item>
        ''' <item><description>Covariance prediction</description></item>
        ''' <item><description>Kalman gain calculation</description></item>
        ''' <item><description>State correction</description></item>
        ''' <item><description>Covariance update</description></item>
        ''' </list>
        ''' Returns the first element of the updated state vector (x[0])
        ''' </remarks>
        <Extension>
        Public Function kalman2_filter(state As kalman2_state, z_measure As Single) As Single
            Dim temp0 = 0.0F
            Dim temp1 = 0.0F
            Dim temp = 0.0F

            ' Step1: Predict 
            state.x(0) = state.A(0)(0) * state.x(0) + state.A(0)(1) * state.x(1)
            state.x(1) = state.A(1)(0) * state.x(0) + state.A(1)(1) * state.x(1)
            ' p(n|n-1)=A^2*p(n-1|n-1)+q 
            state.p(0)(0) = state.A(0)(0) * state.p(0)(0) + state.A(0)(1) * state.p(1)(0) + state.q(0)
            state.p(0)(1) = state.A(0)(0) * state.p(0)(1) + state.A(1)(1) * state.p(1)(1)
            state.p(1)(0) = state.A(1)(0) * state.p(0)(0) + state.A(0)(1) * state.p(1)(0)
            state.p(1)(1) = state.A(1)(0) * state.p(0)(1) + state.A(1)(1) * state.p(1)(1) + state.q(1)

            ' Step2: Measurement 
            ' gain = p * H^T * [r + H * p * H^T]^(-1), H^T means transpose. 
            temp0 = state.p(0)(0) * state.H(0) + state.p(0)(1) * state.H(1)
            temp1 = state.p(1)(0) * state.H(0) + state.p(1)(1) * state.H(1)
            temp = state.r + state.H(0) * temp0 + state.H(1) * temp1
            state.gain(0) = temp0 / temp
            state.gain(1) = temp1 / temp
            ' x(n|n) = x(n|n-1) + gain(n) * [z_measure - H(n)*x(n|n-1)]
            temp = state.H(0) * state.x(0) + state.H(1) * state.x(1)
            state.x(0) = state.x(0) + state.gain(0) * (z_measure - temp)
            state.x(1) = state.x(1) + state.gain(1) * (z_measure - temp)

            ' Update @p: p(n|n) = [I - gain * H] * p(n|n-1) 
            state.p(0)(0) = (1 - state.gain(0) * state.H(0)) * state.p(0)(0)
            state.p(0)(1) = (1 - state.gain(0) * state.H(1)) * state.p(0)(1)
            state.p(1)(0) = (1 - state.gain(1) * state.H(0)) * state.p(1)(0)
            state.p(1)(1) = (1 - state.gain(1) * state.H(1)) * state.p(1)(1)

            Return state.x(0)
        End Function
    End Module
End Namespace
