Imports System.Globalization
Imports System.IO
Imports sysmath = System.Math
Imports Microsoft.VisualBasic.MachineLearning.XGBoost
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

''' <summary>
''' 测试公共设施：资源定位、libsvm 读取、期望值读取、误差统计。
''' </summary>
Public Module TestSupport

    ''' <summary>
    ''' 测试资源根目录（优先使用输出目录下的 <c>resources</c>，其次向上回溯）
    ''' </summary>
    Public ReadOnly Property ResourcesRoot As String
        Get
            Dim dir As String = Path.Combine(AppContext.BaseDirectory, "resources")

            If Directory.Exists(dir) Then
                Return dir
            End If

            Dim cur As New DirectoryInfo(AppContext.BaseDirectory)

            For i As Integer = 0 To 8

                If cur Is Nothing Then
                    Exit For
                End If

                Dim probe As String = Path.Combine(cur.FullName, "resources")

                If Directory.Exists(probe) Then
                    Return probe
                End If

                cur = cur.Parent
            Next

            Throw New DirectoryNotFoundException($"找不到测试资源目录 resources（起始于 {AppContext.BaseDirectory}）")
        End Get
    End Property

    Public Function ResourcePath(relative As String) As String
        Return Path.Combine(ResourcesRoot, relative.Replace("/"c, "\"c))
    End Function

    ''' <summary>
    ''' 读取 libsvm 格式的数据集
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function LoadLibSvm(path As String) As DMatrix
        Dim handler = DMatrix.Builder

        For Each line As String In File.ReadLines(path)
            Dim text As String = line.Trim()

            If text.Length = 0 Then
                Continue For
            End If

            Dim parts As String() = text.Split(" "c)
            Dim map As New Dictionary(Of String, Single)()

            For i As Integer = 1 To parts.Length - 1
                Dim kv As String() = parts(i).Split(":"c)

                If kv.Length <> 2 Then
                    Continue For
                End If

                map(kv(0)) = Single.Parse(kv(1), NumberStyles.Float, CultureInfo.InvariantCulture)
            Next

            ' 第一列是标签：分类数据集是类别编号，回归数据集是实数值
            Dim label As Double = Double.Parse(parts(0), NumberStyles.Float, CultureInfo.InvariantCulture)

            handler.add(CInt(sysmath.Round(label)), FVecTransformer.fromMap(Of Single)(map))
        Next

        Return handler.getMatrix()
    End Function

    ''' <summary>
    ''' 读取期望值文件（每行一个样本，多列用逗号/空格分隔）
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    Public Function ReadExpectation(path As String) As Double()()
        Dim lines As String() = File.ReadAllLines(path)
        Dim ret As Double()() = New Double(lines.Length - 1)() {}
        Dim separator As Char() = {","c, " "c, ControlChars.Tab}

        For i As Integer = 0 To lines.Length - 1
            Dim parts As String() = lines(i).Split(separator, StringSplitOptions.RemoveEmptyEntries)
            Dim values As Double() = New Double(sysmath.Max(parts.Length, 1) - 1) {}

            For j As Integer = 0 To parts.Length - 1
                values(j) = Double.Parse(parts(j), NumberStyles.Float, CultureInfo.InvariantCulture)
            Next

            ret(i) = values
        Next

        Return ret
    End Function

    ''' <summary>
    ''' 载入 XGBoost 二进制模型
    ''' </summary>
    ''' <param name="modelFile"></param>
    ''' <returns></returns>
    Public Function LoadPredictor(modelPath As String) As Predictor
        Using stream As Stream = File.OpenRead(modelPath)
            Return New Predictor(stream)
        End Using
    End Function

    ''' <summary>
    ''' 逐元素比较两组预测结果，返回 (最大绝对误差, 超过阈值的元素个数)
    ''' </summary>
    Public Function Compare(expected As Double()(), actual As Double()(),
                            threshold As Double) As (maxDiff As Double, outliers As Integer, compared As Integer)
        Dim maxDiff As Double = 0
        Dim outliers As Integer = 0
        Dim compared As Integer = 0

        For i As Integer = 0 To sysmath.Min(expected.Length, actual.Length) - 1
            Dim width As Integer = sysmath.Min(expected(i).Length, actual(i).Length)

            For j As Integer = 0 To width - 1
                Dim diff As Double = sysmath.Abs(expected(i)(j) - actual(i)(j))

                If Double.IsNaN(diff) Then
                    diff = Double.MaxValue
                End If

                If diff > maxDiff Then
                    maxDiff = diff
                End If

                If diff > threshold Then
                    outliers += 1
                End If

                compared += 1
            Next
        Next

        Return (maxDiff, outliers, compared)
    End Function

    ''' <summary>
    ''' 对数损失（自带裁剪，避免 log(0)）
    ''' </summary>
    Public Function LogLoss(pred As Double(), label As Double()) As Double
        Dim total As Double = 0

        For i As Integer = 0 To pred.Length - 1
            Dim q As Double = sysmath.Min(sysmath.Max(pred(i), 0.000000000000001), 1 - 0.000000000000001)

            total += label(i) * sysmath.Log(q) + (1 - label(i)) * sysmath.Log(1 - q)
        Next

        Return -total / pred.Length
    End Function

    ''' <summary>
    ''' 用最终预测值与「仅常数项」的预测值对比，衡量训练是否真的降低了损失
    ''' </summary>
    Public Function BasePrediction(value As Double, n As Integer) As Double()
        Dim ret As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            ret(i) = value
        Next

        Return ret
    End Function
End Module
