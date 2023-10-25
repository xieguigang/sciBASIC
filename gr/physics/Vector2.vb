Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports System.Math

Public Class Vector2 : Inherits Vector2D

    Public Shared ReadOnly Property down As Vector2
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New Vector2(0, 1)
        End Get
    End Property

    Public Shared ReadOnly Property up As Vector2
        Get
            Return New Vector2(0, -1)
        End Get
    End Property

    Public Shared ReadOnly Property right As Vector2
        Get
            Return New Vector2(1, 0)
        End Get
    End Property

    Public Shared ReadOnly Property left As Vector2
        Get
            Return New Vector2(-1, 0)
        End Get
    End Property

    Public Shared ReadOnly Property zero As Vector2
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New Vector2(0, 0)
        End Get
    End Property

    Public Shared ReadOnly Property one As Vector2
        Get
            Return New Vector2(1, 1)
        End Get
    End Property

    Public ReadOnly Property magnitude As Double
        Get
            Return Sqrt(x ^ 2 + y ^ 2)
        End Get
    End Property

    Default Public Property Field(offset As Integer) As Double
        Get
            If offset = 0 Then
                Return x
            ElseIf offset = 1 Then
                Return y
            Else
                Throw New InvalidProgramException(offset)
            End If
        End Get
        Set(value As Double)
            If offset = 0 Then
                x = value
            ElseIf offset = 1 Then
                y = value
            Else
                Throw New InvalidProgramException(offset)
            End If
        End Set
    End Property

    Sub New()
    End Sub

    Sub New(x#, y#)
        Call MyBase.New(x, y)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(x!, y!)
        Call MyBase.New(CDbl(x), CDbl(y))
    End Sub

    Public Shared Function random(box As SizeF) As Vector2
        Return New Vector2(randf.NextDouble(0, box.Width), randf.NextDouble(0, box.Height))
    End Function

    Public Shared Function random(box As Size) As Vector2
        Return New Vector2(randf.NextInteger(box.Width), randf.NextInteger(box.Height))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(v As Vector2) As PointF
        Return New PointF(v.x, v.y)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator *(v As Vector2, a As Double) As Vector2
        Return New Vector2(v.x * a, v.y * a)
    End Operator

    Public Overloads Shared Operator *(a As Double, v As Vector2) As Vector2
        Return New Vector2(v.x * a, v.y * a)
    End Operator

    Public Overloads Shared Operator *(a As Vector2, b As Vector2) As Vector2
        Return New Vector2(a.x * b.x, a.y * b.y)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator +(v As Vector2, a As Layout2D) As Vector2
        Return New Vector2(v.x + a.X, v.y + a.Y)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator -(n As Double, v As Vector2) As Vector2
        Return New Vector2(n - v.x, n - v.y)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator -(a As Vector2, b As Layout2D) As Vector2
        Return New Vector2(a.x - b.X, a.y - b.Y)
    End Operator

    Public Overloads Shared Operator /(v As Vector2, n As Double) As Vector2
        If n = 0.0 Then
            Return Vector2.zero
        Else
            Return New Vector2(v.x / n, v.y / n)
        End If
    End Operator
End Class
