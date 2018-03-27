Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging.Math2D

    Public Structure Line

        Public Shared ReadOnly Empty As New Line

        Dim P1, P2 As PointF

        Public Sub New(p1 As PointF, p2 As PointF)
            Me.P1 = p1
            Me.P2 = p2
        End Sub

#Region "Points"
        Public Property X1() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P1.X
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P1.X = Value
            End Set
        End Property

        Public Property X2() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P2.X
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P2.X = Value
            End Set
        End Property

        Public Property Y1() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P1.Y
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P1.Y = Value
            End Set
        End Property

        Public Property Y2() As Single
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return P2.Y
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                P2.Y = Value
            End Set
        End Property
#End Region

        Public Overrides Function ToString() As String
            Return $"[{{{X1}, {Y1}}}, {{{X2}, {Y2}}}]"
        End Function
    End Structure
End Namespace