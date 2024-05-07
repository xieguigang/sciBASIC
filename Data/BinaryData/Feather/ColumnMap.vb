Imports System
Imports System.Collections.Generic

''' <summary>
''' Utility class for addressing a dataframes columns.
''' </summary>
Public Class ColumnMap
    Private Parent As DataFrame

    ''' <summary>
    ''' Number of columns in the dataframe
    ''' </summary>
    Public ReadOnly Property Count As Long
        Get
            Return Parent.ColumnCount
        End Get
    End Property

    ''' <summary>
    ''' Returns the column at the given index (in the dataframe's basis).
    ''' 
    ''' Throws if the index is out of range.
    ''' </summary>
    Default Public ReadOnly Property Item(index As Long) As Column
        Get
            Dim translatedIndex = Parent.TranslateIndex(index)

            If translatedIndex < 0 OrElse translatedIndex >= Parent.Metadata.Columns.Length Then
                Dim minLegal As Long
                Dim maxLegal As Long
                Select Case Parent.Basis
                    Case BasisType.One
                        minLegal = 1
                        maxLegal = Parent.Metadata.Columns.Length
                    Case BasisType.Zero
                        minLegal = 0
                        maxLegal = Parent.Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                End Select

                Throw New ArgumentOutOfRangeException(NameOf(index), $"Column index out of range, valid between [{minLegal}, {maxLegal}] found {index}")
            End If

            Return New Column With {
.Parent = Parent,
.TranslatedColumnIndex = translatedIndex
}
        End Get
    End Property

    ''' <summary>
    ''' Returns the column with the given name.
    ''' 
    ''' Throws if no column has the given name.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Column
        Get
            Dim translatedIndex As Long
            If Not Parent.TryLookupTranslatedColumnIndex(columnName, translatedIndex) Then
                Throw New KeyNotFoundException($"Could not find column with name ""{columnName}""")
            End If

            Return New Column(Parent, translatedIndex)
        End Get
    End Property

    Friend Sub New(parent As DataFrame)
        Me.Parent = parent
    End Sub
End Class
