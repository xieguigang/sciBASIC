Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Public Interface IDynamicsComponent(Of T) : Inherits ICloneable(Of T)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="X">系统变量/样本数据</param>
    ''' <returns></returns>
    Function Evaluate(X As Vector) As Double
End Interface
