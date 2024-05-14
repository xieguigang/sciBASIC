#Region "Microsoft.VisualBasic::120e617da9876e19c2c0930311e21c74, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\save\DeepRBMPersister.vb"

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

    '   Total Lines: 95
    '    Code Lines: 12
    ' Comment Lines: 66
    '   Blank Lines: 17
    '     File Size: 3.37 KB


    '     Class DeepRBMPersister
    ' 
    '         Function: load
    ' 
    '         Sub: save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep

Namespace nn.rbm.save

    ''' <summary>
    ''' Created by kenny on 5/22/14.
    ''' </summary>
    Public Class DeepRBMPersister

        Private Const DELIM As Char = ","c

        Private Shared ReadOnly RBM_PERSISTER As RBMPersister = New RBMPersister()

        Public Sub save(deepRBM As DeepRBM, file As String)
            'try
            '{
            '	System.IO.StreamWriter writer = new System.IO.StreamWriter(file);

            '	// write out layer info
            '	RBMLayer[] rbmLayers = deepRBM.RbmLayers;
            '	for (int l = 0; l < rbmLayers.Length; l++)
            '	{
            '		writer.Write((rbmLayers[l].size()).ToString());
            '		writer.BaseStream.WriteByte(DELIM);
            '		writer.Write((rbmLayers[l].getRBM(0).VisibleSize).ToString());
            '		writer.BaseStream.WriteByte(DELIM);
            '		writer.Write((rbmLayers[l].getRBM(0).HiddenSize).ToString());
            '		if (l < rbmLayers.Length - 1)
            '		{
            '			writer.BaseStream.WriteByte(DELIM);
            '		}
            '	}
            '	writer.BaseStream.WriteByte('\n');

            '	// for each layer, write out each rbm
            '	for (int l = 0; l < rbmLayers.Length; l++)
            '	{
            '		for (int r = 0; r < rbmLayers[l].size(); r++)
            '		{
            '			RBM_PERSISTER.writeStringBuilderData(rbmLayers[l].getRBM(r), writer);
            '		}
            '	}
            '	writer.Close();
            '}
            'catch (IOException e)
            '{
            '	LOGGER.error(e.Message, e);
            '}
        End Sub

        Public Function load(file As String) As DeepRBM
            'try
            '{
            '	IList<string> lines = IOUtils.readLines(new System.IO.StreamReader(file));

            '	int[] layerInfo = COMMA_TO_INT_ARRAY_DESERIALIZER.apply(lines[0]);
            '	int layers = layerInfo.Length / 3;

            '	LayerParameters[] layerParameters = new LayerParameters[layers];
            '	for (int l = 0; l < layers; l++)
            '	{
            '		layerParameters[l] = (new LayerParameters()).setNumRBMS(layerInfo[l * 3]).setVisibleUnitsPerRBM(layerInfo[l * 3 + 1]).setHiddenUnitsPerRBM(layerInfo[l * 3 + 2]);
            '	}

            '	RBMLayer[] rbmLayers = new RBMLayer[layers];

            '	int startIndex = 1;
            '	for (int l = 0; l < layers; l++)
            '	{

            '		RBM[] rbms = new RBM[layerParameters[l].NumRBMS];

            '		int length = 1 + layerParameters[l].VisibleUnitsPerRBM;
            '		for (int r = 0; r < layerParameters[l].NumRBMS; r++)
            '		{

            '			IList<string> rbmData = lines.subList(startIndex, startIndex + length);
            '			rbms[r] = RBM_PERSISTER.buildRBM(rbmData);
            '			startIndex += length;
            '		}
            '		rbmLayers[l] = new RBMLayer(rbms);
            '	}

            '	return new DeepRBM(rbmLayers);
            '}
            'catch (IOException e)
            '{
            '	LOGGER.error(e.Message, e);
            '	return null;
            '}
            Throw New NotImplementedException()
        End Function

    End Class
End Namespace
