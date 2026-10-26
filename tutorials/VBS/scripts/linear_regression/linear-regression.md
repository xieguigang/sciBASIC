# 1. Synthetic dataset

The dataset contains 100 samples of one feature column x and the label y = 2x + 3 + noise.

# 2. Fitted models

| model | R2 | RMSE |
| --- | --- | --- |
| linear | 0.998790 | 0.201119 |
| poly(2) | 0.998794 | 0.200790 |

# 3. Observation and fitted line

![linear regression](G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\scripts\linear_regression\linear-regression.png)

# 4. First 5 prediction rows

|  | x | label:y | label:prediction | label:residual |
| :--- | ---: | ---: | ---: | ---: |
| sample_1 | 0 | 2.7 | 2.98624 | 0.286238 |
| sample_2 | 0.1 | 3 | 3.18641 | 0.186415 |
| sample_3 | 0.2 | 3.3 | 3.38659 | 0.0865917 |
| sample_4 | 0.3 | 3.6 | 3.58677 | -0.0132313 |
| sample_5 | 0.4 | 3.9 | 3.78695 | -0.113054 |