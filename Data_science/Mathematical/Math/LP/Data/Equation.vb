Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LP

    ''' <summary>
    ''' Subject to these <see cref="Equation"/>
    ''' </summary>
    Public Structure Equation

        Dim xyz#(), c#

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace