Imports std = System.Math

Namespace train

    ''' <summary>
    ''' 目标函数（损失）抽象。
    '''
    ''' 梯度约定：<c>grad = ∂L/∂pred</c>，<c>hess = ∂²L/∂pred²</c>，
    ''' 叶值 <c>w* = -G / (H + lambda)</c>。
    ''' </summary>
    Public MustInherit Class Loss

        Public MustOverride Function grad(pred As Double(), label As Double()) As Double()
        Public MustOverride Function hess(pred As Double(), label As Double()) As Double()
        Public MustOverride Function transform(pred As Double()) As Double()

        ''' <summary>
        ''' 一次同时产出梯度与海森。
        '''
        ''' 默认实现退化为两次调用；<see cref="LogisticLoss"/> 覆写它以复用同一次 sigmoid，
        ''' 避免每轮对全量样本重复计算 <c>exp()</c>（旧实现每轮多扫 2 遍全量数据）。
        ''' </summary>
        Public Overridable Sub grad_hess(pred As Double(), label As Double(),
                                         ByRef gradient As Double(), ByRef hessian As Double())
            gradient = grad(pred, label)
            hessian = hess(pred, label)
        End Sub

        ''' <summary>
        ''' 第 0 轮的初始预测值（加性模型的常数项）。
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        Public Overridable Function base_score(label As Double()) As Double
            If label Is Nothing OrElse label.Length = 0 Then
                Return 0
            End If

            Dim sum As Double = 0

            For Each x As Double In label
                sum += x
            Next

            Return sum / label.Length
        End Function

        ''' <summary>
        ''' 损失名称（用于序列化与参数校验）
        ''' </summary>
        Public MustOverride ReadOnly Property Name As String
    End Class

    ''' <summary>
    ''' 二次-线性（pseudo-Huber）回归损失：<c>L(r) = r²</c>（<c>|r| ≤ 1</c>），
    ''' <c>L(r) = 2|r| − 1</c>（<c>|r| &gt; 1</c>）。
    '''
    ''' 旧实现（继承自上游 tgboost）的梯度为 <c>1 / f((pred − label)²)</c>：
    ''' 由于平方丢失了符号，梯度恒为正——所有样本都会朝同一个方向推动预测值，
    ''' 优化方向与残差无关，模型不可能正确收敛；且 <c>x &lt; 1 → 1</c> 与
    ''' <c>x = log(x)</c> 两支不连续。本实现给出带正确符号的一阶/二阶导。
    ''' </summary>
    Friend Class QLinearLoss : Inherits Loss

        Public Overrides ReadOnly Property Name As String
            Get
                Return "qlinearloss"
            End Get
        End Property

        Public Overrides Function transform(pred As Double()) As Double()
            Return pred
        End Function

        Public Overrides Function grad(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                Dim r As Double = pred(i) - label(i)

                If r > 1 Then
                    ret(i) = 2.0
                ElseIf r < -1 Then
                    ret(i) = -2.0
                Else
                    ret(i) = r
                End If
            Next

            Return ret
        End Function

        Public Overrides Function hess(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            ' 线性区间的二阶导为 0，会导致叶值 -G/(H+λ) 爆炸；
            ' 用常数 1 让牛顿步退化为带步长的一阶更新（与 XGBoost 对 L1 的处理一致）。
            For i As Integer = 0 To ret.Length - 1
                ret(i) = 1.0
            Next

            Return ret
        End Function
    End Class

    ''' <summary>
    ''' 平方损失：<c>grad = pred − label</c>，<c>hess = 1</c>。
    ''' </summary>
    Friend Class SquareLoss : Inherits Loss

        Public Overrides ReadOnly Property Name As String
            Get
                Return "squareloss"
            End Get
        End Property

        Public Overrides Function transform(pred As Double()) As Double()
            Return pred
        End Function

        Public Overrides Function grad(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = pred(i) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = 1.0
            Next

            Return ret
        End Function
    End Class

    ''' <summary>
    ''' 逻辑损失（二分类）。
    '''
    ''' 与旧实现的差别：
    ''' + 求导使用**未裁剪**的概率：<c>grad = p − y</c>、<c>hess = p(1 − p)</c>。
    '''   旧实现先 <c>clip</c> 概率再求导，等价于把海森整体抬到 <c>1e-5</c> 的量级，
    '''   使叶子永远无法"纯"，也让 <c>min_child_weight</c> 的语义偏离二阶导；
    ''' + 裁剪只保留在 <see cref="transform"/>（输出概率）中；
    ''' + sigmoid 按 <c>z</c> 的符号分支计算，避免 <c>exp(-z)</c> 上溢。
    ''' </summary>
    Friend Class LogisticLoss : Inherits Loss

        Private Const CLIP_LOW As Double = 0.00001
        Private Const CLIP_HIGH As Double = 0.99999

        Public Overrides ReadOnly Property Name As String
            Get
                Return "logloss"
            End Get
        End Property

        ''' <summary>
        ''' 数值稳定的 sigmoid
        ''' </summary>
        Public Shared Function sigmoid(z As Double) As Double
            If z >= 0 Then
                Dim e As Double = std.Exp(-z)
                Return 1.0 / (1.0 + e)
            Else
                Dim e As Double = std.Exp(z)
                Return e / (1.0 + e)
            End If
        End Function

        Private Shared Function clip(val As Double) As Double
            If val < CLIP_LOW Then
                Return CLIP_LOW
            ElseIf val > CLIP_HIGH Then
                Return CLIP_HIGH
            Else
                Return val
            End If
        End Function

        Public Overrides Function transform(pred As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = clip(sigmoid(pred(i)))
            Next

            Return ret
        End Function

        Public Overrides Function grad(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = sigmoid(pred(i)) - label(i)
            Next

            Return ret
        End Function

        Public Overrides Function hess(pred As Double(), label As Double()) As Double()
            Dim ret = New Double(pred.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                Dim p As Double = sigmoid(pred(i))
                ret(i) = p * (1.0 - p)
            Next

            Return ret
        End Function

        Public Overrides Sub grad_hess(pred As Double(), label As Double(),
                                       ByRef gradient As Double(), ByRef hessian As Double())
            gradient = New Double(pred.Length - 1) {}
            hessian = New Double(pred.Length - 1) {}

            For i As Integer = 0 To pred.Length - 1
                Dim p As Double = sigmoid(pred(i))

                gradient(i) = p - label(i)
                hessian(i) = p * (1.0 - p)
            Next
        End Sub

        ''' <summary>
        ''' 二分类的常数项应为先验的 log-odds：<c>log(p̄ / (1 − p̄))</c>。
        ''' 旧实现固定为 0（等价于先验 0.5），类别不平衡时首轮残差很大、收敛变慢。
        ''' </summary>
        Public Overrides Function base_score(label As Double()) As Double
            If label Is Nothing OrElse label.Length = 0 Then
                Return 0
            End If

            Dim positive As Double = 0

            For Each y As Double In label
                positive += y
            Next

            Dim p As Double = clip(positive / label.Length)

            Return std.Log(p / (1.0 - p))
        End Function
    End Class
End Namespace
