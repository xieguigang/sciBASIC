#Region "Microsoft.VisualBasic::0ebbcc66232061388db6b453151835f8, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\Strategies\DefaultCharacterStrategy.vb"

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

    '     Class DefaultCharacterStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals.Strategies

    Friend NotInheritable Class DefaultCharacterStrategy
        Implements ICharacterStrategy
        Public Sub Execute(context As JsonFormatterStrategyContext) Implements ICharacterStrategy.Execute
            context.AppendCurrentChar()
        End Sub

        Public ReadOnly Property ForWhichCharacter() As Char Implements ICharacterStrategy.ForWhichCharacter
            Get
                Const msg As String = "This strategy was not intended for any particular character."
                Throw New InvalidOperationException(msg)
            End Get
        End Property
    End Class
End Namespace
