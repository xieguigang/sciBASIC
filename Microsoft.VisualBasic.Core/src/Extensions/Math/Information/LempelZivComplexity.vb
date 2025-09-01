Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Statistics.Linq
Imports std = System.Math

Namespace Math.Information

    ''' <summary>
    ''' Lempel-Ziv复杂度（Lempel-Ziv Complexity, LZC）是一种用来衡量**序列（如字符串、时间序列等）结构化程度**或**随机性**的算法。
    ''' 它由以色列科学家Abraham Lempel和Jacob Ziv在1976年提出。其核心思想源于数据压缩领域——**一个序列的复杂度越高，它越难以被压缩**；
    ''' 反之，规律性强的序列则更容易被压缩。
    '''
    ''' 下面是一个表格，汇总了 Lempel-Ziv 复杂度的核心特性和应用领域：
    '''
    ''' | 特性维度           | 说明                                                                 |
    ''' | :---------------- | :------------------------------------------------------------------- |
    ''' | **核心原理**       | 通过计算将序列分解为最少数量的**唯一子串**所需的步骤来衡量复杂度         |
    ''' | **计算依据**       | 序列的可压缩性：越随机越难压缩，复杂度越高；越规则越易压缩，复杂度越低 |
    ''' | **算法类型**       | 属于**无参数**算法，结果仅依赖于输入数据本身，无需预先设置参数                   |
    ''' | **符号化方法**     | 通常先将原始数据（如时间序列）转换为符号序列（如二进制）再进行计算         |
    ''' | **主要复杂度类型** | 被归类为**Ⅰ型复杂度**，其值通常随序列随机性的增加而增加                   |
    ''' | **抗噪声能力**     | 经过改进的变体算法（如MEDLZC）展现出较好的抗噪声性能                      |
    ''' | **应用领域**       | 神经科学（如脑电图分析）、故障诊断、生物医学信号处理、时间序列分析、水声信号处理 |
    '''
    ''' ### 🧠 核心思想与直观理解
    '''
    ''' Lempel-Ziv复杂度的核心思想是：**一个序列的复杂度，可以用构建它所需的最少“新花样”（即独特的、从未出现过的子串）的数量来衡量**。
    '''
    ''' 举个例子来理解：
    ''' *   **序列 "00000000..."**：非常规则。从第二个字符开始，几乎都是在重复“0”这个已有的“花样”。因此，它的LZ复杂度会非常低。
    ''' *   **序列 "0100110001110000..."**：有一些局部重复的块，但整体模式不如上一个规则。构建它需要更多新的“片段”，复杂度中等。
    ''' *   **序列 "0110100011011110..."**：看起来非常随机，几乎每个新位置都可能需要一个新的“花样”。因此，它的LZ复杂度会很高。
    '''
    ''' ### 📊 计算方法（基本步骤）
    '''
    ''' 计算一个序列S的Lempel-Ziv复杂度C(S)，其基本步骤如下：
    '''
    ''' 1.  **初始化**：创建一个空字典（或列表）D，用于记录已经出现过的独特子串。设置两个指针i=0和j=1，初始化复杂度计数器c=0。
    ''' 2.  **扫描与匹配**：从序列开头开始扫描。检查子串S[i:j]（从i到j-1）是否已经存在于字典D中。
    '''     *   如果**存在**，则将j向右移动一位（扩展当前子串），继续检查更长的子串S[i:j]是否存在于D中。
    '''     *   如果**不存在**，则意味着发现了一个新的独特子串。将这个新子串S[i:j]添加到字典D中，然后将复杂度计数器c加1。接着将指针i移动到当前位置j，并将j设置为i+1，从新的位置开始下一轮检查。
    ''' 3.  **循环与终止**：重复步骤2，直到指针j扫描完整个序列。
    ''' 4.  **归一化（可选）**：为了便于在不同长度的序列之间进行比较，有时会对复杂度进行归一化处理，例如除以序列长度n或n/log₂(n)等。
    '''
    ''' 其数学定义可以表述为：将一个字符串分割成最小数量的独特子串所需的次数。
    ''' 
    ''' ### 🌐 主要应用领域
    '''
    ''' Lempel-Ziv复杂度因其无需预设参数、计算相对高效的特点，在许多领域得到了应用：
    '''
    ''' *   **神经科学与脑电图分析**：用于评估大脑活动的复杂性。例如，在麻醉过程中，大脑EEG信号的LZ复杂度会显著降低，反映了大脑信息处理能力的下降和意识的丧失。
    ''' *   **故障诊断与特征提取**：在机械系统、电力系统或水力系统中，通过分析振动信号、电流信号等时间序列的LZ复杂度，可以检测系统的异常状态或故障模式。例如，有研究将其应用于**抽水蓄能机组**的故障诊断。
    ''' *   **生物医学信号处理**：分析心率变异性、肌电信号等，以评估生理状态或疾病诊断。
    ''' *   **时间序列分析**：适用于任何领域的时间序列，如金融数据、地震波、风速、温度等，用于衡量其随机性和结构性。
    ''' *   **水声信号处理**：用于分析船舶辐射噪声等水下声学信号，进行特征提取和分类。
    '''
    ''' ### ⚠️ 重要特性与局限性
    '''
    ''' 1.  **对随机性的敏感度**：LZC是一种**Ⅰ型复杂度**度量，其值通常随序列随机性的增加而增加。最大随机性的序列会具有很高的LZ复杂度。
    ''' 2.  **符号化过程的信息丢失**：标准的LZC算法通常需要先将连续值的时间序列**转换为符号序列（如二进制序列）**，这个过程中可能会丢失原始信号的一部分信息。为了克服这个缺点，研究者提出了许多改进算法，例如**余弦相似度Lempel-Ziv复杂度**，它利用余弦相似度来保留更多信息。
    ''' 3.  **单一尺度限制**：标准LZC只在单一尺度上衡量复杂度。然而，真实世界的信号往往在不同时间尺度上表现出不同的特性。为此，发展出了**多尺度Lempel-Ziv复杂度**、**复合多尺度Lempel-Ziv复杂度**等变体，以捕捉多尺度的复杂性信息。
    ''' 4.  **计算效率**：对于超长序列，计算LZC可能需要较多的计算资源。
    '''
    ''' ### 💎 总结
    '''
    ''' Lempel-Ziv复杂度是一个强大而直观的工具，它通过衡量序列的“不可压缩性”来量化其复杂性。虽然它最初为数据压缩而生，
    ''' 但其应用已远超越此领域，广泛应用于从神经科学到工业故障诊断的诸多方面。
    ''' </summary>
    Public Class LempelZivComplexity

        Public Property Complexity As Integer
        Public Property NormalizedComplexity As Double

        Public Shared ReadOnly Property Zero As LempelZivComplexity
            Get
                Return New LempelZivComplexity
            End Get
        End Property

        ''' <summary>
        ''' 计算二进制序列的Lempel-Ziv复杂度
        ''' </summary>
        ''' <param name="sequence">二进制序列（由'0'和'1'组成的字符串）</param>
        ''' <returns>返回复杂度值c(n)和归一化复杂度C(n)</returns>
        Public Shared Function ComputeLZC(sequence As String) As LempelZivComplexity
            If sequence.StringEmpty Then
                Return LempelZivComplexity.Zero
            Else
                Return ComputeLZCString(sequence)
            End If
        End Function

        Private Shared Function ComputeLZCString(sequence As String) As LempelZivComplexity
            Dim n As Integer = sequence.Length
            Dim i As Integer = 0
            Dim j As Integer = 1
            Dim c As Integer = 0
            Dim dictionary As New HashSet(Of String)

            While i < n
                Dim currentSubstring As String = sequence.Substring(i, j - i)

                If dictionary.Contains(currentSubstring) Then
                    j += 1
                    If j > n Then
                        c += 1
                        Exit While
                    End If
                Else
                    dictionary.Add(currentSubstring)
                    c += 1
                    i = j
                    j = i + 1
                    If j > n Then
                        Exit While
                    End If
                End If
            End While

            ' 计算归一化复杂度 
            Dim b_n As Double = n / std.Log(n, 2)
            Dim normalizedC As Double = c / b_n

            Return New LempelZivComplexity With {
                .Complexity = c,
                .NormalizedComplexity = normalizedC
            }
        End Function

        ''' <summary>
        ''' 将数值序列转换为二进制序列（基于中值）用于LZC计算
        ''' </summary>
        ''' <param name="data">输入数值序列</param>
        ''' <returns>二进制序列字符串</returns>
        Public Shared Function ConvertToBinarySequence(data As Double()) As String
            If data.IsNullOrEmpty Then
                Return String.Empty
            Else
                ' 计算中值作为阈值
                Dim median As Double = data.Median
                Dim binarySeq As New StringBuilder()

                For Each value As Double In data
                    If value >= median Then
                        binarySeq.Append("1")
                    Else
                        binarySeq.Append("0")
                    End If
                Next

                Return binarySeq.ToString()
            End If
        End Function

        Public Shared Function ConvertToDecimalSequence(data As Double()) As String
            If data.IsNullOrEmpty Then
                Return String.Empty
            Else
                Dim range As New DoubleRange(data)
                Dim offset As New DoubleRange(0, 10)
                Dim i As Integer
                Dim decimals As New StringBuilder

                Static chars As Char() = "0123456789"

                For Each value As Double In data
                    i = CInt(range.ScaleMapping(value, offset))
                    decimals.Append(chars(i))
                Next

                Return decimals.ToString
            End If
        End Function

        Public Shared Function ConvertToHexadecimalSequence(data As Double()) As String
            If data.IsNullOrEmpty Then
                Return String.Empty
            Else
                Dim range As New DoubleRange(data)
                Dim offset As New DoubleRange(0, 16)
                Dim i As Integer
                Dim hexadecimal As New StringBuilder

                Static chars As Char() = "0123456789ABCDEF"

                For Each value As Double In data
                    i = CInt(range.ScaleMapping(value, offset))
                    hexadecimal.Append(chars(i))
                Next

                Return hexadecimal.ToString
            End If
        End Function

    End Class
End Namespace