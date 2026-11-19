# Microsoft.VisualBasic.MachineVision.UNet

A U-Net semantic image segmentation model built on the sciBASIC# CNN layers, with symmetric encoder-decoder, skip connections and a complete training/evaluation loop.

## Features

- `UNetModel` built from `UNetOptions` with forward/backward propagation and model persistence
- mini-batch Adam / SGD(momentum) trainer with class-weighted softmax cross-entropy
- DSB2018 dataset loading, PNG mask decoding and RLE mask codec
- deterministic synthetic sample generator for numerical self-checks
- IoU, Dice, pixel accuracy and foreground/background IoU metrics
