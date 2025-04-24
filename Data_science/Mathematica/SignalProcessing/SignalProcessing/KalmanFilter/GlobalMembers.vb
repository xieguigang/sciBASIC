Public Module GlobalMembers


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

    Public Sub kalman1_init(state As kalman1_state, init_x As Single, init_p As Single)
        state.x = init_x
        state.p = init_p
        state.A = 1F
        state.H = 1F
        state.q = 2e2F '10e-6;  /* predict noise convariance 
        state.r = 5e2F '10e-5;  /* measure error convariance 
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

    Public Sub kalman2_init(state As kalman2_state, init_x As Single(), init_p As Single()())
        state.x(0) = init_x(0)
        state.x(1) = init_x(1)
        state.p(0)(0) = init_p(0)(0)
        state.p(0)(1) = init_p(0)(1)
        state.p(1)(0) = init_p(1)(0)
        state.p(1)(1) = init_p(1)(1)
        'state->A       = {{1, 0.1}, {0, 1}};
        state.A(0)(0) = 1F
        state.A(0)(1) = 0.1F
        state.A(1)(0) = 0F
        state.A(1)(1) = 1F
        'state->H       = {1,0};
        state.H(0) = 1F
        state.H(1) = 0F
        'state->q       = {{10e-6,0}, {0,10e-6}};  /* measure noise convariance 
        state.q(0) = 10e-7F
        state.q(1) = 10e-7F
        state.r = 10e-7F ' estimated error convariance
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
