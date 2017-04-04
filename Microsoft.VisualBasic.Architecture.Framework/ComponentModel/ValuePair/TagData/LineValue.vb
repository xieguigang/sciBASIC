Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.TagData

    Public Structure LineValue(Of T)
        Implements IAddress(Of Integer)
        Implements Value(Of T).IValueOf

        Public Property Line As Integer Implements IAddress(Of Integer).Address
        Public Property value As T Implements Value(Of T).IValueOf.value

    End Structure
End Namespace