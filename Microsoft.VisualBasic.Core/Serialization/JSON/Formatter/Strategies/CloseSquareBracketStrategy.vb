#Region "Microsoft.VisualBasic::9a90389d755caca5ca64fb98d74cd0c7, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\CloseSquareBracketStrategy.vb"

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

    '     Class CloseSquareBracketStrategy
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

    Friend NotInheritable Class CloseSquareBracketStrategy
        Implements ICharacterStrategy

        ''' <summary>
        ''' ]
        ''' </summary>
        Sub New()

        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            If context.IsProcessingString Then
                context.AppendCurrentChar()
                Return
            End If

            context.CloseCurrentScope()
            ' context.BuildContextIndents()
            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return "]"c
            End Get
        End Property
    End Class
End Namespace
