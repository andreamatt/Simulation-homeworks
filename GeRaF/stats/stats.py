from matplotlib import pyplot as plt
import numpy as np
import json
from classes import *
from plot_success import *
from plot_success_over_distance import *
from plot_delay import *
from plot_outcomes import *


jsonFile = open("runResults.json")
data = json.load(jsonFile)

simulations = data['simulationStats']

sim_params = SimulationParameters(data['simulationParameters'])
protocol_params = ProtocolParamaters(data['protocolParameters'])
sim_stats = [SimulationStat(s) for s in data['simulationStats']]

plot_outcomes(sim_stats)