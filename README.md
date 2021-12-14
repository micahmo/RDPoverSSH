![logo](https://raw.githubusercontent.com/micahmo/RDPoverSSH/main/RDPoverSSH/Images/logo.png)


# RDPoverSSH

A peer-to-peer Windows Remote Desktop solution using SSH.

### About

RDPoverSSH uses SSH tunnels with certificate authentication to map a port on the local machine to any port on the remote machine. It is specifically designed for remote control with Microsoft's Remote Desktop Protocol, although any port mapping may be configured.

It is especially useful in an environment where the target machine is behind a NAT router or firewall that is under someone else's control. Using reverse SSH tunnels, the target machine can initiate the connection outwards. Once the tunnel is open and a port is mapped to the target machine, it is available to receive connections without any port forwarding required on the target side.

Note that port forwarding is required on either the source or target side of the connection. RDPoverSSH will not work if no port forwarding is possible.

### Download

Download the latest release [here](https://github.com/micahmo/RDPoverSSH/releases/latest).

### How to Use

Read the scenario descriptions to determine which is most applicable. Note that the following is true in both cases:
 - Port forwarding must be configured on one end or the other.
 - It is recommended to begin configuration on whichever machine will be the SSH server because...
   1. You will have to copy the SSH server private key to the SSH client.
   2. In case of a reverse tunnel, you will have copy the server local tunnel port to the client.

#### Scenario 1 (Normal Tunnel)

> Use this mode if you want to connect from machine A to B, and you have permissions configure port forwarding on machine B's network.

**Machine B (Connection Target, SSH Server)**

**Machine A (Connection Source, SSH Client)**

#### Scenario 2 (Reverse Tunnel)

> Use this mode if you want to conect from machine A to B, but you only have permissions to configure port forwarding on machine A (for example, if machine B is behind a corporate firewall).

This mode uses an SSH reverse tunnel, in which machine B initiates the connection to machine A, after which connections can travel over a port mapped from machine A to another port on machine B.

**Machine A (Connection Source, SSH Server)**

**Machine B (Connection Target, SSH Client)**

### Misc

In the examples above, public port 443 was forwarded to the SSH server machine's port 22. This is a useful practice for a few reasons.
1. Many port scanners hammer port 22. While using 443 as the public SSH port doesn't make your SSH server more secure per se (that's what the public/private keys are for), using a non-standard public port will reduce the amount of unwanted traffic.
2. Using a reverse tunnel to initiate an SSH connection from inside a corporate firewall is often not sufficient. Corporate firewalls may also block traffic on port 22, even outgoing. But, without deep packet inspection, they cannot block a standard HTTP port like 443. Using that as the SSH endpoint raises the chances that it will be able to get out of a corporate firewall.

That said, you may already be hosting other web services on your public port 443, which you may already be forwarding to an nginx reverse proxy, for example. In that case, how can you also use it for SSH? In comes a useful tool called [sslh](https://www.rutschle.net/tech/sslh/README.html). Their log line describes this exact scenario.

> A typical use case is to allow serving several services on port 443 (e.g. to connect to SSH from inside a corporate firewall, which almost never block port 443) while still serving HTTPS on that port.
You can configure it to handle all requests on 443. It will do packet inspection and route HTTP packets to port 443 your web service while routing SSH packets to port 22 of your SSH server.

It sits behind your port 443 and receives all traffic on that port. It then routes based on packet type. For HTTP(S) packets, it can route to your web server (e.g., nginx) and for SSH packets, it can route to your SSH server.

Configuring sslh is beyond the scope of this document.

### Requirements

Requires Windows 10 or later.

# Attribution
[Icon](https://www.flaticon.com/premium-icon/data-transfer_2985993) made by [Prosymbols Premium](https://www.flaticon.com/authors/prosymbols-premium) from [www.flaticon.com](https://www.flaticon.com/).