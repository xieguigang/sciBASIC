Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Text.Xml.Models

    ''' <summary>
    ''' A <see cref="Double"/> type numeric sequence container
    ''' </summary>
    Public Class NumericVector

        <XmlAttribute> Public Property Vector As Double()

        ''' <summary>
        ''' Get/Set Element ``Xi``
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public Property Xi(i As Integer) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Vector(i)
            End Get
            Set(value As Double)
                Vector(i) = value
            End Set
        End Property

        ''' <summary>
        ''' The vector length for counting the elements in <see cref="Vector"/> property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Vector?.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace