#Region "Microsoft.VisualBasic::6e505faee304339630f6b3e462d36b90, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\CloseBracketStrategy.vb"

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

    '     Class CloseBracketStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Execute, PeformNonStringPrint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies

    Friend NotInheritable Class CloseBracketStrategy
        Implements ICharacterStrategy

        ''' <summary>
        ''' }
        ''' </summary>
        Sub New()

        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            PeformNonStringPrint(context)
        End Sub

        Private Shared Sub PeformNonStringPrint(context As JsonFormatterStrategyContext)
            context.CloseCurrentScope()
            context.BuildContextIndents()
            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "}"c
            End Get
        End Property
    End Class
End Namespace
