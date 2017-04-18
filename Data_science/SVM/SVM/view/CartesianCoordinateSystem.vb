Imports System
Imports System.Collections.Generic
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
    'Inherits android.view.View

    'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    '    Private Const net As net.vrallev.android.base.util.L

    ' TODO: fix hardcoded values
    Private Const STROKE_WIDTH As Integer = 4
    Private Const CIRCLE_RADIUS As Integer = 20

    Private mPaint As Brush
    Private mTextBounds As Rectangle



    Private mState As __State
    Private mLineBuilder As LineBuilder


    '  Private mObjectAnimator As ObjectAnimator

    Private mIgnoreTouch As Boolean

    ''JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    'Public Sub New(context As android.content.Context)
    '    MyBase.New(context)
    '    construtor()
    'End Sub

    ''JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    'Public Sub New(context As android.content.Context, attrs As android.util.AttributeSet)
    '    MyBase.New(context, attrs)
    '    construtor()
    'End Sub

    ''JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    'Public Sub New(context As android.content.Context, attrs As android.util.AttributeSet, defStyle As Integer)
    '    MyBase.New(context, attrs, defStyle)
    '    construtor()
    'End Sub

    Sub New()
        mPaint = New SolidBrush(Color.White)
        '   mPaint.Style = android.graphics.Paint.Style.FILL_AND_STROKE
        ' mPaint.AntiAlias = True
        '  mPaint.Color = android.graphics.Color.WHITE

        ' mPaint.StrokeWidth = STROKE_WIDTH

        mState = New __State(Me)

        mTextBounds = New Rectangle
    End Sub

    Protected Friend Sub onDraw(canvas As Canvas, mwidth!, mheight!)
        Dim pen As Pen = Pens.Black

        canvas.DrawLine(pen, 0, 0, 0, mheight)
        canvas.DrawLine(pen, 0, mheight, mwidth, mheight)

        'Dim strokeWidth As Single = mPaint.StrokeWidth

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

        'mPaint.TextSize = 24
        'mPaint.StrokeWidth = 1

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

        For Each p As LabeledPoint In mState.mPoints
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

        '   mPaint.StrokeWidth = strokeWidth
        '  mPaint.Color = android.graphics.Color.WHITE

        If mLineBuilder IsNot Nothing Then
            '  mPaint.Color = android.graphics.Color.WHITE
            canvas.DrawLine(pen, mLineBuilder.StartX * mwidth, (1 - mLineBuilder.StartY) * mheight, mLineBuilder.EndX * mwidth, (1 - mLineBuilder.EndY) * mheight)

        ElseIf mState.Line IsNot Nothing Then
            ' mPaint.Color = android.graphics.Color.WHITE
            Dim p1 As New Point(0, CSng(1 - mState.Line.CalcY(0)) * mheight)
            Dim p2 As New Point(mwidth, CSng(1 - mState.Line.CalcY(1)) * mheight)
            canvas.DrawLine(pen, p1, p2)
        End If
    End Sub

    Private mPendingPoint As LabeledPoint

    'Public Function onTouchEvent([event] As android.view.MotionEvent) As Boolean
    '    If [event].Action = android.view.MotionEvent.ACTION_DOWN Then mIgnoreTouch = net.vrallev.android.svm.OptimizerCalculator.Instance.Calculating

    '    If mIgnoreTouch Then Return True

    '    Select Case [event].Action
    '        Case android.view.MotionEvent.ACTION_DOWN
    '            If net.vrallev.android.svm.MenuState.STATE_LINE.Equals(mMenuState) Then
    '                If mObjectAnimator IsNot Nothing Then mObjectAnimator.cancel()
    '                mState.mLine = Nothing
    '                mLineBuilder = New Line.Builder([event].X / mWidth, 1 - [event].Y / mHeight, [event].X / mWidth, 1 - [event].Y / mHeight)
    '                invalidate()

    '            Else
    '                Dim onClickPoint As LabeledPoint = getPointOnClick([event].X, [event].Y)
    '                If onClickPoint IsNot Nothing Then
    '                    mState.removePoint(onClickPoint)
    '                Else
    '                    mPendingPoint = LabeledPoint.getInstance([event].X / mWidth, 1 - [event].Y / mHeight, mMenuState.ColorClass)
    '                    mState.addPoint(mPendingPoint)
    '                End If
    '            End If
    '            Return True

    '        Case android.view.MotionEvent.ACTION_MOVE
    '            If net.vrallev.android.svm.MenuState.STATE_LINE.Equals(mMenuState) AndAlso mLineBuilder IsNot Nothing Then
    '                mLineBuilder.updateEndPoint([event].X / mWidth, 1 - [event].Y / mHeight)

    '            ElseIf mPendingPoint IsNot Nothing Then
    '                mPendingPoint.X1 = [event].X / mWidth
    '                mPendingPoint.X2 = 1 - [event].Y / mHeight
    '            End If
    '            invalidate()
    '            Return True

    '        Case android.view.MotionEvent.ACTION_UP
    '            If net.vrallev.android.svm.MenuState.STATE_LINE.Equals(mMenuState) AndAlso mLineBuilder IsNot Nothing Then

    '                If mLineBuilder.Length < 1 / 20.0R Then
    '                    mState.Line = Nothing
    '                    mLineBuilder = Nothing

    '                Else
    '                    mLineBuilder.buildAnimate(Me)
    '                End If
    '                invalidate()
    '            End If

    '            mPendingPoint = Nothing
    '            Return True
    '    End Select

    '    Return MyBase.onTouchEvent([event])
    'End Function

    'Protected Friend Function onSaveInstanceState() As android.os.Parcelable
    '    Dim bundle As New android.os.Bundle
    '    bundle.putParcelable("instanceState", MyBase.onSaveInstanceState())
    '    bundle.putString("state", (New com.google.gson.Gson).toJson(mState))

    '    Return bundle
    'End Function

    'Public Sub onRestoreInstanceState(state As android.os.Parcelable)

    '    If TypeOf state Is android.os.Bundle Then
    '        Dim bundle As android.os.Bundle = CType(state, android.os.Bundle)
    '        Dim json As String = bundle.getString("state", Nothing)
    '        If json IsNot Nothing Then
    '            Dim state1 As State = (New com.google.gson.Gson).fromJson(json, GetType(State))
    '            mState = New State(Me, state1.Points, state1.Line)
    '            state1.releasePoints()
    '        End If

    '        MyBase.onRestoreInstanceState(bundle.getParcelable("instanceState"))
    '        Return
    '    End If

    '    MyBase.onRestoreInstanceState(state)
    'End Sub

    'Public  Property MenuState As net.vrallev.android.svm.MenuState
    '    Set(menuState As net.vrallev.android.svm.MenuState)
    '        mMenuState = menuState
    '    End Set
    'End Property

    Public Sub setLine(line As Line)

        mState.Line = line
        mLineBuilder = Nothing
    End Sub


    Public ReadOnly Property Line As Line
        Get
            If mState.Line IsNot Nothing Then
                Return mState.Line
            ElseIf mLineBuilder IsNot Nothing Then
                Return mLineBuilder.Build()
            End If
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property Points As IList(Of LabeledPoint)
        Get
            Return mState.mPoints
        End Get
    End Property

    Public Sub addPoint(point As LabeledPoint)
        mState.addPoint(point)
    End Sub

    Public Sub clearPoints()
        mState.clearPoints()
    End Sub

    Public Sub removePoint(point As LabeledPoint)
        mState.removePoint(point)
    End Sub

    Public ReadOnly Property State As __State
        Get
            Return New __State(Me, mState.mPoints, mState.Line)
        End Get
    End Property

    'Private Function getPointOnClick(x As Single, y As Single) As LabeledPoint
    '    For Each p As LabeledPoint In mState.mPoints
    '        If Math.Pow(x - p.X1 * mWidth, 2) + Math.Pow(y - (1 - p.X2) * mHeight, 2) <= Math.Pow(CIRCLE_RADIUS * 2, 2) Then Return p
    '    Next

    '    Return Nothing
    'End Function

    'Private Sub animateLineToPosition(start As Line, [end] As Line)
    '    mLineBuilder = New Line.Builder(0, CSng(start.getY(0)), 1, CSng(start.getY(1)))

    '    Dim holderStartY As PropertyValuesHolder = PropertyValuesHolder.ofFloat("startY", mLineBuilder.StartY, CSng([end].getY(0)))
    '    Dim holderEndY As PropertyValuesHolder = PropertyValuesHolder.ofFloat("endY", mLineBuilder.EndY, CSng([end].getY(1)))

    '    net.vrallev.android.base.util.L.d("Values ", mLineBuilder.StartY, " ", [end].getY(0), " ", mLineBuilder.EndY, " ", [end].getY(1))

    '    mObjectAnimator = ObjectAnimator.ofPropertyValuesHolder(mLineBuilder, holderStartY, holderEndY) 'holderStartX, holderEndX,
    '    mObjectAnimator.Interpolator = New android.view.animation.AccelerateDecelerateInterpolator
    '    mObjectAnimator.Duration = 1000L
    '    'JAVA TO VB CONVERTER TODO TASK: Anonymous inner classes are not converted to VB if the base type is not defined in the code being converted:
    '    '			mObjectAnimator.addUpdateListener(New ValueAnimator.AnimatorUpdateListener()
    '    '		{
    '    '			@Override public void onAnimationUpdate(ValueAnimator animation)
    '    '			{
    '    '				invalidate();
    '    '			}
    '    '		});
    '    'JAVA TO VB CONVERTER TODO TASK: Anonymous inner classes are not converted to VB if the base type is not defined in the code being converted:
    '    '			mObjectAnimator.addListener(New AnimatorListenerAdapter()
    '    '		{
    '    '			private boolean mCancelled = False;
    '    '
    '    '			@Override public void onAnimationCancel(Animator animation)
    '    '			{
    '    '				mCancelled = True;
    '    '			}
    '    '
    '    '			@Override public void onAnimationEnd(Animator animation)
    '    '			{
    '    '				if (!mCancelled && mLineBuilder != Nothing)
    '    '				{
    '    '					setLine(mLineBuilder.build(), False);
    '    '				}
    '    '				mObjectAnimator = Nothing;
    '    '			}
    '    '		});
    '    mObjectAnimator.start()
    'End Sub

    Public Class __State
        Friend ReadOnly outerInstance As CartesianCoordinateSystem


        Friend mPoints As New List(Of LabeledPoint)

        Sub New(outerInstance As CartesianCoordinateSystem)
            Me.New(outerInstance, New List(Of LabeledPoint)(), Nothing)
        End Sub

        Sub New(outerInstance As CartesianCoordinateSystem, points As IList(Of LabeledPoint), line As Line)
            Me.outerInstance = outerInstance
            mPoints = New List(Of LabeledPoint)(points.Count)
            For Each p As LabeledPoint In points
                mPoints.Add(p.clone())
            Next

            If line IsNot Nothing Then Me.Line = line.clone()
        End Sub

        Public Property Line As Line


        Public ReadOnly Property Points As IList(Of LabeledPoint)
            Get
                Return mPoints
            End Get
        End Property

        Sub addPoint(point As LabeledPoint)
            mPoints.Add(point)
            '    invalidate()
            '    de.greenrobot.event.EventBus.Default.postSticky(DirtyLineEvent.INSTANCE)
        End Sub

        Sub removePoint(point As LabeledPoint)
            mPoints.Remove(point)
            point.release()
            '   invalidate()
            '   de.greenrobot.event.EventBus.Default.postSticky(DirtyLineEvent.INSTANCE)
        End Sub

        Sub clearPoints()
            releasePoints()
            mPoints.Clear()
            '  invalidate()
            '   de.greenrobot.event.EventBus.Default.postSticky(DirtyLineEvent.INSTANCE)
        End Sub

        Public Overrides Function Equals(o As Object) As Boolean
            If TypeOf o Is __State Then
                Dim state As __State = CType(o, __State)
                If Not LabeledPoint.ListEqual(state.mPoints, mPoints) Then Return False
                If Me.Line Is Nothing AndAlso state.Line Is Nothing Then Return True
                If Me.Line IsNot Nothing AndAlso state.Line IsNot Nothing Then Return state.Line.Equals(Me.Line)

                Return False
            End If

            Return MyBase.Equals(o)
        End Function

        Public Sub releasePoints()
            For Each p As LabeledPoint In mPoints
                p.release()
            Next
        End Sub
    End Class
End Class
