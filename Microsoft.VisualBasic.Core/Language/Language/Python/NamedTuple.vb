#Region "Microsoft.VisualBasic::6cd06372a0ab61fdc6935b640899b086, Microsoft.VisualBasic.Core\Language\Language\Python\NamedTuple.vb"

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

    '     Class NamedTuple
    ' 
    '         Properties: Type
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Python

    ''' <summary>
    ''' ``namedtuple()`` Factory Function for Tuples with Named Fields
    ''' </summary>
    Public Class NamedTuple : Inherits [Property](Of Object)
        Implements INamedValue

        Public Property Type As String Implements INamedValue.Key

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
