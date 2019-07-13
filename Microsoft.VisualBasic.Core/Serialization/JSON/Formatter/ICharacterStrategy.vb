#Region "Microsoft.VisualBasic::5af56f2dc28939b61c9be6a21c70e9ab, Microsoft.VisualBasic.Core\Serialization\JSON\Formatter\ICharacterStrategy.vb"

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

    '     Interface ICharacterStrategy
    ' 
    '         Properties: ForWhichCharacter
    ' 
    '         Sub: Execute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization.JSON.Formatter.Internals

    Friend Interface ICharacterStrategy
        Sub Execute(context As JsonFormatterStrategyContext)

        ReadOnly Property ForWhichCharacter() As Char
    End Interface
End Namespace
