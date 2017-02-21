Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting

Namespace HTML.CSS

    ''' <summary>
    ''' Represents padding or margin information associated with a gdi element. (padding: top, right, bottom, left)
    ''' </summary>
    <TypeConverter(GetType(PaddingConverter))> Public Structure Padding

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Top = 0 AndAlso Bottom = 0 AndAlso Left = 0 AndAlso Right = 0
            End Get
        End Property

        '
        ' Summary:
        '     Initializes a new instance of the System.Windows.Forms.Padding class using the
        '     supplied padding size for all edges.
        '
        ' Parameters:
        '   all:
        '     The number of pixels to be used for padding for all edges.
        Public Sub New(all As Integer)
            Call Me.New(all, all, all, all)
        End Sub
        '
        ' Summary:
        '     Initializes a new instance of the System.Windows.Forms.Padding class using a
        '     separate padding size for each edge.
        '
        ' Parameters:
        '   left:
        '     The padding size, in pixels, for the left edge.
        '
        '   top:
        '     The padding size, in pixels, for the top edge.
        '
        '   right:
        '     The padding size, in pixels, for the right edge.
        '
        '   bottom:
        '     The padding size, in pixels, for the bottom edge.
        Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
            With Me
                .Left = left
                .Top = top
                .Right = right
                .Bottom = bottom
            End With
        End Sub

        Sub New(margin As Size)
            Call Me.New(margin.Width, margin.Height)
        End Sub

        Sub New(width%, height%)
            Call Me.New(left:=width, right:=width, top:=height, bottom:=height)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="layoutVector%"><see cref="LayoutVector"/></param>
        Sub New(layoutVector%())
            Top = layoutVector(0)
            Right = layoutVector(1)
            Bottom = layoutVector(2)
            Left = layoutVector(3)
        End Sub

        ''' <summary>
        ''' Gets the combined padding for the right and left edges.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)> Public ReadOnly Property Horizontal As Integer
            Get
                Return Left + Right
            End Get
        End Property

        '
        ' Summary:
        '     Gets or sets the padding value for the top edge.
        '
        ' Returns:
        '     The padding, in pixels, for the top edge.
        <RefreshProperties(RefreshProperties.All)>
        Public Property Top As Integer
        '
        ' Summary:
        '     Gets or sets the padding value for the right edge.
        '
        ' Returns:
        '     The padding, in pixels, for the right edge.
        <RefreshProperties(RefreshProperties.All)>
        Public Property Right As Integer
        '
        ' Summary:
        '     Gets or sets the padding value for the left edge.
        '
        ' Returns:
        '     The padding, in pixels, for the left edge.
        <RefreshProperties(RefreshProperties.All)>
        Public Property Left As Integer
        '
        ' Summary:
        '     Gets or sets the padding value for the bottom edge.
        '
        ' Returns:
        '     The padding, in pixels, for the bottom edge.
        <RefreshProperties(RefreshProperties.All)>
        Public Property Bottom As Integer

        ''' <summary>
        ''' Gets the combined padding for the top and bottom edges.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)> Public ReadOnly Property Vertical As Integer
            Get
                Return Top + Bottom
            End Get
        End Property

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"padding:{Top}px {Right}px {Bottom}px {Left}px;"
        End Function

        ''' <summary>
        ''' padding: top, right, bottom, left
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LayoutVector As Integer()
            Get
                Return {Top, Right, Bottom, Left}
            End Get
        End Property

        ''' <summary>
        ''' 转换为css字符串
        ''' </summary>
        ''' <param name="padding"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(padding As Padding) As String
            Return padding.ToString
        End Operator

        ''' <summary>
        ''' 同时兼容padding css，以及使用size表达式统一赋值
        ''' </summary>
        ''' <param name="css$"></param>
        ''' <returns></returns>
        Public Shared Widening Operator CType(css$) As Padding
            Dim value As NamedValue(Of String) = css.GetTagValue(":", trim:=True)

            If value.Name.StringEmpty AndAlso css.IndexOf(","c) > -1 Then
                Dim size As Size = css.SizeParser
                Return New Padding(margin:=size)
            End If

            Dim tokens$() = value.Value.Trim(";"c).Split
            Dim vector%() = tokens _
                .Select(Function(s) CInt(s.ParseNumeric)) _
                .ToArray

            If vector.Length = 1 Then  ' all
                Return New Padding(all:=vector(Scan0))
            Else
                Return New Padding(vector)
            End If
        End Operator

        '
        ' Summary:
        '     Determines whether the value of the specified object is equivalent to the current
        '     System.Windows.Forms.Padding.
        '
        ' Parameters:
        '   other:
        '     The object to compare to the current System.Windows.Forms.Padding.
        '
        ' Returns:
        '     true if the System.Windows.Forms.Padding objects are equivalent; otherwise, false.
        Public Overrides Function Equals(other As Object) As Boolean
            If other Is Nothing Then
                Return False
            ElseIf Not other.GetType.Equals(GetType(Padding)) Then
                Return False
            Else
                With DirectCast(other, Padding)
                    Return Left = .Left AndAlso Right = .Right AndAlso Top = .Top AndAlso Bottom = .Bottom
                End With
            End If
        End Function

        '
        ' Summary:
        '     Performs vector addition on the two specified System.Windows.Forms.Padding objects,
        '     resulting in a new System.Windows.Forms.Padding.
        '
        ' Parameters:
        '   p1:
        '     The first System.Windows.Forms.Padding to add.
        '
        '   p2:
        '     The second System.Windows.Forms.Padding to add.
        '
        ' Returns:
        '     A new System.Windows.Forms.Padding that results from adding p1 and p2.
        Public Shared Operator +(p1 As Padding, p2 As Padding) As Padding
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out%() = New Integer(4) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) + b(i)
            Next

            Return New Padding(layoutVector:=out)
        End Operator
        '
        ' Summary:
        '     Performs vector subtraction on the two specified System.Windows.Forms.Padding
        '     objects, resulting in a new System.Windows.Forms.Padding.
        '
        ' Parameters:
        '   p1:
        '     The System.Windows.Forms.Padding to subtract from (the minuend).
        '
        '   p2:
        '     The System.Windows.Forms.Padding to subtract from (the subtrahend).
        '
        ' Returns:
        '     The System.Windows.Forms.Padding result of subtracting p2 from p1.
        Public Shared Operator -(p1 As Padding, p2 As Padding) As Padding
            Dim a = p1.LayoutVector
            Dim b = p2.LayoutVector
            Dim out%() = New Integer(4) {}

            For i As Integer = 0 To out.Length - 1
                out(i) = a(i) - b(i)
            Next

            Return New Padding(layoutVector:=out)
        End Operator

        '
        ' Summary:
        '     Tests whether two specified System.Windows.Forms.Padding objects are equivalent.
        '
        ' Parameters:
        '   p1:
        '     A System.Windows.Forms.Padding to test.
        '
        '   p2:
        '     A System.Windows.Forms.Padding to test.
        '
        ' Returns:
        '     true if the two System.Windows.Forms.Padding objects are equal; otherwise, false.
        Public Shared Operator =(p1 As Padding, p2 As Padding) As Boolean
            Return p1.LayoutVector.SequenceEqual(p2.LayoutVector)
        End Operator
        '
        ' Summary:
        '     Tests whether two specified System.Windows.Forms.Padding objects are not equivalent.
        '
        ' Parameters:
        '   p1:
        '     A System.Windows.Forms.Padding to test.
        '
        '   p2:
        '     A System.Windows.Forms.Padding to test.
        '
        ' Returns:
        '     true if the two System.Windows.Forms.Padding objects are different; otherwise,
        '     false.
        Public Shared Operator <>(p1 As Padding, p2 As Padding) As Boolean
            Return Not (p1 = p2)
        End Operator
    End Structure
End Namespace