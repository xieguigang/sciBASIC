#Region "Microsoft.VisualBasic::408d5f9d8f0b610734baa01728c56287, Data\SearchEngine\SearchEngine\Adapter\Text.vb"

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

    ' Structure Text
    ' 
    '     Properties: Text
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Structure Text

    ''' <summary>
    ''' The string value.
    ''' </summary>
    ''' <returns></returns>
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Structure
