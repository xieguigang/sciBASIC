Imports System.Globalization
Imports System.Runtime.CompilerServices

Namespace SVG.XML

    Public Structure SvgViewBox

        Public Property Left As Double
        Public Property Top As Double
        Public Property Width As Double
        Public Property Height As Double

        Sub New(left As Double, top As Double, width As Double, height As Double)
            _Left = left
            _Top = top
            _Width = width
            _Height = height
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", Left, Top, Width, Height)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(v As Double()) As SvgViewBox
            Return New SvgViewBox(v(0), v(1), v(2), v(3))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(vb As (left As Double, top As Double, width As Double, height As Double)) As SvgViewBox
            Return New SvgViewBox(vb.left, vb.top, vb.width, vb.height)
        End Operator

    End Structure
End Namespace
