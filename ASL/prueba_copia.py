import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib import animation
import re

# Read the CSV file
df = pd.read_csv('prueba_csv.csv')

# Extract unique finger numbers
finger_numbers = df['FingerName'].str.extract(r'(\d)')[0].unique().astype(int)

# Create a dictionary to map finger numbers to colors
color_map = plt.cm.get_cmap('hsv', len(finger_numbers))

# Create a 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Initialize the lists
x = []
y = []
z = []
colors = []

# Iterate over the rows of the DataFrame
for index, row in df.iterrows():
    frame = row['Frame']
    position_x = row['PositionX']
    position_y = row['PositionY']
    position_z = row['PositionZ']
    finger = int(re.search(r'\d+', row['FingerName']).group())  # Extract finger number from the column

    # Append the position values and corresponding color to the lists
    x.append(position_x)
    y.append(position_y)
    z.append(position_z)
    colors.append(color_map(finger - 1))

# Plot the points
ax.scatter(x, y, z, c=colors, marker='o')

# Set labels and title
ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')
ax.set_title('Hand Motion')

# Initialize line objects
lines = []

# Initialize lines for each finger group
for _ in finger_numbers:
    lines.append(ax.plot([], [], [], lw=2)[0])

# Update function for the animation
def update(frame):
    # Clear previous frame
    ax.clear()
    
    # Plot the points
    ax.scatter(x, y, z, c=colors, marker='o')
    
    # Set labels and title
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    ax.set_title('Hand Motion')
    
    # Set the limits of the animation based on frame range
    start_frame = df['Frame'].min()
    end_frame = df['Frame'].max()
    ax.set_xlim(df['PositionX'].min(), df['PositionX'].max())
    ax.set_ylim(df['PositionY'].min(), df['PositionY'].max())
    ax.set_zlim(df['PositionZ'].min(), df['PositionZ'].max())

    # Filter data based on current frame
    frame_data = df[df['Frame'] == frame]

    # Update lines for each finger group
    for finger_number, line in zip(finger_numbers, lines):
        finger_data = frame_data[frame_data['FingerName'].str.contains(f'FingerName{finger_number}')]
        
        if not finger_data.empty:
            line.set_data(finger_data['PositionX'], finger_data['PositionY'])
            line.set_3d_properties(finger_data['PositionZ'])
            line.set_color(color_map(finger_number - 1))
            line.set_linewidth(2)
    
    # Show frame number
    ax.text2D(0.05, 0.95, f'Frame: {frame}', transform=ax.transAxes, color='black')
    
# Create and display the animation
ani = animation.FuncAnimation(fig, update, frames=df['Frame'].unique(), interval=200, blit=False)

plt.show()
