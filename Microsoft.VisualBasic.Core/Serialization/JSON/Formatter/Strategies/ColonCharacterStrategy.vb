#Region "Microsoft.VisualBasic::95f725c3216ea3db06f0ede28f2d80fd, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\ColonCharacterStrategy.vb"

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

    '     Class ColonCharacterStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies
    Friend NotInheritable Class ColonCharacterStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            context.IsProcessingVariableAssignment = True
            context.AppendCurrentChar()
            context.AppendSpace()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return ":"c
            End Get
        End Property
    End Class
End Namespace
