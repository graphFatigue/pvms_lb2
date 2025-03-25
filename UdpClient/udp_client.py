import socket

def main():
    host = "192.168.56.101"
    port = 8080

    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
        server_addr = (host, port)
        print(f"Ready to send to {host}:{port}")
        
        while True:
            message = input("Enter number (or 'quit'): ")
            if message.lower() == 'quit':
                break
            
            # Send message to server
            s.sendto((message + '\n').encode(), server_addr)
            
            # Receive response from server
            data, server = s.recvfrom(1024)
            print(f"From Server: {data.decode()}")

if __name__ == "__main__":
    main()