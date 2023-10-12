import re
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# Define the 3D points
points = [
    ["RightFinger1Metacarpal", -0.01627529, 0.02286363, 0.01598704],
    ["RightFinger1Proximal", -0.01694441, 0.0283941, 0.05235562],
    ["RightFinger1Distal", -0.007557034, 0.03201461, 0.07840627],
    ["RightFinger1Tip", 0.005271435, 0.03595877, 0.107349],
    ["RightFinger2Metacarpal", -0.004224181, 0.02230704, 0.01765347],
    ["RightFinger2Proximal", 0.00714004, 0.06087565, 0.04931271],
    ["RightFinger2Medial", 0.01982117, 0.06141067, 0.08991256],
    ["RightFinger2Distal", 0.018695, 0.04349315, 0.1089537],
    ["RightFinger2Tip", 0.01179564, 0.01992798, 0.1163598],
    ["RightFinger3Metacarpal", 0.005186319, 0.01930285, 0.01697645],
    ["RightFinger3Proximal", 0.02444434, 0.0540601, 0.04707414],
    ["RightFinger3Medial", 0.03786957, 0.05344224, 0.08862415],
    ["RightFinger3Distal", 0.03360868, 0.03048337, 0.1054873],
    ["RightFinger3Tip", 0.02247632, 0.004955411, 0.1048271],
    ["RightFinger4Metacarpal", 0.01435649, 0.01523054, 0.01543576],
    ["RightFinger4Proximal", 0.04008067, 0.04481351, 0.04267478],
    ["RightFinger4Medial", 0.049227, 0.04567039, 0.08241588],
    ["RightFinger4Distal", 0.04424989, 0.02698565, 0.100251],
    ["RightFinger4Tip", 0.03400898, 0.003796458, 0.1039037],
    ["RightFinger5Metacarpal", 0.02140844, 0.01020908, 0.0127964],
    ["RightFinger5Proximal", 0.0516212, 0.03363132, 0.0360738],
    ["RightFinger5Medial", 0.05851674, 0.03517818, 0.06831345],
    ["RightFinger5Distal", 0.0583154, 0.027547, 0.08760378],
    ["RightFinger5Tip", 0.05503368, 0.01484561, 0.104171],
]

# Separate the points into x, y, z lists
x = [point[1] for point in points]
y = [point[2] for point in points]
z = [point[3] for point in points]

# Create a 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Plot the points as a scatter plot
ax.scatter(x, y, z)

# Dictionary to store the lines by finger number
lines = {}

# Connect points with the same finger number
for point in points:
    finger_match = re.match(r"RightFinger(\d+)", point[0])
    if finger_match:
        finger_num = int(finger_match.group(1))
        if finger_num not in lines:
            lines[finger_num] = [[point[1], point[2], point[3]]]
        else:
            lines[finger_num].append([point[1], point[2], point[3]])

# Set colors for the lines
colors = ["red", "blue", "green", "purple", "orange"]

# Plot lines connecting points with the same finger number
for finger_num, line_points in lines.items():
    line_x = [point[0] for point in line_points]
    line_y = [point[1] for point in line_points]
    line_z = [point[2] for point in line_points]
    ax.plot(line_x, line_y, line_z, color=colors[finger_num-1])

# Set labels for axes
ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')

# Show the plot
plt.show()
