import socket
import torch
import numpy as np
import sys
import torch
import torch.nn as nn
from model import parseState, parseImage  # Replace 'your_module' with the actual module name
import matplotlib.pyplot as plt

ACTION_SCALE = torch.tensor([0.01, 0.01, 0.01, 1, 1, 1, 1])  # Example per-dimension max

class ImageEncoder(nn.Module):
    def __init__(self):
        super().__init__()
        self.cnn = nn.Sequential(
            nn.Conv2d(3, 16, kernel_size=5, stride=2),  # (16, 40, 82)
            nn.ReLU(),
            nn.Conv2d(16, 32, kernel_size=3, stride=2),  # (32, 19, 40)
            nn.ReLU(),
            nn.Conv2d(32, 64, kernel_size=3, stride=2),  # (64, 9, 19)
            nn.ReLU(),
        )
        self.pool = nn.AdaptiveAvgPool2d(1)  # (64, 1, 1) after pooling
        self.flattened_size = 64  # This is the size after flattening

    def forward(self, x):
        x = self.cnn(x)
        x = self.pool(x)
        x = x.view(x.size(0), -1)  # flatten (B, 64)
        return x

class StateEncoder(nn.Module):
    def __init__(self, state_dim, hidden_size):
        super().__init__()
        self.fc = nn.Sequential(
            nn.Linear(state_dim, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, hidden_size),
            nn.ReLU()
        )

    def forward(self, x):
        return self.fc(x)

class MultiModalRegNet(nn.Module):
    def __init__(self, state_dim, action_dim, hidden_size=128):
        super().__init__()
        self.state_encoder = StateEncoder(state_dim, hidden_size)
        self.image_encoder = ImageEncoder()

        combined_input_size = hidden_size + self.image_encoder.flattened_size
        self.decoder = nn.Sequential(
            nn.Linear(combined_input_size, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, action_dim)
        )

    def forward(self, state, image):
        s = self.state_encoder(state)       # (B, hidden_size)
        i = self.image_encoder(image)       # (B, flattened_img_size)
        x = torch.cat([s, i], dim=1)        # (B, combined_input_size)
        return self.decoder(x)              # (B, action_dim)

    
print("Loading model...")
model = MultiModalRegNet(state_dim=7, action_dim=7)
model.load_state_dict(torch.load('C:/Users/Devin/CS933 Project/Assets/PythonScripts/bc_squareBasic.pth'))
model.eval()
print("Model loaded.")

def process_state(states_string):
    states = states_string.split('~')

    times, dones, eef_pos, eef_quat, eef_rot, gripper = parseState(states[0])
    stateData = [eef_pos, eef_rot, gripper]
    # print("STATE DATA: ", stateData)

    img1 = parseImage(states[1])

    img2 = parseImage(states[2])
    # plt.imshow(img2)
    # plt.title("Parsed Image")
    # plt.axis("off")
    # plt.show()
    image_list = [img1, img2]
    image_obs = np.concatenate(image_list, axis=1)  # (T, 3*num_keys, 84, 84)
    image_obs = image_obs.astype(np.float32) / 255.0

    select_obs = np.hstack(stateData).reshape(1, -1)


    state_tensor = torch.tensor(select_obs, dtype=torch.float32)
    image_tensor = torch.tensor(image_obs, dtype=torch.float32).unsqueeze(0)  # (1, 84, 168, 3)
    image_tensor = image_tensor.permute(0, 3, 1, 2)  # (1, 3, 84, 168)

    with torch.no_grad():
        out = model(state_tensor, image_tensor)
        action = (out[0] * ACTION_SCALE).tolist() 

    return action

# Server config
HOST = '127.0.0.1'  # Localhost (or use '0.0.0.0' for all interfaces)
PORT = 65432        # Port to listen on (non-privileged ports are > 1023)

# Create a socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind socket to address and port
server_socket.bind((HOST, PORT))

# Listen for incoming connections
server_socket.listen()

def receive_until_delimiter(client_socket, delimiter="!"):
    buffer = b""
    while True:
        part = client_socket.recv(4096)
        if not part:
            break
        buffer += part
        if delimiter.encode() in buffer:
            break
    return buffer.decode().replace(delimiter, "")

try:
    while True:
        client_socket, client_address = server_socket.accept()
        # print(f"Connected by {client_address}")

        with client_socket:
            full_data = receive_until_delimiter(client_socket)
            # print("Received state string", full_data)

            try:
                output = process_state(full_data)
                # print("OUTPUT:", output)
                client_socket.sendall(str(output).encode())
            except Exception as e:
                error_msg = f"Error: {str(e)}"
                print(error_msg)
                client_socket.sendall(error_msg.encode())

except KeyboardInterrupt:
    print("Server shutting down...")

finally:
    server_socket.close()