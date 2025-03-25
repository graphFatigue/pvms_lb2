import socket

def main():
    host = "192.168.56.101"
    port = 8080

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        print(f"Connected to {host}:{port}")
        
        while True:
            message = input("Enter number (or 'quit'): ")
            if message.lower() == 'quit':
                break
            
            s.sendall((message + '\n').encode())
            data = s.recv(1024)
            print(f"Received: {data.decode()}")

if __name__ == "__main__":
    main()