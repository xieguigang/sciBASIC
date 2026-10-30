Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Dynamics.Data

    ''' <summary>
    ''' ODEs output, this object can populates the <see cref="ODESolution.y"/> 
    ''' variables values through its enumerator interface.
    ''' (常微分方程求解结果，同时也是所有求解器 —— 内建求解器与外部插件求解器 —— 的统一输出模型)
    ''' </summary>
    ''' <remarks>
    ''' 除了原来的 ``x`` / ``y`` / ``y0`` / ``params`` 之外，这个类型还提供了与具体求解器无关的
    ''' 通用访问接口（<see cref="Count"/>、<see cref="Item(Integer)"/>、<see cref="GetComponent(Integer)"/>、
    ''' <see cref="ToCSV"/>），因此像 ``Sundials.CVODE`` 这类只知道「N 维状态向量」的求解器也可以直接
    ''' 使用它来输出结果，而不需要为每个求解器维护一套独立的结果类型。
    ''' 
    ''' 注意：<see cref="y"/> 是按键排序的字典（``SortedDictionary(Of String, T)``），
    ''' 所以列顺序必须通过 <see cref="columnNames"/> 显式指定，不能依赖 <see cref="y"/> 的枚举顺序。
    ''' </remarks>
    Public Class ODESolution : Implements Enumeration(Of NamedCollection(Of Double))

        Private _y As Dictionary(Of NamedCollection(Of Double))
        Private _columnNames As String()
        Private _columns As NamedCollection(Of Double)()

        ''' <summary>
        ''' The sampling time points of the integration result.(积分结果的采样时间点)
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double()

        ''' <summary>
        ''' The result series of each state variable, the key is the variable name.
        ''' (各个状态变量的结果序列，键为变量名称)
        ''' </summary>
        ''' <returns></returns>
        Public Property y As Dictionary(Of NamedCollection(Of Double))
            Get
                Return _y
            End Get
            Set(value As Dictionary(Of NamedCollection(Of Double)))
                _y = value
                _columns = Nothing
            End Set
        End Property

        ''' <summary>
        ''' 方程组的初始值，积分结果会受到初始值的极大的影响
        ''' </summary>
        ''' <returns></returns>
        Public Property y0 As Dictionary(Of String, Double)
        Public Property params As Dictionary(Of String, Double)

        ''' <summary>
        ''' The column order of <see cref="y"/>: the index of this array is the state variable
        ''' index, when this value is not presented the key order of <see cref="y"/> will be used.
        ''' (列顺序，按状态变量序号排列；未指定的时候使用 y 的键顺序)
        ''' </summary>
        ''' <returns></returns>
        Public Property columnNames As String()
            Get
                Return _columnNames
            End Get
            Set(value As String())
                _columnNames = value
                _columns = Nothing
            End Set
        End Property

        ''' <summary>
        ''' The solver specific diagnostics, e.g. the integration steps, the right-hand side
        ''' evaluations of the external solvers. (求解器诊断信息)
        ''' </summary>
        ''' <returns></returns>
        Public Property info As Dictionary(Of String, String)

        ''' <summary>
        ''' The result series which are ordered by <see cref="columnNames"/>.
        ''' (按列顺序排列的结果序列)
        ''' </summary>
        ''' <returns></returns>
        Private ReadOnly Property Columns As NamedCollection(Of Double)()
            Get
                If _columns Is Nothing Then
                    _columns = ResolveColumns()
                End If

                Return _columns
            End Get
        End Property

        Private Function ResolveColumns() As NamedCollection(Of Double)()
            If _y Is Nothing Then
                Return New NamedCollection(Of Double)() {}
            End If
            If _columnNames Is Nothing OrElse _columnNames.Length = 0 Then
                Return _y.Values.ToArray
            End If

            Dim order As NamedCollection(Of Double)() = New NamedCollection(Of Double)(_columnNames.Length - 1) {}

            For i As Integer = 0 To _columnNames.Length - 1
                Dim name As String = _columnNames(i)

                If Not _y.ContainsKey(name) Then
                    Throw New InvalidOperationException($"The column '{name}' which is declared in the columnNames is not exists in the y series collection!")
                End If

                order(i) = _y(name)
            Next

            Return order
        End Function

        ''' <summary>
        ''' The number of the sampling points of the integration result.(采样点个数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                If x Is Nothing Then
                    Return 0
                Else
                    Return x.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' Get the state vector of the sampling point at the given index, the components are
        ''' ordered by <see cref="columnNames"/>.
        ''' (获取指定采样点的状态向量，分量的顺序与 columnNames 一致)
        ''' </summary>
        ''' <param name="index">The index of the sampling point.</param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(index As Integer) As Vector
            Get
                Dim columns As NamedCollection(Of Double)() = Me.Columns
                Dim values As Double() = New Double(columns.Length - 1) {}

                For i As Integer = 0 To columns.Length - 1
                    values(i) = columns(i).value(index)
                Next

                Return New Vector(values)
            End Get
        End Property

        ''' <summary>
        ''' Get the time series of the given state variable component, the component index is
        ''' the position in <see cref="columnNames"/>.
        ''' (获取指定状态变量分量的时间序列)
        ''' </summary>
        ''' <param name="componentIndex"></param>
        ''' <returns></returns>
        Public Function GetComponent(componentIndex As Integer) As Double()
            Dim columns As NamedCollection(Of Double)() = Me.Columns

            If componentIndex < 0 OrElse componentIndex >= columns.Length Then
                Throw New ArgumentOutOfRangeException(NameOf(componentIndex),
                    $"The component index should be in the range [0, {columns.Length - 1}]!")
            End If

            Return columns(componentIndex).value
        End Function

        ''' <summary>
        ''' Export the integration result into the csv text which is compatible with the legacy
        ''' ``ODESolution.ToCSV``: the header is ``Time`` followed by the column names, and all
        ''' of the values are formatted by ``G10``.
        ''' (导出为 csv 文本，格式与旧的 ODESolution.ToCSV 保持一致)
        ''' </summary>
        ''' <returns></returns>
        Public Function ToCSV() As String
            Dim columns As NamedCollection(Of Double)() = Me.Columns
            Dim n As Integer = Count

            If n = 0 Then
                Return String.Empty
            End If

            Dim sb As New StringBuilder()
            Dim size As Integer = columns.Length

            sb.Append("Time")

            For i As Integer = 0 To size - 1
                sb.Append(",").Append(columns(i).name)
            Next

            sb.AppendLine()

            For i As Integer = 0 To n - 1
                sb.Append(x(i).ToString("G10"))

                For j As Integer = 0 To size - 1
                    sb.Append(",").Append(columns(j).value(i).ToString("G10"))
                Next

                sb.AppendLine()
            Next

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' 得到进行积分的步进值，采样点不足的时候返回零
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dx As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If x Is Nothing OrElse x.Length < 2 Then
                    Return 0.0
                Else
                    Return x(1) - x(0)
                End If
            End Get
        End Property

        ''' <summary>
        ''' 获取得到积分计算的分辨率的数值，采样点不足的时候返回零
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Resolution As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Dim dh As Double = dx

                If x Is Nothing OrElse x.Length < 2 OrElse dh = 0.0 Then
                    Return 0.0
                Else
                    Return (x(x.Length - 1) - x(0)) / dh
                End If
            End Get
        End Property

        ''' <summary>
        ''' Using the first value of <see cref="y"/> as ``y0``
        ''' </summary>
        ''' <returns></returns>
        Public Function GetY0() As Dictionary(Of String, Double)
            Return y.ToDictionary(Function(x) x.Key,
                                  Function(x)
                                      Return x.Value(Scan0)
                                  End Function)
        End Function

        ''' <summary>
        ''' Is there NAN value in the function value <see cref="y"/> ???
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HaveNaN As Boolean
            Get
                If y Is Nothing Then
                    Return False
                End If

                For Each val As NamedCollection(Of Double) In y.Values
                    For Each x As Double In val.value
                        If x.IsNaNImaginary Then
                            Return True
                        End If
                    Next
                Next

                Return False
            End Get
        End Property

        ''' <summary>
        ''' Merge <see cref="y0"/> into <see cref="params"/>
        ''' </summary>
        Public Function Join() As ODESolution
            Dim params As New Dictionary(Of String, Double)(If(Me.params, New Dictionary(Of String, Double)()))

            If Not y0 Is Nothing Then
                For Each v In y0
                    Call params.Add(v.Key, v.Value)
                Next
            End If

            Return New ODESolution With {
                .params = params,
                .x = x,
                .y = y,
                .y0 = y0,
                .columnNames = columnNames,
                .info = info
            }
        End Function

        ''' <summary>
        ''' Cast the integration result into a <see cref="NumericTable"/>: each row is a
        ''' sampling time point and each feature column is a state variable.
        ''' (将积分结果转换为数值表：每一行是一个时间采样点，每一列是一个状态变量)
        ''' </summary>
        ''' <returns></returns>
        Public Function AsTable() As NumericTable
            Dim columns As NamedCollection(Of Double)() = Me.Columns
            Dim tbl As New NumericTable(
                columns.ToList(Function(v) v.name),
                columns.Select(Function(v) v.value),
                x.ToList(Function(xi) CStr(xi))) With {
                .description = "ODE output data",
                .name = "ODEs Output Table"
            }

            Return tbl
        End Function

        ''' <summary>
        ''' Create the integration result from the raw time points and the result columns, this
        ''' factory makes the object usable by the solvers which are not aware of the variable
        ''' names (e.g. the external CVODE solver).
        ''' (由时间点与结果列构造结果对象，供不掌握变量名称的外部求解器使用)
        ''' </summary>
        ''' <param name="times">The ``n + 1`` sampling time points.</param>
        ''' <param name="columns">The result column of each state variable.</param>
        ''' <param name="names">
        ''' The variable names, when this value is not presented the name will be generated as
        ''' ``y0``, ``y1``, ...
        ''' </param>
        ''' <returns></returns>
        Public Shared Function FromColumns(times As Double(), columns As Double()(), Optional names As String() = Nothing) As ODESolution
            If times Is Nothing Then
                Throw New ArgumentNullException(NameOf(times))
            End If
            If columns Is Nothing OrElse columns.Length = 0 Then
                Throw New ArgumentException("The result columns can not be empty!", NameOf(columns))
            End If

            Dim size As Integer = columns.Length
            Dim resolved As String() = ResolveNames(names, size)
            Dim y As New Dictionary(Of NamedCollection(Of Double))
            Dim y0 As New Dictionary(Of String, Double)(size)

            For i As Integer = 0 To size - 1
                Dim column As Double() = columns(i)

                If column Is Nothing OrElse column.Length <> times.Length Then
                    Throw New ArgumentException(
                        $"The length of the result column '{resolved(i)}' ({If(column Is Nothing, "null", column.Length.ToString)}) is not matched to the time points ({times.Length})!",
                        NameOf(columns))
                End If

                y(resolved(i)) = New NamedCollection(Of Double) With {
                    .name = resolved(i),
                    .value = column
                }
                y0(resolved(i)) = column(0)
            Next

            Return New ODESolution With {
                .x = times,
                .y = y,
                .y0 = y0,
                .params = New Dictionary(Of String, Double)(),
                .columnNames = resolved
            }
        End Function

        ''' <summary>
        ''' Create the integration result from the sampling time points and the state vectors.
        ''' (由采样时间点与状态向量序列构造结果对象)
        ''' </summary>
        ''' <param name="times"></param>
        ''' <param name="states"></param>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Public Shared Function FromPoints(times As IEnumerable(Of Double), states As IEnumerable(Of Vector),
                                          Optional names As String() = Nothing) As ODESolution

            Dim timePoints As Double() = times.ToArray
            Dim stateValues As Vector() = states.ToArray

            If stateValues.Length = 0 Then
                Throw New ArgumentException("The state vectors can not be empty!", NameOf(states))
            End If

            Dim size As Integer = stateValues(0).Length
            Dim columns As Double()() = New Double(size - 1)() {}

            For i As Integer = 0 To size - 1
                columns(i) = New Double(timePoints.Length - 1) {}
            Next

            For k As Integer = 0 To stateValues.Length - 1
                Dim state As Vector = stateValues(k)

                For i As Integer = 0 To size - 1
                    columns(i)(k) = state(i)
                Next
            Next

            Return FromColumns(timePoints, columns, names)
        End Function

        Private Shared Function ResolveNames(names As String(), size As Integer) As String()
            If names Is Nothing OrElse names.Length = 0 Then
                Dim generated As String() = New String(size - 1) {}

                For i As Integer = 0 To size - 1
                    generated(i) = $"y{i}"
                Next

                Return generated
            End If
            If names.Length <> size Then
                Throw New ArgumentException(
                    $"The number of the variable names ({names.Length}) is not matched to the result columns ({size})!",
                    NameOf(names))
            End If

            Return names
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Double)) Implements Enumeration(Of NamedCollection(Of Double)).GenericEnumerator
            For Each var As NamedCollection(Of Double) In y.Values
                Yield var
            Next
        End Function
    End Class
End Namespace
