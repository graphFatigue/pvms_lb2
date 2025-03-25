import socket

def main():
    host = "192.168.56.101"
    port = 8080
    
    try:
        # Create a TCP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        
        # Connect to server
        sock.connect((host, port))
        print(f"Connected to {host}:{port}")
        
        # Get user input
        message = input("Please enter the message: ")
        
        # Send message to server
        sock.sendall(message.encode())
        
        # Receive response from server
        response = sock.recv(256).decode()
        print(f"From Server: {response}")
        
    except socket.error as e:
        print(f"Socket error: {e}")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        # Close the socket
        sock.close()

if __name__ == "__main__":
    main()