Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.SVM.Model
Imports Microsoft.VisualBasic.Imaging
Imports Canvas = System.Drawing.Graphics

''' <summary>
''' The view used for drawing the coordinate system.
''' 
''' @author Ralf Wondratschek
''' </summary>
Public Class CartesianCoordinateSystem

    Private Const STROKE_WIDTH As Integer = 4
    Private Const CIRCLE_RADIUS As Integer = 20

    Private mPaint As Brush
    Private mTextBounds As Rectangle
    Private mState As State
    Private mLineBuilder As LineBuilder

    Public Property Line As Line
        Get
            If mState.Line IsNot Nothing Then
                Return mState.Line
            ElseIf mLineBuilder IsNot Nothing Then
                Return mLineBuilder.Build()
            End If
            Return Nothing
        End Get
        Set(value As Line)
            mState.Line = value
            mLineBuilder = Nothing
        End Set
    End Property

    Public ReadOnly Property Points As IList(Of LabeledPoint)
        Get
            Return mState.Points
        End Get
    End Property

    Public ReadOnly Property State As State
        Get
            Return New State(Me, mState.Points, mState.Line)
        End Get
    End Property

    Sub New(Optional points As IEnumerable(Of LabeledPoint) = Nothing)
        mPaint = New SolidBrush(Color.White)
        mState = New State(Me)
        mTextBounds = New Rectangle

        If Not points Is Nothing Then
            For Each x In points
                Call mState.AddPoint(x)
            Next
        End If
    End Sub

    Sub Draw(canvas As Canvas, mwidth!, mheight!)
        Dim pen As Pen = Pens.Black

        canvas.DrawLine(pen, 0, 0, 0, mheight)
        canvas.DrawLine(pen, 0, mheight, mwidth, mheight)

        Dim textLineWidth As Integer = 10
        Dim textPadding As Integer = textLineWidth + 6

        canvas.DrawLine(pen, 0, 0, textLineWidth, 0)
        canvas.DrawLine(pen, 0, mheight \ 4 * 3, textLineWidth, mheight \ 4 * 3)
        canvas.DrawLine(pen, 0, mheight \ 2, textLineWidth, mheight \ 2)
        canvas.DrawLine(pen, 0, mheight \ 4, textLineWidth, mheight \ 4)


        canvas.DrawLine(pen, mwidth, mheight, mwidth, mheight - textLineWidth)
        canvas.DrawLine(pen, mwidth \ 4, mheight, mwidth \ 4, mheight - textLineWidth)
        canvas.DrawLine(pen, mwidth \ 2, mheight, mwidth \ 2, mheight - textLineWidth)
        canvas.DrawLine(pen, mwidth \ 4 * 3, mheight, mwidth \ 4 * 3, mheight - textLineWidth)

        Dim text As String = "0.25"
        Dim f As New Font("Microsoft YaHei", 12)
        Dim mTextBounds = canvas.MeasureString(text, f)
        canvas.DrawString(text, f, Brushes.Black, textPadding, mheight \ 4 * 3 + mTextBounds.Height() \ 2)
        canvas.DrawString(text, f, Brushes.Black, mwidth \ 4 - mTextBounds.Width() \ 2, mheight - textPadding)

        text = "0.5"
        mTextBounds = canvas.MeasureString(text, f)
        canvas.DrawString(text, f, Brushes.Black, textPadding, mheight \ 2 + mTextBounds.Height() \ 2)
        canvas.DrawString(text, f, Brushes.Black, mwidth \ 2 - mTextBounds.Width() \ 2, mheight - textPadding)

        text = "0.75"
        mTextBounds = canvas.MeasureString(text, f)
        canvas.DrawString(text, f, Brushes.Black, textPadding, mheight \ 4 + mTextBounds.Height() \ 2)
        canvas.DrawString(text, f, Brushes.Black, mwidth \ 4 * 3 - mTextBounds.Width() \ 2, mheight - textPadding)

        text = "1.0"
        mTextBounds = canvas.MeasureString(text, f)
        canvas.DrawString(text, f, Brushes.Black, textPadding, mTextBounds.Height())
        canvas.DrawString(text, f, Brushes.Black, mwidth - mTextBounds.Width() - 3, mheight - textPadding)

        For Each p As LabeledPoint In mState.Points
            If p.ColorClass = ColorClass.RED Then
                canvas.DrawCircle(Pens.Red, CSng(p.X1) * mwidth, CSng(1 - p.X2) * mheight, CIRCLE_RADIUS, fill:=False)
            Else
                canvas.DrawCircle(Pens.Blue, CSng(p.X1) * mwidth, CSng(1 - p.X2) * mheight, CIRCLE_RADIUS, fill:=False)
            End If
        Next

        Dim lineText As Line = mState.Line
        If lineText Is Nothing AndAlso mLineBuilder IsNot Nothing Then lineText = mLineBuilder.Build()

        If lineText IsNot Nothing Then
            text = "y = " & Math.Round(lineText.Increase * 100) / 100.0R & " * x + " & Math.Round(lineText.Offset * 100) / 100.0R
            mTextBounds = canvas.MeasureString(text, f)
            canvas.DrawString(text, f, Brushes.Black, mwidth - textPadding - mTextBounds.Width(), textPadding + mTextBounds.Height())
        End If

        If mLineBuilder IsNot Nothing Then
            canvas.DrawLine(pen, mLineBuilder.StartX * mwidth, (1 - mLineBuilder.StartY) * mheight, mLineBuilder.EndX * mwidth, (1 - mLineBuilder.EndY) * mheight)
        ElseIf mState.Line IsNot Nothing Then
            Dim p1 As New Point(0, CSng(1 - mState.Line.CalcY(0)) * mheight)
            Dim p2 As New Point(mwidth, CSng(1 - mState.Line.CalcY(1)) * mheight)
            canvas.DrawLine(pen, p1, p2)
        End If
    End Sub

    Public Sub AddPoint(point As LabeledPoint)
        mState.addPoint(point)
    End Sub

    Public Sub ClearPoints()
        mState.clearPoints()
    End Sub

    Public Sub RemovePoint(point As LabeledPoint)
        mState.removePoint(point)
    End Sub
End Class
