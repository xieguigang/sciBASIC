import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// =========================================================================
// Spiking neural network tutorial — the njs twin of spiking_nn.vb
// =========================================================================

const pad = (value, width) => String(value).padStart(width);
const padEnd = (value, width) => String(value).padEnd(width);
const fixed = (value, digits) => value.toFixed(digits);
const percent = (value) => (value * 100).toFixed(1) + '%';

console.log('  Spiking Neural Network  ·  LIF + SurrogateGrad + Adam');
console.log('');

const result = stdlib.ml.snnTrain(120, 40, 2024, 64, 30, 36);

console.log(`  数据: 3 类 × 8 维高斯团簇（σ=0.09）, 训练 ${result.trainSamples}, 测试 ${result.testSamples}`);
console.log(`  网络结构: 8 → [LIF×${result.hidden} β=0.85 θ=1.0] → [LIF×3], T=${result.timeSteps}`);
console.log('  训练配置: Adam(lr=0.002) + 梯度裁剪(1.0) + FastSigmoid 替代梯度(α=2)');
console.log('');

console.log('  样本 #0 的频率编码脉冲栅格（行=输入神经元, 列=时间步, █=脉冲）:');
console.log(result.inputRaster);
console.log('');

console.log('  epoch |   loss   | test-acc');

for (let i = 0; i < result.lossHistory.length; i++) {
    if (i === 0 || (i + 1) % 5 === 0) {
        console.log(`   ${pad(i + 1, 4)} | ${pad(fixed(result.lossHistory[i], 4), 8)} | ${pad(percent(result.accuracyHistory[i]), 8)}`);
    }
}

console.log('');
console.log(`  loss 曲线: ${stdlib.ml.sparkline(result.lossHistory)}`);
console.log(`  准确率曲线: ${stdlib.ml.sparkline(result.accuracyHistory)}`);
console.log('');
console.log(`  [结果] 测试集准确率: ${percent(result.accuracy)} (${Math.round(result.accuracy * result.testSamples)}/${result.testSamples})`);

console.log('');
console.log('  —— 单样本推理轨迹（测试样本 #0）——');
console.log('  输入脉冲栅格:');
console.log(result.inputRaster);
console.log('');
console.log(result.outputRaster);

// a formatted report through the standard report api
const doc = stdlib.report.document('Spiking neural network', 'sciBASIC standard library');

doc.add(stdlib.report.heading('1. 训练配置'));
doc.add(stdlib.report.list([
    `3 类 × 8 维高斯团簇（σ=0.09），训练 ${result.trainSamples} / 测试 ${result.testSamples} 样本`,
    `LIF×${result.hidden} → LIF×3，仿真步数 T=${result.timeSteps}，β=0.85，θ=1.0`,
    'Adam(lr=0.002) + 梯度裁剪 1.0 + FastSigmoid 替代梯度'
], false));
doc.add(stdlib.report.heading('2. 测试集准确率'));
doc.add(stdlib.report.paragraph(
    `${percent(result.accuracy)}，${result.epochs} 轮训练后的 loss = ${fixed(result.lossHistory[result.lossHistory.length - 1], 4)}`));
doc.add(stdlib.report.heading('3. 推理轨迹'));
doc.add(stdlib.report.code(result.inputRaster, 'text'));
doc.add(stdlib.report.code(result.outputRaster, 'text'));

console.log('');
console.log('md  : ' + doc.save(here('spiking-nn.md')));
console.log('html: ' + doc.save(here('spiking-nn.html')));

console.log('done: spiking-nn');
