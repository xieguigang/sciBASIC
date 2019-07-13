#Region "Microsoft.VisualBasic::5e52fb923691786f6ed99bbb88af77c4, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\Hypothesis.vb"

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

    '     Enum Hypothesis
    ' 
    '         Greater, Less, TwoSided
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Hypothesis

    Public Enum Hypothesis
        ''' <summary>
        ''' ``mu > mu0``
        ''' </summary>
        Greater
        ''' <summary>
        ''' ``mu &lt; mu0``
        ''' </summary>
        Less
        ''' <summary>
        ''' ``mu &lt;> mu0``
        ''' </summary>
        TwoSided
    End Enum
End Namespace
