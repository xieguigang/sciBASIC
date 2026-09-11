# Word2Vec Embedding Trainer with Skip-Gram and CBOW

Trains word embedding vectors with skip-gram or CBOW over a Huffman tree, and queries the resulting vector model for similar words and analogies.

## Overview
- Multi-threaded Word2Vec training (`setNumOfThread`) driven by a fluent factory for vector size, window, sampling rate, frequency threshold and learning rate.
- Skip-gram and CBOW objectives implemented over Huffman-tree word neurons (`TrainMethod.Skip_Gram` / `TrainMethod.CBow`).
- Corpus ingestion from sentences or plain token collections, with a frequency threshold for pruning rare words.
- Vector model queries: nearest neighbours by word or by a raw centre vector, plus `word a - word b + word c` analogy search.

## Key Types
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.Word2VecFactory` — fluent builder (`setVectorSize`, `setWindow`, `setMethod`, `setSample`, `setNumOfThread`) returning a trainer via `build`.
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.Word2Vec` — the trainer: `readTokens`, `training` and `outputVector`.
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.VectorModel` — the trained embedding set; `similar`, `analogy`, `getWordVector`.
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.TrainMethod` — `CBow` or `Skip_Gram`.
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.WordNeuron` — a vocabulary word with its Huffman path and vector.
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.WordScore` — one ranked neighbour (word name plus score).
- `Microsoft.VisualBasic.Data.NLP.Word2Vec.Trainer` — background training task used by the parallel trainer.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec

Dim w2v As Word2Vec = New Word2VecFactory() _
    .setVectorSize(100) _
    .setWindow(5) _
    .setMethod(TrainMethod.Skip_Gram) _
    .setNumOfThread(4) _
    .build()

Call w2v.readTokens(New Sentence({"the", "quick", "brown", "fox", "jumps"}))
Call w2v.training()

Dim model As VectorModel = w2v.outputVector()

For Each hit As WordScore In model.similar("fox", 10)
    Console.WriteLine($"{hit.name}  {hit.score}")
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.NLP.Word2Vec`
- TargetFramework: `net10.0`
- Tags: `scibasic;word2vec;word-embedding;skip-gram;machine-learning`

## License
GPL-3.0-or-later
