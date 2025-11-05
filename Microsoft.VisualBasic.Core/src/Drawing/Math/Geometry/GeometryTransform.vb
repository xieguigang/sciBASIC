Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' make geometry transform of a given <see cref="Polygon2D"/> with given <see cref="Transform"/> 
    ''' or <see cref="AffineTransform"/> parameters.
    ''' </summary>
    ''' <remarks>
    ''' + `Transform` (相似变换)** 是 **`AffineTransform` (仿射变换)** 的一个特例：从 Transform 到 AffineTransform：总是可以。因为 Transform 类所描述的变换（旋转、平移、缩放）是 AffineTransform 类（仿射变换）的一个子集。任何相似变换都可以用一个仿射变换矩阵来表示。
    ''' + `Transform` -> `AffineTransform`** 的转换是直接且总是可行的：从 AffineTransform 到 Transform：不一定可以。只有当 AffineTransform 不包含剪切效果时，才可以等价转换为一个 Transform 对象。如果 AffineTransform 包含剪切，那么它无法用 theta, scalex, scaley 这几个参数来唯一描述。
    ''' + `AffineTransform` -> `Transform`** 的转换需要检查是否存在剪切。通过检查矩阵左上角 2x2 部分的两个列向量是否正交（`a*b + d*e ≈ 0`）来判断。
    ''' 
    ''' ### 变换的矩阵表示
    ''' 
    ''' 一个 2D 点 (x, y) 用齐次坐标表示为 [x, y, 1]。变换矩阵 M 与之相乘得到新的点 [x', y', 1]。
    ''' 
    ''' #### Transform 的组合矩阵
    ''' 
    ''' Transform 类描述的变换顺序通常是：缩放 -> 旋转 -> 平移。
    ''' 
    ''' 缩放矩阵 (S):
    ''' 
    '''     | scalex   0      0 |
    '''     | 0        scaley 0 |
    '''     | 0        0      1 |
    '''     
    ''' 旋转矩阵 ® (注意：VB.NET 的 Math.Sin 和 Math.Cos 使用弧度):
    ''' 
    '''     | cos(theta)  -sin(theta)  0 |
    '''     | sin(theta)   cos(theta)  0 |
    '''     | 0            0           1 |
    '''     
    ''' 平移矩阵 (T):
    ''' 
    '''     | 1  0  tx |
    '''     | 0  1  ty |
    '''     | 0  0  1  |
    '''     
    ''' 最终的组合矩阵 M_transform 是 T * R * S：
    ''' 
    ''' M_transform = T * R * S =
    ''' | cos(theta)*scalex   -sin(theta)*scaley   tx |
    ''' | sin(theta)*scalex    cos(theta)*scaley   ty |
    ''' | 0                    0                   1  |
    ''' 
    ''' #### AffineTransform 的矩阵
    ''' 
    ''' AffineTransform 类直接定义了矩阵的元素：
    ''' 
    ''' M_affine =
    ''' | a  b  c |
    ''' | d  e  f |
    ''' | 0  0  1 |
    ''' 
    ''' 其中 x' = ax + by + c 和 y' = dx + ey + f。
    ''' </remarks>
    Public Interface GeometryTransform

        ''' <summary>
        ''' Apply the current transformation parameters to the target polygon object.
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        Function ApplyTo(polygon As Polygon2D) As Polygon2D

    End Interface

    Partial Module GeomTransform

        ''' <summary>
        ''' 将一个 Transform 对象等价转换为 AffineTransform 对象。
        ''' 此转换总是可行的。
        ''' </summary>
        ''' <param name="source">源 Transform 对象。</param>
        ''' <returns>等价的 AffineTransform 对象。</returns>
        ''' <remarks>
        ''' 注意：此函数假设 source.theta 的单位是**弧度**。
        ''' 如果您的角度是度数，请先使用 `angle * Math.PI / 180.0` 进行转换。
        ''' 
        ''' + a = cos(theta) * scalex
        ''' + b = -sin(theta) * scaley
        ''' + c = tx
        ''' + d = sin(theta) * scalex
        ''' + e = cos(theta) * scaley
        ''' + f = ty
        ''' </remarks>
        <Extension>
        Public Function ToAffineTransform(source As Transform) As AffineTransform
            If source Is Nothing Then
                Return Nothing
            End If

            Dim cosTheta As Double = std.Cos(source.theta)
            Dim sinTheta As Double = std.Sin(source.theta)

            Dim result As New AffineTransform With {
                .a = cosTheta * source.scalex,
                .b = -sinTheta * source.scaley,
                .c = source.tx,
                .d = sinTheta * source.scalex,
                .e = cosTheta * source.scaley,
                .f = source.ty
            }

            Return result
        End Function

        ''' <summary>
        ''' 尝试将一个 AffineTransform 对象等价转换为 Transform 对象。
        ''' </summary>
        ''' <param name="source">源 AffineTransform 对象。</param>
        ''' <returns>
        ''' 如果转换成功，返回等价的 Transform 对象。
        ''' 如果源变换包含剪切效果，无法转换，则返回 Nothing。
        ''' </returns>
        ''' <remarks>
        ''' 注意：返回的 theta 单位是**弧度**。
        ''' 
        ''' #### `AffineTransform` -> `Transform` (有条件)
        ''' 
        ''' 我们需要从 `a, b, d, e` 中反解出 `theta`, `scalex`, `scaley`。
        ''' 
        ''' 1.  **平移部分很简单**：
        ''' 
        '''     *   `tx = c`
        '''     *   `ty = f`
        ''' 
        ''' 2.  **旋转和缩放部分**：
        ''' 
        '''     我们有：
        '''     
        '''     *   `a = cos(theta) * scalex`
        '''     *   `d = sin(theta) * scalex`
        '''     *   `b = -sin(theta) * scaley`
        '''     *   `e = cos(theta) * scaley`
        ''' 
        '''     *   **计算 `theta`**:
        '''     
        '''         使用 `Math.Atan2(y, x)` 是最稳健的方法，它可以正确处理所有象限。
        '''         
        '''         `theta = Atan2(d, a)`  (因为 `tan(theta) = d/a`)
        ''' 
        '''     *   **计算 `scalex` 和 `scaley`**:
        '''     
        '''         利用三角恒等式 `sin²θ + cos²θ = 1`，我们可以避免除以零的问题，并且结果更精确。
        '''         
        '''         *   `a² + d² = (cos²θ + sin²θ) * scalex² = scalex²`
        '''         *   `b² + e² = (sin²θ + cos²θ) * scaley² = scaley²`
        '''         
        '''         所以：
        '''         
        '''         *   `scalex = sqrt(a² + d²)`
        '''         *   `scaley = sqrt(b² + e²)`
        ''' 
        ''' 3.  **检测剪切**：
        ''' 
        '''     一个纯粹的旋转+缩放矩阵，其两个列向量 `[a, d]` 和 `[b, e]` 是**正交**的（点积为0）。
        '''     
        '''     *   点积 = `a*b + d*e`
        '''     *   如果这个值不为0（考虑到浮点数精度，只要不为0），就说明存在剪切。
        '''     
        ''' </remarks>
        ''' 
        <Extension>
        Public Function ToTransform(source As AffineTransform) As Transform
            If source Is Nothing Then
                Return Nothing
            End If

            ' 用于浮点数比较的极小值
            Dim epsilon As Double = 0.000000001

            ' 检查是否存在剪切。
            ' 对于一个纯旋转+缩放的仿射变换，其左上2x2矩阵的两个列向量是正交的。
            ' 即 a*b + d*e 应该为 0。
            Dim shear As Double = source.a * source.b + source.d * source.e

            If std.Abs(shear) > epsilon Then
                ' 存在剪切，无法用 Transform 结构表示
                Call $"{source} is a shear transformation!".warning
                Return Nothing
            End If

            ' 如果没有剪切，则进行转换
            Dim result As New Transform With {
                .tx = source.c,
                .ty = source.f
            }

            ' 使用勾股定理计算缩放值，这比除法更稳定
            result.scalex = std.Sqrt(source.a * source.a + source.d * source.d)
            result.scaley = std.Sqrt(source.b * source.b + source.e * source.e)

            ' 使用 Atan2 计算旋转角度，这比 Atan 更可靠
            result.theta = std.Atan2(source.d, source.a)

            Return result
        End Function
    End Module

End Namespace