# Exercise 6
# Verify that the CDF inversion formula for the generation of exponential random variates of average value equal
# to 2 actually generates exponential variates.
# 1. Extract Ntr exponential random variates with the same average value equal to 2 using through the CDF inversion method.
# 2. Draw a QQ-plot to compare your draws against the quantiles of the exponential distribution with the same average value.
# 3. What happens if, instead, you draw your QQ-plot against the quantiles of an exponential distribution with a different average value?
# And what if you increase (e.g., 2×, 4×, . . . ) the number of exponential draws? Discuss.

# import
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log
from statsmodels.api import qqplot

Ntr = 10000
# use the inverted cdf: x = - ln(U) / mean
average = 2
results = -np.log(np.random.rand(Ntr)) / average

# draw qq plot
qqplot(results, dist=expon, scale=1 / average, line='45')

# draw qq against different average values: 5 and 0.5
qqplot(results, dist=expon, scale=1 / 5, line='45')
qqplot(results, dist=expon, scale=1 / 0.5, line='45')

# try with more draws (x2, x4, x20)
Ntr_2 = Ntr*2
results = -np.log(np.random.rand(Ntr_2)) / average
qqplot(results, dist=expon, scale=1 / average, line='45')

Ntr_4 = Ntr*4
results = -np.log(np.random.rand(Ntr_4)) / average
qqplot(results, dist=expon, scale=1 / average, line='45')

Ntr_20 = Ntr*20
results = -np.log(np.random.rand(Ntr_20)) / average
qqplot(results, dist=expon, scale=1 / average, line='45')

plt.show()