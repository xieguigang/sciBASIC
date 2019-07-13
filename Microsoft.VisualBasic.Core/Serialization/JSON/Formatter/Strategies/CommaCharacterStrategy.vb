#Region "Microsoft.VisualBasic::0b971f95937a975d61ec82a40a5de12a, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\CommaCharacterStrategy.vb"

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

    '     Class CommaStrategy
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
    Friend NotInheritable Class CommaStrategy
        Implements ICharacterStrategy

        ''' <summary>
        ''' ,
        ''' </summary>
        Sub New()

        End Sub

        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            context.AppendCurrentChar()

            If context.IsProcessingString Then
                Return
            End If

            If Not context.IsInArrayScope Then
                context.BuildContextIndents()
            End If

            context.IsProcessingVariableAssignment = False
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Return ","c
            End Get
        End Property
    End Class
End Namespace
