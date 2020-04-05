# import and solve data
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log
from statsmodels.api import qqplot


# define corrected functions for std and variance
def std(values):
	return np.std(values, ddof=1)


Ntr = 10000
results = -np.log(np.random.rand(Ntr)) / 2
results = expon.ppf(np.random.rand(Ntr), scale=1 / 2)

qqplot(results, dist=expon, scale=1 / 2, line='45')
qqplot(results, dist=expon, scale=1 / 5, line='45')
qqplot(results, dist=expon, scale=1 / 0.5, line='45')
plt.show()