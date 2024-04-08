Imports System.Collections.Generic

Namespace util

    Public Class Arguments
        ' Variables
        Private Parameters As Dictionary(Of String, String)

        ''' <summary>
        ''' constructs an Arguments </summary>
        ''' <paramname="Args"> command line </param>
        Public Sub New(Args As String())
            'Parameters = new Dictionary<string, string>();
            'Pattern Spliter = Pattern.compile("^-{1,2}|^/|=", Pattern.CASE_INSENSITIVE);
            'Pattern Remover = Pattern.compile("^['\"\"]?(.*?)['\"\"]?$", Pattern.CASE_INSENSITIVE);
            'string Parameter = null;
            'string[] Parts;

            ''' Valid parameters forms:
            ''' {-,/,--}param{ ,=,:}((",')value(",'))
            ''' Examples:
            ''' -param1 value1 --param2 /param3:"Test-:-work"
            ''' /param4=happy -param5 '--=nice=--'
            'foreach (string Txt in Args)
            '{
            '	// Look for new parameters (-,/ or --) and a
            '	// possible enclosed value (=,:)
            '	Parts = Spliter.split(Txt, 3);

            '	switch (Parts.Length)
            '	{
            '	// Found a value (for the last parameter
            '	// found (space separator))
            '	case 1:
            '		if (!string.ReferenceEquals(Parameter, null))
            '		{
            '			if (!Parameters.ContainsKey(Parameter))
            '			{
            '				Parts[0] = Remover.matcher(Parts[0]).replaceAll("$1");
            '				//"$1"means group(1)
            '				// Remover.Replace(Parts[0], "$1");

            '				Parameters[Parameter] = Parts[0];
            '			}
            '			Parameter = null;
            '		}
            '		// else Error: no parameter waiting for a value (skipped)
            '		break;

            '		// Found just a parameter
            '	case 2:
            '		// The last parameter is still waiting.
            '		// With no value, set it to true.
            '		if (!string.ReferenceEquals(Parameter, null))
            '		{
            '			if (!Parameters.ContainsKey(Parameter))
            '			{
            '				Parameters[Parameter] = "true";
            '			}
            '		}
            '		Parameter = Parts[1];
            '		break;

            '		// Parameter with enclosed value
            '	case 3:
            '		// The last parameter is still waiting.
            '		// With no value, set it to true.
            '		if (!string.ReferenceEquals(Parameter, null))
            '		{
            '			if (!Parameters.ContainsKey(Parameter))
            '			{
            '				Parameters[Parameter] = "true";
            '			}
            '		}

            '		Parameter = Parts[1];

            '		// Remove possible enclosing characters (",')
            '		if (!Parameters.ContainsKey(Parameter))
            '		{
            '			Parts[2] = Remover.matcher(Parts[2]).replaceAll("$1");
            '			Parameters[Parameter] = Parts[2];
            '		}

            '		Parameter = null;
            '		break;
            '	}
            '}
            ''' In case a parameter is still waiting
            'if (!string.ReferenceEquals(Parameter, null))
            '{
            '	if (!Parameters.ContainsKey(Parameter))
            '	{
            '		Parameters[Parameter] = "true";
            '	}
            '}
        End Sub

        ''' <summary>
        ''' Retrieve a parameter value if it exists </summary>
        ''' <paramname="Param"> parameter name </param>
        ''' <returns> parameter value </returns>

        Public Overridable Function getValue(Param As String) As String
            If Parameters.ContainsKey(Param) Then
                Return Parameters(Param)
            End If
            Return Nothing
        End Function
    End Class

End Namespace
