import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot
from pandas import read_csv

R = 5
N = 10


def prob_overral_failure(N, R, p):
	Ks = np.arange(0, N + 1, 1)
	prev_s = 1 - np.power(p, Ks)
	prev_s_repeated = np.tile(prev_s, (N + 1, 1)).T
	prob_next_K_given_prev_K = binom.pmf(Ks, N, prev_s_repeated)
	# first step, power for all stages, last step
	res = binom.pmf(Ks, N, 1 - p) @ np.linalg.matrix_power(prob_next_K_given_prev_K, R - 1) * np.power(p, Ks)
	return sum(res)


delta = 1 / 200
X = np.arange(0, 1, delta)
Y = []
for x in X:
	Y.append(prob_overral_failure(10, 5, x))

Y = np.array(Y)
plt.scatter(X, Y, s=0.5)
pdf_X = X[1:]
pdf_Y = (Y[1:] - Y[:-1])/delta
plt.scatter(pdf_X, pdf_Y, s=0.5, c='r')

plt.show()