#Region "Microsoft.VisualBasic::13104db3a3ee0ac77538902faca29f28, mime\text%yaml\1.2\Syntax\MappingEntry.vb"

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

    '     Class MappingEntry
    ' 
    '         Properties: Key, Name, Value
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Syntax

    Public Class MappingEntry : Implements INamedValue

        Public Property Key As DataItem
        Public Property Value As DataItem

        Private Property Name As String Implements INamedValue.Key
            Get
                Return Scripting.ToString(Key)
            End Get
            Set(value As String)
                Throw New ReadOnlyException
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{Key}: {Value}"
        End Function
    End Class
End Namespace
