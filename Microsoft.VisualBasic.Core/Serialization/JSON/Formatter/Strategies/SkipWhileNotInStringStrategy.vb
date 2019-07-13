#Region "Microsoft.VisualBasic::9ae3f74d01beece56f19fb285cf49d22, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\SkipWhileNotInStringStrategy.vb"

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

    '     Class SkipWhileNotInStringStrategy
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
    Friend NotInheritable Class SkipWhileNotInStringStrategy
        Implements ICharacterStrategy
        Private ReadOnly selectionCharacter As Char

        Public Sub New(selectionCharacter As Char)
            Me.selectionCharacter = selectionCharacter
        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
            End If
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return Me.selectionCharacter
            End Get
        End Property
    End Class
End Namespace
