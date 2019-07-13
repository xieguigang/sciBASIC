#Region "Microsoft.VisualBasic::79b2798fa052d1d45c811bd4bea65f3b, Data\SearchEngine\SearchEngine\QueryArgument.vb"

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

    ' Class QueryArgument
    ' 
    '     Properties: Data, Expression, Name
    ' 
    '     Function: Compile, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class QueryArgument : Implements INamedValue

    Public Property Name As String Implements INamedValue.Key
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' Example: 
    ''' 
    ''' ```
    ''' #\d+
    ''' #\d+ AND Name:'#\s{3,}' TOP 100
    ''' value:'#[a-zA-Z]{5,}' LIMIT 10
    ''' ```
    ''' </remarks>
    Public Property Expression As String

    ''' <summary>
    ''' The additional extension data.
    ''' </summary>
    ''' <returns></returns>
    Public Property Data As Dictionary(Of String, String)

    Public Function Compile() As Expression
        Return Build(Expression)
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
