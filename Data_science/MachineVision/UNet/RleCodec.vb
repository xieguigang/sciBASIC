' ---------------------------------------------------------------------------
' DSB2018 的 RLE 掩膜编解码
'
' 数据格式：``ImageId,EncodedPixels,Height,Width,Usage``，
' 其中 EncodedPixels 是 "起点 长度 起点 长度 ..."，起点是**1 起**的扁平下标。
'
' 扁平下标的展开方向（行主序还是列主序）在流传的各种实现里并不统一：
'   常见的 Python 写法是 ``img.reshape((H, W)).T``，等价于"先按行主序铺开，再整体转置"，
'   也就是列主序（下标 = x * H + y）。
' 本项目**不靠猜**：selftest 会用 stage1_train_labels.csv（同格式）去对那些
' 本来就带着 masks/*.png 的训练图做交叉验证 —— 两种方向各解一遍，
' 与 PNG 掩膜的并集算 IoU，接近 1 的那个就是正确方向。
' ---------------------------------------------------------------------------

Namespace UNet

    ''' <summary>
    ''' DSB 风格的游程编码（RLE）掩膜解码。
    ''' </summary>
    Public Module RleCodec

        ''' <summary>
        ''' 把一条 RLE 串解码成前景掩膜。
        ''' </summary>
        ''' <param name="encoded">形如 "起点 长度 起点 长度 ..." 的 RLE 串。</param>
        ''' <param name="height">掩膜高度。</param>
        ''' <param name="width">掩膜宽度。</param>
        ''' <param name="columnMajor">
        ''' True（默认）表示扁平下标按**列主序**铺开（下标 = x * height + y），
        ''' 即 <c>reshape((H, W)).T</c> 的语义。
        ''' </param>
        ''' <returns>长度 height*width 的前景掩膜，按行主序 <c>y*width + x</c> 排列。</returns>
        Public Function Decode(encoded As String, height As Integer, width As Integer,
                               Optional columnMajor As Boolean = True) As Boolean()
            Dim total = height * width
            Dim flat(total - 1) As Boolean

            If String.IsNullOrWhiteSpace(encoded) Then Return flat

            Dim parts = encoded.Split(New Char() {" "c, ChrW(9), ","c, ";"c},
                                      StringSplitOptions.RemoveEmptyEntries)

            For i As Integer = 0 To parts.Length - 2 Step 2
                Dim start As Integer
                Dim length As Integer

                If Not Integer.TryParse(parts(i), start) Then Continue For
                If Not Integer.TryParse(parts(i + 1), length) Then Continue For

                start -= 1 ' 1 起 → 0 起

                For k As Integer = 0 To length - 1
                    Dim idx = start + k

                    If idx >= 0 AndAlso idx < total Then
                        flat(idx) = True
                    End If
                Next
            Next

            If Not columnMajor Then Return flat

            ' 列主序铺开后再转置回行主序：flat[x * height + y] → mask[y * width + x]
            Dim mask(total - 1) As Boolean

            For idx As Integer = 0 To total - 1
                If Not flat(idx) Then Continue For

                Dim y = idx Mod height
                Dim x = idx \ height

                If y < width AndAlso x < height Then
                    mask(y * width + x) = True
                End If
            Next

            Return mask
        End Function

        ''' <summary>
        ''' 把同一张图的多条 RLE 合并成一个前景掩膜。
        ''' </summary>
        ''' <param name="encodedList">同一 ImageId 下的全部 RLE 串。</param>
        ''' <param name="height">掩膜高度。</param>
        ''' <param name="width">掩膜宽度。</param>
        ''' <param name="columnMajor">展开方向，见 <see cref="Decode"/>。</param>
        Public Function Merge(encodedList As IEnumerable(Of String), height As Integer, width As Integer,
                              Optional columnMajor As Boolean = True) As Boolean()
            Dim result(height * width - 1) As Boolean

            If encodedList Is Nothing Then Return result

            For Each encoded In encodedList
                Dim mask = Decode(encoded, height, width, columnMajor)

                For i As Integer = 0 To result.Length - 1
                    If mask(i) Then result(i) = True
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 两个布尔掩膜的 IoU（用于校对展开方向）。
        ''' </summary>
        Public Function IoU(a As Boolean(), b As Boolean()) As Double
            Dim inter As Long = 0
            Dim union As Long = 0

            For i As Integer = 0 To a.Length - 1
                If a(i) OrElse b(i) Then union += 1
                If a(i) AndAlso b(i) Then inter += 1
            Next

            Return If(union = 0, 0.0, inter / CDbl(union))
        End Function

        ''' <summary>
        ''' 把布尔掩膜转成类别标签数组（True → 1，False → 0）。
        ''' </summary>
        Public Function ToLabels(mask As Boolean()) As Integer()
            Dim labels(mask.Length - 1) As Integer

            For i As Integer = 0 To mask.Length - 1
                labels(i) = If(mask(i), 1, 0)
            Next

            Return labels
        End Function

    End Module

End Namespace
