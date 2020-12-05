Friend Class RowCol : Implements IEquatable(Of RowCol)

    Public Sub New(row As Integer, col As Integer)
        Me.Row = row
        Me.Col = col
    End Sub

    Public ReadOnly Property Row As Integer
    Public ReadOnly Property Col As Integer

    ' 2019-06-24 DWR: Structs get default Equals and GetHashCode implementations but they can be slow - having these versions makes the code run much quicker
    ' and it seems a good practice to throw in IEquatable<RowCol> to avoid boxing when Equals is called
    Public Overloads Function Equals(other As RowCol) As Boolean Implements IEquatable(Of RowCol).Equals
        Return other.Row = Row AndAlso other.Col = Col
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim rc = TryCast(obj, RowCol)
        Return (Not rc Is Nothing) AndAlso rc.Equals(Me)
    End Function

    Public Overrides Function GetHashCode() As Integer ' Courtesy of https://stackoverflow.com/a/263416/3813189
        ' BEGIN TODO : Visual Basic does Not support checked statements!
        Dim hash = 17 ' Overflow is fine, just wrap
        hash = hash * 23 + Row
        hash = hash * 23 + Col
        Return hash
        ' End TODO : Visual Basic does Not support checked statements!
    End Function
End Class