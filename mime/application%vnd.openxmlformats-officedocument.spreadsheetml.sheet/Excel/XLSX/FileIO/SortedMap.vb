Namespace XLSX.FileIO

    ''' <summary>
    ''' Class to manage key value pairs (string / string). The entries are in the order how they were added
    ''' </summary>
    Public Class SortedMap

        ''' <summary>
        ''' Defines the keyEntries
        ''' </summary>
        Private ReadOnly keyEntries As List(Of String)

        ''' <summary>
        ''' Defines the valueEntries
        ''' </summary>
        Private ReadOnly valueEntries As List(Of String)

        ''' <summary>
        ''' Defines the index
        ''' </summary>
        Private ReadOnly index As Dictionary(Of String, Integer)

        ''' <summary>
        ''' Gets the Count
        ''' Number of map entries
        ''' </summary>
        Public ReadOnly Property Count As Integer

        ''' <summary>
        ''' Gets the keys of the map as list
        ''' </summary>
        Public ReadOnly Property Keys As IEnumerable(Of String)
            Get
                Return keyEntries
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SortedMap"/> class
        ''' </summary>
        Public Sub New()
            keyEntries = New List(Of String)()
            valueEntries = New List(Of String)()
            index = New Dictionary(Of String, Integer)()
            Count = 0
        End Sub

        ''' <summary>
        ''' Method to add a key value pair
        ''' </summary>
        ''' <param name="key">Key as string.</param>
        ''' <param name="value">Value as string.</param>
        ''' <returns>Returns the resolved string (either added or returned from an existing entry).</returns>
        Public Function Add(key As String, value As String) As String
            If index.ContainsKey(key) Then
                Return valueEntries(index(key))
            End If
            index.Add(key, Count)
            _Count += 1
            keyEntries.Add(key)
            valueEntries.Add(value)
            Return value
        End Function
    End Class

End Namespace