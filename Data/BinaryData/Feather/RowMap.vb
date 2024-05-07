Imports System

Namespace FeatherDotNet
    ''' <summary>
    ''' Utility class for addressing a dataframe's rows.
    ''' </summary>
    Public Class RowMap
        Private Parent As DataFrame

        ''' <summary>
        ''' Number of rows in the dataframe
        ''' </summary>
        Public ReadOnly Property Count As Long
            Get
                Return Parent.RowCount
            End Get
        End Property

        ''' <summary>
        ''' Returns the row at the given index (in the dataframe's basis).
        ''' 
        ''' Throws if the index is out of range.
        ''' </summary>
        Default Public ReadOnly Property Item(index As Long) As Row
            Get
                Dim translatedIndex = Parent.TranslateIndex(index)

                If translatedIndex < 0 OrElse translatedIndex >= Parent.Metadata.NumRows Then
                    Dim minLegal As Long
                    Dim maxLegal As Long
                    Select Case Parent.Basis
                        Case BasisType.One
                            minLegal = 1
                            maxLegal = Parent.Metadata.NumRows
                        Case BasisType.Zero
                            minLegal = 0
                            maxLegal = Parent.Metadata.NumRows - 1
                        Case Else
                            Throw New InvalidOperationException($"Unexpected Basis: {Parent.Basis}")
                    End Select

                    Throw New ArgumentOutOfRangeException(NameOf(index), $"Row index out of range, valid between [{minLegal}, {maxLegal}] found {index}")
                End If

                Return New Row(Parent, translatedIndex)
            End Get
        End Property

        Friend Sub New(parent As DataFrame)
            Me.Parent = parent
        End Sub
    End Class
End Namespace
