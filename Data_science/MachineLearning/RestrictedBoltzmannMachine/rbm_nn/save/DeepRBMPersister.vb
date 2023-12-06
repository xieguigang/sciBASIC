Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.deep

Namespace nn.rbm.save

    ''' <summary>
    ''' Created by kenny on 5/22/14.
    ''' </summary>
    Public Class DeepRBMPersister

        Private Const DELIM As Char = ","c

        Private Shared ReadOnly RBM_PERSISTER As RBMPersister = New RBMPersister()

        Public Overridable Sub save(deepRBM As DeepRBM, file As String)
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

        Public Overridable Function load(file As String) As DeepRBM
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
