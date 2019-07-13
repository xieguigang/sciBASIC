#Region "Microsoft.VisualBasic::957d2d7580674b7f43a4e520abcac7b0, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\OpenSquareBracketStrategy.vb"

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

    '     Class OpenSquareBracketStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class OpenSquareBracketStrategy
        Implements ICharacterStrategy

        ''' <summary>
        ''' [
        ''' </summary>
        Sub New()

        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            context.AppendCurrentChar()

            If context.IsProcessingString Then
                Return
            End If

            context.EnterArrayScope()
            ' context.BuildContextIndents()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "["c
            End Get
        End Property
    End Class
End Namespace
