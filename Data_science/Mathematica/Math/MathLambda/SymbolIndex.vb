Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.xml

''' <summary>
''' the parameter symbol index of the lambda function
''' </summary>
Public Class SymbolIndex

    Dim m_index As Dictionary(Of String, ParameterExpression)
    Dim m_alignments As ParameterExpression()

    Default Public ReadOnly Property GetArgument(name As String) As ParameterExpression
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return m_index(name)
        End Get
    End Property

    Public ReadOnly Property Alignments As ParameterExpression()
        Get
            Return m_alignments.ToArray
        End Get
    End Property

    Private Sub New()
    End Sub

    Private Function GetParameterAlignment(lambda As MathML.LambdaExpression) As IEnumerable(Of ParameterExpression)
        Return From par As String
               In lambda.parameters
               Select m_index(par)
    End Function

    Public Shared Function FromLambda(lambda As MathML.LambdaExpression) As SymbolIndex
        Return New SymbolIndex With {
            .m_index = lambda.parameters _
                .Select(Function(name) Expression.Parameter(GetType(Double), name)) _
                .ToDictionary(Function(par)
                                  Return par.Name
                              End Function),
            .m_alignments = .GetParameterAlignment(lambda) _
                            .ToArray
        }
    End Function
End Class
