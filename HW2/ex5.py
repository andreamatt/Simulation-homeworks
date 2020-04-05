# Exercise 5
# Compute an approximate value for π by using Monte-Carlo simulation to approximate the ratio between the
# area of a circle of radius 1 to the area of the square circumscribed to it (which has side length equal to 2).
# 1. Set a stopping rule in terms of the confidence interval for the success probability (where a success occurs if
# a point falls within the circle).
# Make an algorithm that keeps drawing additional points until the stopping rule is satisfied.

# import
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, pi

# define corrected functions for std and variance


def std(values):
	return np.std(values, ddof=1)


outcomes = []
points_inside = []
points_outside = []
# generate some data first
i = 0
while i <= 100:
	i += 1
	x = np.random.rand() * 2 - 1
	y = np.random.rand() * 2 - 1
	d = sqrt(x**2 + y**2)
	if d <= 1:
		outcomes.append(1)
		points_inside.append([x, y])
	else:
		outcomes.append(0)
		points_outside.append([x, y])

l = 0.02
gamma = 0.95
z = norm.ppf((1 + gamma) / 2)
while (True):
	S = std(outcomes)
	interval_len = 2 * z * S / sqrt(len(outcomes))
	if interval_len < l:
		break

	i += 1
	x = np.random.rand() * 2 - 1
	y = np.random.rand() * 2 - 1
	d = sqrt(x**2 + y**2)
	if d <= 1:
		outcomes.append(1)
		points_inside.append([x, y])
	else:
		outcomes.append(0)
		points_outside.append([x, y])

points_inside = np.array(points_inside)
points_outside = np.array(points_outside)
plt.scatter(points_inside[:, 0], points_inside[:, 1], s=0.2)
plt.scatter(points_outside[:, 0], points_outside[:, 1], s=0.2)
plt.axis('equal')
plt.show()

print(f'Estimated π value: {mean(outcomes) * 4: .4f}, using {i} points')
print(f'Estimated probability: {mean(outcomes): .4f}, with CI at 0.95 of size {interval_len: .4f}, compared to the theoretical probability {pi/4: .4f}')
