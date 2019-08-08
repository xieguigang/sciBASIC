Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Public Interface IDynamicsComponent(Of T) : Inherits ICloneable(Of T)

    ''' <summary>
    ''' 获取的到这个动力学系统中的系统变量的个数
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property Width As Integer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="X">系统变量/样本数据</param>
    ''' <returns></returns>
    Function Evaluate(X As Vector) As Double
End Interface
