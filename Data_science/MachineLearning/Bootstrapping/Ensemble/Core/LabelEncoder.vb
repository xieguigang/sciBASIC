Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' 把连续的 <see cref="Double"/> 标签编码成升序类别数组与整数索引。
    ''' </summary>
    ''' <remarks>
    ''' 集成器在入口处只做一次编码，之后所有基学习器都使用同一份
    ''' <c>Classes</c> 顺序，因此不会出现多模型之间的类别索引错位。
    ''' </remarks>
    Public NotInheritable Class LabelEncoder

        Private ReadOnly classesArray As Double()
        Private ReadOnly indexByValue As Dictionary(Of Double, Integer)

        Private Sub New(classes As Double())
            Me.classesArray = classes
            Me.indexByValue = New Dictionary(Of Double, Integer)()

            For i As Integer = 0 To classes.Length - 1
                Me.indexByValue(classes(i)) = i
            Next
        End Sub

        ''' <summary>升序类别标签</summary>
        Public ReadOnly Property Classes As Double()
            Get
                Return classesArray
            End Get
        End Property

        ''' <summary>类别数量</summary>
        Public ReadOnly Property ClassCount As Integer
            Get
                Return classesArray.Length
            End Get
        End Property

        ''' <summary>
        ''' 从标签向量构建编码器。至少需要两个不同类别，且标签必须是有限数值。
        ''' </summary>
        Public Shared Function Fit(y As Double(), Optional label As String = "y") As LabelEncoder
            If y Is Nothing OrElse y.Length = 0 Then
                Throw New ArgumentException($"the label vector '{label}' can not be empty!", NameOf(y))
            End If

            Dim distinct As New SortedSet(Of Double)()

            For i As Integer = 0 To y.Length - 1
                Dim value As Double = y(i)

                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException(
                        $"the label at index {i} is not a finite number ({value})!", NameOf(y))
                End If

                Call distinct.Add(value)
            Next

            If distinct.Count < 2 Then
                Throw New ArgumentException(
                    $"the label vector '{label}' contains only one class ({distinct.First()}), classification requires at least two classes!",
                    NameOf(y))
            End If

            Return New LabelEncoder(distinct.ToArray())
        End Function

        ''' <summary>标签 → 类别索引</summary>
        Public Function Encode(y As Double()) As Integer()
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            Dim index As Integer() = New Integer(y.Length - 1) {}

            For i As Integer = 0 To y.Length - 1
                Dim slot As Integer = 0

                If Not indexByValue.TryGetValue(y(i), slot) Then
                    Throw New ArgumentException(
                        $"the label at index {i} ({y(i)}) was not seen when the encoder was fitted!", NameOf(y))
                End If

                index(i) = slot
            Next

            Return index
        End Function

        ''' <summary>类别索引 → 标签</summary>
        Public Function Decode(index As Integer()) As Double()
            If index Is Nothing Then
                Throw New ArgumentNullException(NameOf(index))
            End If

            Dim y As Double() = New Double(index.Length - 1) {}

            For i As Integer = 0 To index.Length - 1
                y(i) = DecodeOne(index(i))
            Next

            Return y
        End Function

        ''' <summary>单个类别索引 → 标签</summary>
        Public Function DecodeOne(index As Integer) As Double
            If index < 0 OrElse index >= classesArray.Length Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(index),
                    $"the class index {index} is out of range [0, {classesArray.Length})!")
            End If

            Return classesArray(index)
        End Function

    End Class
End Namespace
