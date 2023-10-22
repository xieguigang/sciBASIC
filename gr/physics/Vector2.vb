Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Class Vector2 : Inherits Vector2D

    Public Shared ReadOnly Property down As Vector2
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New Vector2(0, 1)
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

    Sub New()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(x!, y!)
        Call MyBase.New(x, y)
    End Sub

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

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator +(v As Vector2, a As Layout2D) As Vector2
        Return New Vector2(v.x + a.X, v.y + a.Y)
    End Operator

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Operator -(n As Double, v As Vector2) As Vector2
        Return New Vector2(n - v.x, n - v.y)
    End Operator
End Class
