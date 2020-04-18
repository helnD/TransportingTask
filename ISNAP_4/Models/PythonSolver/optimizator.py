from scipy.optimize import linprog
import socket
import json

class Request:

    def __init__(self, a, b, c):
        self.a = a
        self.b = b
        self.c = c

class Responce:

    def __init__(self, x, fun):
        self.x = x
        self.fun = fun

def deserialize(message):

    json_obj = json.loads(message)

    weights = json_obj['A']
    limitations = json_obj['B']
    target = json_obj['C']

    return Request(weights, limitations, target)

def solve(c, A, b):

    print(A)
    print(b)
    print(c)

    res = linprog(c, A_eq = A, b_eq = b, method='Simplex', bounds=(0, None))
    print(res)
    
    return res.x, res.fun

def serialize(response):

    json_str = json.dumps(response.__dict__)

    return json_str


if __name__ == "__main__":


    sock = socket.socket()
    sock.bind(('localhost', 5265))
    sock.listen(5)
    
    conn, addr = sock.accept()
    
    while True:
        
        try:
        
            message = conn.recv(1024).decode('utf-8')
            
        except ConnectionResetError:
            break
        
        request = deserialize(message)
    
        x, fun = solve(request.c, request.a, request.b)

        response = serialize(Responce(x.tolist(), fun))
    
        conn.send(response.encode('utf-8'))
    
    conn.close()


