Imports System.ComponentModel

Public Enum NetworkSignalType

    ''' <summary>
    ''' 孤立节点信息（异常数据）
    ''' </summary>
    <Description("孤立节点信息（异常数据）")>
    Isolated = 1

    ''' <summary>
    ''' 递增信号（异常数据）
    ''' </summary>
    <Description("递增信号（异常数据）")>
    Increasing = 2

    ''' <summary>
    ''' 递减信号（异常数据）
    ''' </summary>
    <Description("递减信号（异常数据）")>
    Decreasing = 3

    ''' <summary>
    ''' 周期性震荡信号（好数据）
    ''' </summary>
    <Description("周期性震荡信号（好数据）")>
    Oscillating = 4

    ''' <summary>
    ''' 随机信号（好数据）
    ''' </summary>
    <Description("随机信号（好数据）")>
    RandomSignal = 5
End Enum
