import socket

def main():
    host = "192.168.56.101"
    port = 8080
    
    try:
        # Create a UDP socket
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        
        # Server address tuple
        server_addr = (host, port)
        
        # Get user input
        message = input("Please enter the message: ")
        
        # Send message to server
        sock.sendto(message.encode(), server_addr)
        print(f"Sent message to {host}:{port}")
        
        # Receive response from server
        response, server = sock.recvfrom(256)
        print(f"From Server: {response.decode()}")
        
    except socket.error as e:
        print(f"Socket error: {e}")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        # Close the socket
        sock.close()

if __name__ == "__main__":
    main()