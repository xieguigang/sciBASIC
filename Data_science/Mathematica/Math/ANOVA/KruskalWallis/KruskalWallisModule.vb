Imports System.Math

''' <summary>
''' Kruskal-Wallis 检验模块：提供完整的非参数多组比较检验功能
''' </summary>
''' <remarks>
''' ============================================
''' 基于 VB.NET 基础数学函数实现的 Kruskal-Wallis 检验模块
''' 适用于微生物丰度矩阵的非参数多组比较分析
'''
''' 功能说明：
'''   1. 对微生物丰度矩阵（行=物种/OTU，列=样本）逐行执行 Kruskal-Wallis 检验
'''   2. 自动处理结值（tied ranks）并应用校正因子
'''   3. 通过卡方分布计算精确 p 值（基于不完全 Gamma 函数的数值实现）
'''   4. 输出 H 统计量、自由度、p 值、各组平均秩等完整统计结果
'''
''' 数学原理：
'''   H = [12 / N(N+1)] * Σ(ni * (R̄i - (N+1)/2)²) - 结值校正
'''   p = 1 - CDF_ChiSquared(H, k-1)
'''   其中 CDF_ChiSquared 基于 Lanczos 近似的 Gamma 函数与不完全 Gamma 函数实现
''' ============================================
''' </remarks>
Public Module KruskalWallisModule

    ' ========================================================
    '  第一部分：基础数学函数
    '  实现 Gamma 函数、不完全 Gamma 函数、卡方分布 CDF
    '  全部基于 VB.NET 基础运算，不依赖外部统计库
    ' ========================================================

    ''' <summary>
    ''' Lanczos 近似系数（g=7, n=9），用于计算 Gamma 函数的对数
    ''' 参考：Lanczos, C. (1964). A precision approximation of the gamma function.
    ''' </summary>
    Private ReadOnly LanczosCoefficients As Double() = {
        0.99999999999980993,
        676.5203681218851,
        -1259.1392167224028,
        771.32342877765313,
        -176.61502916214059,
        12.507343278686905,
        -0.13857109526572012,
        0.0000099843695780195716,
        0.00000015056327351493116
    }

    ''' <summary>
    ''' Lanczos 近似参数 g
    ''' </summary>
    Private Const LanczosG As Double = 7.0

    ''' <summary>
    ''' 计算 Gamma 函数的对数 ln(Γ(x))
    ''' 使用 Lanczos 近似，精度约 15 位有效数字
    ''' </summary>
    ''' <param name="x">正实数</param>
    ''' <returns>ln(Γ(x))</returns>
    Public Function LogGamma(ByVal x As Double) As Double
        ' 对于 x < 0.5，利用反射公式：Γ(x) = π / (sin(πx) * Γ(1-x))
        If x < 0.5 Then
            Dim sinPiX As Double = Sin(PI * x)
            If Abs(sinPiX) < 1.0E-300 Then
                ' 极点处，返回极大值
                Return Double.MaxValue
            End If
            Return Log(PI / Abs(sinPiX)) - LogGamma(1.0 - x)
        End If

        ' Lanczos 近似：Γ(x) ≈ √(2π) * (x + g - 0.5)^(x-0.5) * e^(-(x+g-0.5)) * Ag(x)
        x = x - 1.0
        Dim ag As Double = LanczosCoefficients(0)

        For i As Integer = 1 To LanczosCoefficients.Length - 1
            ag = ag + LanczosCoefficients(i) / (x + i)
        Next

        Dim t As Double = x + LanczosG + 0.5
        Dim result As Double = 0.5 * Log(2.0 * PI) + (x + 0.5) * Log(t) - t + Log(ag)

        Return result
    End Function

    ''' <summary>
    ''' 计算 Gamma 函数 Γ(x)
    ''' 基于 LogGamma 的指数，注意大数溢出风险
    ''' </summary>
    ''' <param name="x">正实数</param>
    ''' <returns>Γ(x)</returns>
    Public Function GammaFunc(ByVal x As Double) As Double
        Dim lg As Double = LogGamma(x)
        If lg > 700 Then
            Return Double.PositiveInfinity
        End If
        Return Exp(lg)
    End Function

    ''' <summary>
    ''' 计算正则化不完全 Gamma 函数 P(a, x) = γ(a,x) / Γ(a)
    ''' 使用级数展开（当 x &lt; a+1 时）和连分数展开（当 x >= a+1 时）
    ''' 
    ''' P(a,x) = (1/Γ(a)) * ∫₀ˣ e^(-t) * t^(a-1) dt
    ''' </summary>
    ''' <param name="a">形状参数，a > 0</param>
    ''' <param name="x">积分上限，x >= 0</param>
    ''' <returns>P(a, x)，值域 [0, 1]</returns>
    Public Function RegularizedIncompleteGammaP(ByVal a As Double, ByVal x As Double) As Double
        ' 边界情况处理
        If x < 0 Then Return 0.0
        If x = 0 Then Return 0.0
        If a <= 0 Then Return 0.0

        If x < a + 1 Then
            ' 使用级数展开
            Return GammaSeries(a, x)
        Else
            ' 使用连分数展开，计算 Q(a,x) = 1 - P(a,x)
            Return 1.0 - GammaContinuedFraction(a, x)
        End If
    End Function

    ''' <summary>
    ''' 不完全 Gamma 函数的级数展开
    ''' P(a,x) = e^(-x) * x^a * Σ(n=0..∞) x^n / [Γ(a+1+n)]
    ''' 等价形式：P(a,x) = e^(-x) * x^a / Γ(a) * Σ(n=0..∞) x^n / [a*(a+1)*...*(a+n)]
    ''' </summary>
    Private Function GammaSeries(ByVal a As Double, ByVal x As Double) As Double
        Dim maxIterations As Integer = 1000
        Dim epsilon As Double = 0.000000000001

        ' 计算首项：x^a * e^(-x) / (a * Γ(a))
        ' 使用对数避免溢出
        Dim logFirstTerm As Double = a * Log(x) - x - LogGamma(a)
        Dim firstFactor As Double = Exp(logFirstTerm)

        ' 级数求和：Σ(n=0..∞) x^n / [a * (a+1) * ... * (a+n)]
        Dim term As Double = 1.0 / a  ' n=0 时的项
        Dim sum As Double = term

        For n As Integer = 1 To maxIterations
            term = term * x / (a + n)
            sum = sum + term
            If Abs(term) < Abs(sum) * epsilon Then
                Exit For
            End If
        Next

        Return firstFactor * sum
    End Function

    ''' <summary>
    ''' 不完全 Gamma 函数的连分数展开（计算 Q(a,x) = 1 - P(a,x)）
    ''' 使用修正 Lentz 算法求连分数值
    ''' Q(a,x) = e^(-x) * x^a / Γ(a) * CF(a,x)
    ''' </summary>
    Private Function GammaContinuedFraction(ByVal a As Double, ByVal x As Double) As Double
        Dim maxIterations As Integer = 1000
        Dim epsilon As Double = 0.000000000001
        Dim tiny As Double = 1.0E-30

        ' 连分数形式：
        ' CF = 1/(x+1-a+ 1*(1-a)/(x+3-a+ 2*(2-a)/(x+5-a+ ...)))
        ' 使用标准不完全 Gamma 函数的连分数表示

        ' 前置因子：e^(-x) * x^a / Γ(a)
        Dim logPrefix As Double = a * Log(x) - x - LogGamma(a)
        Dim prefix As Double = Exp(logPrefix)

        ' Lentz 算法
        Dim b As Double = x + 1.0 - a  ' b_1
        Dim c As Double = 1.0 / tiny
        Dim d As Double = 1.0 / b
        If Abs(b) < tiny Then d = 1.0 / tiny

        Dim h As Double = d

        For i As Integer = 1 To maxIterations
            Dim an As Double = -i * (i - a)  ' 分子系数
            b = b + 2.0                       ' 分母递增
            d = an * d + b
            If Abs(d) < tiny Then d = tiny
            d = 1.0 / d

            c = b + an / c
            If Abs(c) < tiny Then c = tiny

            Dim delta As Double = c * d
            h = h * delta

            If Abs(delta - 1.0) < epsilon Then
                Exit For
            End If
        Next

        Return prefix * h
    End Function

    ''' <summary>
    ''' 计算卡方分布的累积分布函数 CDF
    ''' CDF(x; k) = P(k/2, x/2) = γ(k/2, x/2) / Γ(k/2)
    ''' 其中 P 为正则化不完全 Gamma 函数
    ''' </summary>
    ''' <param name="x">卡方统计量，x >= 0</param>
    ''' <param name="k">自由度，正整数</param>
    ''' <returns>累积概率 P(X <= x)，值域 [0, 1]</returns>
    Public Function ChiSquaredCDF(ByVal x As Double, ByVal k As Integer) As Double
        If x <= 0 Then Return 0.0
        If k <= 0 Then Return 0.0

        Dim a As Double = k / 2.0
        Dim xHalf As Double = x / 2.0

        Return RegularizedIncompleteGammaP(a, xHalf)
    End Function

    ''' <summary>
    ''' 计算卡方分布的右尾概率（生存函数）
    ''' P(X > x) = 1 - CDF(x; k)
    ''' 即 Kruskal-Wallis 检验中计算 p 值所需的函数
    ''' </summary>
    ''' <param name="x">卡方统计量</param>
    ''' <param name="k">自由度</param>
    ''' <returns>右尾概率 p 值</returns>
    Public Function ChiSquaredPValue(ByVal x As Double, ByVal k As Integer) As Double
        If x <= 0 Then Return 1.0
        If k <= 0 Then Return 1.0

        Dim cdf As Double = ChiSquaredCDF(x, k)

        ' 防止浮点误差导致 p < 0
        Dim p As Double = 1.0 - cdf
        If p < 0 Then p = 0.0
        If p > 1 Then p = 1.0

        Return p
    End Function

    ' ========================================================
    '  第二部分：排名计算函数
    '  处理结值（tied values），采用平均秩法
    ' ========================================================

    ''' <summary>
    ''' 对一组数值进行排名，结值取平均秩
    ''' 例如：[3, 1, 4, 1, 5] → [3, 1.5, 4, 1.5, 5]
    ''' </summary>
    ''' <param name="values">待排名的数值数组</param>
    ''' <returns>排名数组，与输入等长</returns>
    Public Function ComputeRanks(ByVal values As Double()) As Double()
        Dim n As Integer = values.Length
        Dim ranks As Double() = New Double(n - 1) {}

        ' 创建索引数组，用于排序时追踪原始位置
        Dim indices As Integer() = New Integer(n - 1) {}
        For i As Integer = 0 To n - 1
            indices(i) = i
        Next

        ' 按值对索引进行升序排序（简单冒泡排序，适用于中小规模数据）
        ' 对于大规模数据可替换为快速排序
        For i As Integer = 0 To n - 2
            For j As Integer = 0 To n - 2 - i
                If values(indices(j)) > values(indices(j + 1)) Then
                    Dim temp As Integer = indices(j)
                    indices(j) = indices(j + 1)
                    indices(j + 1) = temp
                End If
            Next
        Next

        ' 分配排名，处理结值
        Dim iRank As Integer = 0
        While iRank < n
            Dim j As Integer = iRank + 1
            ' 找出所有与当前值相等的元素
            While j < n AndAlso values(indices(j)) = values(indices(iRank))
                j += 1
            End While

            ' 结值组：从 iRank 到 j-1，共 (j - iRank) 个相同值
            ' 平均秩 = (iRank+1 + iRank+2 + ... + j) / (j - iRank)
            '        = (iRank + 1 + j) / 2
            Dim avgRank As Double = (iRank + 1.0 + j) / 2.0

            For k As Integer = iRank To j - 1
                ranks(indices(k)) = avgRank
            Next

            iRank = j
        End While

        Return ranks
    End Function

    ''' <summary>
    ''' 快速排序版本：按值对索引数组排序（适用于大规模数据）
    ''' </summary>
    Private Sub QuickSortIndices(ByVal values As Double(), ByRef indices As Integer(), ByVal low As Integer, ByVal high As Integer)
        If low < high Then
            Dim pivotIndex As Integer = Partition(values, indices, low, high)
            QuickSortIndices(values, indices, low, pivotIndex - 1)
            QuickSortIndices(values, indices, pivotIndex + 1, high)
        End If
    End Sub

    Private Function Partition(ByVal values As Double(), ByRef indices As Integer(), ByVal low As Integer, ByVal high As Integer) As Integer
        Dim pivot As Double = values(indices(high))
        Dim i As Integer = low - 1

        For j As Integer = low To high - 1
            If values(indices(j)) <= pivot Then
                i += 1
                Dim temp As Integer = indices(i)
                indices(i) = indices(j)
                indices(j) = temp
            End If
        Next

        Dim tmp As Integer = indices(i + 1)
        indices(i + 1) = indices(high)
        indices(high) = tmp

        Return i + 1
    End Function

    ''' <summary>
    ''' 对一组数值进行排名（使用快速排序，适用于大规模数据）
    ''' 结值取平均秩
    ''' </summary>
    Public Function ComputeRanksFast(ByVal values As Double()) As Double()
        Dim n As Integer = values.Length
        Dim ranks As Double() = New Double(n - 1) {}

        Dim indices As Integer() = New Integer(n - 1) {}
        For i As Integer = 0 To n - 1
            indices(i) = i
        Next

        ' 使用快速排序
        QuickSortIndices(values, indices, 0, n - 1)

        ' 分配排名，处理结值
        Dim iRank As Integer = 0
        While iRank < n
            Dim j As Integer = iRank + 1
            While j < n AndAlso values(indices(j)) = values(indices(iRank))
                j += 1
            End While

            Dim avgRank As Double = (iRank + 1.0 + j) / 2.0
            For k As Integer = iRank To j - 1
                ranks(indices(k)) = avgRank
            Next

            iRank = j
        End While

        Return ranks
    End Function

    ''' <summary>
    ''' 计算结值校正因子 C
    ''' C = 1 - Σ(ti³ - ti) / (N³ - N)
    ''' 其中 ti 为每组结值的个数，N 为总样本量
    ''' 当无结值时 C = 1
    ''' </summary>
    ''' <param name="values">全部观测值数组</param>
    ''' <returns>校正因子 C，值域 (0, 1]</returns>
    Public Function ComputeTieCorrection(ByVal values As Double()) As Double
        Dim n As Integer = values.Length
        If n <= 1 Then Return 1.0

        ' 排序以便统计结值
        Dim sorted As Double() = CType(values.Clone(), Double())
        Array.Sort(sorted)

        Dim denominator As Double = n ^ 3 - n
        Dim numerator As Double = 0.0

        Dim i As Integer = 0
        While i < n
            Dim j As Integer = i + 1
            While j < n AndAlso sorted(j) = sorted(i)
                j += 1
            End While

            Dim tieCount As Integer = j - i
            If tieCount > 1 Then
                numerator += tieCount ^ 3 - tieCount
            End If

            i = j
        End While

        If denominator = 0 Then Return 1.0
        Return 1.0 - numerator / denominator
    End Function

    ' ========================================================
    '  第三部分：Kruskal-Wallis 检验核心函数
    ' ========================================================

    ''' <summary>
    ''' 对单个分类单元（物种/OTU）的丰度数据执行 Kruskal-Wallis 检验
    ''' 
    ''' 数学公式：
    '''   H = [12 / (N(N+1))] * Σ_i [ni * (R̄i - (N+1)/2)²]
    '''   校正后：H_corrected = H / C
    '''   其中 C = 1 - Σ(ti³ - ti) / (N³ - N)
    '''   p = P(χ² > H_corrected)，自由度 df = k - 1
    ''' </summary>
    ''' <param name="abundanceData">各组丰度数据的数组（外层数组=组，内层数组=该组样本的丰度值）</param>
    ''' <param name="groupNames">各组名称数组（可选）</param>
    ''' <param name="taxonName">分类单元名称（可选）</param>
    ''' <returns>KWResult 结构体，包含完整检验结果</returns>
    Public Function KruskalWallisTest(ByVal abundanceData As Double()(),
                                      Optional ByVal groupNames As String() = Nothing,
                                      Optional ByVal taxonName As String = "") As KWResult

        Dim result As New KWResult()
        result.TaxonName = taxonName
        result.IsValid = True

        ' ---------- 输入验证 ----------
        Dim k As Integer = abundanceData.Length  ' 组数

        If k < 2 Then
            result.IsValid = False
            result.InvalidReason = "组数不足：Kruskal-Wallis 检验至少需要 2 组数据。"
            Return result
        End If

        ' 计算各组样本量和总样本量
        Dim groupSizes As Integer() = New Integer(k - 1) {}
        Dim totalN As Integer = 0
        For i As Integer = 0 To k - 1
            groupSizes(i) = abundanceData(i).Length
            totalN += groupSizes(i)
            If groupSizes(i) = 0 Then
                result.IsValid = False
                result.InvalidReason = String.Format("第 {0} 组没有数据。", i + 1)
                Return result
            End If
        Next

        If totalN < 3 Then
            result.IsValid = False
            result.InvalidReason = "总样本量不足：至少需要 3 个观测值。"
            Return result
        End If

        result.TotalN = totalN
        result.GroupCount = k
        result.GroupSizes = groupSizes
        result.DegreesOfFreedom = k - 1

        ' ---------- 合并所有数据并计算排名 ----------
        Dim allValues As Double() = New Double(totalN - 1) {}
        Dim groupMembership As Integer() = New Integer(totalN - 1) {}
        Dim idx As Integer = 0
        For g As Integer = 0 To k - 1
            For j As Integer = 0 To groupSizes(g) - 1
                allValues(idx) = abundanceData(g)(j)
                groupMembership(idx) = g
                idx += 1
            Next
        Next

        ' 计算排名（使用快速排序版本）
        Dim ranks As Double() = ComputeRanksFast(allValues)

        ' ---------- 计算结值校正因子 ----------
        Dim tieCorrection As Double = ComputeTieCorrection(allValues)
        result.TieCorrectionFactor = tieCorrection

        ' ---------- 计算各组秩和与平均秩 ----------
        Dim groupRankSums As Double() = New Double(k - 1) {}
        For i As Integer = 0 To totalN - 1
            groupRankSums(groupMembership(i)) += ranks(i)
        Next

        Dim groupMeanRanks As Double() = New Double(k - 1) {}
        For g As Integer = 0 To k - 1
            groupMeanRanks(g) = groupRankSums(g) / groupSizes(g)
        Next

        result.GroupRankSums = groupRankSums
        result.GroupMeanRanks = groupMeanRanks

        ' ---------- 计算 H 统计量 ----------
        ' H = [12 / (N(N+1))] * Σ ni * (R̄i - (N+1)/2)²
        Dim expectedMeanRank As Double = (totalN + 1.0) / 2.0
        Dim hNumerator As Double = 0.0

        For g As Integer = 0 To k - 1
            Dim diff As Double = groupMeanRanks(g) - expectedMeanRank
            hNumerator += groupSizes(g) * diff * diff
        Next

        Dim hDenominator As Double = totalN * (totalN + 1.0) / 12.0
        Dim hUncorrected As Double = hNumerator / hDenominator

        result.HUncorrected = hUncorrected

        ' 应用结值校正
        Dim hCorrected As Double
        If tieCorrection > 0 Then
            hCorrected = hUncorrected / tieCorrection
        Else
            hCorrected = hUncorrected
        End If

        result.HStatistic = hCorrected

        ' ---------- 计算 p 值 ----------
        ' H 近似服从 χ²(k-1) 分布
        Dim pValue As Double = ChiSquaredPValue(hCorrected, k - 1)
        result.PValue = pValue

        Return result
    End Function

    ''' <summary>
    ''' 对微生物丰度矩阵执行逐行 Kruskal-Wallis 检验
    ''' 
    ''' 丰度矩阵格式：
    '''   - 行（第一维）：分类单元（物种/OTU/ASV）
    '''   - 列（第二维）：样本
    ''' 
    ''' groupLabels 格式：
    '''   - 长度等于样本数（矩阵列数）
    '''   - 每个元素为对应样本的分组标签
    ''' 
    ''' taxonNames 格式（可选）：
    '''   - 长度等于分类单元数（矩阵行数）
    '''   - 每个元素为对应分类单元的名称
    ''' </summary>
    ''' <param name="abundanceMatrix">丰度矩阵 [taxon, sample]</param>
    ''' <param name="groupLabels">样本分组标签数组</param>
    ''' <param name="taxonNames">分类单元名称数组（可选）</param>
    ''' <returns>KWResult 数组，每个元素对应一个分类单元的检验结果</returns>
    Public Function KruskalWallisMatrixTest(ByVal abundanceMatrix As Double(,),
                                            ByVal groupLabels As String(),
                                            Optional ByVal taxonNames As String() = Nothing) As KWResult()

        Dim nTaxa As Integer = abundanceMatrix.GetUpperBound(0) + 1   ' 行数=分类单元数
        Dim nSamples As Integer = abundanceMatrix.GetUpperBound(1) + 1 ' 列数=样本数

        ' 输入验证
        If groupLabels.Length <> nSamples Then
            Throw New ArgumentException("分组标签数量与样本数不匹配。")
        End If

        ' 获取唯一分组标签及其索引映射
        Dim uniqueGroups As New System.Collections.Generic.List(Of String)
        Dim groupIndexMap As New System.Collections.Generic.Dictionary(Of String, Integer)

        For Each label As String In groupLabels
            If Not groupIndexMap.ContainsKey(label) Then
                groupIndexMap(label) = uniqueGroups.Count
                uniqueGroups.Add(label)
            End If
        Next

        Dim k As Integer = uniqueGroups.Count  ' 组数

        ' 预计算每个分组包含的样本索引
        Dim groupSampleIndices As System.Collections.Generic.List(Of Integer)() =
            New System.Collections.Generic.List(Of Integer)(k - 1) {}
        For g As Integer = 0 To k - 1
            groupSampleIndices(g) = New System.Collections.Generic.List(Of Integer)
        Next

        For s As Integer = 0 To nSamples - 1
            Dim gIdx As Integer = groupIndexMap(groupLabels(s))
            groupSampleIndices(gIdx).Add(s)
        Next

        ' 逐行（逐分类单元）执行检验
        Dim results As KWResult() = New KWResult(nTaxa - 1) {}

        For t As Integer = 0 To nTaxa - 1
            ' 提取该分类单元在各组的丰度数据
            Dim groupData As Double()() = New Double(k - 1)() {}
            For g As Integer = 0 To k - 1
                Dim sampleCount As Integer = groupSampleIndices(g).Count
                groupData(g) = New Double(sampleCount - 1) {}
                For j As Integer = 0 To sampleCount - 1
                    groupData(g)(j) = abundanceMatrix(t, groupSampleIndices(g)(j))
                Next
            Next

            ' 获取分类单元名称
            Dim tName As String = ""
            If taxonNames IsNot Nothing AndAlso t < taxonNames.Length Then
                tName = taxonNames(t)
            Else
                tName = "Taxon_" & (t + 1).ToString()
            End If

            ' 执行检验
            results(t) = KruskalWallisTest(groupData, uniqueGroups.ToArray(), tName)
        Next

        Return results
    End Function

    ' ========================================================
    '  第四部分：结果输出与格式化
    ' ========================================================

    ''' <summary>
    ''' 将单个 KWResult 格式化为可读字符串
    ''' </summary>
    Public Function FormatKWResult(ByVal result As KWResult) As String
        Dim sb As New System.Text.StringBuilder()

        sb.AppendLine("========================================")
        sb.AppendLine(String.Format("  Kruskal-Wallis 检验结果"))
        sb.AppendLine("========================================")
        sb.AppendLine()

        If Not result.IsValid Then
            sb.AppendLine(String.Format("分类单元: {0}", result.TaxonName))
            sb.AppendLine(String.Format("检验无效: {0}", result.InvalidReason))
            Return sb.ToString()
        End If

        sb.AppendLine(String.Format("分类单元: {0}", result.TaxonName))
        sb.AppendLine(String.Format("总样本量 N = {0}", result.TotalN))
        sb.AppendLine(String.Format("组数 k = {0}", result.GroupCount))
        sb.AppendLine(String.Format("自由度 df = {0}", result.DegreesOfFreedom))
        sb.AppendLine()

        sb.AppendLine("--- 统计量 ---")
        sb.AppendLine(String.Format("H 统计量（校正后）  = {0:F6}", result.HStatistic))
        sb.AppendLine(String.Format("H 统计量（未校正）  = {0:F6}", result.HUncorrected))
        sb.AppendLine(String.Format("结值校正因子 C      = {0:F6}", result.TieCorrectionFactor))
        sb.AppendLine(String.Format("p 值                = {0:E6}", result.PValue))
        sb.AppendLine()

        ' 显著性判断
        Dim significance As String = "不显著 (p >= 0.05)"
        If result.PValue < 0.001 Then
            significance = "极显著 (***) (p < 0.001)"
        ElseIf result.PValue < 0.01 Then
            significance = "非常显著 (**) (p < 0.01)"
        ElseIf result.PValue < 0.05 Then
            significance = "显著 (*) (p < 0.05)"
        End If
        sb.AppendLine(String.Format("显著性: {0}", significance))
        sb.AppendLine()

        sb.AppendLine("--- 各组详情 ---")
        sb.AppendLine(String.Format("{0,-12} {1,10} {2,12} {3,14}", "组", "样本量", "秩和", "平均秩"))
        sb.AppendLine(New String("-"c, 50))
        For g As Integer = 0 To result.GroupCount - 1
            sb.AppendLine(String.Format("{0,-12} {1,10} {2,12:F2} {3,14:F4}",
                                        "Group " & (g + 1).ToString(),
                                        result.GroupSizes(g),
                                        result.GroupRankSums(g),
                                        result.GroupMeanRanks(g)))
        Next

        sb.AppendLine()
        sb.AppendLine("========================================")

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 将多个 KWResult 格式化为汇总表格字符串
    ''' 适用于微生物丰度矩阵的批量检验结果展示
    ''' </summary>
    Public Function FormatKWResultsTable(ByVal results As KWResult()) As String
        Dim sb As New System.Text.StringBuilder()

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("  Kruskal-Wallis 检验汇总表（微生物丰度矩阵逐行检验）")
        sb.AppendLine("==========================================================================================")
        sb.AppendLine()
        sb.AppendLine(String.Format("{0,-15} {1,8} {2,6} {3,12} {4,12} {5,12} {6,12} {7,8}",
                                    "分类单元", "N", "k", "H(校正)", "H(未校正)", "校正因子C", "p值", "显著性"))
        sb.AppendLine(New String("-"c, 90))

        For Each r As KWResult In results
            If r.IsValid Then
                Dim sig As String = ""

                If r.PValue < 0.001 Then
                    sig = "***"
                ElseIf r.PValue < 0.01 Then
                    sig = "**"
                ElseIf r.PValue < 0.05 Then
                    sig = "*"
                Else
                    sig = "ns"
                End If

                sb.AppendLine(String.Format("{0,-15} {1,8} {2,6} {3,12:F4} {4,12:F4} {5,12:F6} {6,12:E4} {7,8}",
                                            r.TaxonName, r.TotalN, r.GroupCount,
                                            r.HStatistic, r.HUncorrected,
                                            r.TieCorrectionFactor, r.PValue, sig))
            Else
                sb.AppendLine(String.Format("{0,-15} {1,8} {2,6} {3,12} {4,12} {5,12} {6,12} {7,8}",
                                            r.TaxonName, r.TotalN, r.GroupCount,
                                            "N/A", "N/A", "N/A", "N/A", "无效"))
            End If
        Next

        sb.AppendLine()
        sb.AppendLine("显著性标记: *** p<0.001, ** p<0.01, * p<0.05, ns 不显著")
        sb.AppendLine("==========================================================================================")

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 筛选出显著差异的分类单元
    ''' </summary>
    ''' <param name="results">检验结果数组</param>
    ''' <param name="alpha">显著性水平（默认 0.05）</param>
    ''' <returns>显著差异分类单元的 KWResult 数组</returns>
    Public Function GetSignificantTaxa(ByVal results As KWResult(),
                                       Optional ByVal alpha As Double = 0.05) As KWResult()
        Dim significant As New System.Collections.Generic.List(Of KWResult)

        For Each r As KWResult In results
            If r.IsValid AndAlso r.PValue < alpha Then
                significant.Add(r)
            End If
        Next

        ' 按 p 值升序排列
        significant.Sort(Function(a, b) a.PValue.CompareTo(b.PValue))

        Return significant.ToArray()
    End Function

    ''' <summary>
    ''' 对检验结果进行 Bonferroni 多重检验校正
    ''' </summary>
    ''' <param name="results">原始检验结果数组</param>
    ''' <returns>校正后的 p 值数组（与 results 等长）</returns>
    Public Function BonferroniCorrection(ByVal results As KWResult()) As Double()
        Dim m As Integer = 0
        For Each r As KWResult In results
            If r.IsValid Then m += 1
        Next

        Dim corrected As Double() = New Double(results.Length - 1) {}
        For i As Integer = 0 To results.Length - 1
            If results(i).IsValid Then
                corrected(i) = Math.Min(results(i).PValue * m, 1.0)
            Else
                corrected(i) = Double.NaN
            End If
        Next

        Return corrected
    End Function

    ''' <summary>
    ''' 对检验结果进行 Benjamini-Hochberg (BH) FDR 多重检验校正
    ''' </summary>
    ''' <param name="results">原始检验结果数组</param>
    ''' <returns>校正后的 p 值数组（与 results 等长）</returns>
    Public Function BenjaminiHochbergCorrection(ByVal results As KWResult()) As Double()
        Dim n As Integer = results.Length
        Dim corrected As Double() = New Double(n - 1) {}

        ' 收集有效结果的索引和 p 值
        Dim validIndices As New List(Of Integer)
        Dim validPValues As New List(Of Double)

        For i As Integer = 0 To n - 1
            If results(i).IsValid Then
                validIndices.Add(i)
                validPValues.Add(results(i).PValue)
            Else
                corrected(i) = Double.NaN
            End If
        Next

        If validIndices.Count = 0 Then Return corrected

        ' 按 p 值升序排序
        Dim order As Integer() = validIndices.ToArray()
        Array.Sort(validPValues.ToArray(), order)

        Dim m As Integer = validIndices.Count

        ' BH 校正：从最大 p 值开始，反向累积最小值
        Dim bhValues As Double() = New Double(m - 1) {}
        For i As Integer = 0 To m - 1
            bhValues(i) = validPValues(i) * m / (i + 1)
        Next

        ' 从后向前取最小值
        For i As Integer = m - 2 To 0 Step -1
            If bhValues(i) > bhValues(i + 1) Then
                bhValues(i) = bhValues(i + 1)
            End If
        Next

        ' 限制在 [0, 1] 范围内
        For i As Integer = 0 To m - 1
            bhValues(i) = Math.Min(Math.Max(bhValues(i), 0.0), 1.0)
        Next

        ' 写回结果
        For i As Integer = 0 To m - 1
            corrected(order(i)) = bhValues(i)
        Next

        Return corrected
    End Function
End Module
