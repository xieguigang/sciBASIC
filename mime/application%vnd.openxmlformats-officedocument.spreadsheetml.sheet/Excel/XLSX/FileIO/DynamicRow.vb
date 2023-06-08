Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

Namespace XLSX.FileIO

    ''' <summary>
    ''' Class representing a row that is either empty or containing cells. Empty rows can also carry information about height or visibility
    ''' </summary>
    Friend Class DynamicRow
        ''' <summary>
        ''' Defines the cellDefinitions
        ''' </summary>
        Private cellDefinitionsField As List(Of Cell)

        ''' <summary>
        ''' Gets or sets the row number (zero-based)
        ''' </summary>
        Public Property RowNumber As Integer

        ''' <summary>
        ''' Gets the List of cells if not empty
        ''' </summary>
        Public ReadOnly Property CellDefinitions As List(Of Cell)
            Get
                Return cellDefinitionsField
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DynamicRow"/> class
        ''' </summary>
        Public Sub New()
            cellDefinitionsField = New List(Of Cell)()
        End Sub
    End Class
End Namespace