#Region "Microsoft.VisualBasic::492cd526ea4401f85ce10e1ded332e5d, Data_science\Mathematica\Math\Math\Algebra\Solvers\OLS.vb"

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

    '   Total Lines: 142
    '    Code Lines: 40 (28.17%)
    ' Comment Lines: 95 (66.90%)
    '    - Xml Docs: 74.74%
    ' 
    '   Blank Lines: 7 (4.93%)
    '     File Size: 8.18 KB


    '     Module OLS
    ' 
    '         Function: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace LinearAlgebra.Solvers

    ''' <summary>
    ''' **OLS（普通最小二乘法，Ordinary Least Squares）**的计算原理和用途。
    ''' OLS 是统计学和机器学习中最基础、也最重要的回归分析方法之一。它的核心目标是：**在众多数据点中，找到一条能“最佳拟合”这些点的直线（或超平面）。**
    ''' 
    ''' ---
    ''' 
    ''' ### 一、 OLS 的计算原理
    ''' 
    ''' 为了便于理解，我们以**一元线性回归**（只有一个自变量 $X$ 和一个因变量 $Y$）为例来讲解其数学原理。
    ''' #### 1. 核心思想：最小化误差的平方和
    ''' 假设我们有一组数据点 $(x_1, y_1), (x_2, y_2), ..., (x_n, y_n)$。我们想用一条直线 $Y = \beta_0 + \beta_1 X$ 来拟合这些数据。
    ''' 其中：
    ''' *   $\beta_1$ 是直线的斜率
    ''' *   $\beta_0$ 是直线的截距
    ''' 对于每一个点 $x_i$，模型预测的值是 $\hat{y}_i = \beta_0 + \beta_1 x_i$。
    ''' 但实际观测到的值是 $y_i$。两者之间会存在误差（在统计学中称为**残差**）：
    ''' $$e_i = y_i - \hat{y}_i = y_i - (\beta_0 + \beta_1 x_i)$$
    ''' **为什么用“平方”而不是绝对值？**
    ''' *   绝对值不容易进行求导等数学运算。
    ''' *   平方可以避免正负误差相互抵消。
    ''' *   平方会**对较大的误差给予更大的惩罚**，使得拟合直线尽量靠近所有点。
    ''' 因此，OLS 的目标函数（损失函数）是**残差平方和（RSS 或 SSE）**：
    ''' $$Q = \sum_{i=1}^n e_i^2 = \sum_{i=1}^n (y_i - \beta_0 - \beta_1 x_i)^2$$
    ''' 
    ''' #### 2. 数学求解：求导法
    ''' 
    ''' 我们要找到一组 $\beta_0$ 和 $\beta_1$，使得 $Q$ 最小。这需要用到微积分中的求导并令导数为 0。
    ''' 对 $\beta_0$ 求偏导并令其等于 0：
    ''' $$\frac{\partial Q}{\partial \beta_0} = -2 \sum_{i=1}^n (y_i - \beta_0 - \beta_1 x_i) = 0$$
    ''' 化简后得到：
    ''' $$\bar{y} = \beta_0 + \beta_1 \bar{x} \quad \text{（其中 } \bar{y}, \bar{x} \text{ 为均值）}$$
    ''' 即：$\beta_0 = \bar{y} - \beta_1 \bar{x}$
    ''' 对 $\beta_1$ 求偏导并令其等于 0：
    ''' $$\frac{\partial Q}{\partial \beta_1} = -2 \sum_{i=1}^n x_i(y_i - \beta_0 - \beta_1 x_i) = 0$$
    ''' 将 $\beta_0$ 的表达式代入上式，最终可以解得 $\beta_1$ 的公式：
    ''' $$\beta_1 = \frac{\sum_{i=1}^n (x_i - \bar{x})(y_i - \bar{y})}{\sum_{i=1}^n (x_i - \bar{x})^2}$$
    ''' *   分子是 $X$ 和 $Y$ 的协方差，分母是 $X$ 的方差。这说明回归系数本质上反映了 $X$ 和 $Y$ 协同变化的程度。
    ''' 
    ''' #### 3. 矩阵形式（多元线性回归）
    ''' 
    ''' 如果有多个自变量（比如 $X_1, X_2, ...$），公式可以用矩阵表示：
    ''' $$Y = X\beta + \epsilon$$
    ''' 通过矩阵微积分求解，最小二乘估计量为：
    ''' $$\hat{\beta} = (X^T X)^{-1} X^T Y$$
    ''' *(注：这要求矩阵 $X^T X$ 可逆，即自变量之间不能存在完全多重共线性)*
    ''' ---
    ''' 
    ''' ### 二、 OLS 的用途
    ''' 
    ''' OLS 不仅仅是一个数学公式，它在现实世界中有着极其广泛的应用：
    ''' 
    ''' #### 1. 预测
    ''' 
    ''' 这是最直接的用途。当我们建立了模型并计算出参数后，就可以输入新的 $X$ 值来预测未知的 $Y$ 值。
    ''' *   **例子**：已知房子的面积（$X$），预测房子的价格（$Y$）。
    ''' *   **例子**：根据历史广告投入（$X$）预测下个月的销售额（$Y$）。
    ''' 
    ''' #### 2. 关系量化与推断（解释变量间的关系）
    ''' 
    ''' OLS 可以告诉我们自变量对因变量的影响程度和方向。
    ''' *   **系数显著性检验（P值）**：通过计算 $\beta_1$ 的 t 统计量和 P值，我们可以判断 $X$ 对 $Y$ 的影响是否具有统计学显著性（即这种关系是真实存在的，还是偶然发生的）。
    ''' *   **例子**：在医学中，分析吸烟数量（$X$）与患肺癌概率（$Y$）的关系，在控制其他变量后，看吸烟的系数是否显著为正。
    ''' 
    ''' #### 3. 控制变量（隔离净效应）
    ''' 
    ''' 在多元 OLS 中，我们可以控制其他干扰因素，单独看某一个变量的影响。
    ''' *   **例子**：你想研究“教育水平”对“收入”的影响。如果不考虑其他因素，OLS 结果可能被夸大。但在多元 OLS 中，你可以把“工作经验”、“性别”、“家庭背景”作为控制变量加入模型，此时算出的“教育水平”的系数，就是**在保持其他条件不变的情况下**，教育对收入的净影响。
    ''' 
    ''' #### 4. 模型评估与基准
    ''' 
    ''' 在机器学习和数据科学中，OLS 常常作为**基线模型**。
    ''' *   在尝试复杂的机器学习算法（如随机森林、神经网络）之前，先跑一个 OLS 线性回归。如果复杂模型的预测效果连 OLS 都比不过，说明特征工程或数据本身存在严重问题。
    ''' *   衍生出的统计量如 $R^2$（决定系数），可以直观告诉我们模型解释了数据中多大比例的方差。
    ''' 
    ''' ---
    ''' 
    ''' ### 三、 补充：OLS 的经典假设（高斯-马尔可夫假设）
    ''' 
    ''' 为了让 OLS 算出来的结果是“最佳线性无偏估计量（BLUE）”，需要满足几个假设：
    ''' 1.  **线性关系**：因变量与自变量之间是线性关系。
    ''' 2.  **误差项条件均值为零**：$E(\epsilon|X) = 0$，即没有遗漏重要的解释变量。
    ''' 3.  **不存在完全多重共线性**：自变量之间不能有完全的线性相关关系。
    ''' 4.  **同方差性**：误差项的方差恒定，不随 $X$ 的变化而变化。
    ''' 5.  **误差项无自相关**：误差项之间相互独立（多见于时间序列数据）。
    ''' *(如果满足前4点，OLS 是无偏的；如果全部满足，OLS 是最有效率的)*
    ''' **总结来说：**
    ''' OLS 最小二乘法通过**最小化预测值与真实值之间的平方误差**，找到了一条最佳拟合线。它不仅在数学上极其优雅（有解析解），而且在实际中是进行**预测、因果推断、变量关系分析**的基石工具。
    ''' </summary>
    Public Module OLS

        ''' <summary>
        ''' OLS 最小二乘法
        ''' </summary>
        Public Function Solve(X As Double(,), y As Double(), nS As Integer, nP As Integer) As Double()
            ' X'X
            Dim XtX As Double(,) = New Double(nP - 1, nP - 1) {}
            For i = 0 To nP - 1
                For j = 0 To nP - 1
                    Dim sum As Double = 0
                    For k = 0 To nS - 1
                        sum += X(k, i) * X(k, j)
                    Next
                    XtX(i, j) = sum
                Next
            Next

            ' X'y
            Dim Xty As Double() = New Double(nP - 1) {}
            For i = 0 To nP - 1
                Dim sum As Double = 0
                For k = 0 To nS - 1
                    sum += X(k, i) * y(k)
                Next
                Xty(i) = sum
            Next

            ' 求逆
            Dim invXtX As Double(,) = MatrixOps.Inverse(XtX, strict:=True, throwSingularity:=False)
            If invXtX Is Nothing Then
                Dim result As Double() = New Double(nP - 1) {}
                result(0) = y.Average()
                Return result
            End If

            ' β = (X'X)^(-1) X'y
            Dim beta As Double() = New Double(nP - 1) {}
            For i = 0 To nP - 1
                Dim sum As Double = 0
                For j = 0 To nP - 1
                    sum += invXtX(i, j) * Xty(j)
                Next
                beta(i) = sum
            Next

            Return beta
        End Function
    End Module
End Namespace
