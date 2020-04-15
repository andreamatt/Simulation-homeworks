import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot

A = 1.8988


def f(x):
	if x == 0:
		return 1 / A
	return abs(sin(pi * x) / (A * pi * x))


Ntr = 1000000
U1 = np.random.rand(Ntr) * 12 - 6
U2 = np.random.rand(Ntr) * 1 / A
samples = []

for i in range(Ntr):
	if U2[i] <= f(U1[i]):
		samples.append(U1[i])

plt.hist(samples, bins=200, density=True)

X = np.arange(-6, 6, 0.01)
Y = np.array([f(x) for x in X])
plt.plot(X, Y, 'k-')

plt.plot([-6, -6], [0, 1/A], 'r-')
plt.plot([6, 6], [0, 1/A], 'r-')
plt.plot([-6, 6], [1/A, 1/A], 'r-')

print(f"Efficiency: {len(samples)/Ntr}")
plt.show()