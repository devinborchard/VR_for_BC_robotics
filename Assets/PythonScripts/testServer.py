import socket
import torch
import numpy as np
import sys
import torch
import torch.nn as nn
from model import parseState, parseImage  # Replace 'your_module' with the actual module name
import matplotlib.pyplot as plt
import h5py
    
step = 0

file_path = "C:/Users/Devin/Desktop/demos/dataSquarev4.hdf5"
demo = 'demo_129'
    

# Server config
HOST = '127.0.0.1'  # Localhost (or use '0.0.0.0' for all interfaces)
PORT = 65432        # Port to listen on (non-privileged ports are > 1023)

actions = []
with h5py.File(file_path, 'r') as f:
    # Get the data at the specified path
    actions = f[f'data/{demo}/actions'][:]

# Create a socket
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind socket to address and port
server_socket.bind((HOST, PORT))

# Listen for incoming connections
server_socket.listen()

try:
    while True:
        client_socket, client_address = server_socket.accept()
        # print(f"Connected by {client_address}")

        with client_socket:

            try:
                action = actions[step]
                print("OUTPUT:", action)
                step = step +1
                client_socket.sendall(str(action.tolist()).encode())
            except Exception as e:
                error_msg = f"Error: {str(e)}"
                print(error_msg)
                client_socket.sendall(error_msg.encode())

except KeyboardInterrupt:
    print("Server shutting down...")

finally:
    server_socket.close()