Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Structure Rectf

        Public Shared ReadOnly zero As Rectf = New Rectf(0, 0, 0, 0)
        Public Shared ReadOnly one As Rectf = New Rectf(1, 1, 1, 1)

        Public x, y, width, height As Single

        Public Sub New(x As Single, y As Single, width As Single, height As Single)
            Me.x = x
            Me.y = y
            Me.width = width
            Me.height = height
        End Sub

        Public ReadOnly Property left As Single
            Get
                Return x
            End Get
        End Property

        Public ReadOnly Property right As Single
            Get
                Return x + width
            End Get
        End Property

        Public ReadOnly Property top As Single
            Get
                Return y
            End Get
        End Property

        Public ReadOnly Property bottom As Single
            Get
                Return y + height
            End Get
        End Property

        Public ReadOnly Property topLeft As Vector2
            Get
                Return New Vector2(left, top)
            End Get
        End Property

        Public ReadOnly Property bottomRight As Vector2
            Get
                Return New Vector2(right, bottom)
            End Get
        End Property
    End Structure
End Namespace