Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Public Class DataSet
    Implements IEnumerable
    Private data As List(Of Datum)

    Private components_Renamed As Integer

    Public Sub New(fileName As String, components As Integer)
        components_Renamed = components
        data = New List(Of Datum)()
        'read in data from file
        Try
            Dim reader As StreamReader = New StreamReader(fileName)
            Dim line As Value(Of String) = ""
            While Not (line = reader.ReadLine()) Is Nothing
                data.Add(New Datum(line, components))
            End While
        Catch e As FileNotFoundException
            Console.WriteLine("error: " & e.ToString())
        Catch e As IOException
            Console.WriteLine("error: " & e.ToString())
        End Try
    End Sub

    Public Overridable ReadOnly Property Stdev As Double
        Get
            Dim mean = Me.Mean
            Dim lStdev = 0.0
            For Each d In data
                lStdev += std.Pow(d.val() - mean, 2)
            Next

            lStdev /= data.Count
            lStdev = std.Sqrt(lStdev)
            Return lStdev
        End Get
    End Property

    Public Overridable ReadOnly Property Mean As Double
        Get
            Dim lMean = 0.0
            For Each d In data
                lMean += d.val()
            Next

            lMean /= data.Count
            Return lMean
        End Get
    End Property

    Public Overridable Function components() As Integer
        Return components_Renamed
    End Function

    Public Overridable Function nI(i As Integer) As Double
        Dim sum = 0.0
        For Each d In data
            sum += d.getProb(i)
        Next
        Return sum
    End Function

    Public Overridable Function size() As Integer
        Return data.Count
    End Function

    Public Overridable Function [get](i As Integer) As Datum
        Return data(i)
    End Function

    Public Overridable Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return data.GetEnumerator()
    End Function
End Class
