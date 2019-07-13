#Region "Microsoft.VisualBasic::f4ae181d6a8e6536abf199259fe36a72, mime\application%json\Parser\JsonElement.vb"

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

    '     Class JsonModel
    ' 
    ' 
    ' 
    '     Class JsonElement
    ' 
    '         Function: [As], ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Parser

    Public MustInherit Class JsonModel : Inherits JsonElement

#Region "Json property and value"
        Default Public Property Item(str As String) As JsonElement
            Get
                Return CType(Me, JsonObject)(str)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonObject)(str) = value
            End Set
        End Property

        Default Public Property Item(index As Integer) As JsonElement
            Get
                Return CType(Me, JsonArray)(index)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonArray)(index) = value
            End Set
        End Property
#End Region

    End Class

    Public MustInherit Class JsonElement

        Public MustOverride Function BuildJsonString() As String

        Public Overrides Function ToString() As String
            Return "base::json"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [As](Of T As JsonElement)() As T
            Return DirectCast(Me, T)
        End Function
    End Class
End Namespace
