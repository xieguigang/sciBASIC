''' <summary>
''' Example CLR model demonstrating dictionary-property flattening to CSV columns.
''' Saving one instance writes:  name,age,bmi,weight
''' </summary>
Public Class Person

    Public Property name As String

    Public Property age As Double

    ''' <summary>
    ''' Each key becomes an extra CSV column; values are Double.
    ''' </summary>
    Public Property data As Dictionary(Of String, Double)

End Class
