import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.animation as animation
import re

# Step 1: Read the data from CSV file
data = pd.read_csv('NewRightHand_20230618172227.csv')

# Step 2: Create a 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Step 3: Group the points based on finger name
groups = data.groupby('FingerName')

# Step 4: Connect the points within each group using lines
colors = ['r', 'g', 'b', 'c', 'm']  # Define colors for each group
lines = []

for name, group in groups:
    finger_number = re.findall(r'\d+', name)[-1]  # Extract the last number from the finger name
    line, = ax.plot([], [], [], marker='o', linestyle='-', color=colors[int(finger_number)-1])
    lines.append(line)


# Set plot limits
min_x, max_x = data['PositionX'].min(), data['PositionX'].max()
min_y, max_y = data['PositionY'].min(), data['PositionY'].max()
min_z, max_z = data['PositionZ'].min(), data['PositionZ'].max()
ax.set_xlim(min_x, max_x)
ax.set_ylim(min_y, max_y)
ax.set_zlim(min_z, max_z)

# Step 5: Animate the plot
# Update function for the animation
def update(frame):
    # Clear previous frame
    ax.cla()
    
    # Set labels and title
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    ax.set_title('Hand Motion')
    
    # Set the limits of the animation based on frame range
    ax.set_xlim(min_x, max_x)
    ax.set_ylim(min_y, max_y)
    ax.set_zlim(min_z, max_z)

    # Filter data based on current frame
    frame_data = data[data['Frame'] == frame]

    # Plot the points
    ax.scatter(frame_data['PositionX'], frame_data['PositionY'], frame_data['PositionZ'], c=frame_data['FingerName'].apply(lambda x: colors[int(''.join(filter(str.isdigit, x))) - 1]), marker='o')
    
    # Connect the points with lines
    for line, group in zip(lines, groups):
        finger_name = group[0]
        finger_data = frame_data[frame_data['FingerName'] == finger_name]
        line.set_data(finger_data['PositionX'], finger_data['PositionY'])
        line.set_3d_properties(finger_data['PositionZ'])

    # Show frame number
    ax.text2D(0.05, 0.95, f'Frame: {frame}', transform=ax.transAxes, color='black')


# Determine the frame range
frame_start = data['Frame'].min()
frame_end = data['Frame'].max()

# Create the animation
ani = animation.FuncAnimation(fig, update, frames=range(frame_start, frame_end+1), interval=200)

# Step 6: Show the animation
plt.show()
