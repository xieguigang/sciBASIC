#Region "Microsoft.VisualBasic::bc03d914d599c8998230d843a443ae88, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Set\BadBehaviourResponses.vb"

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

    '     Enum BadBehaviourResponses
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' Enum of values to determine the aggressiveness of the response of the
    ''' class to bad data.
    ''' </summary>
    Public Enum BadBehaviourResponses
        ''' <summary>
        ''' If the user enters bad data, throw an exception they have to deal with.
        ''' </summary>
        BeAggressive = 0

        ''' <summary>
        ''' If the user enters bad data, just eat it quietly.
        ''' </summary>
        BeCool = 1
    End Enum
End Namespace
