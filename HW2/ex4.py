# import and solve data
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp

# define corrected functions for std and variance
def variance(values):
    return np.var(values, ddof=1)

def std(values):
    return np.std(values, ddof=1)

def triangular(k):
	if k >=2 and k<=7:
		return 1/36 * (k-1)
	else:
		return 1/36 * (13-k)

data = np.array([1, 4, 2, 7, 10, 9, 9, 14, 7, 5, 3])
N = sum(data)
X = range(2, 13)
Y = data/sum(data)
plt.bar(X, Y) # bar, lines o cosa?
Y = [triangular(k) for k in X]
plt.bar(X, Y)

K = len(X)


T = 0
for i in range(K):
	k = X[i]
	T += (data[i] - triangular(k)*N)**2 / (N*triangular(k))

print("T=",T)
print("Prob t>=T (p-value)", 1 - chi2.cdf(T, K-1))

possib = [1, 2, 3, 4, 5, 6]
d1 = np.random.choice(possib, size=10000)
d2 = np.random.choice(possib, size=10000)
tot = list(d1+d2)

plt.show()