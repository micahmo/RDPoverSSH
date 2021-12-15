![logo](https://raw.githubusercontent.com/micahmo/RDPoverSSH/main/RDPoverSSH/Images/logo.png)


# RDPoverSSH

A peer-to-peer Windows Remote Desktop solution using SSH.

## About

RDPoverSSH uses SSH tunnels with certificate authentication to map a port on the local machine to any port on the remote machine. It is specifically designed for remote control with Microsoft's Remote Desktop Protocol, although any port mapping may be configured.

It is especially useful in an environment where the target machine is behind a NAT router or firewall that is under someone else's control. Using reverse SSH tunnels, the target machine can initiate the connection outwards. Once the tunnel is open and a port is mapped to the target machine, it is available to receive connections without any port forwarding required on the target side.

For some background on how reverse SSH tunnels work, check out the several great answers on this StackOverflow question. https://unix.stackexchange.com/questions/46235/how-does-reverse-ssh-tunneling-work

> Note that port forwarding is required on at least one side of the connection. RDPoverSSH will not work if no port forwarding is possible.

## Download

Download the latest release [here](https://github.com/micahmo/RDPoverSSH/releases/latest).

## How to Use

See the [wiki](https://github.com/micahmo/RDPoverSSH/wiki) for full instructions.

## Screenshots

### RDP over Normal Tunnel

![image](https://user-images.githubusercontent.com/7417301/146233570-3d1c7d81-2845-419b-b882-79f7c8957f95.png)

![image](https://user-images.githubusercontent.com/7417301/146233646-5060137e-3164-42a6-b30c-5817db151f20.png)

### RDP over Reverse Tunnel

![image](https://user-images.githubusercontent.com/7417301/146233725-779bbe0c-c694-4d99-9ea9-98df4e6a7598.png)

![image](https://user-images.githubusercontent.com/7417301/146233760-7b44d715-49d9-43ba-b266-d1709d77b44d.png)

### Condensed UI

![image](https://user-images.githubusercontent.com/7417301/146233797-91029011-f427-45f0-a430-1110afa89168.png)

### Dark Mode

![image](https://user-images.githubusercontent.com/7417301/146233997-29b4c707-a31b-475a-905a-703313e03260.png)

## Requirements

Requires Windows 10 or later.

# Attribution
[Icon](https://www.flaticon.com/premium-icon/data-transfer_2985993) made by [Prosymbols Premium](https://www.flaticon.com/authors/prosymbols-premium) from [www.flaticon.com](https://www.flaticon.com/).