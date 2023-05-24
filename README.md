![logo](https://raw.githubusercontent.com/micahmo/RDPoverSSH/main/RDPoverSSH/Images/logo.png)


# RDPoverSSH

A peer-to-peer Windows Remote Desktop solution using SSH.

## About

RDPoverSSH uses SSH tunnels with certificate authentication to map a port on the local machine to any port on the remote machine. It is specifically designed for remote control with Microsoft's Remote Desktop Protocol, although any port mapping may be configured.

It is especially useful in an environment where the target machine is behind a NAT router or firewall that is under someone else's control. Using reverse SSH tunnels, the target machine can initiate the connection outwards. Once the tunnel is open and a port is mapped to the target machine, it is available to receive connections without any port forwarding required on the target side.

For some background on how reverse SSH tunnels work, check out the several great answers on this StackOverflow question. https://unix.stackexchange.com/questions/46235/how-does-reverse-ssh-tunneling-work

> Note that either direct WAN access or router port forwarding is required on at least one side of the connection. RDPoverSSH will not work if the tunnel target's port is not publicly accessible.

## Download

Download the latest release [here](https://github.com/micahmo/RDPoverSSH/releases/latest).

## How to Use

See the [wiki](https://github.com/micahmo/RDPoverSSH/wiki) for full instructions.

## Screenshots

### RDP over Normal Tunnel

#### Connection Source

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/6e7f7f96-6514-4183-8bce-f02f6a339db3)

#### Connection Target

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/08ca77a8-f495-47de-8c71-e1d65efb3f9b)

### RDP over Reverse Tunnel

#### Connection Source

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/10b5cf31-5cbb-4212-a245-fd0f5f4758d3)

#### Connection Target

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/f28a343a-d8fd-4068-805d-ed517815d4e0)

### Condensed UI

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/fb33b4e4-a5df-4bea-bd85-78b39e083ad1)

### Dark Mode

![image](https://github.com/micahmo/RDPoverSSH/assets/7417301/8556ed36-e374-41fb-9f3c-45ad717513a9)

## Requirements

Requires Windows 10 or later.

# Attribution
[Icon](https://www.flaticon.com/premium-icon/data-transfer_2985993) made by [Prosymbols Premium](https://www.flaticon.com/authors/prosymbols-premium) from [www.flaticon.com](https://www.flaticon.com/).