
''' <summary>
''' TextBoxLine class
''' </summary>
Public Class TextBoxLine


    ''' <summary>
    ''' Gets array of line segments.
    ''' </summary>
    Private _Ascent As Double, _Descent As Double, _EndOfParagraph As Boolean, _SegArray As TextBoxSeg()

    ''' <summary>
    ''' Gets line ascent.
    ''' </summary>
    Public Property Ascent As Double
        Get
            Return _Ascent
        End Get
        Friend Set(value As Double)
            _Ascent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets line descent.
    ''' </summary>
    Public Property Descent As Double
        Get
            Return _Descent
        End Get
        Friend Set(value As Double)
            _Descent = value
        End Set
    End Property

    ''' <summary>
    ''' Line is end of paragraph.
    ''' </summary>
    Public Property EndOfParagraph As Boolean
        Get
            Return _EndOfParagraph
        End Get
        Friend Set(value As Boolean)
            _EndOfParagraph = value
        End Set
    End Property

    Public Property SegArray As TextBoxSeg()
        Get
            Return _SegArray
        End Get
        Friend Set(value As TextBoxSeg())
            _SegArray = value
        End Set
    End Property

    ''' <summary>
    ''' Gets line height.
    ''' </summary>
    Public ReadOnly Property LineHeight As Double
        Get
            Return Ascent + Descent
        End Get
    End Property

    ''' <summary>
    ''' TextBoxLine constructor.
    ''' </summary>
    ''' <param name="Ascent">Line ascent.</param>
    ''' <param name="Descent">Line descent.</param>
    ''' <param name="EndOfParagraph">Line is end of paragraph.</param>
    ''' <param name="SegArray">Segments' array.</param>
    Public Sub New(Ascent As Double, Descent As Double, EndOfParagraph As Boolean, SegArray As TextBoxSeg())
        Me.Ascent = Ascent
        Me.Descent = Descent
        Me.EndOfParagraph = EndOfParagraph
        Me.SegArray = SegArray
    End Sub
End Class
