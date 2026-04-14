
''' <summary>
''' 右端函数委托类型
''' 定义常微分方程组 dy/dt = f(t, y)
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态向量</param>
''' <param name="ydot">导数向量（输出）</param>
Public Delegate Sub RHSFunction(t As Double, y As NVector, ydot As NVector)

''' <summary>
''' Jacobian矩阵计算委托类型
''' 计算Jacobian矩阵 J = ∂f/∂y
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态向量</param>
''' <param name="fy">当前导数向量</param>
''' <param name="J">Jacobian矩阵（输出）</param>
Public Delegate Sub JacobianFunction(t As Double, y As NVector, fy As NVector, J As DenseMatrix)

''' <summary>
''' 根函数委托类型
''' </summary>
''' <param name="t">当前时间</param>
''' <param name="y">当前状态</param>
''' <param name="g">根函数值数组（输出）</param>
Public Delegate Sub RootFunction(t As Double, y As NVector, g As Double())