#Region "Microsoft.VisualBasic::5922be49c16581d2f9acb65ba92610e2, Microsoft.VisualBasic.Core\Language\Linq\Vectorization\Strings.vb"

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

    '     Class Strings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Len
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Base = Microsoft.VisualBasic.Strings

Namespace Language.Vectorization

    ''' <summary>
    ''' The <see cref="String"/> module contains procedures used to perform string operations.
    ''' </summary>
    Public NotInheritable Class Strings

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Returns an integer containing either the number of characters in a string or 
        ''' the nominal number of bytes required to store a variable.
        ''' </summary>
        ''' <param name="strings">
        ''' Any valid String expression or variable name. If Expression is of type Object, 
        ''' the Len function returns the size as it will be written to the file by the 
        ''' FilePut function.
        ''' </param>
        ''' <returns>
        ''' Returns an integer containing either the number of characters in a string or 
        ''' the nominal number of bytes required to store a variable.
        ''' </returns>
        Public Shared Function Len(strings As IEnumerable(Of String)) As IEnumerable(Of Integer)
            Return strings.Select(AddressOf Base.Len)
        End Function
    End Class
End Namespace
