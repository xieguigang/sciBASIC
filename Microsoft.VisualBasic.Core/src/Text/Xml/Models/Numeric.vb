Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    Public Class ints

        <XmlAttribute> Public Property ints As Integer()

        Sub New()
        End Sub

        Sub New(ints As IEnumerable(Of Integer))
            If ints IsNot Nothing Then
                _ints = ints.ToArray
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return ints.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(xml As ints) As Integer()
            Return If(xml Is Nothing, Nothing, xml.ints)
        End Operator
    End Class

    Public Class uints
        <XmlAttribute> Public Property uints As UInteger()
    End Class

    Public Class longs
        <XmlAttribute> Public Property longs As Long()
    End Class

    Public Class ulongs
        <XmlAttribute> Public Property ulongs As ULong()
    End Class

    Public Class doubles
        <XmlAttribute> Public Property doubles As Double()
    End Class

    Public Class floats
        <XmlAttribute> Public Property floats As Single()
    End Class
End Namespace