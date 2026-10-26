Namespace Global.Microsoft.VisualBasic.Computing.JSCuda

    ''' <summary>
    ''' Raised when a JavaScript function cannot be translated into a CUDA kernel: an
    ''' unsupported statement / expression, a value that is not a script function, a captured
    ''' outer variable, an unmapped math call, and so on.
    ''' <para>
    ''' The pipeline deliberately fails loudly instead of emitting doubtful device code: a
    ''' kernel that compiles but computes something else is far more expensive to debug than a
    ''' translation error that names the offending construct.
    ''' </para>
    ''' </summary>
    Public Class JsCudaException : Inherits Exception

        ''' <summary>Name of the JavaScript function being translated (may be empty).</summary>
        Public Property FunctionName As String = ""

        ''' <summary>AST node kind that could not be translated (may be empty).</summary>
        Public Property NodeKind As String = ""

        ''' <summary>Source line / column of the node, when the parser recorded one.</summary>
        Public Property SourcePosition As String = ""

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(message As String, inner As Exception)
            MyBase.New(message, inner)
        End Sub

        Public Shared Function OfNode(functionName As String, node As Object, message As String) As JsCudaException
            Dim kind As String = If(node Is Nothing, "", node.GetType().Name)
            Dim position As String = ""

            Try
                ' every JS AST node carries a source position (Language/SourcePosition)
                Dim prop = node.GetType().GetProperty("Position")

                If prop IsNot Nothing Then
                    Dim positionValue = prop.GetValue(node)

                    If positionValue IsNot Nothing Then
                        Dim positionText = positionValue.ToString()

                        If Not String.IsNullOrEmpty(positionText) AndAlso positionText <> "None" Then
                            position = positionText
                        End If
                    End If
                End If
            Catch ex As Exception
                ' position is best effort only
            End Try

            Dim detail As String = If(String.IsNullOrEmpty(position),
                                      "node " & kind,
                                      "node " & kind & " at " & position)

            Return New JsCudaException(
                $"js->cuda: function '{functionName}': {message} ({detail})") With {
                .FunctionName = functionName,
                .NodeKind = kind,
                .SourcePosition = position
            }
        End Function

    End Class

End Namespace
