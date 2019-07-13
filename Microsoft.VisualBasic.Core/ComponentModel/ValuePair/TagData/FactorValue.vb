#Region "Microsoft.VisualBasic::e5f6f962adb352ccd0e7877755b9c8e5, Microsoft.VisualBasic.Core\ComponentModel\ValuePair\TagData\FactorValue.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FactorValue
    ' 
    '         Properties: factor, Value
    ' 
    '     Class FactorString
    ' 
    '         Properties: factor, text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.TagData

    Public Class FactorValue(Of T As {Structure, IComparable(Of T)}, V)

        Public Property factor As T
        Public Property Value As V

    End Class

    Public Class FactorString(Of T As {Structure, IComparable(Of T)})

        Public Property factor As T
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"Dim {text} As {GetType(T).FullName} = {factor.GetJson}"
        End Function
    End Class
End Namespace
