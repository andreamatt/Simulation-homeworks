from matplotlib import pyplot as plt
import numpy as np
import json
from classes import *
from plot_delay import *
from plot_success import *
from plot_energy import *
from plot_heatmaps import *
from plot_outcomes import *
from plot_success_over_distance import *


jsonFile = open("runResults.json")
data = json.load(jsonFile)
runResult = RunResult(data)

# plot_outcomes(sim_stats)
# plot_success_over_distance(sim_stats)

# plot_delay_over_lambda_and_duty(runResult)
# plot_delay_over_lambda_and_n(runResult)

# plot_success_over_lambda_and_n(runResult)
# plot_success_over_lambda_and_duty(runResult)

# plot_energy_over_lambda_and_n(runResult)
# plot_energy_over_lambda_and_duty(runResult)

plot_heatmaps(runResult)
# plot_heatmaps_hist(runResult)