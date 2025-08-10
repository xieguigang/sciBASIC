Imports std = System.Math

Public Class PIDController

    ' PID参数
    Private Kp As Double
    Private Ki As Double
    Private Kd As Double

    ' 状态变量
    Private integralSum As Double = 0.0
    Private prevMeasurement As Double = 0.0 ' 用于改进微分项

    ' 抗饱和配置
    Private outputMin As Double = Double.MinValue
    Private outputMax As Double = Double.MaxValue
    Private integralMin As Double = Double.MinValue
    Private integralMax As Double = Double.MaxValue

    Public ReadOnly Property prevError As Double = 0.0

    Public Sub New(kp As Double, ki As Double, kd As Double,
                  Optional outMin As Double = Double.MinValue,
                  Optional outMax As Double = Double.MaxValue,
                  Optional intMin As Double = Double.MinValue,
                  Optional intMax As Double = Double.MaxValue)

        If ki < 0 OrElse kd < 0 Then
            Throw New ArgumentException("Ki and Kd must be non-negative")
        End If

        Me.Kp = kp
        Me.Ki = ki
        Me.Kd = kd
        Me.outputMin = outMin
        Me.outputMax = outMax
        Me.integralMin = intMin
        Me.integralMax = intMax
    End Sub

    ''' <summary>
    ''' 核心计算函数（带抗饱和和噪声抑制）
    ''' </summary>
    ''' <param name="setpoint"></param>
    ''' <param name="measurement"></param>
    ''' <param name="deltaTime"></param>
    ''' <returns></returns>
    Public Function Calculate(setpoint As Double, measurement As Double, deltaTime As Double) As Double
        Dim [error] As Double = setpoint - measurement

        ' 1. 比例项计算
        Dim proportional = Kp * [error]

        ' 2. 积分项计算（带限幅）
        integralSum += [error] * deltaTime
        integralSum = std.Max(integralMin, std.Min(integralSum, integralMax)) ' 积分限幅

        ' 3. 改进微分项（对测量值微分而非误差，减少设定值突变影响）
        Dim derivative = If(deltaTime > 0, Kd * (measurement - prevMeasurement) / deltaTime, 0)
        prevMeasurement = measurement

        ' 4. 计算原始输出
        Dim output = proportional + Ki * integralSum - derivative ' 注意微分项符号

        ' 5. 输出限幅与抗饱和处理
        If output > outputMax Then
            output = outputMax
            ' 遇限削弱积分：仅累加与输出方向相反的误差
            If [error] > 0 Then integralSum -= [error] * deltaTime
        ElseIf output < outputMin Then
            output = outputMin
            If [error] < 0 Then integralSum -= [error] * deltaTime
        End If

        _prevError = [error]

        Return output
    End Function

    ''' <summary>
    ''' 重置控制器状态
    ''' </summary>
    Public Sub Reset()
        integralSum = 0
        _prevError = 0
        prevMeasurement = 0
    End Sub
End Class