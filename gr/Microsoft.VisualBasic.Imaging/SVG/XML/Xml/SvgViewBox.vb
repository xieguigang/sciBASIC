Imports System.Globalization

Namespace SvgLib
    Public Structure SvgViewBox
        Public Property Left As Double
        Public Property Top As Double
        Public Property Width As Double
        Public Property Height As Double

        Public Overrides Function ToString() As String
            Return String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", Left, Top, Width, Height)
        End Function
    End Structure
End Namespace
