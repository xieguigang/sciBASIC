Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace train.kernels

    ''' <summary>
    ''' 列主序（SoA）训练数据块。
    '''
    ''' 取代旧的 <c>Single()()()</c> 表示（每个 (样本,特征) 单元一个两元素 <c>Single()</c> 对象，
    ''' 约 32~40 字节且全部进入 Gen2）。本类把每个特征拆成三条平行数组：
    '''
    ''' + <see cref="Values"/>：按特征值升序排列的特征值；
    ''' + <see cref="RowIds"/>：与 <see cref="Values"/> 对齐的原始行号；
    ''' + <see cref="Boundaries"/>：区间切点位置（长度 = 不同值个数 + 1），区间视图零复制。
    '''
    ''' 类别特征额外维护 <see cref="CategoryCodes"/> / <see cref="CategoryValues"/>，
    ''' 把类别值映射为稠密整数编码。旧的实现经过 <c>CStr -&gt; CDbl -&gt; CInt</c> 往返，
    ''' 非整数编码会被截断，导致训练期与预测期的分支判断不一致；整数编码消除了该问题，
    ''' 且不依赖当前区域设置。
    ''' </summary>
    Public Class ColumnBlock

        ''' <summary>
        ''' 特征维度
        ''' </summary>
        Public ReadOnly FeatureDim As Integer
        ''' <summary>
        ''' 样本行数
        ''' </summary>
        Public ReadOnly RowCount As Integer
        ''' <summary>
        ''' 每个特征按值升序排列的特征值
        ''' </summary>
        Public ReadOnly Values As Single()()
        ''' <summary>
        ''' 与 <see cref="Values"/> 对齐的原始行号
        ''' </summary>
        Public ReadOnly RowIds As Integer()()
        ''' <summary>
        ''' 区间切点位置，长度 = 不同值个数 + 1，首元素恒为 0，末元素恒为样本数
        ''' </summary>
        Public ReadOnly Boundaries As Integer()()
        ''' <summary>
        ''' 每个特征的缺失样本行号
        ''' </summary>
        Public ReadOnly NanRows As Integer()()
        ''' <summary>
        ''' 标记哪些特征是类别特征
        ''' </summary>
        Public ReadOnly IsCategorical As Boolean()
        ''' <summary>
        ''' 原始（行主序）特征矩阵，用于按行随机访问
        ''' </summary>
        Public ReadOnly Origin As Single()()
        ''' <summary>
        ''' 类别值 -&gt; 整数编码，仅类别特征非空
        ''' </summary>
        Public ReadOnly CategoryCodes As Dictionary(Of Single, Integer)()
        ''' <summary>
        ''' 整数编码 -&gt; 原始类别值，仅类别特征非空
        ''' </summary>
        Public ReadOnly CategoryValues As Single()()

        ''' <summary>
        ''' 直方图分桶结果缓存（仅在 histogram 模式下按需构建）
        ''' </summary>
        Private ReadOnly bins As FeatureBinning()

        Friend Sub New(featureDim As Integer, rowCount As Integer,
                       values As Single()(), rowIds As Integer()(),
                       boundaries As Integer()(), nanRows As Integer()(),
                       categorical As Boolean(), origin As Single()(),
                       catCodes As Dictionary(Of Single, Integer)(),
                       catValues As Single()())

            Me.FeatureDim = featureDim
            Me.RowCount = rowCount
            Me.Values = values
            Me.RowIds = rowIds
            Me.Boundaries = boundaries
            Me.NanRows = nanRows
            Me.IsCategorical = categorical
            Me.Origin = origin
            Me.CategoryCodes = catCodes
            Me.CategoryValues = catValues
            Me.bins = New FeatureBinning(featureDim - 1) {}
        End Sub

        ''' <summary>
        ''' 判断某个值是否应被视为缺失值。
        '''
        ''' 以 <see cref="Single.NaN"/> 为准，同时兼容历史上使用的
        ''' <see cref="Data.NA"/> 哨兵值（<c>-Single.MaxValue</c>）。
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function IsMissing(value As Single) As Boolean
            Return Single.IsNaN(value) OrElse value = Data.NA
        End Function

        ''' <summary>
        ''' 按行号直取特征值（缺失返回缺失标记）
        ''' </summary>
        ''' <param name="row">原始行号</param>
        ''' <param name="feature">特征列号</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ValueAt(row As Integer, feature As Integer) As Single
            Return Origin(row)(feature)
        End Function

        ''' <summary>
        ''' 某个特征的区间（不同取值）个数
        ''' </summary>
        ''' <param name="feature"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SegmentCount(feature As Integer) As Integer
            Return Boundaries(feature).Length - 1
        End Function

        ''' <summary>
        ''' 获取某个特征的分桶表，按需构建并缓存（同一 <see cref="ColumnBlock"/> 在多棵树之间共享）。
        '''
        ''' 采用等频分桶：把有序行按 <paramref name="maxBin"/> 等分，桶边界取切点处的特征值，
        ''' 因此同一桶内的样本数与不同取值的个数无关，能显著压缩「高基数数值特征」的分裂搜索空间。
        ''' </summary>
        ''' <param name="feature"></param>
        ''' <param name="maxBin">最大分桶数，建议 64~512</param>
        ''' <returns></returns>
        Public Function GetBins(feature As Integer, maxBin As Integer) As FeatureBinning
            Dim cached = bins(feature)

            If cached IsNot Nothing Then
                Return cached
            End If

            SyncLock bins
                cached = bins(feature)

                If cached IsNot Nothing Then
                    Return cached
                End If

                cached = FeatureBinning.Build(Values(feature), maxBin)
                bins(feature) = cached
            End SyncLock

            Return cached
        End Function

        ''' <summary>
        ''' 从 <see cref="TrainData"/> 构建列块
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Shared Function FromTrainData(data As TrainData) As ColumnBlock
            Dim featureDim As Integer = data.feature_dim
            Dim rowCount As Integer = data.dataset_size

            If featureDim <= 0 Then
                Throw New ArgumentException("训练数据的特征维度必须为正数！", NameOf(data))
            End If

            Dim values As Single()() = New Single(featureDim - 1)() {}
            Dim rowIds As Integer()() = New Integer(featureDim - 1)() {}
            Dim boundaries As Integer()() = New Integer(featureDim - 1)() {}
            Dim nanRows As Integer()() = New Integer(featureDim - 1)() {}
            Dim categorical As Boolean() = New Boolean(featureDim - 1) {}
            Dim catCodes(featureDim - 1) As Dictionary(Of Single, Integer)
            Dim catValues(featureDim - 1)() As Single

            Dim catCols = If(data.cat_features_cols, New List(Of Integer)())

            For Each col As Integer In catCols

                If col >= 0 AndAlso col < featureDim Then
                    categorical(col) = True
                End If
            Next

            Dim raw = data.feature_value_index

            For feature As Integer = 0 To featureDim - 1
                Dim src As Single()() = If(raw Is Nothing, Nothing, raw(feature))
                Dim keys As Single()
                Dim ids As Integer()

                If src IsNot Nothing AndAlso src.Length > 0 Then
                    Dim n As Integer = src.Length

                    keys = New Single(n - 1) {}
                    ids = New Integer(n - 1) {}

                    For k As Integer = 0 To n - 1
                        keys(k) = src(k)(0)
                        ids(k) = CInt(src(k)(1))
                    Next
                Else
                    ' 解析阶段未构造 (value,row) 对（默认路径），直接从原始矩阵重建
                    Dim pair = buildFromOrigin(data, feature)

                    keys = pair.keys
                    ids = pair.ids
                End If

                ' 双数组重载：内建 Single 比较，避免 IComparer(Of Single()) 的接口虚调用
                Call Array.Sort(keys, ids)

                values(feature) = keys
                rowIds(feature) = ids
                boundaries(feature) = BuildBoundaries(keys, keys.Length)

                Dim nan = If(data.missing_index, New Integer(featureDim - 1)() {})(feature)
                nanRows(feature) = If(nan, New Integer() {})

                If categorical(feature) Then
                    catCodes(feature) = buildCategoryCodes(keys, boundaries(feature))
                    catValues(feature) = buildCategoryValues(keys, boundaries(feature))
                End If
            Next

            Return New ColumnBlock(featureDim, rowCount,
                                   values, rowIds, boundaries, nanRows,
                                   categorical, data.origin_feature,
                                   catCodes, catValues)
        End Function

        ''' <summary>
        ''' 构建区间切点：<c>[0, 第一个不同值的位置, ..., n]</c>
        ''' </summary>
        Private Shared Function BuildBoundaries(keys As Single(), n As Integer) As Integer()
            Dim list As New List(Of Integer)()

            Call list.Add(0)

            For k As Integer = 1 To n - 1

                If keys(k) <> keys(k - 1) Then
                    Call list.Add(k)
                End If
            Next

            Call list.Add(n)

            Return list.ToArray
        End Function

        ''' <summary>
        ''' 从原始（行主序）矩阵重建某列的有序 <c>(value, row)</c> 对。
        '''
        ''' 解析阶段不再预先构造 <c>Single()()()</c>，而是在这里一次性扫描原始矩阵：
        ''' 峰值内存从「每单元一个对象」降为「每特征两条平行数组」。
        ''' </summary>
        Private Shared Function buildFromOrigin(data As TrainData, feature As Integer) As (keys As Single(), ids As Integer())
            Dim origin = data.origin_feature
            Dim rowCount As Integer = data.dataset_size

            If origin Is Nothing OrElse rowCount <= 0 Then
                Return (Nothing, Nothing)
            End If

            ' 先按上界分配，最后再收缩到实际长度
            Dim keys As Single() = New Single(rowCount - 1) {}
            Dim ids As Integer() = New Integer(rowCount - 1) {}
            Dim cursor As Integer = 0
            Dim upper As Integer = std.Min(rowCount, origin.Length) - 1

            For row As Integer = 0 To upper
                Dim rowValues = origin(row)

                If rowValues Is Nothing OrElse feature >= rowValues.Length Then
                    Continue For
                End If

                Dim v As Single = rowValues(feature)

                If IsMissing(v) Then
                    Continue For
                End If

                keys(cursor) = v
                ids(cursor) = row
                cursor += 1
            Next

            If cursor <> rowCount Then
                Call Array.Resize(keys, cursor)
                Call Array.Resize(ids, cursor)
            End If

            Return (keys, ids)
        End Function

        ''' <summary>
        ''' 建立类别值到整数编码的映射
        ''' </summary>
        Private Shared Function buildCategoryCodes(keys As Single(), boundaries As Integer()) As Dictionary(Of Single, Integer)
            Dim distinct As Integer = boundaries.Length - 1

            If distinct <= 0 Then
                Return New Dictionary(Of Single, Integer)()
            End If

            Dim map As New Dictionary(Of Single, Integer)(distinct)

            For code As Integer = 0 To distinct - 1
                map(keys(boundaries(code))) = code
            Next

            Return map
        End Function

        ''' <summary>
        ''' 建立整数编码到原始类别值的映射
        ''' </summary>
        Private Shared Function buildCategoryValues(keys As Single(), boundaries As Integer()) As Single()
            Dim distinct As Integer = boundaries.Length - 1

            If distinct <= 0 Then
                Return New Single() {}
            End If

            Dim list As Single() = New Single(distinct - 1) {}

            For code As Integer = 0 To distinct - 1
                list(code) = keys(boundaries(code))
            Next

            Return list
        End Function
    End Class
End Namespace
