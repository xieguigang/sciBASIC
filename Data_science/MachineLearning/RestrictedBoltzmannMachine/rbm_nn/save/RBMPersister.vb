#Region "Microsoft.VisualBasic::4b8ab23e95d8e50aece23a89a4f80f79, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\save\RBMPersister.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 94
    '    Code Lines: 18
    ' Comment Lines: 61
    '   Blank Lines: 15
    '     File Size: 2.78 KB


    '     Class RBMPersister
    ' 
    '         Function: buildRBM, load
    ' 
    '         Sub: (+2 Overloads) save, writeStringBuilderData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace nn.rbm.save

    ''' <summary>
    ''' Created by kenny on 5/22/14.
    ''' </summary>
    Public Class RBMPersister


        Private Const DELIM As Char = ","c

        Public Sub save(rbm As RBM, file As String)
            'try
            '{
            '	System.IO.StreamWriter writer = new System.IO.StreamWriter(file);
            '	writeStringBuilderData(rbm, writer);
            '	writer.Close();
            '}
            'catch (IOException e)
            '{
            '	LOGGER.error(e.Message, e);
            '}
        End Sub

        Public Sub save(rbm As RBM, writer As TextWriter)
            'try
            '{
            '	writeStringBuilderData(rbm, writer);
            '}
            'catch (IOException e)
            '{
            '	LOGGER.error(e.Message, e);
            '}
        End Sub

        Public Sub writeStringBuilderData(rbm As RBM, writer As TextWriter)
            'writer.write(rbm.VisibleSize.ToString());
            'writer.write(DELIM);
            'writer.write(rbm.HiddenSize.ToString());
            'writer.write('\n');

            'Matrix weights = rbm.Weights;
            'for (int i = 0; i < rbm.VisibleSize; i++)
            '{
            '	for (int j = 0; j < rbm.HiddenSize; j++)
            '	{
            '		writer.write(weights.get(i, j).ToString());

            '		if (j < rbm.HiddenSize - 1)
            '		{
            '			writer.write(DELIM);
            '		}
            '	}
            '	writer.write('\n');
            '}
        End Sub

        Public Function load(file As String) As RBM
            'try
            '{
            '	return buildRBM(IOUtils.readLines(new System.IO.StreamReader(file)));
            '}
            'catch (IOException e)
            '{
            '	LOGGER.error(e.Message, e);
            '	return null;
            '}
            Throw New NotImplementedException()
        End Function

        Public Function buildRBM(lines As IList(Of String)) As RBM
            'int[] metaData = COMMA_TO_INT_ARRAY_DESERIALIZER.apply(lines[0]);
            'int visibleSize = metaData[0];
            'int hiddenSize = metaData[1];

            'RBM rbm = new RBM(visibleSize, hiddenSize);

            'Matrix weights = rbm.Weights;
            'for (int i = 0; i < visibleSize; i++)
            '{
            '	double[] values = COMMA_TO_DOUBLE_ARRAY_DESERIALIZER.apply(lines[i + 1]);
            '	for (int j = 0; j < hiddenSize; j++)
            '	{
            '		weights.set(i, j, values[j]);
            '	}
            '}
            'return rbm;
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace
